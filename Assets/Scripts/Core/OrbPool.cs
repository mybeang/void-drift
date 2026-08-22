using UnityEngine;

namespace VD.Core
{
    /// <summary>
    /// 오브 전용 풀 — <see cref="PooledObjectPool{T}"/> 상속. (M1-5)
    /// 자석 타깃(플레이어)을 태그 "Player"로 1회 탐색해 캐시하고, Get 시 오브에 타깃·반납 콜백·자석보너스·<b>페이드 밴드(Z)</b>를 배선한다.
    /// (Core→Player 타입 결합 회피를 위해 컴포넌트 타입이 아닌 태그로 참조.)
    /// 위치는 드랍 측(<see cref="Enemy"/> 사망 훅)이 Get 후 세팅.
    /// </summary>
    public sealed class OrbPool : PooledObjectPool<Orb>
    {
        [Header("페이드 밴드 (Orb, 월드 Z — 근-카메라/뒤쪽 정리)")]
        [Tooltip("red: 이 Z(월드)부터 오브가 흐려지기 시작(이보다 앞=또렷)")]
        [SerializeField] float fadeStartZ = -3f;
        [Tooltip("blue: 이 Z(월드) 이하에서 완전 투명. red~blue 사이는 선형")]
        [SerializeField] float fadeEndZ = -25f;
        [Tooltip("[임시] 페이드 평면 기즈모 표시(red/blue). 튜닝용")]
        [SerializeField] bool drawFadeGizmo = true;
        [Tooltip("기즈모 평면 크기(X 반폭, Y 반높이)")]
        [SerializeField] Vector2 gizmoHalfSize = new Vector2(26f, 13f);

        Transform _player;
        float _magnetBonus;   // 자석범위 강화 누적(M1-8) — 스폰 시 오브에 주입

        protected override void Awake()
        {
            base.Awake();
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) _player = p.transform;
        }

        protected override void OnGet(Orb item)
        {
            item.OnSpawned(_player, Return, _magnetBonus, fadeStartZ, fadeEndZ);
        }

        /// <summary>자석 반경 가산 강화 — M1-8. 이후 스폰되는 오브에 반영(가산 누적).</summary>
        public void AddMagnetRadius(float amount) => _magnetBonus += amount;

        // [임시] 페이드 밴드 시각화 — red 평면(fadeStartZ)·blue 평면(fadeEndZ). 튜닝 후 제거.
        void OnDrawGizmos()
        {
            if (!drawFadeGizmo) return;
            DrawPlane(fadeStartZ, new Color(1f, 0.25f, 0.25f, 0.9f));   // red = 페이드 시작
            DrawPlane(fadeEndZ, new Color(0.3f, 0.5f, 1f, 0.9f));       // blue = 완전 페이드
        }

        void DrawPlane(float z, Color c)
        {
            Gizmos.color = c;
            float w = gizmoHalfSize.x, h = gizmoHalfSize.y;
            Vector3 a = new Vector3(-w, -h, z), b = new Vector3(w, -h, z), d = new Vector3(w, h, z), e = new Vector3(-w, h, z);
            Gizmos.DrawLine(a, b); Gizmos.DrawLine(b, d); Gizmos.DrawLine(d, e); Gizmos.DrawLine(e, a);
            Gizmos.DrawLine(new Vector3(-w, 0f, z), new Vector3(w, 0f, z));   // 중앙 가로선
        }
    }
}
