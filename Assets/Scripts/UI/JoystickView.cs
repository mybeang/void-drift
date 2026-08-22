using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using VD.Core;

namespace VD.UI
{
    /// <summary>
    /// 플로팅 <b>방향형</b> 조이스틱 — 가상 스틱의 <b>단일 소스</b>(입력+시각). 밸런싱 패스 ①.
    /// 포인터를 누른 화면 지점에 베이스(<c>joystick_back</c>)가 뜨고, 노브(<c>joystick</c>)는
    /// <b>(현재 포인터 − 누른 시작점)</b>을 <see cref="knobRadius"/>로 클램프한 오프셋만큼 밀린다.
    /// <see cref="Value"/> = 노브 오프셋 ÷ 반경(단위 원반, 방향+세기) — <see cref="VD.Player.PlayerMovement"/>가
    /// 이 값을 읽어 속도로 사용(스틱 유지=계속 이동, 떼면 정지). 시각과 이동이 같은 반경 기준이라 항상 일치.
    /// <para><b>플랫폼 게이트</b>: 에디터 또는 모바일에서만 활성(테스트 편의+모바일 실사용). PC 스탠드얼론
    /// 빌드에선 비활성 → 표시 안 되고 <see cref="Value"/>=0(마우스 이동 차단, 키보드는 PlayerMovement가 별도 처리).</para>
    /// 표시만 하며 입력을 소비하지 않는다(이미지 raycastTarget=Off, 이동은 Pointer 직접 읽기라 무간섭).
    /// </summary>
    public sealed class JoystickView : MonoBehaviour
    {
        [Header("참조")]
        [Tooltip("베이스 컨테이너(joystick_back Image). 눌린 지점으로 이동하며, 눌린 동안만 활성.")]
        [SerializeField] RectTransform root;
        [Tooltip("노브(joystick Image). 베이스 중심 기준으로 드래그 오프셋만큼 이동.")]
        [SerializeField] RectTransform knob;
        [Tooltip("비우면 부모에서 Canvas 탐색.")]
        [SerializeField] Canvas canvas;

        [Header("동작 (시작값 — 튜닝 대상)")]
        [Tooltip("노브 최대 이동 반경(캔버스 유닛). 이 반경 = 최대 세기(Value 크기 1). 드래그가 더 멀어도 여기서 멈춤.")]
        [SerializeField] float knobRadius = 90f;
        [Tooltip("이보다 작은 세기(0~1)는 무시(중심부 데드존). 미세 오터치 드리프트 방지.")]
        [SerializeField] float deadZone01 = 0.08f;
        [Tooltip("누른 지점이 UI(버튼) 위면 조이스틱을 띄우지 않음(실드/설정 버튼 오터치 방지).")]
        [SerializeField] bool blockWhenOverUI = true;

        /// <summary>가상 스틱 출력(단위 원반, 방향+세기 0~1). 비활성/미눌림 시 0. PlayerMovement가 읽음.</summary>
        public Vector2 Value { get; private set; }

        RectTransform _canvasRect;
        bool _pointerEnabled;   // 에디터/모바일에서만 true (PC 스탠드얼론 차단)
        bool _active;
        Vector2 _originLocal;    // 누른 시작점(캔버스 로컬)

        void Awake()
        {
            if (canvas == null) canvas = GetComponentInParent<Canvas>();
            _canvasRect = canvas != null ? canvas.transform as RectTransform : null;
            _pointerEnabled = Application.isEditor || Application.isMobilePlatform;
            Hide();
        }

        void Update()
        {
            var pointer = Pointer.current;
            if (!_pointerEnabled || pointer == null || !IsPlaying())
            {
                if (_active) Hide();
                return;
            }

            Vector2 screenPos = pointer.position.ReadValue();

            if (pointer.press.wasPressedThisFrame)
            {
                if (blockWhenOverUI && IsOverUI()) return;   // 버튼 위에선 안 띄움
                Begin(screenPos);
            }
            else if (_active && pointer.press.isPressed)
            {
                UpdateKnob(screenPos);
            }
            else if (_active && !pointer.press.isPressed)
            {
                Hide();
            }
        }

        void Begin(Vector2 screenPos)
        {
            if (root == null || !ToLocal(screenPos, out _originLocal)) return;
            root.anchoredPosition = _originLocal;   // root 앵커/피벗=중앙 전제 → 로컬점이 곧 위치
            if (knob != null) knob.anchoredPosition = Vector2.zero;
            root.gameObject.SetActive(true);
            _active = true;
            Value = Vector2.zero;
        }

        void UpdateKnob(Vector2 screenPos)
        {
            if (!ToLocal(screenPos, out var cur)) return;
            Vector2 offset = Vector2.ClampMagnitude(cur - _originLocal, knobRadius);
            if (knob != null) knob.anchoredPosition = offset;

            Vector2 v = knobRadius > 0f ? offset / knobRadius : Vector2.zero;   // 단위 원반
            Value = v.magnitude < deadZone01 ? Vector2.zero : v;
        }

        void Hide()
        {
            _active = false;
            Value = Vector2.zero;
            if (root != null) root.gameObject.SetActive(false);
        }

        bool ToLocal(Vector2 screenPos, out Vector2 local)
        {
            local = Vector2.zero;
            if (_canvasRect == null) return false;
            var cam = (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay) ? canvas.worldCamera : null;
            return RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvasRect, screenPos, cam, out local);
        }

        static bool IsOverUI()
        {
            var es = EventSystem.current;
            return es != null && es.IsPointerOverGameObject();
        }

        static bool IsPlaying()
        {
            var gm = GameManager.Instance;
            return gm == null || gm.State == GameState.Playing;   // 단독 테스트 허용
        }
    }
}
