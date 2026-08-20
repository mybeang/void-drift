# 진행·종료 & GameEvents 채널 확장 (M1-6 · M1-9)

> 대상: **M1-6 (경험치/레벨업)** + **M1-9 (HP/게임오버/점수)**.
> [02_GameStateArchitecture.md](02_GameStateArchitecture.md)에서 예고한 **GameEvents 확장 지점**이 실제로
> 진행(progression)·종료(점수) 이벤트를 갖게 된 지점을 정리한다. 상태머신 자체는 02, 여기는 **그 위에 얹힌 게임플레이 채널**.
> 작업 전 [context.md](../../context.md) → [backlog.md](backlog.md) 확인.

관련 파일 (`VD.Runtime`, ns `VD.Core` 중심)
- `Assets/Scripts/Core/GameEvents.cs` — 중앙 pub/sub 채널(상태 + **진행 + 종료/점수**)
- `Assets/Scripts/Core/ExperienceSystem.cs` — 경험치 누적·임계값·레벨업 (M1-6, GameScene 1개)
- `Assets/Scripts/Core/ScoreSystem.cs` — 생존시간·처치점수 집계 (M1-9, GameScene 1개)
- `Assets/Scripts/Core/Orb.cs` — 습득 시 경험치 발행 (M1-5) / `Enemy.cs` — 실사망 시 처치점수 발행 (M1-9)
- `Assets/Scripts/Player/PlayerHealth.cs` — HP% 게시 + HP0→게임오버 (M1-9)

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

## 종료/점수 (M1-9)

**게임오버 = GameScene 정지형**(사용자 결정): `PlayerHealth`가 HP 0 도달 시 `GameManager.GameOver()` 호출 →
`GameState.GameOver` 전이 + **`Time.timeScale = 0`**(스폰·사격·이동 프리즈). 결과값은 `GameEvents`에 보관
(ResultScene 전환·결과 UI는 이후 M1-10/M2).

**점수 = 생존시간 + 적 처치별 점수**(progression §3). `ScoreSystem`이 집계.

### GameEvents 종료/점수 멤버 (M1-9 추가)

| 방향 | 멤버 | 타입 | 갱신/발행자 | 구독자 |
|---|---|---|---|---|
| 입력 | `EnemyKilled` | `Observable<int>` (처치점수) | `Enemy`(`PublishEnemyKilled`) | `ScoreSystem` |
| 출력(상태) | `HpNormalized` | `ReadOnlyReactiveProperty<float>` (0~1) | `PlayerHealth`(`SetHpNormalized`) | HUD(M1-10) |
| 출력(상태) | `HpValues` | `ReadOnlyReactiveProperty<HpAmount>` (현재/최대, M3-4) | `PlayerHealth`(`SetHpValues`) | HUD(HP 숫자 표기) |
| 출력(상태) | `Score` | `ReadOnlyReactiveProperty<int>` | `ScoreSystem`(`SetScore`) | HUD/결과 |
| 출력(상태) | `SurvivalTime` | `ReadOnlyReactiveProperty<float>` (초) | `ScoreSystem`(`SetSurvivalTime`) | HUD/결과 |

- 발행/갱신 메서드는 M1-6과 동일하게 **`internal`**(정당한 소유자만).
- `HpNormalized`/`HpValues`는 `PlayerHealth`(VD.Player)가 갱신(`PublishHp`로 일원화, M3-4) — HP 로직 자체는 플레이어에, **채널 노출만** GameEvents.

### ScoreSystem 로직

```
Update(Playing 동안만): _survivalTime += Time.deltaTime;  push
EnemyKilled(s): _killScore += s;  push
push: SetSurvivalTime(_survivalTime); SetScore(round(_survivalTime×timeScoreRate) + _killScore)
State→GameOver: 최종값 임시 로그   // "[TEMP] 게임오버 — 생존 …s / 점수 … (처치점수 …)"
```
- 생존시간은 `Playing` 상태에서만 누적(일시정지=3choice·게임오버 시 정지 — `timeScale 0`이라 이중 안전).
- 파라미터: `timeScoreRate`(기본 1, 초당 점수) · `Enemy.killScore`(기본 10). **수치 Day5**(→ 처치점수는 M2-2 적 SO).

### 종료 흐름

```
Enemy 실사망 → PublishEnemyKilled → ScoreSystem 합산
플레이어 피격 누적 → HP 0 → GameManager.GameOver() → timeScale 0 (프리즈)
    └ ScoreSystem: State→GameOver 최종 점수 로그
    └ 결과값(Score/SurvivalTime) GameEvents에 보관 → (M1-10 HUD / 이후 ResultScene 표시)
```

---

## 임시 요소 / 후속 정리

- **`[TEMP] 레벨업` 로그**(`ExperienceSystem`): 3choice(M1-7)가 `LevelUp`을 구독해 팝업을 띄우면 대체·제거.
- **`[TEMP] 피격/게임오버` 로그**(`PlayerHealth`·`ScoreSystem`): 결과 화면(ResultScene/오버레이)·HUD 붙을 때 정리.
- **경험치량/처치점수 하드코딩**(`Orb.xpValue`=1, `Enemy.killScore`=10): M2-2 적 SO 스탯(드랍 오브량·처치점수)으로 데이터화.

## 검증 (DoD)

| 항목 | 결과 |
|---|---|
| 오브 습득이 게이지(경험치)를 채움 | ✅ `XpNormalized` 갱신 |
| 임계값마다 레벨업 이벤트 | ✅ `[TEMP] 레벨업 → Lv 2 (다음 임계 6.5)` — 5개→Lv2, 다음 5×1.3 |
| 레벨 오를수록 더 많이 필요(점증) | ✅ 지수형 `base×growth^(n-1)` |
| 컴파일/런타임 에러 | ✅ 0 |

→ M1-6 DoD **충족**. (`LevelUp` 소비 = M1-7, `Level`/`XpNormalized` 표기 = M1-10)

**M1-9** (execute_code 검증): 생존 53.1s + 처치 180(18×10) = **점수 233** / `GameOver()` → `state=GameOver`·`timeScale=0` 프리즈 / 게임오버 최종 로그 / 에러 0. → M1-9 DoD **충족**(결과 "화면"은 M1-10/M2). 실피격 HP0은 사용자 육안(HP감소는 M1-4 검증, GameOver 분기는 강제 호출 검증).
