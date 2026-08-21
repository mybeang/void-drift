using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace VD.Core
{
    /// <summary>
    /// 씬 전환 연출 매니저(M4-10). DontDestroyOnLoad 싱글톤 — 최초 요청 시 자기 자신과 UI를 코드로 생성한다(프리팹 불필요).
    /// 전환 = <b>이클립스 와이프</b>: 화면보다 큰 검은 원형 그림자가 좌→우로 한 번 통과하며
    /// 커버(선단 곡선 가장자리로 덮음) → 씬 로드 → 리빌(후단 가장자리가 빠짐). 일식처럼 그림자가 왔다 사라지는 연출.
    /// <para>모든 트윈은 <see cref="Time.unscaledDeltaTime"/> 기반 — 게임오버(timeScale 0)에서도 동작한다.
    /// 전환 중 원형 Image가 레이캐스트를 막아 입력을 차단한다.</para>
    /// </summary>
    public sealed class SceneTransition : MonoBehaviour
    {
        const float CoverDuration = 0.7f;   // 커버(덮기) 시간
        const float RevealDuration = 0.7f;  // 리빌(걷히기) 시간
        const float HoldAtCover = 0.05f;    // 완전히 덮인 상태 유지(로드 여유)

        static SceneTransition _instance;

        /// <summary>싱글톤 접근 — 없으면 즉석 생성(DontDestroyOnLoad).</summary>
        public static SceneTransition Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("[SceneTransition]");
                    _instance = go.AddComponent<SceneTransition>();
                    DontDestroyOnLoad(go);
                    _instance.Build();
                }
                return _instance;
            }
        }

        /// <summary>전환 진행 중 여부(중복 트리거 방지용).</summary>
        public bool IsTransitioning { get; private set; }

        RectTransform _circle;
        Image _circleImage;

        void Awake()
        {
            if (_instance != null && _instance != this) { Destroy(gameObject); return; }
            _instance = this;
        }

        void Build()
        {
            var canvasGo = new GameObject("TransitionCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 32760; // HUD(100)·레벨업/배너 위 최상단

            var circleGo = new GameObject("EclipseCircle", typeof(Image));
            circleGo.transform.SetParent(canvasGo.transform, false);
            _circle = circleGo.GetComponent<RectTransform>();
            _circle.anchorMin = _circle.anchorMax = _circle.pivot = new Vector2(0.5f, 0.5f);
            _circleImage = circleGo.GetComponent<Image>();
            _circleImage.sprite = BuildCircleSprite(256, 10);
            _circleImage.color = Color.black;
            _circleImage.raycastTarget = true; // 전환 중 입력 차단
            circleGo.SetActive(false);
        }

        /// <summary>이클립스 와이프로 지정 씬을 로드한다.</summary>
        public void TransitionTo(string sceneName)
        {
            if (IsTransitioning) return;
            StartCoroutine(Run(sceneName));
        }

        IEnumerator Run(string sceneName)
        {
            IsTransitioning = true;
            _circle.gameObject.SetActive(true);

            // 커버: 화면 왼쪽 밖 → 중앙(완전히 덮임)
            Layout(out float xStart, out float xEnd);
            yield return Slide(xStart, 0f, CoverDuration);
            if (HoldAtCover > 0f) yield return WaitUnscaled(HoldAtCover);

            // 완전히 가려진 상태에서 씬 로드
            var op = SceneManager.LoadSceneAsync(sceneName);
            while (op != null && !op.isDone) yield return null;

            // 로드된 씬이 정상 진행하도록 시간 복구(게임오버서 0이었을 수 있음)
            Time.timeScale = 1f;

            // 리빌: 중앙 → 화면 오른쪽 밖(새 해상도 재계산)
            Layout(out _, out xEnd);
            yield return Slide(0f, xEnd, RevealDuration);

            _circle.gameObject.SetActive(false);
            IsTransitioning = false;
        }

        // 화면 크기에 맞춰 원 크기·시작 위치 세팅, 커버/리빌 x 목표를 산출.
        void Layout(out float xStart, out float xEnd)
        {
            float w = Screen.width;
            float h = Screen.height;
            float diag = Mathf.Sqrt(w * w + h * h);
            float size = diag * 2f;      // 반지름=대각선 → 중앙서 여유 있게 완전 덮음
            float r = size * 0.5f;
            _circle.sizeDelta = new Vector2(size, size);
            xStart = -(w * 0.5f + r);    // 오른쪽(선단) 가장자리가 화면 왼쪽에 걸침 = 화면 clear
            xEnd = w * 0.5f + r;         // 왼쪽(후단) 가장자리가 화면 오른쪽에 걸침 = 화면 clear
        }

        IEnumerator Slide(float fromX, float toX, float dur)
        {
            float t = 0f;
            while (t < dur)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / dur);
                k = k * k * (3f - 2f * k); // smoothstep
                _circle.anchoredPosition = new Vector2(Mathf.Lerp(fromX, toX, k), 0f);
                yield return null;
            }
            _circle.anchoredPosition = new Vector2(toX, 0f);
        }

        static IEnumerator WaitUnscaled(float sec)
        {
            float t = 0f;
            while (t < sec) { t += Time.unscaledDeltaTime; yield return null; }
        }

        // 부드러운 가장자리(페더)의 검은 원형 스프라이트를 절차적으로 생성.
        static Sprite BuildCircleSprite(int size, int feather)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
            float c = (size - 1) * 0.5f;
            float rad = c;
            var px = new Color32[size * size];
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float d = Mathf.Sqrt((x - c) * (x - c) + (y - c) * (y - c));
                    float a = Mathf.Clamp01((rad - d) / Mathf.Max(1, feather));
                    px[y * size + x] = new Color32(0, 0, 0, (byte)(a * 255f));
                }
            tex.SetPixels32(px);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }
    }
}
