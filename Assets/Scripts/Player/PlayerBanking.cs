using UnityEngine;

namespace VD.Player
{
    /// <summary>
    /// 기체 "연출" 전담 — 화면 중심 오프셋에 비례한 뱅킹(기울기)을 이 오브젝트의 로컬 회전에 적용한다.
    /// 이동(<see cref="PlayerMovement"/>)·발사와 분리된 순수 시각 연출. (M1-3 구조 분리로 PlayerMovement에서 이관)
    /// Model(연출 오브젝트)에 붙인다. 동작은 이관 전과 동일.
    /// </summary>
    public sealed class PlayerBanking : MonoBehaviour
    {
        [Header("참조")]
        [Tooltip("비우면 Camera.main 사용")]
        [SerializeField] Camera targetCamera;

        [Header("뱅킹 (오프셋 비례 기울기, 직접 보간)")]
        [Tooltip("세로 오프셋 최대 시 pitch(도). 위로 갈수록 아래면이 보이도록 nose-down")]
        [SerializeField] float maxPitch = 28f;
        [Tooltip("가로 오프셋 최대 시 yaw(도). 코가 중심을 향하도록(조준) nose-in")]
        [SerializeField] float maxYaw = 28f;
        [Tooltip("가로 이동 시 롤(도) 연출. 0이면 롤 없음")]
        [SerializeField] float maxRoll = 12f;
        [Tooltip("뱅킹 회전 보간 속도(클수록 빠르게 목표각 도달)")]
        [SerializeField] float bankLerpSpeed = 8f;
        [SerializeField] bool invertPitch = false;
        [SerializeField] bool invertYaw = false;
        [Tooltip("화면 가장자리 여백(정규화용). 이 지점에서 최대각 도달.")]
        [SerializeField] Vector2 edgeInset = new Vector2(0.06f, 0.06f);

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

            // 화면 중심(0.5) 대비 정규화 오프셋(-1~+1). 경계에서 ±1 근처.
            Vector3 vp = targetCamera.WorldToViewportPoint(transform.position);
            float denomX = Mathf.Max(0.0001f, 0.5f - edgeInset.x);
            float denomY = Mathf.Max(0.0001f, 0.5f - edgeInset.y);
            float offX = Mathf.Clamp((vp.x - 0.5f) / denomX, -1f, 1f);
            float offY = Mathf.Clamp((vp.y - 0.5f) / denomY, -1f, 1f);

            // 위(+offY) → nose-down(Euler X +) → 아래면이 카메라로.
            float pitch = maxPitch * offY * (invertPitch ? -1f : 1f);
            // 오른쪽(+offX) → 코가 중심(왼쪽)으로 → 조준 방향이 안쪽.
            float yaw = maxYaw * -offX * (invertYaw ? -1f : 1f);
            float roll = maxRoll * -offX;

            Quaternion target = _baseLocalRotation * Quaternion.Euler(pitch, yaw, roll);
            float t = 1f - Mathf.Exp(-bankLerpSpeed * Time.deltaTime);
            transform.localRotation = Quaternion.Slerp(transform.localRotation, target, t);
        }
    }
}
