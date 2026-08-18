using UnityEngine;
using EnemyEntity = VD.Enemy.Enemy;   // 'Enemy'가 네임스페이스(VD.Enemy)와 겹쳐 별칭 사용

namespace VD.Player
{
    /// <summary>
    /// 플레이어 체력 (M1-4: 적 접촉 데미지). 적(트리거 콜라이더)이 플레이어에 닿으면 HP가 감소한다.
    /// <b>아군 오사 방지</b>를 위해 <c>IDamageable</c>를 구현하지 <b>않고</b> 스스로 <see cref="OnTriggerEnter"/>로 적 접촉을 감지한다
    /// (플레이어가 IDamageable이면 발사 시 자기 콜라이더 안에서 생성되는 총알이 자신을 때림 → 회피, 레이어 불필요).
    /// HP 0 시 게임오버 전이·HP UI·결과화면은 M1-9/M1-10. 지금은 HP 감소만.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public sealed class PlayerHealth : MonoBehaviour
    {
        [Tooltip("최대 체력. 수치는 Day5")]
        [SerializeField] float maxHp = 100f;

        float _hp;

        void Awake()
        {
            _hp = maxHp;
        }

        void OnTriggerEnter(Collider other)
        {
            var e = other.GetComponentInParent<EnemyEntity>();
            if (e == null) return;   // 적이 아님(자기 총알 등) → 무시

            _hp -= e.ContactDamage;
            if (_hp < 0f) _hp = 0f;
            Debug.Log($"[TEMP] 플레이어 피격 -{e.ContactDamage} → HP {_hp}/{maxHp}", this);   // TODO: 임시 로그, 검증 후 제거
            // HP 0 시 게임오버 전이는 M1-9.
        }
    }
}
