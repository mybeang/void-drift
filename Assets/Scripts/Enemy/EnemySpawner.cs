using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VD.Core;

namespace VD.Enemy
{
    /// <summary>
    /// 적 스포너 (M2-5e: 툴 데이터 → 런타임 스폰) — <see cref="GameState.Playing"/>에서 일정 간격으로,
    /// SO DB(<see cref="spawnTable"/>)에서 <b>가중 랜덤</b>으로 <see cref="EnemyDefinition"/>을 골라
    /// 빌더(<see cref="EnemyBuilder"/>)로 조립(비주얼+스탯)해 스폰한다. 위치는 랜덤(먼 +Z 평면), 공간 포메이션은 M5-8.
    /// <para>시작 시 DB의 유니크 비주얼을 <see cref="EnemyVisualCache"/>로 프리로드(async) 완료 후 스폰 개시.
    /// 튜닝(간격·스폰 범위·despawn 경계)은 여기, 적 속도/체력/데미지는 적 stats(데이터, M2-5b) → despawn 경계만 <see cref="Enemy.Launch"/>로 주입.</para>
    /// </summary>
    public sealed class EnemySpawner : MonoBehaviour
    {
        /// <summary>스폰 후보 1행 — 적 정의 + 가중치(등장 확률). weight 0 이하는 제외.</summary>
        [Serializable]
        public struct SpawnEntry
        {
            public EnemyDefinition def;
            [Min(0f)] public float weight;
        }

        [Header("참조")]
        [Tooltip("적 풀. 비우면 씬에서 자동 탐색")]
        [SerializeField] EnemyPool pool;
        [Tooltip("오브 풀(M1-5). 비우면 씬에서 자동 탐색. 없으면 오브 드랍 없음")]
        [SerializeField] OrbPool orbPool;
        [Tooltip("적탄 풀(M3-2, 탄막 발사용). 비우면 씬에서 자동 탐색. 없으면 탄막 무발사")]
        [SerializeField] EnemyBulletPool bulletPool;
        [Tooltip("난이도 배율 소스(M2-5d 스텁, M4-5 실배율). 비우면 배율 1.0")]
        [SerializeField] DifficultyProvider difficulty;

        [Header("스폰 DB (툴 데이터 → 가중 랜덤)")]
        [Tooltip("스폰 후보 = EnemyDefinition + 가중치. 인스펙터에서 SO 드래그 + weight 지정")]
        [SerializeField] SpawnEntry[] spawnTable = Array.Empty<SpawnEntry>();

        [Tooltip("오브 색 드랍(3-5) — 순서=색 인덱스(0녹·1빨·2파, Orb.colorVisuals와 일치). 각 SO의 minWave/weight/xp가 데이터.")]
        [SerializeField] OrbDefinition[] orbColors = Array.Empty<OrbDefinition>();

        [Tooltip("웨이브 밴드 스폰 프로파일(3-4). 현재 wave ≤ maxWave인 가장 좁은 밴드에서 가중 랜덤. 비우면 spawnTable 폴백. Spawn Profile Authoring 편집.")]
        [SerializeField] SpawnProfileDefinition[] waveBands = Array.Empty<SpawnProfileDefinition>();

        [Header("스폰 (수치는 Day5 튜닝)")]
        [Tooltip("스폰 간격(초)")]
        [SerializeField] float spawnInterval = 1.0f;
        [Tooltip("스폰 Z(먼 안쪽). 여기서 -Z로 접근 시작")]
        [SerializeField] float spawnZ = 70f;
        [Tooltip("이 Z 이하로 지나가면 풀 반납(화면 뒤). 카메라 z보다 뒤")]
        [SerializeField] float despawnZ = -50f;
        [Tooltip("스폰 X 랜덤 범위(월드). 화면 폭에 맞춰 튜닝(뷰포트 기반 아님 — 프레이밍 바뀌면 재조정)")]
        [SerializeField] Vector2 spawnXRange = new Vector2(-18f, 18f);
        [Tooltip("스폰 Y 랜덤 범위(월드)")]
        [SerializeField] Vector2 spawnYRange = new Vector2(-9f, 9f);

        readonly EnemyVisualCache _cache = new EnemyVisualCache();
        EnemyBuilder _builder;
        bool _ready;
        float _cooldown;

        void Awake()
        {
            if (pool == null) pool = FindAnyObjectByType<EnemyPool>();
            if (orbPool == null) orbPool = FindAnyObjectByType<OrbPool>();
            if (bulletPool == null) bulletPool = FindAnyObjectByType<EnemyBulletPool>();
            if (difficulty == null) difficulty = FindAnyObjectByType<DifficultyProvider>();
        }

        void Start()
        {
            Warmup().Forget();
        }

        /// <summary>DB의 유니크 비주얼을 프리로드하고 빌더를 준비. 완료 후 스폰 개시.</summary>
        async UniTaskVoid Warmup()
        {
            await _cache.PreloadAsync(DistinctVisuals());
            _builder = new EnemyBuilder(_cache, difficulty, bulletPool);
            _ready = true;
            Debug.Log($"[TEMP] EnemySpawner 준비 완료 — 스폰 후보 {CountValid()}종");   // TODO: 임시 로그
        }

        IEnumerable<UnityEngine.AddressableAssets.AssetReferenceGameObject> DistinctVisuals()
        {
            // 스폰 표 + 모든 웨이브 밴드의 적 비주얼 프리로드(캐시가 GUID로 dedupe).
            foreach (var entry in spawnTable)
                if (entry.def != null) yield return entry.def.visual;

            if (waveBands != null)
                foreach (var b in waveBands)
                    if (b != null && b.table != null)
                        foreach (var e in b.table)
                            if (e.def != null) yield return e.def.visual;
        }

        void Update()
        {
            if (!_ready || !IsPlaying() || pool == null) return;

            // 3-1: 웨이브 기반 동시상한 + 주기(전부 WaveDifficultyConfig 데이터). 상한 미만일 때 주기마다 1마리씩.
            int wave = difficulty != null ? difficulty.CurrentWave : 1;
            int cap = difficulty != null ? difficulty.CurrentCap : 10;

            _cooldown -= Time.deltaTime;
            if (_cooldown > 0f) return;
            if (pool.ActiveCount >= cap) return;            // 상한 도달 → 슬롯 빌 때까지 보류(쿨다운 0 유지 → 다음 프레임 재시도)
            _cooldown = difficulty != null ? difficulty.CurrentInterval : 2f;

            // WHICH 적 = 현재 웨이브 밴드에서 가중 랜덤(3-4). 밴드 없으면 spawnTable 폴백.
            EnemyDefinition def = PickForWave(wave);
            if (def == null) return;

            float x = UnityEngine.Random.Range(spawnXRange.x, spawnXRange.y);
            float y = UnityEngine.Random.Range(spawnYRange.x, spawnYRange.y);

            Enemy e = pool.Get();
            _builder.Build(e, def);   // ① 비주얼 ② effective 스탯 조립
            // 진행 방향(-Z)을 바라보게: 코가 플레이어 쪽. (모델 nose 축 차이는 Day5 비주얼 튜닝)
            e.transform.SetPositionAndRotation(new Vector3(x, y, spawnZ), Quaternion.LookRotation(Vector3.back, Vector3.up));
            e.Launch(despawnZ);           // 속도는 적 stats에서(M2-5b). 여기선 despawn 경계만 주입
            e.SetDropHandler(DropOrb);   // 실사망 시 오브 드랍(M1-5). dropOrb 데이터화(비주얼/xp)는 이후
        }

        /// <summary>현재 웨이브에 맞는 밴드에서 적 하나 선택(3-4). 밴드 없으면 spawnTable 폴백.</summary>
        EnemyDefinition PickForWave(int wave)
        {
            var band = CurrentBand(wave);
            if (band != null) return PickFromEntries(band.table);
            return PickFromTable(spawnTable);
        }

        /// <summary>현재 wave ≤ maxWave인 <b>가장 좁은</b>(maxWave 최소) 유효 밴드. 없으면 null.</summary>
        SpawnProfileDefinition CurrentBand(int wave)
        {
            if (waveBands == null) return null;
            SpawnProfileDefinition best = null;
            foreach (var b in waveBands)
            {
                if (b == null || b.table == null || b.table.Length == 0) continue;
                if (wave <= b.maxWave && (best == null || b.maxWave < best.maxWave)) best = b;
            }
            return best;
        }

        static EnemyDefinition PickFromEntries(SpawnProfileDefinition.Entry[] entries)
        {
            float total = 0f;
            foreach (var e in entries)
                if (e.def != null && e.weight > 0f) total += e.weight;
            if (total <= 0f) return null;

            float r = UnityEngine.Random.Range(0f, total);
            foreach (var e in entries)
            {
                if (e.def == null || e.weight <= 0f) continue;
                r -= e.weight;
                if (r <= 0f) return e.def;
            }
            return null;
        }

        static EnemyDefinition PickFromTable(SpawnEntry[] entries)
        {
            float total = 0f;
            foreach (var entry in entries)
                if (entry.def != null && entry.weight > 0f) total += entry.weight;
            if (total <= 0f) return null;

            float r = UnityEngine.Random.Range(0f, total);
            foreach (var entry in entries)
            {
                if (entry.def == null || entry.weight <= 0f) continue;
                r -= entry.weight;
                if (r <= 0f) return entry.def;
            }
            return null;   // 부동소수 안전망(도달 안 함)
        }

        int CountValid()
        {
            int n = 0;
            foreach (var entry in spawnTable)
                if (entry.def != null && entry.weight > 0f) n++;
            return n;
        }

        /// <summary>적 실사망 콜백 — 사망 위치에 오브 드랍 + 웨이브 기반 색 롤(3-5).</summary>
        void DropOrb(Vector3 pos)
        {
            if (orbPool == null) return;
            Orb orb = orbPool.Get();
            orb.transform.position = pos;
            RollOrbColor(orb);
        }

        /// <summary>현재 웨이브에 가능한 색(wave ≥ minWave)만으로 가중 재정규화해 롤 → 오브에 색·xp 주입. 데이터 없으면 프리팹 기본 유지.</summary>
        void RollOrbColor(Orb orb)
        {
            if (orbColors == null || orbColors.Length == 0) return;
            int wave = difficulty != null ? difficulty.CurrentWave : 1;

            float total = 0f;
            foreach (var d in orbColors)
                if (d != null && wave >= d.minWave && d.weight > 0f) total += d.weight;
            if (total <= 0f) return;

            float r = UnityEngine.Random.Range(0f, total);
            for (int i = 0; i < orbColors.Length; i++)
            {
                var d = orbColors[i];
                if (d == null || wave < d.minWave || d.weight <= 0f) continue;
                r -= d.weight;
                if (r <= 0f) { orb.Configure(i, d.xpValue); return; }
            }
        }

        void OnDestroy()
        {
            _cache.ReleaseAll();
        }

        static bool IsPlaying()
        {
            var gm = GameManager.Instance;
            return gm == null || (gm.State == GameState.Playing && !gm.CombatFrozen);
        }
    }
}
