# CONTEXT — 작업 시 최우선 확인 문서 (레포 루트)

> **Claude는 이 프로젝트에서 무언가를 진행하기 전에 이 문서를 무조건 먼저 확인한다.**
> 사용자와의 약속 + 문서 탐색 우선순위 + 핵심 사실을 담는다. 규칙이 바뀌면 여기부터 갱신.

## 0. 폴더 구조

| 경로 | 용도 |
|---|---|
| `context.md` (레포 루트, 이 문서) | **최우선 확인** — 작업 규칙·약속·문서 우선순위·핵심 사실 |
| `Docs/Designs/` | **기획 문서** (게임 디자인). 허브 = [onepage-design.md](Docs/Designs/onepage-design.md) (§0 문서 인덱스) |
| `Docs/Dev/` | **개발 문서 및 이슈 관리** (기술 설계, 작업/이슈 트래킹) |

## 1. 사용자와의 약속 (작업 규칙)

1. **기획 문서는 `Docs/Designs/`, 개발·이슈 문서는 `Docs/Dev/` 에 누적**한다.
2. **결정/변경이 생기면 관련 세부 문서를 즉시 업데이트**하고, `onepage-design.md`의 문서 인덱스·TODO·크로스링크를 함께 갱신해 정합성을 유지한다.
3. **검증하고 한 번만 작성** — 기술 문서에 코드 고유명사(클래스·메서드·필드·enum 등)를 쓸 때는 **먼저 grep/검색으로 실제 존재를 확인**한 뒤 쓴다. 검증 안 된 이름은 쓰지 않는다. (전역 CLAUDE.md 규칙) 이름은 맞아도 역할이 불확실하면 헷지하거나 사용자에게 확인.
4. **스코프 존중** — 마감 2026-08-31. 기획 풍부함 ≠ 전부 구현. [scope-tiering.md](Docs/Designs/scope-tiering.md)의 Must/Should/Nice를 기준으로 우선순위를 판단한다.
5. **관심사 분리** — 서로 다른 도메인(예: 적 스폰 풀 vs 3choice 풀)을 한 툴/문서에 억지로 묶지 않는다.
6. 결정을 문서화할 때 **폐기/변경된 구버전 항목은 명시적으로 폐기 표시**해 혼동을 막는다.
7. **구현은 사용자의 명시적 지시·승인 후에만.** Claude는 기능의 **디자인·구현 방법·판단 기준을 임의로 정해 코드/씬/에셋을 만들지 않는다.** 사용자가 주제(아이템)만 던진 것은 구현 지시가 아니다. 사용자가 디자인·방법·기준을 상세히 결정하고 **명시적 구현 지시**를 내린 뒤에만 착수한다. 불명확하거나 선택지가 있으면 **먼저 질문**한다. (사례: M0-2 큐브 회전을 사용자 지시 없이 임의 해석·구현 → 폐기, 재작업.)
8. **Backlog·문서 갱신도 사용자 체크 후.** 작업 내용·완료(`[x]`) 처리·문서 상태 변경을 **임의로 반영하지 않는다.** 진행 전/후 **사용자에게 작업 내용을 확인받고** 갱신한다.
9. **기능 단위 진행 + 사용자 주도 페이스.** 기능 하나를 구현하면 **사용자가 직접 테스트하고 피드백을 준다.** Claude는 완료 후 **반사적으로 "다음 백로그 진행할까?"라고 묻지 않는다.** 다음 작업의 진행 여부·시점·범위는 사용자가 정한다. Claude는 **완료 보고 + 꼭 필요한 확인만 하고 멈춰서** 사용자 지시를 기다린다.

## 2. 문서 탐색 우선순위 (뭔가 찾을 때 순서)

1. **이 문서(context.md)** — 규칙·핵심 사실
2. **[onepage-design.md](Docs/Designs/onepage-design.md)** — 기획 허브: 개요 + §0 문서 인덱스 + TODO
3. **주제별 세부 문서** — onepage §0 인덱스에서 이동
   - 기획(`Docs/Designs/`): 업그레이드/무기(upgrade-pool, weapon-acquisition), 적/에디터툴(enemy-design), 조작(controls-design), UI(ui-design), 진행·난이도·점수(progression-design), 우선순위(scope-tiering)
   - 개발·이슈(`Docs/Dev/`): [backlog.md](Docs/Dev/backlog.md)(**허브** — 개요·스냅샷·크로스컷·진행로그) + **마일스톤별 상세 `backlog-M0..M5.md`**(각 상단 `⚡ 특이사항` 헤더만 읽어도 크로스 마일스톤 파악 — 특정 마일스톤 작업 시 그 파일 하나면 충분), [issues.md](Docs/Dev/issues.md)(알려진 문제·보류·버그 트래커), [01_AssemblyDefinition.md](Docs/Dev/01_AssemblyDefinition.md)(asmdef 어셈블리 구조·이유), [02_GameStateArchitecture.md](Docs/Dev/02_GameStateArchitecture.md)(게임 상태머신·이벤트 채널, M1-1), [03_PlayerMovementAndCamera.md](Docs/Dev/03_PlayerMovementAndCamera.md)(플레이어 이동·뱅킹·카메라 리그, M1-2), [04_ObjectPooling.md](Docs/Dev/04_ObjectPooling.md)(재사용 풀 베이스 `PooledObjectPool<T>`, M1-3~M1-5), [05_ProgressionAndEvents.md](Docs/Dev/05_ProgressionAndEvents.md)(경험치/레벨업 + GameEvents 진행 채널, M1-6), [06_EnemyPipeline.md](Docs/Dev/06_EnemyPipeline.md)(⭐적 조합 파이프라인 — 스키마·오서링 툴·유효성·런타임 조립 + **AI 모듈(이동/공격, §4.7)**, M2~M3) 등 기술 설계 문서

> 세부 문서와 onepage가 어긋나면 **세부 문서가 최신**(onepage §3·§7·§8은 초안 잔재 가능). 발견 즉시 정합화.

## 3. 핵심 사실 (빠른 참조)

- **타이틀**: Void Drift (한글 부제 "공허 속으로")
- **목적**: 그라비티 판교 DevHub Unity 클라이언트 공고 대응 포트폴리오
- **핵심 어필**: UI Toolkit **에디터 커스텀 툴** (적 조합 오서링 + 유효성 경고 + 스폰 풀) — 공고 1순위
- **장르/플랫폼**: 3D 로우폴리 **모바일(가로)** 로그라이트 비행슈팅 / 오토 사격 / 3choice 성장
- **UI**: 런타임 = uGUI, 에디터 = UI Toolkit
- **마감**: 2026-08-31 ("Day5 튜닝"은 밸런싱 단계를 뜻하는 관용 라벨, 실제 5일 아님)
- **개발 도구**: Claude + UnityMCP (CoplayDev `com.coplaydev.unity-mcp`, HTTP `127.0.0.1:8080`, `.mcp.json` 등록)
- **씬(결정 2026-08-17, M0-4)**: **TitleScene / GameScene / ResultScene** 3개(가볍게). Build Settings 순서 Title=0·Game=1·Result=2. **Loading은 별도 씬 아님** — GameScene 진입 시 오버레이로 뜨고 리소스 로딩 완료 후 **Fade Out**(방침만, 구현은 M2 Addressables 이후). `SampleScene` = **테스트·실험 전용**, 빌드 제외 유지.

## 4. 진행 상태 & 다음 작업 (세션 인계)

> 다음 작업은 **다른(새) 세션**에서 진행될 수 있음. 이 섹션이 인계 기준.

### ▶ 다음 세션 인계 — 🔶 M4 진행 중(M4-1·M4-2·M4-3 완료), 다음 = M4-4 (2026-08-21)

- **완료 상태**: **M1 코어루프 + M2 적 파이프라인 + M3 전부(적 12종 로스터·이동/공격 AI·3choice 데이터화) 완료 ⇒ Must 코어 충족.** 적 = **조합형**(공통 로직 셸 + `EnemyBuilder` 조립: 비주얼+스탯+**AI 모듈**). 파이프라인 = **[06_EnemyPipeline.md](Docs/Dev/06_EnemyPipeline.md)**, M3 상세 = [backlog-M3.md](Docs/Dev/backlog-M3.md) ⚡.
- **M4 진행 순서(사용자 확정 2026-08-20)**: `M4-1→2→3→4→7→8→5→6→9→10`. M5-1 빌드는 뒤로.
- **✅ M4-1 완료(2026-08-20)**: 무기 3종 **전략 모듈**(`IWeapon`/`WeaponContext`/`PlayerShooter` 오케스트레이터, `Assets/Scripts/Player/Weapons/`) — `StraightGun`·`HomingMissile`·`Railgun`, 각 전용 풀(`ProjectilePool`/`HomingProjectilePool`/`RailProjectilePool`). **동시 오토발사**. 유도=**타입1순위 조준**(원거리 Shooter 우선→가장 먼, `Enemy.Archetype` 주입)·가속·**날개 4발사대 탄약 동시발사**·MissileViking 모델. 레일건=관통(`maxPierce`)+감쇠+**TrailRenderer 궤적**(`RailTrail_Mat`). 탄약(동시줄기수)=레벨연동(M4-2 완료 — 아래 참조). ~~현재 데모=3종 전부 보유~~ → **M4-3에서 시작 로드아웃=기관총만으로 전환·마일스톤 획득 구현 완료(구 Step5 흡수).** 신규 이슈 **I-4**(탄막 무제한 발사). 상세 = [backlog-M4.md](Docs/Dev/backlog-M4.md) ⚡·M4-1. **무기 기술문서는 M4 완료 시 정리(사용자 결정).**
- **✅ M4-2 완료(2026-08-21)**: 무기 레벨 **Lv1~4 = 탄약↑**. 공통 `WeaponBase : IWeapon`(`Assets/Scripts/Player/Weapons/`) 레벨 머신 — `Level`(1~`MaxLevel`=4)·`IsMaxLevel`·`LevelUp()`, **`Ammo => min(Level, 4)`** 매핑. 3무기 상속. `IWeapon`에 레벨 API 노출(M4-3가 폴리모픽 조회). **기관총도 평행 오프셋 멀티샷**(레일건식, `straightStreamSpacing`; Lv1=1발 동작무변화). 유도/레일 수동 `Ammo` 제거→레벨=탄약. `PlayerShooter` 인스펙터 `*Ammo`→**`*StartLevel`(1~4, 검증용 시작값)**. **M4-3 레벨업 훅 = `IWeapon.LevelUp()`**. Play 검증 특이사항 없음.
- **✅ M4-3 완료(2026-08-21)**: **시작 로드아웃=기관총만** + **5레벨 마일스톤 3choice 무기 카드**. `PlayerShooter`가 무기 슬롯 소유(`_owned`+`WeaponId`+`BuildWeapon` 팩토리, API=`HasWeapon`/`WeaponLevel`/`IsWeaponMaxed`/`AcquireOrLevelUp`; Awake=기관총만 Acquire). `UpgradeSystem.Roll(count, playerLevel)`=레벨 5의 배수면 무기 카드 1개 보장+나머지 일반, 아니면 무기 배제(`IsEligible`/`PickWeighted`/`TryWeaponId`). `Apply`→무기획득/레벨업, `Describe`→미보유"획득"/보유"Lv n→n+1". `UpgradeType`에 `Weapon{Straight,Homing,Railgun}` 추가, `LevelUpPopup`=`Queue<int>` 레벨 큐로 레벨값 전달. 무기 SO 3종(`Upgrade_Weapon_*`, 값 미사용) + `pool` 9개(스탯6+무기3) 배선. **무기 카드 가중치·엣지(자격<3)는 Day5**. Play 검증 특이사항 없음.
- **✅ PC 보조 입력 추가(2026-08-21)**: 모바일 원칙 유지하되 데스크톱 플레이 보조로 **WASD/화살표 이동**(드래그와 공존·합산, `PlayerMovement.keyboardMoveSpeed`, New Input System `Keyboard.current` 직접 읽기)을 추가. **Space→실드**는 M4-4에서 코너 버튼과 함께 배선 예정. Standalone 빌드 타깃 전환은 별도(M5-1). 상세=[controls-design.md](Docs/Designs/controls-design.md) §5.5.
- **다음 = M4-4**(실드 스킬 전용 버튼 + 강화 3종, **Space 발동 포함**). 밸런싱은 M4-5 난이도 그래프 뒤 일괄.
- **3choice 데이터화(M3-4)**: `UpgradeDefinition` SO(`Assets/Scripts/Data/`, type/수치/가중치/maxStacks/표시) + **Upgrade Authoring 창**(`Window/Void Drift/Upgrade Authoring`, `SoTableEditorView` 재사용 — 두 번째 Table Tool). `UpgradeSystem`=SO 풀 가중치 롤·중복없음·type 라우팅. 강화 6종 SO(`Upgrade_*`): 이동/최대체력/자석 + 신규 체력재생·오브가치·공격력(=기초 공격력, M4-8 배율 base). 적용 훅=`PlayerHealth.AddRegen`·`ExperienceSystem.AddOrbValueBonus`·`PlayerShooter.AddAttackPower`. HUD=`GameEvents.HpValues`(HP 절대값) 채널 추가로 HP 숫자 표기.
- **AI = 순수 C# 전략 모듈**(사용자 결정, MonoBehaviour 아님): `IMoveBehaviour`(`StraightMove`/`ChaseMove`/`WeaveMove`)·`IAttackBehaviour`(`ContactAttack`/`BarrageAttack`/`AimedShot`/`SuicideAttack`), 위치=`Assets/Scripts/Enemy/AI/`. `Enemy`가 Update에서 `Tick` 위임, `EnemyBuilder.ResolveMove`/`ResolveAttack`가 `def.moveAI`/`def.attackAI`로 주입(무상태=싱글톤 공유, 상태 있는 탄막/조준단발/사행=인스턴스별). 플레이어 조회=`PlayerLocator`(Player 태그). **직진 하드코딩은 M3-1이 제거함.** **`WeaveMove`·`AimedShot`은 M3-3에서 M4-7 선반영 — 견제(Hover)만 미구현(직진 폴백).**
- **적 로스터(M3-3) = 4라인 × 3티어 = 12 SO**: `Enemy_{LightCharger,HeavyCharger,Shooter,Bomber}_T{1,2,3}`(구 `Enemy_Sample_*` 삭제). Light/Heavy는 둘 다 archetype=`Charger`, 네이밍·스탯으로 분리. 티어 = 이동 복잡도(직진→추적→사행)+공격밀도+스탯 에스컬레이션. **모델 크기 편차 보정 = `EnemyDefinition.visualScale`(신설)** — 빌더 ①이 `Enemy.AttachVisual(prefab, scale)`로 비주얼 자식에만 곱(히트박스=셸 고정). 7개 모델 전부 사용(라벨 유효).
- **씬/데이터 상태(GameScene, 저장됨)**: `EnemySpawner`에 `spawnTable`=**12행(티어 가중 T1:3/T2:2/T3:1)** + `DifficultyProvider`(배율 1.0 스텁) + `EnemyBulletPool`(prewarm 64, 스포너 `bulletPool` 연결) 배치. `Enemy.prefab`=**비주얼 없는 로직 셸**(스케일 6, root Collider+`Enemy`), `EnemyPool`=셸 prewarm. **적탄 `EnemyBullet.prefab`=임시 붉은 큐브(비주얼 교체 예정)**.
- **레이어(M3-2 신설)**: `EnemyBullet`(11) 추가 — 물리 매트릭스 **EnemyBullet×Player만 ON**. 기존 Player(8)/Enemy(9)/PlayerBullet(10)에 더함.
- **밸런싱 파킹(M4-5 이후)**: 적 스탯·탄막 밀도·초반 과다 스폰은 전부 **시작점** — **난이도 그래프(M4-5)** 만든 뒤 튜닝. 시간 게이팅(초반=저티어)=**M4-6**. **[I-3] 플레이어 체력 회복 수단 부재**도 그때 함께.
- **M2-5 스코프 밖(이후)**: 드랍오브 데이터화(`dropOrb.visual`/`xpValue` 주입) · 적탄 비주얼 교체·부채꼴 각/탄 수명·`WeaveMove` 진폭·`AimedShot` 등 코드 기본값의 SO화.
- **이슈**: [issues.md](Docs/Dev/issues.md) **I-3**(체력 회복 부재, 보류) · **I-2**(플레이어 Aim 어색, 보류) · **I-1**(이동 관성감, 보류).
- **주의**: 레이어 물리 매트릭스는 **에디터 재부팅 시 재적용** 필요(신설 `EnemyBullet`×Player 포함).

---

**현재 상태**: 기획 완료(`Docs/Designs/` 세트). Unity 프로젝트·에셋 셋업 완료. **Backlog** → [Docs/Dev/backlog.md](Docs/Dev/backlog.md) (M0~M5). **M0 부트스트랩 전부 완료** + **M1-1(게임 상태 골격) 완료** — 어셈블리 골격·씬 3개·입력(New Input System)·리액티브 스택(R3/R3.Unity/UniTask) 확정, `VD.Core`에 상태머신(`GameManager` 싱글톤)+이벤트 채널(`GameEvents`) 안착. **M1-2(플레이어 이동) 완료** — 상대 드래그·물리 이동·뱅킹·고정 카메라 리그·`Player` 프리팹. **M1-3(오토 사격) 완료** — 조준(`PlayerAim`/FirePoint)+발사(`PlayerShooter`·`Projectile`)+상속형 풀(`PooledObjectPool<T>`←`ProjectilePool`). 데미지/히트는 M1-4에서 완료. **M1-4(적 기본 엔티티 & 스폰) 완료(`[x]`, 2026-08-19 마감)** — 스폰·직진 접근·사격 파괴·충돌 HP감소·레이어 분리. 원뿔 각도/사거리는 사용자 튜닝, `[TEMP]` 로그·기즈모는 유지 방침. **M1-5(오브 드랍·자석·습득) 완료** — 적 실사망 드랍→전방 드리프트(못 만나면 지나침)→반경 내 캡처·가속 끌림→거리 기반 습득(현재 `[TEMP]` 로그, 경험치 이벤트는 M1-6). `Orb`/`OrbPool`(VD.Core), 비주얼=Crystal effect green. **M1-6(경험치/레벨업) 완료** — `GameEvents` 확장(진행 채널: `OrbCollected`/`Level`/`XpNormalized`/`LevelUp`)+`ExperienceSystem`(지수형 임계값), 오브 습득→경험치 이벤트 발행. **M1-9(HP/게임오버/점수) 완료** — HP0→`GameManager.GameOver()`(정지형 `timeScale 0`), `ScoreSystem`(생존시간+처치점수), `GameEvents`에 `EnemyKilled`/`HpNormalized`/`Score`/`SurvivalTime` 추가. **M1-10(HUD) 완료** — `VD.UI.HudView`(uGUI+TMP)가 GameEvents 구독→우상단 시간/점수/레벨/HP·경험치바 실시간. **M1-8(최소 3choice 강화 풀) 완료** — `UpgradeType`(이동/최대체력/자석범위) + `UpgradeSystem`(LevelUp→`Roll`/`Apply`, 임시 자동적용). **M1-7(3choice 팝업 UI) 완료** — `VD.UI.LevelUpPopup`(`GameEvents.LevelUp`→`GameManager.Pause()`→`UpgradeSystem.Roll(3)` 3카드→클릭 `Apply(선택)`→`Resume()`, 다중 레벨업 큐 순차)+`VD.Core.UpgradeDisplay`(struct)+`UpgradeSystem.Describe`(효과 수치 실제 필드 렌더). M1-8 임시 자동적용 제거. 한글 폰트=**SUIT SDF** 적용. **⇒ M1 코어루프(이동·사격·적·오브·경험치·레벨업·HP/게임오버/점수·HUD·3choice) 완성.** (Play 검증 완료 2026-08-19.)

**개발 전 순서** (전부 완료):
1. ~~Unity 프로젝트 생성~~ ✅ (사용자)
2. ~~3D 로우폴리 에셋 소싱·삽입~~ ✅ (사용자, `Assets/Imports/`)
3. ~~Backlog + 상세 명세 ListUp~~ ✅ → [Docs/Dev/backlog.md](Docs/Dev/backlog.md)

**M0 진행**:
- **M0-1 (Unity MCP 연결)** ✅ 완료 — CoplayDev MCP for Unity, HTTP `127.0.0.1:8080`, `.mcp.json` 등록·왕복 검증.
- **M0-2 (큐브 회전 스모크)** ✅ **완료(재작업)** — 물리(Rigidbody+angularVelocity) Z축 회전, 인스펙터(속도/크기/방향), `SmokeCube` 재사용. 사용자 육안 확인. `Assets/Scripts/Smoke/CubeSpinner.cs`. (1차 임의구현은 폐기 → 재작업, 사유는 backlog M0-2 참조.)
- **M0-3 (입력 백엔드 & R3.Unity 판단)** ✅ **완료** — 사용자 결정: 입력 = **New Input System**(`com.unity.inputsystem` 1.20.0, `activeInputHandler:1` New 단독), R3.Unity = **설치**(`com.cysharp.r3` 1.3.1 git UPM). MCP로 설치·검증(컴파일 에러 0, 왕복 정상). 구체 입력 액션 오서링은 M1-2에서. 상세는 backlog M0-3 결론.
- **M0-4 (프로젝트 골격)** ✅ **완료** — asmdef 2개 `VD.Runtime`/`VD.Editor`(네임스페이스 루트 `VD.*`), 폴더 `Scripts/{Core,Player,Enemy,UI,Editor}`, 씬 3개(Title/Game/Result). 리플렉션 검증·컴파일 0. 기술 문서 [Docs/Dev/01_AssemblyDefinition.md](Docs/Dev/01_AssemblyDefinition.md). 파일/네임스페이스 규칙은 해당 문서·backlog M0-4 참조.
- **M1-1 (게임 부트/상태 관리 골격)** ✅ **완료** — 상세 = [02_GameStateArchitecture.md](Docs/Dev/02_GameStateArchitecture.md). `VD.Core`에 `GameState`/`GameEvents`(별도 pub/sub 채널, R3)/`GameManager`(MonoBehaviour 싱글톤, 씬 한정, timeScale 제어)/`GameDebugDriver`(임시 키보드 검증). GameScene에 GameManager·카메라·라이트 배치. 검증: Boot→Playing 로그·timeScale 0↔1·컴파일 0. 사용자 결정: 전역=싱글톤, 이벤트=별도 채널.
- **M1-2 (플레이어 이동)** ✅ **완료** — 상세 = [03_PlayerMovementAndCamera.md](Docs/Dev/03_PlayerMovementAndCamera.md). `VD.Player.PlayerMovement`(이동 전담): 상대 드래그→Rigidbody 속도 직접 매핑(해상도 무관 `dragGain`, 현재 5), 뱅킹(자식 `Model` 비주얼 회전, 코 안쪽=조준, `invertYaw` OFF), 뷰포트 선-클램프. `Player` 프리팹(StarSparrow_1_LP_Red 복제, root=Rigidbody+PlayerMovement / child `Model`=메시). 카메라 = 고정 Perspective (0,0,-26) FOV55. 입력 = `Pointer.current` 직접. **알려진 이슈**: 관성감 → [issues.md](Docs/Dev/issues.md) I-1(보류).

**➡️ 진행 상태: M1 코어루프(M1-1~M1-10) 전부 완료(2026-08-19). M2 완료(2026-08-20) — M2-1(Addressables·적 비주얼 7종·라벨·로드 스모크)·M2-2(적 SO 스키마, a~d 전부)·M2-3(⭐UI Toolkit 오서링 창, a~g) 완료. **M2-4(유효성 경고) 완료(a~e)**(코어 R1·R2 `EnemyValidation`(VD.Core)·상세 경고박스·모순 필드 red 테두리/목록 행 ⚠·R3 §6 라벨교차 `AppendLabelWarning`(VD.Editor), e=창 실측 육안 검증 통과·비차단 저장 확인). **M2-5(최소 스폰 연결) 완료(a~f)** — 툴 데이터→런타임 스폰(**조립형/빌더**): 공통 로직 셸(`Enemy.prefab` 비주얼 분리)+주입 조립(`EnemyBuilder` ①비주얼 `EnemyVisualCache`(Addressables)→`AttachVisual` ②effective 스탯 `StatScaler`×`DifficultyProvider`(배율 1.0 스텁, 실배율 M4-5)→`ApplyStats`), `EnemySpawner` DB=`SpawnEntry[]`(def+weight) **가중 랜덤**+프리로드. 스탯 3층 분리(base RO/배율/effective). **스코프=비주얼+스탯만**(AI=M3-1/M3-2, 드랍오브 데이터화·실배율=이후, 비주얼 스케일=Day5). Play 검증 정상(22체·3종 모델·SO별 스탯·에러0, 사용자 육안). **⇒ M2 완료 = 포폴 핵심 데모 성립. 다음 = M3(AI 모듈).** **기술문서 = [06_EnemyPipeline.md](Docs/Dev/06_EnemyPipeline.md)**(스키마·오서링·유효성·런타임 조립 통합). 이슈 I-2(플레이어 Aim 어색) 보류 등록. M2-3 결과물: `Assets/Scripts/Editor/Authoring/`에 재사용 베이스 `SoTableEditorView<T>`(VisualElement, 목록+상세+CRUD+저장) 위에 `EnemyTableEditorView`+`EnemyAuthoringWindow`(메뉴 Window/Void Drift/Enemy Authoring)+`SoTableEditor.uss`. 확장성=도메인별 에디터를 베이스로 저렴하게(플레이어 등 후속, 별창/탭은 2번째 때 결정). 공격AI별 스탯 필드는 선택 AttackAI에 따라 비활성. `Archetype.Barrage→Shooter(사격형)` 개명(AttackAIType.Barrage 충돌 회피). 기술문서 = `06_EnemyPipeline.md`(M2 전체 통합 — 아키텍처 중심). M2-2 결과물: `Assets/Scripts/Data/`에 `EnemyStats`(struct 8필드)·`EnemyDefinition`(SO: visual AssetReference+이동/공격AI+archetype+stats+dropOrb, `RangeLabelOf` 파생 헬퍼)·`OrbDefinition`(SO: xpValue+visual GameObject). `VD.Runtime.asmdef`에 Addressables 참조 추가. 검증 인스턴스 = `Assets/ScriptableObjects/Data/`의 `Orb_Green`·`Enemy_Sample_Barrage`·`Enemy_Sample_Charger`. 오브 결정 (a): 동작은 공유 `Orb` 하나, 종류별로 비주얼+xp만 다름(크리스탈 3종 green/blue/red는 비주얼 전용). 수치=Day5, 런타임 배선(스탯/오브 데이터화 스폰)=M2-5 이후. 상세는 [backlog-M2.md](Docs/Dev/backlog-M2.md).** 아래 M1-4 서술은 이력(단계 분할 기록)으로 보존. — §1-9에 따라 진행. **적 프리팹 소스 = `FREE Low Poly Spaceships`**(spaceship_1~7). **M1-4 첫 적 = spaceship_6 1종**(나머지 볼륨업은 M3-3, 아키타입 잠정 매핑은 backlog M3-3). **진행 = 단계 분할.** ✅ **1단계(스폰+직진 접근 이동)** — `Enemy`/`EnemyPool`(상속형)/`EnemySpawner`(랜덤 위치 스폰), `Enemy.prefab`(spaceship_6, 임시 스케일 6). ✅ **2단계(적 피격→HP 감소→사망 + 타겟 스냅)** — `VD.Core.IDamageable`(최소 TakeDamage), `Enemy` 구현(maxHp 30), 투사체 히트=**트리거 콜라이더**(Projectile kinematic RB+트리거), 데미지 튜닝=`PlayerShooter.projectileDamage`. **원뿔 타겟 스냅**(매 발 nearest-in-cone, 락 아님) + 원뿔 기즈모. ✅ **3단계(플레이어 충돌 데미지 + 레이어 분리)** — `PlayerHealth`(Player root, maxHp 100, IDamageable 미구현=아군오사 방지, 적 접촉→HP 감소), `Enemy.contactDamage`. **레이어**: Player(8)/Enemy(9)/PlayerBullet(10) + 물리 매트릭스(Player×Enemy·Enemy×PlayerBullet만 ON), `PlayerShooter.targetMask`=Enemy. **⇒ M1-4 DoD 충족**(스폰·사격파괴·충돌 HP감소). **⚠️ 임시요소**: `[TEMP]` 로그·조준 원뿔 기즈모 잔존 — 원뿔 튜닝 후 정리 예정. 스폰 거리/폭·카메라(잠정 −36)는 사용자 튜닝. **적 속도 가변화 = 볼륨업(M3) 이관**. **▶ 잔존 = 원뿔 튜닝·임시요소 정리**(게임오버/HP UI = M1-9/M1-10). (※ M1-4 당시 시작점 메모 — 원뿔 각도(`aimConeHalfAngle`)·사거리(`aimRange`)·`[TEMP]` 로그·기즈모는 M1-4 마감 시 "유지 방침"으로 종결됨. **지금 시작점은 위 "▶ 다음 세션 인계 — M3" 블록**을 볼 것.) **M1-5·M1-6·M1-8·M1-9·M1-10·M1-7 전부 완료 ⇒ M1 코어루프 완성.**
> **🧩 M1-7 완료 (2026-08-19 검증·문서화)** — 구현·검증·문서 마감:
> - **구현**: `VD.UI.LevelUpPopup`(LevelUp 구독→`GameManager.Pause()`(ts0)→`UpgradeSystem.Roll(3)` 3장→클릭 `Apply(선택)`→`GameManager.Resume()`, **다중 레벨업 큐 순차**), `VD.Core.UpgradeDisplay`(readonly struct), `UpgradeSystem.Describe`(카드 제목/설명/**효과 수치는 실제 필드에서 렌더**=UI 하드코딩 회피). `UpgradeSystem` **임시 자동적용 제거**(팝업이 구동). GameScene `LevelUp Canvas`(sortOrder 100)+딤 Panel+3 Card(Button+TMP)+`EventSystem`(`InputSystemUIInputModule`).
> - **한글 폰트 = SUIT SDF**: 원본 `Assets/Imports/Fonts/SUIT-*.ttf`(9웨이트), 생성 SDF 2종(`SUIT-Regular SDF`/`SUIT-Heavy SDF`). 런타임 TMP에 적용 → 기본 `LiberationSans SDF` 한글 글리프 없어 깨지던 문제 해소. (배선 세부 점검 보류, 문제 시 대응.)
> - **검증(Play, 2026-08-19)**: 레벨업→프리즈(ts0)·3카드→클릭 시 스탯변경·재개·큐 순차 정상, 한글 렌더 정상. stray-close는 실버그 아님(반응 없을 때 수동 닫음).
> - **⚠️ `[TEMP]` 로그 유지**: 육안 확인용으로 **정리하지 않음**(사용자 방침 — 앞으로도 동일). Experience/PlayerHealth/Score의 로그 그대로 둠.
> - **관련 파일**: `Assets/Scripts/UI/{LevelUpPopup,HudView}.cs`, `Assets/Scripts/Core/{UpgradeType,UpgradeDisplay}.cs`, `Assets/Scripts/Player/UpgradeSystem.cs`. GameScene 활성, 레이어/물리 매트릭스 설정됨(에디터 재부팅 시 매트릭스 재적용 주의).
- **입력**: **New Input System API로만** 읽는다(레거시 `Input.*` 금지, `activeInputHandler:1`). `VD.Runtime`이 `Unity.InputSystem` 참조 완료. 이동은 `Pointer.current` 델타 직접 읽기(상대 드래그). **액션 에셋 미사용** — Unity6 기본 템플릿 `InputSystem_Actions.inputactions`는 M1-2에서 **삭제**(에셋 + `EditorBuildSettings` 전역 참조 해제). 이후 액션 에셋이 필요해지면 그때 도입.
- **코드 위치·규칙**: 런타임 = `VD.Runtime`(ns `VD.Core`/`VD.Player`/`VD.Enemy`/`VD.UI`), 에디터 = `VD.Editor`. 총알은 Player/Enemy 내부. 파일/네임스페이스/struct 규칙은 [Docs/Dev/01_AssemblyDefinition.md](Docs/Dev/01_AssemblyDefinition.md) + backlog M0-4.
- **리액티브·비동기**: R3(`ReactiveProperty`/`Subject`) + R3.Unity(`AddTo(this)` 수명 연동) 사용 가능. 비동기는 UniTask.

> ⚠️ **M1 인계 주의**
> - **GameScene**: `GameManager`(+임시 `GameDebugDriver`)·`Player`(프리팹 인스턴스)·`ProjectilePool`(투사체 풀, prewarm 32)·`EnemySpawner`(+EnemyPool, prewarm 16, M1-4)·`OrbPool`(오브 풀, prewarm 16, M1-5)·`ExperienceSystem`(경험치/레벨, M1-6)·`ScoreSystem`(생존시간/점수, M1-9)·`HUD Canvas`(uGUI+TMP, `HudView`, M1-10)·`UpgradeSystem`(3choice 강화 풀, M1-8)·Main Camera(고정 Perspective, z **−36 잠정** — M1-4서 프레이밍 조정중, 원래 −26)·Directional Light 배치됨. TitleScene/ResultScene은 아직 빈 상태. 현재 에디터 활성 씬 = GameScene.
> - **레이어(M1-4 신설, 물리 매트릭스)**: `Player`(8)·`Enemy`(9)·`PlayerBullet`(10). 매트릭스 = **Player×Enemy·Enemy×PlayerBullet만 ON**(총알↔플레이어=자살 OFF, 동종 OFF). 프리팹(Player/Enemy/Projectile)·씬 Player에 할당됨. 적 탄환용 `EnemyBullet`은 M3-2에서 추가 예정. 재부팅 후 매트릭스 풀리면 재적용.
> - **태그(M1-5)**: Player 프리팹에 builtin 태그 `Player` 부여 — `OrbPool`이 자석 타깃(플레이어) 탐색에 사용(`FindGameObjectWithTag`). Core→Player 타입 결합 회피용.
> - **마커**: `VDRuntimeMarker`는 M1-1에서 **삭제**(실코드가 참조 검증). `VDEditorMarker`(`Editor/`)만 **유지** — M2 에디터 툴 실코드 전까지 VD.Editor 검증용, 그때 삭제.
> - **`GameDebugDriver`**(`Core/`, 디버그·에디터 전용): 키보드 상태 전이(P=일시정지/재개·G=게임오버·R=재시작). 원래 M1-1 임시 검증용(M1-2·M1-9로 대체됨)이나, **ResultScene(M2) 전까지 게임오버 후 재시작 등 테스트 편의로 유지**하기로 결정(2026-08-19). `Update` 본문·`InputSystem` using이 `#if UNITY_EDITOR` 가드 → **빌드에선 무동작(inert)**. 삭제하지 말 것.
> - `SampleScene`/`SmokeCube`는 M0-2 테스트 잔재(빌드 제외). M1과 무관.
> - **Unity 재부팅 시 루틴**: Unity 서버 Start → Claude에서 MCP 재연결(또는 세션 재시작). 창의 "Client Configure: Not Configured"는 무시(우리는 `.mcp.json` 수동 등록 사용).

**⚠️ 설치 상태 실측(2026-08-17)**: UniTask ✅ / R3 코어 1.3.1 ✅(NuGet) / R3.Unity 통합 ✅설치(`com.cysharp.r3` 1.3.1) / Addressables ✅설치(`com.unity.addressables` 4.0.1, M2-1) / MCP ✅설치완료 / Input System ✅New 단독(`com.unity.inputsystem` 1.20.0, handler 1). 상세는 backlog §0.

**Backlog 유지 원칙**: [scope-tiering.md](Docs/Designs/scope-tiering.md)는 티어 수준, backlog는 구현 태스크 단위(DoD 포함). **마일스톤별 파일 분리(2026-08-19)** — 허브 `backlog.md` + `backlog-M0..M5.md`. 완료/결정 반영 시 **해당 마일스톤 파일 + 상단 `⚡ 특이사항` 헤더 + 허브 진행로그**를 함께 갱신해 정합 유지. **갱신은 §1-8에 따라 사용자 확인 후.**

## 5. 유지보수

- 규칙·약속 변경 → §1 갱신
- 새 기획 문서 추가 → onepage §0 인덱스 등록 + 필요 시 §2 갱신 / 새 개발·이슈 문서 → `Docs/Dev/`
- 핵심 사실 변경 → §3 갱신
- 진행 상태 변경(단계 완료 등) → §4 갱신
