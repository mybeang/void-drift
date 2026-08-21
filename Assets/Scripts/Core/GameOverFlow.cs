using System.Collections;
using R3;
using UnityEngine;

namespace VD.Core
{
    /// <summary>
    /// 게임오버 연출·전환 흐름(M4-10). GameScene에 1개 배치.
    /// <see cref="GameState.GameOver"/> 진입 시: 플레이어 위치에 폭발 VFX(<c>CFXR Explosion 1</c>) 스폰 →
    /// 짧은 프리즈(폭발 각인) → 현재 점수를 <see cref="HighScoreRepository"/>에 커밋(최고점 갱신·영속) →
    /// 이클립스 와이프(<see cref="SceneTransition"/>)로 ResultScene 전환.
    /// <para>게임오버 중 timeScale 0이므로 대기는 unscaled. 폭발 파티클도 unscaled로 전환해 프리즈 중에도 재생된다.</para>
    /// </summary>
    public sealed class GameOverFlow : MonoBehaviour
    {
        [Tooltip("최고점 저장소 SO(HighScoreRepository) — ResultScene과 같은 에셋을 참조")]
        [SerializeField] HighScoreRepository highScore;

        [Tooltip("플레이어 사망 폭발 VFX (CFXR Explosion 1). 플레이어 위치에 스폰")]
        [SerializeField] GameObject deathExplosionPrefab;

        [Tooltip("폭발 각인용 프리즈 시간(초, unscaled)")]
        [SerializeField] float preTransitionDelay = 1.5f;

        [Tooltip("폭발 VFX 크기 배수")]
        [SerializeField] float explosionScale = 1.5f;

        [SerializeField] string resultSceneName = "ResultScene";

        bool _fired;

        void Start()
        {
            var events = GameManager.Instance != null ? GameManager.Instance.Events : null;
            if (events == null)
            {
                Debug.LogWarning("[GameOverFlow] GameManager/Events 없음 — 비활성.", this);
                enabled = false;
                return;
            }
            events.State.Subscribe(OnState).AddTo(this);
        }

        void OnState(GameState state)
        {
            if (state != GameState.GameOver || _fired) return;
            _fired = true;
            StartCoroutine(Flow());
        }

        IEnumerator Flow()
        {
            SpawnExplosion();

            float t = 0f;
            while (t < preTransitionDelay) { t += Time.unscaledDeltaTime; yield return null; }

            int score = GameManager.Instance != null ? GameManager.Instance.Events.Score.CurrentValue : 0;
            if (highScore != null) highScore.Commit(score);
            else Debug.LogWarning("[GameOverFlow] highScore 미배선 — 점수 저장 생략.", this);

            SceneTransition.Instance.TransitionTo(resultSceneName);
        }

        void SpawnExplosion()
        {
            if (deathExplosionPrefab == null) return;

            var player = GameObject.FindGameObjectWithTag("Player");
            Vector3 pos = player != null ? player.transform.position : Vector3.zero;
            var go = Instantiate(deathExplosionPrefab, pos, Quaternion.identity);
            if (explosionScale > 0f) go.transform.localScale *= explosionScale;

            // 프리즈(timeScale 0) 중에도 폭발이 재생되도록 파티클을 unscaled로 전환.
            foreach (var ps in go.GetComponentsInChildren<ParticleSystem>(true))
            {
                var main = ps.main;
                main.useUnscaledTime = true;
            }

            // 터진 뒤 플레이어는 사라진다.
            if (player != null) player.SetActive(false);
        }
    }
}
