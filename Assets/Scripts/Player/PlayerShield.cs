using UnityEngine;
using UnityEngine.InputSystem;
using VD.Core;

namespace VD.Player
{
    /// <summary>
    /// 실드 액티브 스킬 (M4-4) — 게임 내 유일 액티브 스킬(upgrade-pool §5).
    /// <b>자체 HP를 가진 방어막</b>(무적 프레임 아님): 발동하면 지속시간 동안 활성, 그동안 들어오는 데미지를
    /// <see cref="TryAbsorb"/>로 실드 HP가 먼저 흡수한다. <b>지속시간 만료 또는 실드 HP 소진</b>이면 종료→쿨다운.
    /// <para>발동 = 코너 전용 버튼(<see cref="TryActivate"/>, HUD M4-4) + <b>Space</b>(PC 보조, controls-design §5.5). 시작부터 보유.
    /// <b>쿨다운 = 첫 15초, 이후 사용마다 +5초 연장</b>(에스컬레이팅, 사용자 결정). 강화 3종(쿨다운감소/지속증가/HP증가)=3choice.</para>
    /// 상태는 <see cref="GameEvents.Shield"/>로 노출(HUD 버튼 활성/쿨다운 필 구독). 수치는 Day5.
    /// </summary>
    public sealed class PlayerShield : MonoBehaviour
    {
        [Header("실드 스탯 (수치 Day5, 강화로 증가)")]
        [Tooltip("실드 HP 총량. 활성 중 이만큼 데미지 흡수. 강화(ShieldHp)가 가산")]
        [SerializeField] float shieldHp = 50f;
        [Tooltip("지속시간(초). 강화(ShieldDuration)가 가산")]
        [SerializeField] float duration = 4f;

        [Header("쿨다운 (첫 15초, 사용마다 +5초 연장)")]
        [Tooltip("첫 발동 후 쿨다운(초)")]
        [SerializeField] float baseCooldown = 15f;
        [Tooltip("사용마다 추가되는 쿨다운 연장(초). 2회차=base+1×, 3회차=base+2×…")]
        [SerializeField] float cooldownPerUse = 5f;

        [Header("비주얼")]
        [Tooltip("활성 시 켜지는 반투명 버블(플레이어 자식). 본격 VFX는 M4-9")]
        [SerializeField] GameObject bubbleVisual;

        bool _active;
        float _timeLeft;         // 남은 지속시간
        float _cooldownLeft;     // 남은 쿨다운
        float _cooldownTotal;    // 이번 쿨다운 총량(HUD 필 정규화용)
        float _curShieldHp;      // 현재 실드 HP
        int _useCount;           // 누적 발동 횟수(쿨다운 연장 계산)
        float _cooldownReduction; // 쿨다운 감소 강화(비율 누적, 0~0.9)

        /// <summary>실드 활성 중.</summary>
        public bool IsActive => _active;
        /// <summary>재발동 가능(비활성 + 쿨다운 없음).</summary>
        public bool IsReady => !_active && _cooldownLeft <= 0f;

        void Awake()
        {
            if (bubbleVisual != null) bubbleVisual.SetActive(false);
        }

        void Start() => Publish();

        void Update()
        {
            if (!IsPlaying()) return;
            float dt = Time.deltaTime;

            if (_active)
            {
                _timeLeft -= dt;
                if (_timeLeft <= 0f) { Deactivate(); return; }
            }
            else if (_cooldownLeft > 0f)
            {
                _cooldownLeft -= dt;
                if (_cooldownLeft < 0f) _cooldownLeft = 0f;
                Publish();
            }

            // Space(PC 보조) 발동 — HUD 버튼과 동일 로직 공유.
            var kb = Keyboard.current;
            if (kb != null && kb.spaceKey.wasPressedThisFrame) TryActivate();
        }

        /// <summary>발동 시도. 준비 상태가 아니면 false(버튼·Space 공용).</summary>
        public bool TryActivate()
        {
            if (!IsReady) return false;
            _active = true;
            _timeLeft = duration;
            _curShieldHp = shieldHp;
            _useCount++;
            if (bubbleVisual != null) bubbleVisual.SetActive(true);
            AudioManager.Instance?.PlaySfx(SfxId.ShieldOn, transform.position);   // 발동음(M5-5)
            Debug.Log($"[TEMP] 실드 발동 #{_useCount} (HP {shieldHp}, 지속 {duration}s)", this);   // TODO: 임시 로그
            Publish();
            return true;
        }

        void Deactivate()
        {
            _active = false;
            if (bubbleVisual != null) bubbleVisual.SetActive(false);

            // 에스컬레이팅 쿨다운: base + perUse×(사용횟수-1), 감소 강화 적용.
            float cd = baseCooldown + cooldownPerUse * (_useCount - 1);
            cd *= Mathf.Max(0f, 1f - _cooldownReduction);
            _cooldownTotal = cd;
            _cooldownLeft = cd;
            Debug.Log($"[TEMP] 실드 종료 → 쿨다운 {cd:0.#}s", this);   // TODO: 임시 로그
            Publish();
        }

        /// <summary>
        /// 실드 활성 시 데미지 흡수 — 흡수하면 true(플레이어 HP 무피해). 실드 HP 소진 시 종료+쿨다운.
        /// 소진시키는 히트의 초과분도 흡수(플레이어 무피해, 관대). <see cref="PlayerHealth.ApplyDamage"/>가 먼저 질의.
        /// </summary>
        public bool TryAbsorb(float amount)
        {
            if (!_active) return false;
            _curShieldHp -= amount;
            if (_curShieldHp <= 0f) Deactivate();   // 소진 → 종료
            return true;
        }

        // ── 강화(3choice, M4-4) — UpgradeSystem이 라우팅 ──────────────
        /// <summary>실드 HP 총량 증가.</summary>
        public void AddShieldHp(float amount) => shieldHp += amount;
        /// <summary>지속시간 증가(초).</summary>
        public void AddDuration(float sec) => duration += sec;
        /// <summary>쿨다운 감소(비율 누적, 상한 90%).</summary>
        public void AddCooldownReduction(float pct) => _cooldownReduction = Mathf.Min(0.9f, _cooldownReduction + pct);

        void Publish()
        {
            var ev = GameManager.Instance?.Events;
            if (ev == null) return;
            float cd01 = (_cooldownTotal > 0f && _cooldownLeft > 0f) ? _cooldownLeft / _cooldownTotal : 0f;
            float cdLeft = Mathf.Max(0f, _cooldownLeft);
            ev.SetShieldState(new ShieldState(_active, IsReady, cd01, cdLeft));
        }

        static bool IsPlaying()
        {
            var gm = GameManager.Instance;
            return gm == null || gm.State == GameState.Playing;
        }
    }
}
