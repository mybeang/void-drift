using System;
using UnityEngine;

namespace VD.Core
{
    /// <summary>
    /// 경험치 오브 (M1-5). 적 <b>실사망</b> 위치에 드랍된다. (화면 밖 despawn은 드랍 안 함)
    /// 비주얼은 자식 <c>Model</c>(Crystal effect VFX). 풀은 <see cref="OrbPool"/>(<see cref="PooledObjectPool{T}"/> 상속).
    /// <para>거동(사용자 결정 2026-08-18):
    /// <list type="bullet">
    /// <item><b>반경 밖</b> — 전방(월드 -Z, 플레이어 쪽)으로 <b>일정 속도로 흘러 지나침</b>. 플레이어가 근처(반경 내)에 없으면 뒤로 빠져 despawn(못 먹음).</item>
    /// <item><b>반경 내</b> — 플레이어가 <see cref="magnetRadius"/> 이내로 들어오면 <b>캡처</b>되어 플레이어로 <b>가속 끌림</b>(가까울수록 빠름). 한 번 캡처되면 래치(놓치지 않음).</item>
    /// <item><b>습득</b> — 캡처된 오브가 <see cref="pickupRadius"/> 이내로 도달하면 <see cref="GameEvents.PublishOrbCollected"/>로 경험치 발행(M1-6) 후 풀 반납.</item>
    /// </list>
    /// 수치는 Day5 튜닝(→ 자석범위/가치는 M2-2 SO·M4-8).</para>
    /// </summary>
    public sealed class Orb : MonoBehaviour
    {
        [Header("드리프트 (반경 밖: 그냥 지나침)")]
        [Tooltip("반경 밖에서 전방(월드 -Z)으로 흐르는 속도(유닛/초). 수치 Day5")]
        [SerializeField] float driftSpeed = 6f;
        [Tooltip("이 Z 이하로 지나가면(플레이어를 못 잡고 통과) 풀 반납. 카메라 뒤")]
        [SerializeField] float despawnZ = -50f;

        [Header("자석 (반경 내: 가속 끌림)")]
        [Tooltip("플레이어가 이 거리 이내로 들어오면 캡처되어 끌려옴. 수치 Day5")]
        [SerializeField] float magnetRadius = 8f;
        [Tooltip("캡처 후 플레이어에 붙을수록 가속하는 최대 속도(dist→0). 경계(dist=반경)에선 driftSpeed. 수치 Day5")]
        [SerializeField] float magnetMaxSpeed = 40f;
        [Tooltip("플레이어에 이 거리 이내로 도달하면 습득(거리 기반). magnetRadius보다 작게. 수치 Day5")]
        [SerializeField] float pickupRadius = 0.6f;
        [Tooltip("습득 시 주는 경험치. 지금은 고정 1(→ M2-2 SO로 데이터화). 수치 Day5")]
        [SerializeField] int xpValue = 1;

        Transform _target;
        Action<Orb> _return;
        float _magnetBonus;   // 자석범위 강화 보너스(OrbPool 주입, M1-8)
        bool _captured;

        /// <summary>풀 Get 시 호출 — 타깃(플레이어)·반납 콜백·자석 보너스 배선 + 캡처 상태 리셋.</summary>
        public void OnSpawned(Transform target, Action<Orb> returnToPool, float magnetBonus)
        {
            _target = target;
            _return = returnToPool;
            _magnetBonus = magnetBonus;
            _captured = false;
        }

        void Update()
        {
            if (_target != null)
            {
                float effRadius = magnetRadius + _magnetBonus;   // 자석범위 강화 반영(M1-8)
                Vector3 toPlayer = _target.position - transform.position;
                float dist = toPlayer.magnitude;

                if (!_captured && dist <= effRadius) _captured = true;   // 한 번 잡히면 래치

                if (_captured)
                {
                    if (dist <= pickupRadius)   // 습득(거리 기반) → 경험치 이벤트 발행 + 풀 반납 (M1-6)
                    {
                        GameManager.Instance?.Events?.PublishOrbCollected(xpValue);
                        _return?.Invoke(this);
                        return;
                    }

                    // 가속 끌림: 경계(dist=반경)=driftSpeed → 접촉(dist→0)=magnetMaxSpeed. 오버슛 방지로 dist 클램프.
                    float t = Mathf.Clamp01(dist / effRadius);
                    float speed = Mathf.Lerp(magnetMaxSpeed, driftSpeed, t);
                    Vector3 dir = dist > 0.0001f ? toPlayer / dist : Vector3.zero;
                    transform.position += dir * Mathf.Min(speed * Time.deltaTime, dist);
                    return;   // 캡처된 오브는 despawn 안 함(습득=3단계에서 풀 반납)
                }
            }

            // 반경 밖(또는 타깃 없음): 전방(-Z)으로 그냥 흘러 지나침 → 뒤로 빠지면 반납
            transform.position += Vector3.back * (driftSpeed * Time.deltaTime);
            if (transform.position.z <= despawnZ) _return?.Invoke(this);
        }
    }
}
