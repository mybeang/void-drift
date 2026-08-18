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
        [Tooltip("최대 체력. 수치는 Day5")]
        [SerializeField] float maxHp = 100f;

        float _hp;
        bool _dead;

        void Awake()
        {
            _hp = maxHp;
        }

        void Start()
        {
            // GameManager.Awake 이후 시점 — 초기 HP% 게시.
            GameManager.Instance?.Events?.SetHpNormalized(1f);
        }

        void OnTriggerEnter(Collider other)
        {
            if (_dead) return;

            var e = other.GetComponentInParent<EnemyEntity>();
            if (e == null) return;   // 적이 아님(자기 총알 등) → 무시

            _hp -= e.ContactDamage;
            if (_hp < 0f) _hp = 0f;
            Debug.Log($"[TEMP] 플레이어 피격 -{e.ContactDamage} → HP {_hp}/{maxHp}", this);   // TODO: 임시 로그, 검증 후 제거

            GameManager.Instance?.Events?.SetHpNormalized(_hp / maxHp);

            if (_hp <= 0f)
            {
                _dead = true;
                GameManager.Instance?.GameOver();   // 정지형 게임오버(M1-9)
            }
        }
    }
}
