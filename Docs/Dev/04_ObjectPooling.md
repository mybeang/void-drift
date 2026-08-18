# 오브젝트 풀링 — PooledObjectPool&lt;T&gt; 베이스 (M1-3~M1-5)

> 대상: 투사체/적/오브가 공유하는 **재사용 풀 인프라**. 런타임 `Instantiate`/`Destroy` GC 스파이크를
> 없애기 위한 공통 베이스와, 도메인별 풀이 그 위에 얹히는 방식을 정리한다.
> 작업 전 [context.md](../../context.md) → [backlog.md](backlog.md) 확인.

관련 파일 (`VD.Runtime`)
- `Assets/Scripts/Core/PooledObjectPool.cs` — 추상 베이스 (`VD.Core`)
- `Assets/Scripts/Player/ProjectilePool.cs` — 투사체 풀 (`VD.Player`, M1-3)
- `Assets/Scripts/Enemy/EnemyPool.cs` — 적 풀 (`VD.Enemy`, M1-4)
- `Assets/Scripts/Core/OrbPool.cs` — 오브 풀 (`VD.Core`, M1-5)

---

## 개요

`PooledObjectPool<T>` 는 **추상 MonoBehaviour 베이스**다. 도메인별 풀이 `T`(풀링 대상 컴포넌트)를
지정해 상속하고, 씬/프리팹에 컴포넌트로 부착해 인스펙터에서 prefab·prewarm을 설정한다.

- **공통 로직만** 베이스에: prewarm, `Get`/`Return`, 부모 정리(자기 자식으로 Instantiate).
- **항목별 초기화**는 하위(`OnGet`/`OnReturn`) 또는 항목 자신이 담당 → 베이스는 **얇게** 유지.

## 설계 결정 (사용자 확정 2026-08-18)

| 항목 | 결정 | 이유 |
|---|---|---|
| 풀 구성 | **상속형** (도메인별 서브클래스) | 한 곳에 몰지 않고 관심사 분리. 인스펙터 prewarm·타입 안전 |
| 반납 방식 | **항목이 반납 콜백을 보유** | 항목이 스스로 수명 판단(수명만료·despawn·습득) 시 콜백 호출 |
| 고갈 시 | **즉시 새로 생성** (`Create`) | 상한 없음 — prewarm은 초기 스파이크 회피용. 필요 시 하위가 정책 추가 |

---

## 베이스 API (`PooledObjectPool<T> where T : Component`)

| 멤버 | 역할 |
|---|---|
| `[SerializeField] T prefab` | 풀링할 프리팹(컴포넌트 `T`를 가진 루트) |
| `[SerializeField] int prewarmCount` | 시작 시 미리 생성해둘 개수 (기본 16) |
| `Awake()` | `prewarmCount`만큼 `Create()` → 비활성화해 대기 큐에 적재 |
| `T Get()` | 큐에서 꺼내 활성화 후 반환(비었으면 `Create()`). 직후 `OnGet(item)` 훅 |
| `void Return(T item)` | `OnReturn(item)` 훅 → 비활성화 → 큐로 반납 |
| `virtual T Create()` | 기본 `Instantiate(prefab, transform)` (풀의 자식). 필요 시 override |
| `virtual void OnGet(T)` / `OnReturn(T)` | 항목별 초기화·정리 훅. 기본 no-op |

> `Awake`는 `protected virtual` — 하위가 override 시 반드시 `base.Awake()` 먼저(prewarm 보장).

## 상속 패턴 — 반납 콜백 배선

각 서브클래스는 `OnGet`에서 항목에 **반납 콜백**을 넘겨, 항목이 스스로 수명이 끝났을 때 풀로 돌아오게 한다.

```csharp
// ProjectilePool / EnemyPool 공통 형태
protected override void OnGet(T item) => item.OnSpawned(Return);
```
- `Projectile`: 수명 만료 시 `Return` 호출(self-return). 위치·회전은 `PlayerShooter`가 Get 후 세팅.
- `Enemy`: 사망/despawn 시 `Return`. 위치·이동은 `EnemySpawner`가 Get 후 세팅.
- `Orb`(M1-5): `OrbPool`이 **타깃(플레이어)까지 함께 주입** → `item.OnSpawned(target, Return)`.
  `OrbPool`은 `Awake`를 override해 `base.Awake()` 후 태그 `"Player"`로 타깃을 1회 캐시(자석용).

> **위치·상태 세팅은 Get 이후 호출자(스포너/발사기/드랍)의 책임.** 풀은 인스턴스 수명만 관리한다.

---

## 확장 지점

- **정책이 필요해지면** 하위에서 `Create`/`OnGet`/`OnReturn` override로 국소 처리(상한·초기화·통계 등).
- **데이터 주도 스폰(M2-5)**: 스포너가 SO/Addressables에서 프리팹을 읽어도, 풀 베이스는 그대로 재사용 가능
  (프리팹 참조만 교체·다형화). 베이스는 항목 종류에 무지.
