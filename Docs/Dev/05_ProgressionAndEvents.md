# 진행(경험치/레벨업) & GameEvents 채널 확장 (M1-6)

> 대상: **M1-6 (경험치/레벨업, 점증형 임계값)**. 오브 습득 → 경험치 누적 → 임계값 도달 → 레벨업 이벤트.
> [02_GameStateArchitecture.md](02_GameStateArchitecture.md)에서 예고한 **GameEvents 확장 지점**이 실제로
> 진행(progression) 이벤트를 갖게 된 지점을 정리한다. 작업 전 [context.md](../../context.md) → [backlog.md](backlog.md) 확인.

관련 파일 (`VD.Runtime`, ns `VD.Core`)
- `Assets/Scripts/Core/GameEvents.cs` — 중앙 pub/sub 채널(상태 + **진행**)
- `Assets/Scripts/Core/ExperienceSystem.cs` — 누적·임계값·레벨업 로직 (GameScene 1개)
- `Assets/Scripts/Core/Orb.cs` — 습득 시 경험치 발행 (오브 상세는 backlog M1-5)

---

## 개요

M1-6은 **상태(로직)와 상태(데이터)를 분리**한 기존 패턴을 진행 시스템에도 적용한다.

- **데이터/이벤트는 `GameEvents`** 에 얹는다 — 상태머신 전용이던 채널을 **중앙 진행 허브**로 확장.
  HUD(M1-10)·3choice(M1-7)가 여기에 바인딩·구독한다.
- **누적·곡선 로직은 `ExperienceSystem`** 이 담당 — `GameEvents`를 갱신만 한다(갱신 권한 `internal`).

## 설계 결정 (사용자 확정 2026-08-18)

| 항목 | 결정 | 이유 |
|---|---|---|
| 상태 배치 | **GameEvents 확장** (별도 시스템 아님) | 02에서 예고한 확장 지점. HUD/3choice가 중앙 채널 하나만 구독 |
| 오브→경험치 연결 | **GameEvents 이벤트 발행 → 시스템 구독** | pub/sub로 오브와 진행 시스템 디커플 |
| 임계값 곡선 | **지수형** `base×growth^(n-1)` | 레벨마다 배수 증가 → 매끄러운 가속(밸서라이크 관용) |
| 오브 1개 경험치 | 지금 **고정 1**(`Orb.xpValue`) | 데이터화는 M2-2 SO. 수치 Day5 |

---

## GameEvents 진행 멤버 (M1-6 추가)

| 방향 | 멤버 | 타입 | 갱신/발행자 | 구독자 |
|---|---|---|---|---|
| 입력 | `OrbCollected` | `Observable<int>` | `Orb`(`PublishOrbCollected`) | `ExperienceSystem` |
| 출력(상태) | `Level` | `ReadOnlyReactiveProperty<int>` (1부터) | `ExperienceSystem`(`SetLevel`) | HUD(M1-10) |
| 출력(상태) | `XpNormalized` | `ReadOnlyReactiveProperty<float>` (0~1) | `ExperienceSystem`(`SetXpNormalized`) | HUD(M1-10) |
| 출력(이벤트) | `LevelUp` | `Observable<int>` (새 레벨) | `ExperienceSystem`(`RaiseLevelUp`) | 3choice(M1-7) |

- 발행/갱신 메서드(`PublishOrbCollected`/`SetLevel`/`SetXpNormalized`/`RaiseLevelUp`)는 **`internal`** —
  어셈블리 내 정당한 소유자만 쓴다(상태 채널 오염 방지). 외부는 **읽기·구독만**.
- `Dispose`에서 추가된 `Subject`/`ReactiveProperty` 전부 정리(GameManager 파기 시).

## ExperienceSystem 로직

```
OrbCollected(xp) 수신
 └ _xpIntoLevel += xp
 └ while(_xpIntoLevel >= threshold):        // 초과분 이월 → 한 번에 여러 레벨 가능
     _xpIntoLevel -= threshold
     level++;  threshold = base×growth^(level-1)
     SetLevel(level);  RaiseLevelUp(level)
 └ SetXpNormalized(_xpIntoLevel / threshold)  // 0~1, HUD 게이지
```
- `Start`에서 `GameManager.Instance.Events` 캐시 + `OrbCollected.Subscribe(...).AddTo(this)`(R3.Unity 수명 연동).
- 임계값 파라미터: `baseThreshold`(기본 5) · `growth`(기본 1.3). **수치 Day5**(→ 곡선 데이터/SO는 이후).

## 데이터 흐름 (전체)

```
Enemy 사망 → Orb 드랍 → (자석) 플레이어 근접 습득
          → GameEvents.PublishOrbCollected(xpValue)
          → OrbCollected 스트림 → ExperienceSystem 누적
          → (임계값 도달) SetLevel / RaiseLevelUp / SetXpNormalized
              ├▶ GameEvents.LevelUp   → 3choice 팝업 (M1-7)
              └▶ GameEvents.Level / XpNormalized → HUD 게이지 (M1-10)
```

---

## 임시 요소 / 후속 정리

- **`[TEMP] 레벨업` 로그**(`ExperienceSystem`): 3choice(M1-7)가 `LevelUp`을 구독해 팝업을 띄우면 대체·제거.
- **경험치량 하드코딩**(`Orb.xpValue`=1): M2-2 적 SO 스탯(드랍 오브량/가치)으로 데이터화.

## 검증 (DoD)

| 항목 | 결과 |
|---|---|
| 오브 습득이 게이지(경험치)를 채움 | ✅ `XpNormalized` 갱신 |
| 임계값마다 레벨업 이벤트 | ✅ `[TEMP] 레벨업 → Lv 2 (다음 임계 6.5)` — 5개→Lv2, 다음 5×1.3 |
| 레벨 오를수록 더 많이 필요(점증) | ✅ 지수형 `base×growth^(n-1)` |
| 컴파일/런타임 에러 | ✅ 0 |

→ M1-6 DoD **충족**. (`LevelUp` 소비 = M1-7, `Level`/`XpNormalized` 표기 = M1-10)
