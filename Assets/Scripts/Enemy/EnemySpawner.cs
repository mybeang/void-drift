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
            // 기본 표 + 모든 페이즈 프로파일(M4-6)의 적 비주얼 프리로드(캐시가 GUID로 dedupe).
            foreach (var entry in spawnTable)
                if (entry.def != null) yield return entry.def.visual;

            if (difficulty != null)
                foreach (var prof in difficulty.Profiles())
                    if (prof.table != null)
                        foreach (var e in prof.table)
                            if (e.def != null) yield return e.def.visual;
        }

        void Update()
        {
            if (!_ready || !IsPlaying() || pool == null) return;

            _cooldown -= Time.deltaTime;
            if (_cooldown > 0f) return;

            // M4-6: 현재 페이즈의 스폰 프로파일 우선(적 조합·밀도). 없으면 기본 표/간격으로 폴백.
            SpawnProfileDefinition prof = CurrentProfile();
            _cooldown = (prof != null && prof.spawnInterval > 0f) ? prof.spawnInterval : spawnInterval;

            EnemyDefinition def = PickWeighted(prof);
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

        /// <summary>현재 페이즈의 유효한 스폰 프로파일(테이블에 유효 항목 있음). 없으면 null(기본 표 폴백, M4-6).</summary>
        SpawnProfileDefinition CurrentProfile()
        {
            var phase = difficulty != null ? difficulty.CurrentPhase : null;
            var prof = phase != null ? phase.spawnProfile : null;
            if (prof != null && prof.table != null)
                foreach (var e in prof.table)
                    if (e.def != null && e.weight > 0f) return prof;   // 유효 항목 하나라도 있으면 사용
            return null;
        }

        /// <summary>가중 랜덤으로 스폰 후보 하나 선택 — 프로파일(있으면) 아니면 기본 표. 유효 없으면 null.</summary>
        EnemyDefinition PickWeighted(SpawnProfileDefinition prof)
        {
            if (prof != null) return PickFromEntries(prof.table);
            return PickFromTable(spawnTable);
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

        /// <summary>적 실사망 콜백 — 사망 위치에 오브를 하나 드랍(M1-5 1단계: 스폰+존재만).</summary>
        void DropOrb(Vector3 pos)
        {
            if (orbPool == null) return;
            Orb orb = orbPool.Get();
            orb.transform.position = pos;
        }

        void OnDestroy()
        {
            _cache.ReleaseAll();
        }

        static bool IsPlaying()
        {
            var gm = GameManager.Instance;
            return gm == null || gm.State == GameState.Playing;
        }
    }
}
