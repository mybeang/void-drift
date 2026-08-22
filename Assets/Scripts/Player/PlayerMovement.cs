using UnityEngine;
using UnityEngine.InputSystem;
using VD.Core;
using VD.UI;

namespace VD.Player
{
    /// <summary>
    /// 플레이어 기체 "이동" 전담. 오로지 위치 이동만 담당한다(뱅킹 연출은 <see cref="PlayerBanking"/>로 분리).
    /// - 입력: <b>방향형 조이스틱</b>(<see cref="JoystickView"/>가 소유, 포인터/터치) + [PC] 키보드. 레거시 Input.* 미사용.
    ///   조이스틱 <see cref="JoystickView.Value"/>(단위 원반, 방향+세기)를 속도로 사용 — 스틱 유지=계속 이동, 떼면 정지.
    ///   조이스틱은 에디터/모바일에서만 활성(PC 스탠드얼론은 Value=0 → 마우스 이동 차단, 키보드만).
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
        [Tooltip("방향형 조이스틱(가상 스틱). 비우면 Start에서 씬에서 탐색. 없으면 포인터 이동 비활성(키보드만).")]
        [SerializeField] JoystickView joystick;

        [Header("이동 (방향형 조이스틱 → 속도)")]
        [Tooltip("조이스틱 최대 세기(Value=1)일 때 초당 이동량(뷰포트 분율). 1.2 → 약 0.8초에 화면 끝에서 끝.")]
        [SerializeField] float pointerMoveSpeed = 1.2f;
        [Tooltip("속도 상한(월드 유닛/초). 0이면 무제한. 빠른 이동 과속 방지용.")]
        [SerializeField] float maxSpeed = 0f;

        [Header("[PC] 키보드 이동 (WASD/화살표 — 조이스틱과 공존·합산)")]
        [Tooltip("초당 이동량(뷰포트 분율). 1.2 → 약 0.8초에 화면 끝에서 끝. 0이면 키보드 이동 비활성.")]
        [SerializeField] float keyboardMoveSpeed = 1.2f;

        [Header("스폰 위치 (뷰포트 좌표 0~1)")]
        [Tooltip("기본 위치: 정중앙에서 약간 아래 → (0.5, 0.42)")]
        [SerializeField] Vector2 spawnViewportPoint = new Vector2(0.5f, 0.42f);

        [Header("이동 경계 (뷰포트 여백)")]
        [Tooltip("화면 가장자리에서 안쪽으로 남길 여백(뷰포트 비율)")]
        [SerializeField] Vector2 viewportMargin = new Vector2(0.06f, 0.06f);

        Rigidbody _rb;
        Vector2 _accumulatedKeyFrac;   // [PC] 키보드 이동(뷰포트 분율): Update에서 누적, FixedUpdate에서 소비
        float _depth;                  // 카메라 → 기체 평면 거리(뷰포트 변환 z)

        /// <summary>이동속도 배율 강화 — M1-8. pct=0.12 → +12%(누적). 조이스틱·키보드 둘 다에 적용.</summary>
        public void AddMoveSpeedMultiplier(float pct)
        {
            pointerMoveSpeed *= (1f + pct);
            keyboardMoveSpeed *= (1f + pct);
        }

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
            if (joystick == null) joystick = FindAnyObjectByType<JoystickView>();
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
                _accumulatedKeyFrac = Vector2.zero;
                return;
            }

            // [PC] 키보드 이동: WASD/화살표 방향을 초당 이동량으로 누적(뷰포트 분율, 조이스틱과 합산).
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

            Vector2 keyFrac = _accumulatedKeyFrac;
            _accumulatedKeyFrac = Vector2.zero;

            // 방향형 조이스틱: Value(단위 원반)×속도×dt = 이번 스텝 뷰포트 이동(스틱 유지=계속 이동).
            Vector2 stick = joystick != null ? joystick.Value : Vector2.zero;

            // 현재 뷰포트 → 목표 뷰포트.
            Vector3 curVp = targetCamera.WorldToViewportPoint(_rb.position);
            Vector2 targetVp = new Vector2(curVp.x, curVp.y);
            targetVp += stick * (pointerMoveSpeed * Time.fixedDeltaTime);
            targetVp += keyFrac;   // [PC] 키보드 이동(이미 뷰포트 분율) — 조이스틱과 합산

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
