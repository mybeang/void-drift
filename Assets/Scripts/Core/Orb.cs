using System;
using UnityEngine;

namespace VD.Core
{
    /// <summary>
    /// 경험치 오브 (M1-5). 적 <b>실사망</b> 위치에 드랍된다. (화면 밖 despawn은 드랍 안 함)
    /// 비주얼은 자식 <c>Model</c>(Crystal effect VFX). 풀은 <see cref="OrbPool"/>(<see cref="PooledObjectPool{T}"/> 상속).
    /// <para>거동(사용자 결정 2026-08-18):
    /// <list type="bullet">
    /// <item><b>반경 밖</b> — 전방(월드 -Z, 플레이어 쪽)으로 <b>일정 속도로 흘러 지나침</b>. 플레이어가 근처(반경 내)에 없으면 뒤로 빠져 despawn(못 먹음). 플레이어 평면(z)을 지난 뒤부터는 <b>페이드아웃</b>(통과 거리 비례 알파↓, I-5) — 캡처되면 복원.</item>
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
        [Tooltip("습득 시 주는 경험치. 드랍 시 색 롤로 주입(3-5, Configure). 인스펙터값은 폴백")]
        [SerializeField] int xpValue = 2;

        [Header("색 비주얼 (3-5, 인덱스=색: 0=녹 1=빨 2=파)")]
        [Tooltip("색별 크리스탈 자식. 드랍 색 롤에 따라 하나만 활성. 페이드는 전 자식 캐시라 무관")]
        [SerializeField] GameObject[] colorVisuals;

        [Header("페이드아웃 (플레이어 평면 통과 후 흐려짐, I-5)")]
        [Tooltip("페이드 시작선을 플레이어 z보다 앞으로(+Z, 오브가 도달하기 전) 당기는 거리. 0=플레이어 평면부터, 양수면 그만큼 앞에서 흐려지기 시작. 수치 Day5")]
        [SerializeField] float fadeStartOffset = 5f;
        [Tooltip("페이드 시작선을 지난 뒤 이 거리만큼 흐르는 동안 알파를 1→0으로 선형 감소. despawn(z<=despawnZ) 전에 완전투명이 될 필요는 없음 — '흐려짐이 인지될' 수준으로만 튜닝하면 됨(반납은 despawnZ 기준 유지). 0 이하면 페이드 비활성. 수치 Day5")]
        [SerializeField] float fadeDistance = 20f;

        Transform _target;
        Action<Orb> _return;
        float _magnetBonus;   // 자석범위 강화 보너스(OrbPool 주입, M1-8)
        bool _captured;

        // 페이드아웃(I-5): 비주얼=파티클 4종(이미 Transparent) → MPB로 _BaseColor 알파만 낮춘다(공유 재질 안전, 복제 없음).
        static readonly int BaseColorID = Shader.PropertyToID("_BaseColor");
        ParticleSystemRenderer[] _fadeRenderers;   // 자식 파티클 렌더러(Crystal/Glow/Sparks/Flares). 프리팹 고정이라 Awake 1회 수집
        Color[] _baseColors;                       // 렌더러별 원본 _BaseColor(알파 포함) — 페이드는 이 알파에 배수
        MaterialPropertyBlock _mpb;
        float _lastAlpha = -1f;                    // 마지막 적용 알파(중복 세팅 회피)

        void Awake()
        {
            // 페이드용 파티클 렌더러 + 원본 _BaseColor 캐시(I-5). 오브 비주얼은 프리팹 고정(적처럼 주입/교체 없음)이라 1회면 충분.
            _fadeRenderers = GetComponentsInChildren<ParticleSystemRenderer>(true);
            _baseColors = new Color[_fadeRenderers.Length];
            for (int i = 0; i < _fadeRenderers.Length; i++)
            {
                var m = _fadeRenderers[i] != null ? _fadeRenderers[i].sharedMaterial : null;
                _baseColors[i] = (m != null && m.HasProperty(BaseColorID)) ? m.GetColor(BaseColorID) : Color.white;
            }
            _mpb = new MaterialPropertyBlock();
        }

        /// <summary>드랍 색 적용(3-5) — 해당 색 비주얼만 활성 + 습득 경험치 설정. OnSpawned 후 드랍 측이 호출.</summary>
        public void Configure(int colorIndex, int xp)
        {
            xpValue = xp;
            if (colorVisuals == null) return;
            for (int i = 0; i < colorVisuals.Length; i++)
                if (colorVisuals[i] != null) colorVisuals[i].SetActive(i == colorIndex);
        }

        /// <summary>풀 Get 시 호출 — 타깃(플레이어)·반납 콜백·자석 보너스 배선 + 캡처 상태 리셋.</summary>
        public void OnSpawned(Transform target, Action<Orb> returnToPool, float magnetBonus)
        {
            _target = target;
            _return = returnToPool;
            _magnetBonus = magnetBonus;
            _captured = false;
            _lastAlpha = -1f;   // 재사용 풀: 이전 사이클의 페이드 잔상 제거
            SetAlpha(1f);       // 또렷한 상태로 시작
        }

        /// <summary>파티클 4종의 _BaseColor 알파를 원본×<paramref name="a"/>로 세팅(MPB, 공유 재질 안전). I-5 페이드.</summary>
        void SetAlpha(float a)
        {
            if (_fadeRenderers == null) return;
            a = Mathf.Clamp01(a);
            if (Mathf.Approximately(a, _lastAlpha)) return;   // 중복 세팅 회피
            _lastAlpha = a;
            for (int i = 0; i < _fadeRenderers.Length; i++)
            {
                var r = _fadeRenderers[i];
                if (r == null) continue;
                r.GetPropertyBlock(_mpb);
                Color c = _baseColors[i];
                c.a *= a;
                _mpb.SetColor(BaseColorID, c);
                r.SetPropertyBlock(_mpb);
            }
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
                    SetAlpha(1f);   // 캡처=플레이어로 끌려옴 → 뒤로 지나며 흐려졌더라도 다시 또렷하게(I-5)

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

            // 페이드아웃(I-5): 미캡처 오브가 페이드 시작선(플레이어 z + 앞당김 오프셋)을 지나면 거리에 비례해 흐려짐.
            // fadeDistance는 완만함만 조절 — despawnZ 도달 시 알파가 0이 아니어도 아래에서 반납(완전투명 불요).
            if (_target != null && fadeDistance > 0f)
            {
                float past = (_target.position.z + fadeStartOffset) - transform.position.z;   // 양수 = 시작선 통과
                SetAlpha(past <= 0f ? 1f : 1f - past / fadeDistance);
            }

            if (transform.position.z <= despawnZ) _return?.Invoke(this);
        }
    }
}
