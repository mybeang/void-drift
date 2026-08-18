using UnityEngine;
using VD.Core;

namespace VD.Enemy
{
    /// <summary>
    /// 적 스포너 (하드코딩 M1-4 1단계) — <see cref="GameState.Playing"/>에서 일정 간격으로 적을 스폰한다.
    /// **스폰 위치 = 랜덤**(먼 +Z 평면의 X/Y 범위 내). 편대/웨이브 등 공간 포메이션 패턴은 M5-8로 이관.
    /// 튜닝(속도·간격·스폰 범위·despawn)은 이 스포너 한 곳에 모으고, 스폰 시 적에 주입(<see cref="Enemy.Launch"/>).
    /// </summary>
    public sealed class EnemySpawner : MonoBehaviour
    {
        [Header("참조")]
        [Tooltip("적 풀. 비우면 씬에서 자동 탐색")]
        [SerializeField] EnemyPool pool;
        [Tooltip("오브 풀(M1-5). 비우면 씬에서 자동 탐색. 없으면 오브 드랍 없음")]
        [SerializeField] OrbPool orbPool;

        [Header("스폰 (수치는 Day5 튜닝)")]
        [Tooltip("스폰 간격(초)")]
        [SerializeField] float spawnInterval = 1.0f;
        [Tooltip("적 이동 속도(월드 유닛/초). -Z 직진")]
        [SerializeField] float enemySpeed = 12f;
        [Tooltip("스폰 Z(먼 안쪽). 여기서 -Z로 접근 시작")]
        [SerializeField] float spawnZ = 70f;
        [Tooltip("이 Z 이하로 지나가면 풀 반납(화면 뒤). 카메라 z보다 뒤")]
        [SerializeField] float despawnZ = -50f;
        [Tooltip("스폰 X 랜덤 범위(월드). 화면 폭에 맞춰 튜닝(뷰포트 기반 아님 — 프레이밍 바뀌면 재조정)")]
        [SerializeField] Vector2 spawnXRange = new Vector2(-18f, 18f);
        [Tooltip("스폰 Y 랜덤 범위(월드)")]
        [SerializeField] Vector2 spawnYRange = new Vector2(-9f, 9f);

        float _cooldown;

        void Awake()
        {
            if (pool == null) pool = FindAnyObjectByType<EnemyPool>();
            if (orbPool == null) orbPool = FindAnyObjectByType<OrbPool>();
        }

        void Update()
        {
            if (!IsPlaying() || pool == null) return;

            _cooldown -= Time.deltaTime;
            if (_cooldown > 0f) return;
            _cooldown = spawnInterval;

            float x = Random.Range(spawnXRange.x, spawnXRange.y);
            float y = Random.Range(spawnYRange.x, spawnYRange.y);

            Enemy e = pool.Get();
            // 진행 방향(-Z)을 바라보게: 코가 플레이어 쪽. (모델 nose 축이 다르면 프리팹 Model 회전으로 보정)
            e.transform.SetPositionAndRotation(new Vector3(x, y, spawnZ), Quaternion.LookRotation(Vector3.back, Vector3.up));
            e.Launch(enemySpeed, despawnZ);
            e.SetDropHandler(DropOrb);   // 실사망 시 그 위치에 오브 드랍(M1-5 1단계)
        }

        /// <summary>적 실사망 콜백 — 사망 위치에 오브를 하나 드랍(M1-5 1단계: 스폰+존재만).</summary>
        void DropOrb(Vector3 pos)
        {
            if (orbPool == null) return;
            Orb orb = orbPool.Get();
            orb.transform.position = pos;
        }

        static bool IsPlaying()
        {
            var gm = GameManager.Instance;
            return gm == null || gm.State == GameState.Playing;
        }
    }
}
