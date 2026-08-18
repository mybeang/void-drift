# Backlog — M1 · 코어 루프 (플레이 가능한 최소 게임)
> 상위 허브: [backlog.md](backlog.md) | 인접: [backlog-M0.md](backlog-M0.md) ← **M1** → [backlog-M2.md](backlog-M2.md)

## ⚡ 특이사항 (이 헤더만 읽어도 크로스 마일스톤 파악)
- **상태**: ✅ **M1-1~M1-10 전부 완료**(2026-08-19 마감). 에디터 툴/Addressables 없이 하드코딩 데이터로 **처음~게임오버 한 판**이 돌아감(M1 게이트 충족).
- **전제(이전 M에서 옴)**: M0 골격(asmdef `VD.*`, 씬 3개, New Input System 단독, R3/UniTask). 상세 기술문서 = [02_GameStateArchitecture.md](02_GameStateArchitecture.md)(상태·이벤트) / [03_PlayerMovementAndCamera.md](03_PlayerMovementAndCamera.md)(이동·카메라) / [04_ObjectPooling.md](04_ObjectPooling.md)(재사용 풀 `PooledObjectPool<T>`) / [05_ProgressionAndEvents.md](05_ProgressionAndEvents.md)(경험치/점수/GameEvents).
- **이후로 이관**:
  - **하드코딩 스포너(M1-4) → M2-5**가 SO DB+Addressables 로드 스폰으로 교체.
  - **적 스탯·처치점수·오브 xp 하드코딩 → M2-2 SO**로 데이터화(`Enemy.killScore`/`contactDamage`, `Orb.xpValue`).
  - **적 이동 속도 가변화·아키타입 → M3**(현재 단일 고정). **공격AI(탄막/자폭)·`EnemyBullet` 레이어 → M3-2**.
  - **결과 "화면"(ResultScene 전환·표시) → M2**(M1-9는 정지형 프리즈 `timeScale 0`+결과값 보관까지). 파괴 VFX → M4-9. 무기 카드(5레벨마다) → M4-3(의존 M1-7).
  - 스폰 포메이션(편대/웨이브) → **M5-8**(M1-4는 랜덤 위치만).
- **이후 M이 여기서 확인할 것**:
  - **레이어/물리 매트릭스**: Player(8)/Enemy(9)/PlayerBullet(10), 매트릭스 = Player×Enemy·Enemy×PlayerBullet만 ON. **에디터 재부팅 시 매트릭스 풀리면 재적용.** `EnemyBullet`은 M3-2서 추가.
  - **GameEvents 채널**(Core, R3 pub/sub): 상태 + 진행(`OrbCollected`/`Level`/`XpNormalized`/`LevelUp`) + 결과(`EnemyKilled`/`HpNormalized`/`Score`/`SurvivalTime`). 발행·갱신은 internal. 신규 시스템은 이 채널로 연동.
  - **재사용 풀**: `VD.Core.PooledObjectPool<T>` 상속(ProjectilePool/EnemyPool/OrbPool). 새 풀도 이 베이스.
  - **태그**: Player 프리팹 builtin 태그 `Player`(OrbPool 자석 타깃 탐색, Core→Player 결합 회피).
  - **3choice**: `UpgradeType` enum(MoveSpeed/MaxHp/MagnetRadius) + `UpgradeSystem.Roll/Apply/Describe`(public). 항목 확장은 M3-4/M4-8. 팝업=`VD.UI.LevelUpPopup`, 한글 폰트=**SUIT SDF**.
- **핵심 방침/주의**:
  - **`[TEMP]` 로그는 유지**(육안 확인용, 사용자 방침 — 앞으로도 정리 안 함). Experience/PlayerHealth/Score 등.
  - **`GameDebugDriver`**(Core, 디버그·에디터 전용, `#if UNITY_EDITOR` 가드): P/G/R 키(일시정지·게임오버·재시작). ResultScene(M2) 전까지 유지. **삭제하지 말 것.**
  - 각종 수치(스폰 거리/폭·카메라 z·자석 반경·임계값·데미지 등) = **Day5 튜닝(사용자 주도)**. 문서에 잠정값만.
  - 미해결: 이동 관성감 = [issues.md](issues.md) I-1(보류).

---

### M1-1 · 게임 부트/상태 관리 골격 🔴 `[x]`
- **목적**: 씬 진입~플레이~게임오버 상태 흐름과 전역 서비스 접근점.
- **작업**: 게임 상태(부팅/플레이/일시정지/게임오버) 관리자, 시간 스케일 제어(3choice 일시정지용), 간단한 서비스 접근(이벤트 버스). R3 `Subject`/`ReactiveProperty` 기반 이벤트 채널 제안(`GameEvents`).
- **DoD**: 상태 전환 로그로 확인. `Time.timeScale=0` 일시정지/재개 동작.
- **의존**: M0-4
- **✅ 완료(2026-08-17, 사용자 결정 반영)**: 상세 = [02_GameStateArchitecture.md](02_GameStateArchitecture.md)
  - **산출(`VD.Core`)**: `GameState`(enum Boot/Playing/Paused/GameOver) · `GameEvents`(별도 pub/sub 채널, R3 `ReactiveProperty<GameState>`를 `ReadOnlyReactiveProperty`로 노출, 갱신은 `internal SetState`로 GameManager만) · `GameManager`(MonoBehaviour **싱글톤**, 씬 한정 수명, 상태 전이+`Time.timeScale` 제어, `StartGame/Pause/Resume/GameOver` 가드 포함) · `GameDebugDriver`(디버그·에디터 전용 키보드: P=일시정지/재개·G=게임오버·R=재진입).
  - **사용자 결정**: 전역 접근 = MonoBehaviour 싱글톤 / 이벤트 버스 = 별도 `GameEvents` 채널. 상태 소유는 GameEvents, 전이·timeScale은 GameManager. M1-1은 순수 상태머신이라 비동기 미사용.
  - **씬**: GameScene에 `GameManager`(+DebugDriver)·Main Camera·Directional Light 배치·저장.
  - **검증**: 플레이 진입 `Boot→Playing` 로그, `Pause()`→timeScale 0 / `Resume()`→1, 가드 동작, 컴파일 에러 0. (Time.timeScale은 `OnDestroy`에서 1로 복구.)
  - **정리**: M0-4 마커 중 `VDRuntimeMarker` **삭제**(실코드가 R3/InputSystem 링크 검증), `VDEditorMarker` **유지·재연결**(M2까지 VD.Editor 검증 유일 수단, 참조를 `GameManager`로 교체).

### M1-2 · 플레이어 이동 — 상대 드래그 (XY 자유, Z 고정) 🔴 `[x]`
- **목적**: controls-design §3. 화면 임의 터치 시작점 기준 델타로 기체 XY 이동, Z 고정.
- **작업**: 입력(포인터/터치) 델타 → 기체 XY 이동. 감도/데드존/델타스케일 파라미터화(수치는 Day5). 이동 범위 클램프(화면 밖 이탈 방지).
- **DoD**: 에디터/디바이스에서 드래그로 기체가 자연스럽게 XY 이동, Z 불변. 손가락에 기체 안 가려짐(상대 방식).
- **의존**: M1-1, M0-3
- **문서**: controls-design.md, [03_PlayerMovementAndCamera.md](03_PlayerMovementAndCamera.md)
- **✅ 완료(2026-08-17, 사용자 결정 반영)**: 상세 = [03_PlayerMovementAndCamera.md](03_PlayerMovementAndCamera.md)
  - **산출**: `VD.Player.PlayerMovement`(이동 전담) + `Player` 프리팹(StarSparrow_1_LP_Red 복제, root=Rigidbody+PlayerMovement / 자식 `Model`=메시).
  - **입력**: `Pointer.current` 델타 직접 읽기(상대 드래그). **해상도 무관 `dragGain`**(손가락 화면분율×게인=기체 이동, 현재 5). 액션 에셋 미사용 → 기본 템플릿 `InputSystem_Actions.inputactions` 삭제(전역 참조 해제).
  - **이동**: 물리(Rigidbody) 속도 직접 매핑, XY 자유·Z 고정. 목표를 뷰포트 경계 안으로 **선-클램프**(경계 떨림 방지).
  - **뱅킹**: 화면중심 오프셋 비례 pitch/yaw/roll을 **자식 `Model`(bankTarget) 로컬 회전**에만 적용(물리 루트 회전 완전 동결 → 물리-회전 충돌 없음). 코 안쪽=조준(`invertYaw` OFF, 그림 측면매핑과 반대는 의도).
  - **카메라**: 고정 Perspective (0,0,-26) FOV55 near0.3 far300(사용자 튜닝 확정). 기체 폭 화면 ~40%.
  - **검증**: 사용자 드래그 확인, 뱅킹 상단 pitch+25°(루트 0 유지), 경계 정지·속도0, 컴파일/런타임 에러 0.
  - **미해결**: 이동 관성감 → [issues.md](issues.md) `I-1`(보류). / 조준 forward=Model 연동은 M1-3.

### M1-3 · 오토 사격 (기관총) + 투사체 🔴 `[x]` (완료 — 발사 파이프라인; 데미지/히트는 M1-4 이관)
- **목적**: 입력 없이 자동 발사(뱀서라이크 문법).
- **작업**: 발사기(발사 간격 파라미터), 투사체 이동/수명/충돌, 오브젝트 풀링(투사체·적·오브 공용 풀 유틸 제안 `SimplePool`). 데미지 전달 인터페이스(`IDamageable`).
- **DoD**: 플레이 시 일정 간격 발사, 투사체가 적 히트 시 데미지. 풀 재사용으로 GC 스파이크 없음.
  - **판정(2026-08-18)**: 일정 간격 발사·투사체·풀 재사용 = **충족**(사용자 육안 확인). "**적 히트 시 데미지**"는 적 엔티티가 없어 검증 불가 → **M1-4로 이관**(사용자 결정 (a)). M1-3은 **발사 파이프라인** 기준으로 완료 처리.
- **의존**: M1-1
- **비고**: `SimplePool`→실제는 상속형 `PooledObjectPool<T>`로 확정. `IDamageable`은 M1-4에서 확정.
- **⚙️ 진행 상태 & 인계 (2026-08-18)** — 3단계까지 완료. 데미지/히트만 M1-4 이관.
  - **확정 결정(사용자)**:
    - 구조를 **이동 / 연출 / 발사 분리**. 기존 `Model`(메시)은 **연출** 쪽.
    - **발사 방향 = 뱅킹 조준**(정면 직진 아님). 이유: 적이 멀리 한 점(소실점)에서 옴. **단, 흔들리는 Model 회전이 아니라 오프셋에서 즉시 계산한 "깨끗한 조준 방향"을 `FirePoint`에 적용** → 조준(원뿔)+안정 동시. Model 뱅킹은 그 조준을 부드럽게 따라가는 시각 연출일 뿐.
    - **조준 "원뿔"의 중심축 = 뱅킹 방향**(= `FirePoint.forward`, `PlayerAim`이 냄). 오프셋 0이면 +Z, 드래그하면 그쪽으로 축이 기욺.
    - **원뿔 내 적 타겟 스냅(기관총·레일건)은 M1-4로 이관.** 적이 있어야 실동작·검증 가능하므로 여기선 축 직사만. 무타겟이면 축(`FirePoint.forward`)으로 직사, 원뿔 안에 적 있으면 그 적으로 조준 스냅 — 이 층은 적 엔티티(M1-4) 붙은 뒤 추가. **유도탄은 축·원뿔과 무관한 별도 호밍(M4-1).**
    - 기체 시각 관성/무게감은 **지금 연출 그대로 유지**(폴리싱 때 손봄, [issues.md](issues.md) I-1).
  - **목표 구조**:
    ```
    Player (root)   ← PlayerMovement (이동만)
    ├── FirePoint   ← 깨끗한 조준 방향 정렬. 발사 원점(2·3단계)
    └── Model       ← Mesh + PlayerBanking (연출: 부드러운 뱅킹)
    ```
  - **✅ 1단계 완료**: 뱅킹을 `PlayerMovement`에서 신규 **`PlayerBanking`**(Model에 부착)으로 분리. `PlayerMovement`=이동만. 동작 동일(컴파일 0, 사용자 플레이 확인). 프리팹 반영.
  - **✅ 2단계 완료(2026-08-18)**: `FirePoint`(root 직속 자식, localPos 0·forward +Z) 생성 + 신규 **`PlayerAim`**(FirePoint 부착)이 오프셋→pitch/yaw를 `LateUpdate`에서 **즉시(보간 없음)** `FirePoint.localRotation`에 적용. 공식은 `PlayerBanking`과 동일하되 **roll 생략**(forward 불변)·**독립 필드**(maxPitch/maxYaw 기본 28/28, 조준 원뿔을 뱅킹 연출과 별도 튜닝 가능). 임시 검증 기즈모(`drawAimGizmo`, 조준 축 레이) 포함. 컴파일 0, 사용자 육안 확인. 프리팹 반영.
  - **✅ 3단계 완료(2026-08-18)**: 발사 로직 — 발사기 `PlayerShooter`(root, `Playing`에서 `fireInterval`마다 `FirePoint` 방향 발사) + 투사체 `Projectile`(자기 forward 직진 + 수명 만료 시 self-return, 콜라이더 없음) + 풀. **풀은 상속형**: `VD.Core.PooledObjectPool<T>`(추상 MonoBehaviour 베이스, prewarm/Get/Return + `Create`/`OnGet`/`OnReturn` 훅) ← `VD.Player.ProjectilePool : PooledObjectPool<Projectile>`(Get 시 반납 콜백 배선). 이후 EnemyPool(M1-4)·OrbPool(M1-5)이 같은 베이스 상속. **튜닝 한 곳**(사용자 결정): 탄속·수명·발사속도를 `PlayerShooter` 인스펙터에 몰아 발사 시 투사체에 주입(`Projectile.Launch`). 투사체 비주얼 = 임시 프리미티브(`Projectile.prefab`, Unlit 노랑-주황). GameScene에 `ProjectilePool` 오브젝트(prewarm 32). 검증: 조준 축으로 총알 스트림·일시정지(P) 시 정지·컴파일 0, **사용자 육안 확인**. 데미지/충돌은 M1-4.
  - **부수 정리(2026-08-18)**: Player 프리팹의 32-박스 `Collider` 그룹을 **단일 `BoxCollider`(root)로 단순화**(날개 제외·앞쪽 트림·유저 관대 = 작게). 피격 판정용이라 M1-4/M1-9에서 트리거 여부·수치 확정.
  - **관련 파일**: `Assets/Scripts/Player/{PlayerMovement,PlayerBanking,PlayerAim,PlayerShooter,Projectile,ProjectilePool}.cs`, `Assets/Scripts/Core/PooledObjectPool.cs`. 프리팹 `Assets/Prefabs/Player.prefab`, `Assets/Prefabs/Projectile.prefab`. GameScene에 `ProjectilePool`. 카메라 리그·이동 상세 = [03_PlayerMovementAndCamera.md](03_PlayerMovementAndCamera.md).

### M1-4 · 적 기본 엔티티 & 스폰(하드코딩) 🔴 `[x]` (2026-08-19 마감 — 스폰·이동·사격파괴·충돌데미지·레이어분리 DoD 충족; 원뿔 각도/사거리는 사용자 튜닝, `[TEMP]` 로그·기즈모는 유지 방침)
- **목적**: 툴 이전 단계. 코드로 직접 적 1~2종을 스폰해 루프 성립.
- **작업**: 적 컴포넌트(체력/접근 이동/피격→사망), 코스 안쪽에서 플레이어 쪽으로 접근, 간단 스포너(시간/간격 하드코딩). 충돌 시 플레이어 데미지.
- **⤷ M1-3에서 이관 (처리 현황)**: (1) ✅ 데미지 전달 `IDamageable` + 투사체 충돌 감지 완료(트리거 콜라이더). (2) ✅ 원뿔 내 적 타겟 스냅 완료(매 발 nearest-in-cone, 무타겟이면 `FirePoint.forward` 축 직사). (3) ✅ 스폰 `EnemyPool`(`PooledObjectPool<T>` 상속) 완료. — 모두 2026-08-18 처리.
- **DoD**: 적이 계속 스폰·접근하고, 사격으로 파괴되며, 플레이어와 충돌 시 HP 감소.
- **의존**: M1-3
- **⚙️ 결정 & 진행 (2026-08-18, 사용자)**:
  - **적 프리팹 소스 = `FREE Low Poly Spaceships`**(`Assets/Imports/FREE Low Poly Spaceships/Prefabs/spaceship_1~7`, 단일 메시 프리팹). Player는 StarSparrow, **적은 이 세트**로 분리.
  - **M1-4 첫 적 = `spaceship_6` 1종만.** 나머지 아키타입 볼륨업은 M3-3. (아키타입 매핑 잠정 = M3-3 참조.)
  - **진행 = 단계 분할**(M1-3처럼). **1단계 = 스폰 + 직진 접근 이동**만 먼저. 이후 단계 = HP/피격→사망 + 충돌 데미지 + 투사체 히트(`IDamageable`).
  - **스폰 위치 = 랜덤 위치만.** 편대/웨이브 등 **공간 포메이션 패턴은 M5-8로 이관·등재**(볼륨 큼, Nice 후순위).
  - **✅ 1단계(스폰 + 직진 접근 이동) 완료** — `Enemy`(-Z 직진, despawn self-return) + `EnemyPool : PooledObjectPool<Enemy>` + `EnemySpawner`(랜덤 위치 스폰, 튜닝 한 곳). `Enemy.prefab`(root=Enemy+BoxCollider trigger / Model=spaceship_6, 임시 스케일 6). GameScene에 `EnemySpawner`(+EnemyPool, prewarm 16). 스폰 거리/폭·카메라 거리(−26→−36)는 **사용자 인스펙터 튜닝**.
  - **적 이동 속도 = 현재 단일 고정.** 가변 속도는 **볼륨업(M3)으로 이관**(M2-2 SO 스탯·M3-3 아키타입에서 데이터화).
  - **✅ 2단계(적 피격 → HP 감소 → 사망 + 타겟 스냅) 완료**:
    - 신규 **`VD.Core.IDamageable`**(최소 `TakeDamage(float)`, `Core/Interface/`). `Enemy`가 구현 — `maxHp`(30, 스폰 시 리셋), 피격 HP 감소, HP≤0 사망→풀 반납(오브 드랍 M1-5·파괴 VFX M4-9는 이후).
    - **투사체 히트 = 트리거 콜라이더**(사용자 결정): `Projectile.prefab`에 kinematic Rigidbody + isTrigger 콜라이더, `Projectile.OnTriggerEnter`→부모의 `IDamageable`만 데미지·즉시 풀 반납(`_spent` 중복가드). 데미지 튜닝 = **`PlayerShooter.projectileDamage`**(10).
    - **원뿔 타겟 스냅**: `PlayerShooter`가 **매 발** `Physics.OverlapSphereNonAlloc`로 조준 축(`FirePoint.forward`) 원뿔(반각 `aimConeHalfAngle` 25°·사거리 `aimRange` 90) 내 **가장 가까운** 대상 발사(락/캐싱 없음). 원뿔 밖·무타겟이면 축 직사.
    - **⚠️ 임시요소(유지)**: `[TEMP]` 히트/피격/사망 로그(`Projectile`·`Enemy`), 조준 원뿔 기즈모(`PlayerShooter.drawAimGizmo`). 원뿔 각도/사거리 튜닝은 사용자 주도.
  - **✅ 3단계(플레이어 충돌 데미지 + 레이어 분리) 완료**:
    - 신규 **`PlayerHealth`**(Player root, `maxHp` 100). **`IDamageable` 미구현**(아군 오사 방지). 스스로 `OnTriggerEnter`로 적(`Enemy`) 접촉 감지 → HP 감소. 접촉 데미지 = `Enemy.contactDamage`(10, `ContactDamage` getter). HP 0 게임오버·HP UI·결과화면은 **M1-9/M1-10**.
    - **레이어 분리(물리 매트릭스)**: `Player`(8)·`Enemy`(9)·`PlayerBullet`(10). 매트릭스 = Player×Enemy ON·Enemy×PlayerBullet ON·**Player×PlayerBullet OFF**·PlayerBullet self OFF·Enemy self OFF. 프리팹 3개+씬 Player에 레이어 할당, `PlayerShooter.targetMask`=Enemy.
  - **✅ DoD 충족**: 스폰·접근 ✓ / 사격 파괴 ✓ / 충돌 시 HP 감소 ✓.
  - **관련 파일**: `Assets/Scripts/Enemy/{Enemy,EnemyPool,EnemySpawner}.cs`, `Assets/Scripts/Core/Interface/IDamageable.cs`, `Assets/Scripts/Player/{PlayerShooter,Projectile,PlayerHealth}.cs`. 프리팹 `Assets/Prefabs/{Enemy,Projectile}.prefab` + `Player.prefab`. 레이어 Player/Enemy/PlayerBullet. GameScene에 `EnemySpawner`(+EnemyPool).

### M1-5 · 오브 드랍 & 자석 습득 🔴 `[x]` (드랍·자석·습득 완료 — 경험치 이벤트 발행만 M1-6 이관)
- **목적**: 적 파괴 → 자원(오브=경험치) 드랍 → 습득.
- **작업**: 적 사망 시 오브 스폰, 일정 반경 내 플레이어로 끌려오는 자석 로직, 접촉 시 습득 이벤트.
- **DoD**: 파괴 시 오브 드랍, 근접 시 빨려와 습득되고 경험치 이벤트 발행.
  - **판정(2026-08-18)**: 드랍·자석 끌림·근접 습득 = **충족**(사용자 육안 확인). "**경험치 이벤트 발행**"은 누적/레벨업 시스템이 있어야 의미 → **M1-6로 이관**. 습득 지점에 `[TEMP]` 로그를 두고 M1-6에서 실이벤트로 대체.
- **의존**: M1-4
- **⚙️ 결정 & 진행 (2026-08-18, 사용자)** — 단계 분할, 각 단계 사용자 육안 확인 후 진행.
  - **오브 비주얼(사용자 준비)**: `Assets/Imports/Hovl Studio/Magic effects pack/Prefabs/Environment/Crystal effect green/blue/red`를 **MCP 복제**(새 GUID)해 `Assets/Prefabs/Orbs/Orb Crystal {green,blue,red}.prefab` 생성. **일단 green 사용**. 게임플레이 프리팹 `Assets/Prefabs/Orbs/Orb.prefab` = root(`Orb`) / 자식 `Model`(green 크리스탈).
  - **구조 배치**: `Orb`/`OrbPool`은 **`VD.Core`**. `OrbPool : PooledObjectPool<Orb>`(상속 베이스 재사용).
  - **✅ 1단계(드랍 + 오브 존재)**: 적 **실사망**(`Enemy.Die`)에만 드랍 훅 — `Enemy`에 `Action<Vector3>` 드랍 콜백 주입(`SetDropHandler`), 화면 밖 `Despawn`은 드랍 안 함. `EnemySpawner`가 `OrbPool` 자동탐색(`FindAnyObjectByType`) 후 스폰 적에 `DropOrb`(사망 위치에 `orbPool.Get()`) 배선. GameScene에 `OrbPool`(prewarm 16).
  - **✅ 2단계(자석)** — 거동 사용자 결정:
    - **반경 밖** = 전방(월드 -Z)으로 **일정 속도 드리프트**. 반경 내 아니면 **그대로 지나쳐** despawn(호밍 아님).
    - **반경 안** = `magnetRadius` 이내 진입 시 **캡처(래치)** → 플레이어로 **가속 끌림**(경계=driftSpeed → 접촉=magnetMaxSpeed, 오버슛 클램프). 캡처되면 안 놓침.
    - 타깃(플레이어) = `OrbPool`이 **태그 "Player"** 로 1회 탐색·캐시 후 `Orb.OnSpawned(target, Return)`로 주입(**Core→Player 결합 회피**). Player 프리팹 태그 "Player".
  - **✅ 3단계(습득)** — **거리 기반**(콜라이더/레이어 불필요): 캡처된 오브가 `pickupRadius` 이내 도달 시 습득 → **`[TEMP]` 로그 + 풀 반납**. 경험치 이벤트 배선은 M1-6.
  - **튜닝(Orb 프리팹, Day5 잠정)**: `driftSpeed` 6 · `magnetRadius` 8 · `magnetMaxSpeed` 40 · `pickupRadius` 0.6 · `despawnZ` −50.
  - **관련 파일**: `Assets/Scripts/Core/{Orb,OrbPool}.cs`, `Assets/Scripts/Enemy/{Enemy,EnemySpawner}.cs`. 프리팹 `Assets/Prefabs/Orbs/{Orb,Orb Crystal green/blue/red}.prefab`, `Player.prefab`(태그). GameScene에 `OrbPool`.

### M1-6 · 경험치 / 레벨업 (점증형 임계값) 🔴 `[x]`
- **목적**: progression §1. 오브 누적 → 임계값 도달 → 레벨업.
- **작업**: 경험치 누적, **레벨별 점증 임계값 곡선**(수치 Day5), 레벨업 시 이벤트(→ 3choice 트리거). R3 `ReactiveProperty<int>`(레벨)·`ReactiveProperty<float>`(경험치%) — HUD 바인딩 대비.
- **DoD**: 오브 습득이 게이지 채우고, 임계값마다 레벨업 이벤트 발생. ✅ **충족**(2026-08-18).
- **의존**: M1-5
- **문서**: progression-design.md §1, **[05_ProgressionAndEvents.md](05_ProgressionAndEvents.md)**
- **✅ 완료(2026-08-18, 사용자 결정 반영)** — 상세 = [05_ProgressionAndEvents.md](05_ProgressionAndEvents.md)
  - **사용자 결정**: (1) 상태 = **GameEvents 확장**, (2) 오브→경험치 = **이벤트 발행→구독**(pub/sub), (3) 임계값 = **지수형** `base×growth^(n-1)`.
  - **산출(`VD.Core`)**: `GameEvents` 확장 — `OrbCollected`(`Observable<int>`) · `Level`(`ReadOnlyReactiveProperty<int>`) · `XpNormalized`(`ReadOnlyReactiveProperty<float>` 0~1) · `LevelUp`(`Observable<int>`). 발행/갱신 `internal`. 신규 **`ExperienceSystem`**(GameScene 1개) — `OrbCollected` 구독·누적, 지수 임계값 도달 시 초과분 이월+레벨업 발행. `Orb`는 습득 시 `PublishOrbCollected(xpValue)`(기본 1).
  - **튜닝(Day5 잠정)**: `baseThreshold` 5 · `growth` 1.3. `Orb.xpValue` 1(→ M2-2 SO).
  - **검증**: 오브 5개→Lv2·다음 임계 6.5, 에러 0.
  - **⚠️ 임시요소**: `[TEMP] 레벨업` 로그(유지). `Level`/`XpNormalized`는 M1-10 HUD 바인딩.
  - **관련 파일**: `Assets/Scripts/Core/{GameEvents,ExperienceSystem,Orb}.cs`. GameScene에 `ExperienceSystem`.

### M1-7 · 3choice 강화 선택 (일시정지 팝업) 🔴 `[x]` (2026-08-19 검증 완료)
- **목적**: progression §1. 레벨업 시 게임 일시정지 + 3택 카드 + 선택 적용 + 재개.
- **작업**: 레벨업 이벤트 수신 → `Time.timeScale=0` → 후보 3개 롤(중복 방지) → uGUI 카드 팝업 → 선택 시 강화 적용 → 재개. 강화 데이터는 M1-8 최소 풀 사용.
- **DoD**: 레벨업 시 프리즈되고 3장 뜸, 하나 고르면 효과 적용 후 게임 재개. ✅ **충족**(2026-08-19, Play 검증).
- **의존**: M1-6, M1-8, M1-10(HUD/캔버스)
- **문서**: progression-design.md, ui-design.md §3
- **✅ 완료(2026-08-19)**:
  - **산출**: `VD.UI.LevelUpPopup`(`GameEvents.LevelUp` 구독 → `GameManager.Pause()`(timeScale 0) → `UpgradeSystem.Roll(3)` 3장 카드 표시 → 카드 클릭 시 `UpgradeSystem.Apply(선택)` → `GameManager.Resume()`, **다중 레벨업 큐 순차**), `VD.Core.UpgradeDisplay`(readonly struct), `UpgradeSystem.Describe(UpgradeType)`(제목/설명/**효과 수치를 실제 필드에서 렌더** → UI 하드코딩 회피). M1-8의 **임시 자동적용 제거** — 팝업이 선택·적용 구동.
  - **씬**: GameScene에 `LevelUp Canvas`(sortOrder 100) + 딤 Panel + 3 Card(Button+TMP) + `EventSystem`(`InputSystemUIInputModule`).
  - **한글 폰트**: 런타임 TMP 텍스트에 **SUIT SDF**(`Assets/Imports/Fonts/SUIT-Regular SDF`·`SUIT-Heavy SDF`) 적용 — 기본 `LiberationSans SDF` 한글 글리프 없어 깨지던 문제 해소. (배선 세부 점검 보류.)
  - **검증**: 레벨업→프리즈·3카드→클릭 시 스탯 변경·재개·큐 순차 정상, 한글 렌더 정상. stray-close는 실버그 아님.
  - **⚠️ `[TEMP]` 로그 유지**(사용자 방침).

### M1-8 · 최소 3choice 강화 풀 (공용 스탯) 🔴 `[x]` (풀·적용·롤 완료 — 팝업 UI·선택은 M1-7)
- **목적**: scope-tiering Must "빌드 선택이 성립할 최소". 공격력/이동속도/최대체력 등 몇 종.
- **작업**: 강화 항목 정의(효과 적용 방식: 스탯 배율/가산), 최소 3~5종 하드코딩 또는 소형 데이터. 3choice 롤 대상이 되게 연결.
- **DoD**: 최소 3종 이상이 롤에 등장하고 각각 실제로 스탯을 바꿈. ✅ **충족**(2026-08-18, execute_code 검증).
- **의존**: M1-2/M1-3(스탯 대상 존재)
- **문서**: [upgrade-pool.md](../Designs/upgrade-pool.md) (풀세트는 M4)
- **✅ 완료(2026-08-18, 사용자 결정 반영)**:
  - **사용자 결정**: (1) 정의 = **하드코딩**(enum+로직, SO는 M2), (2) 효과 = **능력치별 상이**, (3) 항목 3종 = **이동속도/최대체력/자석범위**. 공격력·연사 등은 **무기 스코프**라 M4 후로 미룸.
  - **산출**: `VD.Core.UpgradeType`(enum: MoveSpeed/MaxHp/MagnetRadius) + `VD.Player.UpgradeSystem`(GameScene 1개) — `GameEvents.LevelUp` 구독 → `Roll(3)`(Fisher–Yates, 중복없음) → **임시 자동적용**+`[TEMP]` 로그(→ M1-7이 대체). 라우팅 mutator: `PlayerMovement.AddMoveSpeedMultiplier`(배율%)·`PlayerHealth.AddMaxHp`(가산+회복)·`OrbPool.AddMagnetRadius`(가산). `Roll()`/`Apply()`는 **public → M1-7 팝업이 재사용**.
  - **효과 방식(능력치별)**: 이동=배율 `dragGain*=(1+pct)`, 최대체력=가산 `maxHp+=n`(현재HP도 +n), 자석범위=가산 `magnetRadius + bonus`.
  - **튜닝(Day5)**: `moveSpeedPct` 0.12 · `maxHpAdd` 20 · `magnetRadiusAdd` 2.
  - **검증(execute_code)**: 롤에 3종 등장, MoveSpeed 5→5.6·MaxHp 100→120·MagnetRadius +2, 에러 0.
  - **⚠️ 임시/설계 노트**: 항목 3개라 롤 3장=항상 전부 등장(다양성은 항목 늘면, M3-4/M4-8).
  - **관련 파일**: `Assets/Scripts/Core/{UpgradeType,OrbPool,Orb}.cs`, `Assets/Scripts/Player/{UpgradeSystem,PlayerMovement,PlayerHealth}.cs`. GameScene에 `UpgradeSystem`.

### M1-9 · HP / 데미지 / 게임오버 🔴 `[x]` (게임오버 전이·정지·점수 확정 — 결과 화면 UI는 M1-10/M2)
- **목적**: 종료 조건. HP 0 → 게임오버 → 결과.
- **작업**: 플레이어 HP, 피격 처리, HP 0 시 게임오버 상태 전환 + 결과값(생존시간/점수) 확정.
- **DoD**: 피격 누적으로 HP 0 되면 게임오버 화면/상태로 전환, 최종 점수 표시. ✅ **충족**(2026-08-18) — 상태 전환·점수 확정·임시 로그. **결과 "화면"(ResultScene/HUD 표시)은 M1-10/M2**.
- **의존**: M1-1, M1-4
- **문서**: progression-design.md §3, **[05_ProgressionAndEvents.md](05_ProgressionAndEvents.md) §종료/점수**
- **✅ 완료(2026-08-18, 사용자 결정 반영)**:
  - **사용자 결정**: (1) 게임오버 = **GameScene 정지형**(HP0 → GameOver 상태 + `timeScale 0` + 결과값 보관+임시 로그, ResultScene 전환·결과 UI는 이후), (2) 점수 = **생존시간 + 처치점수**(처치당 점수 하드코딩 → M2-2 SO).
  - **산출**: `GameEvents` 확장 — `EnemyKilled`(`Observable<int>`) · `HpNormalized`·`Score`·`SurvivalTime`(`ReadOnlyReactiveProperty`). `GameManager.GameOver()` → `timeScale 0`. 신규 **`ScoreSystem`**(GameScene 1개) — Playing 동안 생존시간 누적 + `EnemyKilled` 합산, `Score=round(생존×rate)+처치점수`. `Enemy` 실사망 시 `PublishEnemyKilled(killScore)`(기본 10). `PlayerHealth` — HP% 게시 + HP0 시 `GameManager.GameOver()`.
  - **튜닝(Day5 잠정)**: `ScoreSystem.timeScoreRate` 1 · `Enemy.killScore` 10.
  - **검증(execute_code)**: 생존 53.1s + 처치 180 = **점수 233**, `GameOver()`→state GameOver·timeScale 0, 에러 0.
  - **⚠️ 임시요소(유지)**: `[TEMP]` 피격·게임오버 로그. `Score`/`SurvivalTime`/`HpNormalized`는 M1-10 HUD 바인딩.
  - **관련 파일**: `Assets/Scripts/Core/{GameEvents,GameManager,ScoreSystem}.cs`, `Assets/Scripts/Enemy/Enemy.cs`, `Assets/Scripts/Player/PlayerHealth.cs`. GameScene에 `ScoreSystem`.

### M1-10 · HUD (우측 상단) + 점수/생존시간 🔴 `[x]`
- **목적**: ui-design. 생존시간/점수/HP 최소 표기(uGUI).
- **작업**: uGUI 캔버스, 우측 상단 생존시간·점수, HP 표기. 점수=생존시간+처치점수. R3로 상태→UI 바인딩.
- **DoD**: 플레이 중 생존시간·점수·HP가 실시간 갱신. ✅ **충족**(2026-08-18, 스크린샷+런타임값 확인).
- **의존**: M1-9
- **문서**: ui-design.md §3, progression-design.md §3
- **✅ 완료(2026-08-18, 사용자 결정 반영)**:
  - **사용자 결정**: (1) HUD 구성 = **시간·점수·HP + 레벨·경험치바**, (2) HP 표현 = **게이지 바**, (3) 텍스트 = **TextMeshPro**(에센셜 임포트). 레이아웃 = 우측 상단 스택(최소 투자, Day5 조정).
  - **산출**: `VD.UI.HudView`(표시 전용) — `GameEvents` 구독→시간(mm:ss)/점수/레벨/HP바(`HpNormalized`)/경험치바(`XpNormalized`), R3 `AddTo` 수명 연동. GameScene에 **HUD Canvas**(ScreenSpaceOverlay + CanvasScaler 1920×1080 match0.5) + TMP 텍스트 3 + HP/XP 바(Image Filled Horizontal).
  - **인프라**: `VD.Runtime` asmdef에 `UnityEngine.UI`·`Unity.TextMeshPro` 참조 추가. **TMP 에센셜 리소스 임포트**(`Assets/TextMesh Pro`).
  - **검증**: 런타임 값 `time 00:58 / SCORE 248 / Lv 2 / hpFill 1.0 / xpFill 0.4` 실시간 갱신, 에러 0.
  - **⚠️ 잔여**: 레이아웃·색·바 스타일 = 최소 기본(Day5 튜닝). HP 숫자 없음(바만). `[TEMP]` 로그 유지.
  - **관련 파일**: `Assets/Scripts/UI/HudView.cs`, `Assets/Scripts/VD.Runtime.asmdef`. GameScene에 `HUD Canvas`. TMP 에센셜 `Assets/TextMesh Pro`.
