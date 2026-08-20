using UnityEngine;
using UnityEngine.InputSystem;
using VD.Core;

namespace VD.Player
{
    /// <summary>
    /// 플레이어 기체 "이동" 전담. 오로지 위치 이동만 담당한다(뱅킹 연출은 <see cref="PlayerBanking"/>로 분리).
    /// - 입력: New Input System 포인터/터치 델타(상대 드래그, 해상도 무관). 레거시 Input.* 미사용.
    /// - 이동: 물리(Rigidbody) 속도 직접 매핑. XY 자유, Z 고정.
    ///         이동 목표를 뷰포트 경계 안으로 <b>선-클램프</b>해 바깥으로 미는 속도가 생기지 않게 함(경계 떨림 방지).
    /// - 경계: 카메라 뷰포트 기준 자동(해상도/기기 무관).
    /// 게임 상태가 Playing이 아니면 이동·입력 누적을 멈춘다(일시정지 정합).
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public sealed class PlayerMovement : MonoBehaviour
    {
        [Header("참조")]
        [Tooltip("비우면 Camera.main 사용")]
        [SerializeField] Camera targetCamera;

        [Header("이동 (상대 드래그 → 속도 직접 매핑, 해상도 무관)")]
        [Tooltip("게인: 손가락이 화면의 x% 만큼 움직이면 기체는 화면의 (x×게인)% 만큼 이동.\n" +
                 "예) 3 → 화면의 1/3만 드래그해도 기체가 끝에서 끝까지. 클수록 조금만 움직여도 많이 감.\n" +
                 "픽셀이 아닌 화면 분율 기준이라 에디터/폰 해상도가 달라도 동일하게 동작.")]
        [SerializeField] float dragGain = 3f;
        [Tooltip("이보다 작은 프레임 누적 드래그(화면 분율)는 무시(손떨림 데드존). 0.002 = 화면의 0.2%")]
        [SerializeField] float deadZoneScreenFraction = 0.002f;
        [Tooltip("속도 상한(월드 유닛/초). 0이면 무제한(게인에 완전 비례). 빠른 플릭 과속 방지용.")]
        [SerializeField] float maxSpeed = 0f;

        [Header("[PC] 키보드 이동 (WASD/화살표 — 드래그와 공존·합산)")]
        [Tooltip("초당 이동량(뷰포트 분율). 1.2 → 약 0.8초에 화면 끝에서 끝. 0이면 키보드 이동 비활성.")]
        [SerializeField] float keyboardMoveSpeed = 1.2f;

        [Header("스폰 위치 (뷰포트 좌표 0~1)")]
        [Tooltip("기본 위치: 정중앙에서 약간 아래 → (0.5, 0.42)")]
        [SerializeField] Vector2 spawnViewportPoint = new Vector2(0.5f, 0.42f);

        [Header("이동 경계 (뷰포트 여백)")]
        [Tooltip("화면 가장자리에서 안쪽으로 남길 여백(뷰포트 비율)")]
        [SerializeField] Vector2 viewportMargin = new Vector2(0.06f, 0.06f);

        Rigidbody _rb;
        Vector2 _accumulatedDelta;     // 드래그 픽셀 델타: Update에서 누적, FixedUpdate에서 소비
        Vector2 _accumulatedKeyFrac;   // [PC] 키보드 이동(뷰포트 분율): Update에서 누적, FixedUpdate에서 소비
        float _depth;                  // 카메라 → 기체 평면 거리(뷰포트 변환 z)

        /// <summary>이동속도(드래그 게인) 배율 강화 — M1-8. pct=0.12 → +12%(누적).</summary>
        public void AddMoveSpeedMultiplier(float pct) => dragGain *= (1f + pct);

        void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _rb.useGravity = false;
            // 위치 Z 고정 + 회전은 물리에서 완전 동결(회전은 연출/조준 쪽에서 처리).
            _rb.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
            if (targetCamera == null) targetCamera = Camera.main;
        }

        void Start()
        {
            if (targetCamera != null)
            {
                _depth = Mathf.Abs(transform.position.z - targetCamera.transform.position.z);
                MoveToSpawn();
            }
        }

        void Update()
        {
            if (!IsPlaying())
            {
                _accumulatedDelta = Vector2.zero;
                _accumulatedKeyFrac = Vector2.zero;
                return;
            }

            // 상대 드래그: 누르고 있는 동안의 프레임 델타를 누적(포인터=마우스/터치 통합).
            var pointer = Pointer.current;
            if (pointer != null && pointer.press.isPressed)
                _accumulatedDelta += pointer.delta.ReadValue();

            // [PC] 키보드 이동: WASD/화살표 방향을 초당 이동량으로 누적(뷰포트 분율, 드래그와 합산).
            if (keyboardMoveSpeed > 0f)
            {
                var kb = Keyboard.current;
                if (kb != null)
                {
                    Vector2 dir = Vector2.zero;
                    if (kb.aKey.isPressed || kb.leftArrowKey.isPressed)  dir.x -= 1f;
                    if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) dir.x += 1f;
                    if (kb.wKey.isPressed || kb.upArrowKey.isPressed)    dir.y += 1f;
                    if (kb.sKey.isPressed || kb.downArrowKey.isPressed)  dir.y -= 1f;
                    if (dir.sqrMagnitude > 1e-6f)
                        _accumulatedKeyFrac += dir.normalized * (keyboardMoveSpeed * Time.deltaTime);
                }
            }
        }

        void FixedUpdate()
        {
            if (targetCamera == null) return;

            if (!IsPlaying())
            {
                _rb.linearVelocity = Vector3.zero;
                return;
            }

            Vector2 pxDelta = _accumulatedDelta;
            _accumulatedDelta = Vector2.zero;
            Vector2 keyFrac = _accumulatedKeyFrac;
            _accumulatedKeyFrac = Vector2.zero;

            // 픽셀 델타 → 화면 분율(해상도/DPI 무관). 가로=폭, 세로=높이 기준.
            Vector2 frac = new Vector2(
                Screen.width > 0 ? pxDelta.x / Screen.width : 0f,
                Screen.height > 0 ? pxDelta.y / Screen.height : 0f);

            // 현재 뷰포트 → 목표 뷰포트(손가락 화면분율 × 게인).
            Vector3 curVp = targetCamera.WorldToViewportPoint(_rb.position);
            Vector2 targetVp = new Vector2(curVp.x, curVp.y);
            if (frac.magnitude >= deadZoneScreenFraction)
                targetVp += new Vector2(frac.x * dragGain, frac.y * dragGain);
            targetVp += keyFrac;   // [PC] 키보드 이동(이미 뷰포트 분율, 게인 미적용) — 드래그와 합산

            // 목표를 경계 안으로 선-클램프 → 경계에서 바깥으로 미는 속도가 애초에 안 생김(부르르 떨림 방지).
            targetVp.x = Mathf.Clamp(targetVp.x, viewportMargin.x, 1f - viewportMargin.x);
            targetVp.y = Mathf.Clamp(targetVp.y, viewportMargin.y, 1f - viewportMargin.y);

            // 목표 월드 변위 → 이번 스텝 속도(속도 직접 매핑).
            Vector3 disp = ViewportToPlane(targetVp) - _rb.position;
            disp.z = 0f;
            Vector3 vel = disp / Time.fixedDeltaTime;
            if (maxSpeed > 0f) vel = Vector3.ClampMagnitude(vel, maxSpeed);
            _rb.linearVelocity = new Vector3(vel.x, vel.y, 0f);
        }

        void MoveToSpawn()
        {
            Vector3 p = ViewportToPlane(spawnViewportPoint);
            transform.position = p;
            _rb.position = p;
        }

        Vector3 ViewportToPlane(Vector2 vp)
        {
            return targetCamera.ViewportToWorldPoint(new Vector3(vp.x, vp.y, _depth));
        }

        static bool IsPlaying()
        {
            var gm = GameManager.Instance;
            // GameManager 없이 단독 테스트 시엔 동작 허용.
            return gm == null || gm.State == GameState.Playing;
        }
    }
}
