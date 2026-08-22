using UnityEngine;
using VD.Core;
using EnemyEntity = VD.Enemy.Enemy;   // 'Enemy'가 네임스페이스(VD.Enemy)와 겹쳐 별칭 사용

namespace VD.Player
{
    /// <summary>
    /// 플레이어 체력 (M1-4: 적 접촉 데미지 / M1-9: HP0→게임오버 + HP% 노출).
    /// 적(트리거 콜라이더)이 플레이어에 닿으면 HP가 감소한다.
    /// <b>아군 오사 방지</b>를 위해 <c>IDamageable</c>를 구현하지 <b>않고</b> 스스로 <see cref="OnTriggerEnter"/>로 적 접촉을 감지한다
    /// (플레이어가 IDamageable이면 발사 시 자기 콜라이더 안에서 생성되는 총알이 자신을 때림 → 회피, 레이어 불필요).
    /// HP는 <see cref="GameEvents.HpNormalized"/>로 노출(HUD M1-10 대비). HP 0 시 <see cref="GameManager.GameOver"/>.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public sealed class PlayerHealth : MonoBehaviour
    {
        [Tooltip("최대 체력(밸런싱 패스 3-6: 100→50)")]
        [SerializeField] float maxHp = 50f;

        float _hp;
        bool _dead;
        float _regenPerSec;   // 체력재생 강화(M3-4) — 초당 회복량
        PlayerShield _shield;  // 실드 액티브 스킬(M4-4) — 있으면 데미지 우선 흡수
        HitFlash _hitFlash;    // 피격 빨간 깜빡(M4-9)

        void Awake()
        {
            _hp = maxHp;
            _shield = GetComponent<PlayerShield>();
            _hitFlash = GetComponent<HitFlash>();
        }

        void Update()
        {
            // 체력재생(M3-4). timeScale 0(팝업/게임오버)이면 dt=0 → 정지. 만피/사망 시 스킵.
            if (_dead || _regenPerSec <= 0f || _hp >= maxHp) return;
            _hp = Mathf.Min(maxHp, _hp + _regenPerSec * Time.deltaTime);
            PublishHp();
        }

        /// <summary>HP 상태 게시(정규화 + 절대값) — HUD 게이지·숫자 표기 공용(M1-10/M3-4).</summary>
        void PublishHp()
        {
            var ev = GameManager.Instance?.Events;
            if (ev == null) return;
            ev.SetHpNormalized(maxHp > 0f ? _hp / maxHp : 0f);
            ev.SetHpValues(_hp, maxHp);
        }

        /// <summary>체력재생 강화(M3-4, 3choice) — 초당 회복량 가산.</summary>
        public void AddRegen(float perSec)
        {
            _regenPerSec += perSec;
        }

        void Start()
        {
            // GameManager.Awake 이후 시점 — 초기 HP 게시(정규화 + 절대값).
            PublishHp();
        }

        /// <summary>최대 체력 가산 강화 — M1-8. 늘린 만큼 현재 HP도 회복하고 HP% 갱신.</summary>
        public void AddMaxHp(float amount)
        {
            if (_dead) return;
            maxHp += amount;
            _hp = Mathf.Min(_hp + amount, maxHp);
            PublishHp();
        }

        /// <summary>
        /// 데미지 적용(공용) — 적 접촉(<see cref="OnTriggerEnter"/>)과 적탄(<see cref="VD.Enemy.EnemyBullet"/>)/자폭이 호출.
        /// 아군 오사 방지를 위해 <c>IDamageable</c>는 여전히 미구현 — 이 메서드는 적 계열만 명시적으로 호출한다.
        /// </summary>
        public void ApplyDamage(float amount, DamageSource source = DamageSource.Contact)
        {
            if (_dead) return;

            // 실드 활성 시 데미지 우선 흡수 → 플레이어 HP 무피해(M4-4). 흡수 시엔 깜빡·피격음 없음.
            if (_shield != null && _shield.TryAbsorb(amount)) return;

            // 실제 피해 발생 시에만 피격음(M5-5): 접촉(돌진)=chargeAttack, 탄환·자폭=hitPlayer.
            SfxId hitSfx = source == DamageSource.Contact ? SfxId.ChargeAttack : SfxId.HitPlayer;
            AudioManager.Instance?.PlaySfx(hitSfx, transform.position);

            _hitFlash?.Flash();   // 실제 HP 피해 시 빨간 깜빡(M4-9)
            _hp -= amount;
            if (_hp < 0f) _hp = 0f;
            Debug.Log($"[TEMP] 플레이어 피격 -{amount} → HP {_hp}/{maxHp}", this);   // TODO: 임시 로그, 검증 후 제거

            PublishHp();

            if (_hp <= 0f)
            {
                _dead = true;
                GameManager.Instance?.GameOver();   // 정지형 게임오버(M1-9)
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if (_dead) return;

            var e = other.GetComponentInParent<EnemyEntity>();
            if (e == null || e.IsExiting) return;   // 적이 아니거나(자기 총알 등) 이미 접촉 퇴장 중이면 무시(중복 데미지 차단)

            ApplyDamage(e.ContactDamage);
            e.OnContactPlayer();   // 접촉 이펙트 + 명멸 퇴장 개시(I-6)
        }
    }
}
