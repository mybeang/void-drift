# Backlog — M3 · 적 다양성 & 3choice 풀 (Must 완성)
> 상위 허브: [backlog.md](backlog.md) | 인접: [backlog-M2.md](backlog-M2.md) ← **M3** → [backlog-M4.md](backlog-M4.md)

## ⚡ 특이사항 (이 헤더만 읽어도 크로스 마일스톤 파악)
- **상태**: 🟡 진행중 — **M3-1(이동AI)·M3-2(공격AI+적탄) 완료**, **M3-3·M3-4 남음**. **M3 완료 = Must 코어 전부 충족** → 직후 **M5-1 모바일 빌드 1차** 권장(빌드 리스크 조기 확인).
- **AI 아키텍처(M3-1/M3-2 확정)**: 이동·공격 모두 **순수 C# 전략 모듈**(MonoBehaviour 아님, 사용자 결정) — `IMoveBehaviour`/`IAttackBehaviour`. `Enemy`가 메시지 창구(Update)로 `Tick` 위임, `EnemyBuilder`가 `def.moveAI`/`def.attackAI`로 주입. 무상태 모듈은 빌더가 싱글톤 공유, 상태 있는 모듈(탄막 쿨다운·사행 위상)은 인스턴스별 생성. 플레이어 조회 = `PlayerLocator`(Player 태그) 공용. **적탄 = `EnemyBullet`+`EnemyBulletPool`(`PlayerHealth.ApplyDamage` 타격), `EnemyBullet` 레이어(11)+매트릭스(×Player만).**
- **전제(이전 M에서 옴)**:
  - **M2-2 SO 스키마·enum(이동AI 4·공격AI 4·아키타입 3)** 이 여기 실구현의 계약. enum 값과 정합 유지.
  - M1-4 적 기반(`Enemy`/`EnemyPool`/`EnemySpawner`), M1-9 데미지/게임오버. `spaceship_6`은 M1-4서 선행 사용(이동만).
  - M1-8 3choice 최소 풀(`UpgradeType`/`UpgradeSystem`)을 M3-4가 데이터화·정리.
- **이후로 이관**:
  - 이동AI **사행/견제** + 공격AI **조준단발**(§3 4×4 완성) → **M4-7**. M3은 최소 2종(이동)·2~3종(공격)만.
  - 업그레이드 **풀세트**(탄속/관통/재생/오브범위 등) → **M4-8**. M3-4는 공용 스탯 확정 세트까지.
- **이후 M이 여기서 확인할 것**:
  - **M2-5 스폰 연결**이 실제 동작하려면 M3-1·M3-2 AI 모듈 필요(SO가 지정한 AI를 여기서 구현).
  - M4-5 난이도 페이즈·M4-6 스폰 타임라인이 M3 아키타입/AI를 데이터 원천으로 씀.
  - M3-2 탄막/자폭용 **`EnemyBullet` 레이어**는 여기서 신설(M1-4 레이어 매트릭스에 추가). 재부팅 시 매트릭스 재적용 주의.
- **핵심 방침/주의**:
  - AI는 **재사용 모듈**(SO enum으로 선택) — 같은 프리팹이 설정만으로 다르게 거동.
  - 돌진형 vs 자폭형 차별화 = 단발 충돌 vs 범위 폭발.
  - 수치(발사 간격/탄속/탄막수/자폭반경/속도) = **Day5, SO 데이터로 관리**(하드코딩 지양).
- **문서**: [enemy-design.md](../Designs/enemy-design.md) §2·§3, [upgrade-pool.md](../Designs/upgrade-pool.md).

---

### M3-1 · 이동 AI 모듈 (직진/추적) ✅
- **목적**: enemy-design §3. 재사용 이동 모듈 최소 2종.
- **작업**: 이동AI 인터페이스/전략, 직진(코스 따라 접근)·추적(플레이어 XY 보정 접근) 구현. SO의 이동AI enum으로 선택.
- **DoD**: 같은 적 프리팹이 SO 설정만으로 직진/추적 다르게 움직임.
- **의존**: M2-2
- **문서**: enemy-design.md §3
- **완료(2026-08-20)**: `IMoveBehaviour`(`OnSpawned`/`Tick`) + `StraightMove`(직진 하드코딩 이관)/`ChaseMove`. `Enemy.Update`가 `_move.Tick` 위임(직진 하드코딩 제거), `MoveSpeed` 프로퍼티·`SetMoveBehaviour` 추가. `EnemyBuilder.ResolveMove`가 `def.moveAI`로 주입(Weave/Hover는 직진 폴백, M4-7). **비물리 transform 이동**(적끼리 밀림 없이 통과, 사용자 결정). **추적 = 임계거리(`_homingRange` 기본 30) 이내로 들어와야 XY 추적 개시**(먼 거리엔 직진, -Z는 항상 진행해 despawn 보장). 임계·조향 수치는 Day5/SO화 후보. 사용자 Play 검증 완료.

### M3-2 · 공격 AI 모듈 (충돌/탄막/자폭) ✅
- **목적**: enemy-design §3. 재사용 공격 모듈 2~3종.
- **작업**: 충돌(몸통 접촉 데미지)·탄막(방사/부채꼴 다발)·자폭(근접 시 범위 폭발) 구현. 발사 간격/탄속/탄막수/자폭반경 파라미터(수치 Day5).
- **DoD**: SO 설정으로 세 공격 방식이 구분 동작, 플레이어에게 데미지.
- **의존**: M2-2, M1-9
- **문서**: enemy-design.md §3
- **완료(2026-08-20)**: `IAttackBehaviour` + `ContactAttack`(발사 없음 no-op — 접촉 데미지는 `PlayerHealth` 트리거가 `ContactDamage`로 처리, 조준단발 M4-7도 여기 폴백)/`BarrageAttack`(**부채꼴, 플레이어 조준** — 사용자 결정, 월드 Y축 기준 스프레드, 쿨다운 상태라 인스턴스별 생성)/`SuicideAttack`(플레이어와 거리 ≤ `suicideRadius` 시 `ApplyDamage` 후 `Despawn`, 드랍/점수 없음 — 돌진형 단발접촉과 차별=범위 트리거). `EnemyBuilder.ResolveAttack`가 `def.attackAI`로 주입. **적탄** `EnemyBullet`+`EnemyBulletPool`(Projectile/풀 미러, prewarm 64) — `PlayerHealth.ApplyDamage`(접촉/적탄/자폭 공용으로 추출) 타격. **`EnemyBullet` 레이어(11) 신설 + 물리 매트릭스 EnemyBullet×Player만 ON**. 프리팹 `Assets/Prefabs/EnemyBullet.prefab`(**임시 붉은 큐브** 0.35, kinematic RB+트리거, URP/Lit `EnemyBullet_Placeholder.mat` — **비주얼 교체 예정**). 발사간격/탄속/탄수/자폭반경=SO 데이터(부채꼴 각 `_spreadAngle` 50°·탄 수명 6초는 코드 기본값). 사용자 Play 검증 완료(탄막 과다=SO 수치 튜닝 대상, Day5).

### M3-3 · 아키타입 2~3 (탄막형/돌진형/자폭형) 🔴
- **목적**: enemy-design §2. 비주얼 아키타입과 성향 결합.
- **작업**: 아키타입별 대표 조합을 SO 데이터로 구성(탄막형=원거리+탄막, 돌진형=근거리 충돌 고체력, 자폭형=고속 저체력 자폭). 돌진형 vs 자폭형 차별화(단발 충돌 vs 범위 폭발) 반영.
- **DoD**: 세 아키타입이 시각·행동으로 구분되어 등장.
- **의존**: M3-1, M3-2
- **문서**: enemy-design.md §2
- **비주얼→아키타입 매핑 (잠정, 2026-08-18 사용자)** — 소스 = `FREE Low Poly Spaceships`:
  | 프리팹 | 아키타입(잠정) | 비고 |
  |---|---|---|
  | `spaceship_1` | 고체력 돌진/탄막 | **대형·모함급**(스케일 큼) |
  | `spaceship_2` | 탄막형 | 링(원형) 실루엣 |
  | `spaceship_3` | 탄막/범용 | |
  | `spaceship_4` | 범용/돌진 | |
  | `spaceship_5` | 자폭형 | 얇은 다트(고속 저체력) |
  | `spaceship_6` | 돌진/범용 | **M1-4 첫 적으로 선행 사용** |
  | `spaceship_7` | 탄막/특수 | 원반형 |
  > 잠정 매핑 — M3-3에서 실제 AI·스탯 붙이며 확정. `spaceship_6`은 M1-4에서 먼저 등장(이동만 → 이후 확장).

### M3-4 · 3choice 풀 정리 (Must 범위 확정) 🔴
- **목적**: M1-8을 데이터화·정리(공용 스탯 강화 확정 세트).
- **작업**: 공용 스탯(공격력/이동속도/최대체력) 강화를 데이터로 정리, 롤 가중치/중복 규칙 확정. (무기·풀세트는 M4)
- **DoD**: 3choice가 안정적으로 의미 있는 선택지를 제공(빌드 분기 성립).
- **의존**: M1-8
- **문서**: upgrade-pool.md

> **M3 완료 = Must 전부 충족.** 여기서 **M5-1 모바일 빌드 1차**를 먼저 돌려 빌드 리스크를 조기 확인 권장.
