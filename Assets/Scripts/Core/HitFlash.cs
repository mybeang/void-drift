using UnityEngine;

namespace VD.Core
{
    /// <summary>
    /// 피격 시 빨간 깜빡 (M4-9). <see cref="Flash"/> 호출 시 자식 렌더러의 <c>_BaseColor</c>를
    /// <see cref="MaterialPropertyBlock"/>으로 잠깐 빨갛게 틴트했다가 복원한다(재질 자체는 안 건드림 → 공유 재질 안전).
    /// <para>렌더러는 매 Flash마다 재수집(적 비주얼은 스폰마다 주입/교체되므로). unscaled 시간 사용(일시정지 무관).
    /// 적(<see cref="VD.Enemy.Enemy"/>)·플레이어(<see cref="VD.Player.PlayerHealth"/>)가 피격 훅에서 호출.</para>
    /// </summary>
    public sealed class HitFlash : MonoBehaviour
    {
        [Tooltip("깜빡 색(베이스컬러 틴트)")]
        [SerializeField] Color flashColor = Color.red;
        [Tooltip("깜빡 지속(초)")]
        [SerializeField] float duration = 0.08f;

        static readonly int BaseColorID = Shader.PropertyToID("_BaseColor");

        MaterialPropertyBlock _mpb;
        Renderer[] _renderers;
        float _timer;
        bool _flashing;

        public void Flash()
        {
            _renderers = GetComponentsInChildren<Renderer>(true);   // 현재 비주얼 기준 재수집
            if (_renderers == null || _renderers.Length == 0) return;

            _mpb ??= new MaterialPropertyBlock();
            _mpb.SetColor(BaseColorID, flashColor);
            foreach (var r in _renderers)
                if (r != null) r.SetPropertyBlock(_mpb);

            _timer = duration;
            _flashing = true;
        }

        void Update()
        {
            if (!_flashing) return;
            _timer -= Time.unscaledDeltaTime;
            if (_timer <= 0f) Restore();
        }

        void Restore()
        {
            _flashing = false;
            if (_renderers == null) return;
            foreach (var r in _renderers)
                if (r != null) r.SetPropertyBlock(null);   // 오버라이드 제거 → 원래 색
        }
    }
}
