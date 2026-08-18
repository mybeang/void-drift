using UnityEngine;

namespace VD.Player
{
    /// <summary>
    /// 발사 "조준" 전담 — 화면 중심 오프셋에 비례한 pitch/yaw를 이 오브젝트(FirePoint)의
    /// 로컬 회전에 <b>즉시</b>(보간 없이) 적용한다. <see cref="PlayerBanking"/>(연출)과 <b>동일한 조준각 공식</b>을
    /// 쓰되, 흔들림 없는 "깨끗한 조준 방향"을 만들어 투사체가 <c>FirePoint.forward</c>로 발사되게 한다(M1-3 2단계).
    /// root(Player, 회전 동결)의 자식이라 Model 뱅킹과 같은 부모 프레임에서 계산된다.
    /// roll은 forward를 바꾸지 않으므로 제외. 실제 발사 로직·발사 원점(nose 오프셋)은 3단계.
    /// </summary>
    public sealed class PlayerAim : MonoBehaviour
    {
        [Header("참조")]
        [Tooltip("비우면 Camera.main 사용")]
        [SerializeField] Camera targetCamera;

        [Header("조준각 (오프셋 비례, 즉시 적용 · PlayerBanking과 동일 공식)")]
        [Tooltip("세로 오프셋 최대 시 pitch(도). 위로 갈수록 nose-down")]
        [SerializeField] float maxPitch = 28f;
        [Tooltip("가로 오프셋 최대 시 yaw(도). 코가 중심을 향하도록(조준) nose-in")]
        [SerializeField] float maxYaw = 28f;
        [SerializeField] bool invertPitch = false;
        [SerializeField] bool invertYaw = false;
        [Tooltip("화면 가장자리 여백(정규화용). 이 지점에서 최대각 도달.")]
        [SerializeField] Vector2 edgeInset = new Vector2(0.06f, 0.06f);

        [Header("기즈모 (임시 검증 · M1-3 2단계)")]
        [Tooltip("조준 축(= FirePoint.forward) 레이를 Scene/Game 뷰에 표시. 발사 로직 안착 후 정리.")]
        [SerializeField] bool drawAimGizmo = true;
        [Tooltip("조준 레이 길이(월드 유닛)")]
        [SerializeField] float gizmoLength = 20f;
        [SerializeField] Color gizmoColor = new Color(1f, 0.35f, 0.15f, 1f);

        Quaternion _baseLocalRotation;

        void Awake()
        {
            if (targetCamera == null) targetCamera = Camera.main;
        }

        void Start()
        {
            _baseLocalRotation = transform.localRotation;
        }

        void LateUpdate()
        {
            if (targetCamera == null) return;

            // 화면 중심(0.5) 대비 정규화 오프셋(-1~+1). PlayerBanking과 동일.
            Vector3 vp = targetCamera.WorldToViewportPoint(transform.position);
            float denomX = Mathf.Max(0.0001f, 0.5f - edgeInset.x);
            float denomY = Mathf.Max(0.0001f, 0.5f - edgeInset.y);
            float offX = Mathf.Clamp((vp.x - 0.5f) / denomX, -1f, 1f);
            float offY = Mathf.Clamp((vp.y - 0.5f) / denomY, -1f, 1f);

            // 위(+offY) → nose-down(Euler X +). 오른쪽(+offX) → 코가 중심(안쪽)으로 = 조준.
            float pitch = maxPitch * offY * (invertPitch ? -1f : 1f);
            float yaw = maxYaw * -offX * (invertYaw ? -1f : 1f);

            // 즉시 적용(보간 없음) → 흔들림 없는 깨끗한 조준 방향. roll은 forward 불변이라 생략.
            transform.localRotation = _baseLocalRotation * Quaternion.Euler(pitch, yaw, 0f);
        }

        void OnDrawGizmos()
        {
            if (!drawAimGizmo) return;

            // 조준 축(= FirePoint.forward). 플레이 중엔 LateUpdate가 갱신한 방향, 정지 중엔 +Z.
            Vector3 origin = transform.position;
            Vector3 dir = transform.forward;
            Gizmos.color = gizmoColor;
            Gizmos.DrawLine(origin, origin + dir * gizmoLength);
            Gizmos.DrawWireSphere(origin + dir * gizmoLength, 0.4f);
        }
    }
}
