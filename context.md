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
   - 개발·이슈(`Docs/Dev/`): [backlog.md](Docs/Dev/backlog.md)(구현 태스크·DoD), [issues.md](Docs/Dev/issues.md)(알려진 문제·보류·버그 트래커), [01_AssemblyDefinition.md](Docs/Dev/01_AssemblyDefinition.md)(asmdef 어셈블리 구조·이유), [02_GameStateArchitecture.md](Docs/Dev/02_GameStateArchitecture.md)(게임 상태머신·이벤트 채널, M1-1), [03_PlayerMovementAndCamera.md](Docs/Dev/03_PlayerMovementAndCamera.md)(플레이어 이동·뱅킹·카메라 리그, M1-2), [04_ObjectPooling.md](Docs/Dev/04_ObjectPooling.md)(재사용 풀 베이스 `PooledObjectPool<T>`, M1-3~M1-5), [05_ProgressionAndEvents.md](Docs/Dev/05_ProgressionAndEvents.md)(경험치/레벨업 + GameEvents 진행 채널, M1-6) 등 기술 설계 문서

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

**현재 상태**: 기획 완료(`Docs/Designs/` 세트). Unity 프로젝트·에셋 셋업 완료. **Backlog** → [Docs/Dev/backlog.md](Docs/Dev/backlog.md) (M0~M5). **M0 부트스트랩 전부 완료** + **M1-1(게임 상태 골격) 완료** — 어셈블리 골격·씬 3개·입력(New Input System)·리액티브 스택(R3/R3.Unity/UniTask) 확정, `VD.Core`에 상태머신(`GameManager` 싱글톤)+이벤트 채널(`GameEvents`) 안착. **M1-2(플레이어 이동) 완료** — 상대 드래그·물리 이동·뱅킹·고정 카메라 리그·`Player` 프리팹. **M1-3(오토 사격) 완료** — 조준(`PlayerAim`/FirePoint)+발사(`PlayerShooter`·`Projectile`)+상속형 풀(`PooledObjectPool<T>`←`ProjectilePool`). 데미지/히트는 M1-4에서 완료. **M1-4(적 기본 엔티티 & 스폰) DoD 충족** — 폴리싱만 잔존. **M1-5(오브 드랍·자석·습득) 완료** — 적 실사망 드랍→전방 드리프트(못 만나면 지나침)→반경 내 캡처·가속 끌림→거리 기반 습득(현재 `[TEMP]` 로그, 경험치 이벤트는 M1-6). `Orb`/`OrbPool`(VD.Core), 비주얼=Crystal effect green. **M1-6(경험치/레벨업) 완료** — `GameEvents` 확장(진행 채널: `OrbCollected`/`Level`/`XpNormalized`/`LevelUp`)+`ExperienceSystem`(지수형 임계값), 오브 습득→경험치 이벤트 발행. **M1-9(HP/게임오버/점수) 완료** — HP0→`GameManager.GameOver()`(정지형 `timeScale 0`), `ScoreSystem`(생존시간+처치점수), `GameEvents`에 `EnemyKilled`/`HpNormalized`/`Score`/`SurvivalTime` 추가. **다음 = M1-10(HUD, `Hp`·`Level`·`XpNormalized`·`Score`·`SurvivalTime` 바인딩)/M1-8(강화풀)/M1-7(3choice) 등.**

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

**➡️ 다음 작업 (새 세션 인계): M1-4 폴리싱 마무리 후 M1-5 — M1-4는 `[~]` DoD 충족(1·2·3단계 완료), 폴리싱만 잔존** — §1-9에 따라 진행. **적 프리팹 소스 = `FREE Low Poly Spaceships`**(spaceship_1~7). **M1-4 첫 적 = spaceship_6 1종**(나머지 볼륨업은 M3-3, 아키타입 잠정 매핑은 backlog M3-3). **진행 = 단계 분할.** ✅ **1단계(스폰+직진 접근 이동)** — `Enemy`/`EnemyPool`(상속형)/`EnemySpawner`(랜덤 위치 스폰), `Enemy.prefab`(spaceship_6, 임시 스케일 6). ✅ **2단계(적 피격→HP 감소→사망 + 타겟 스냅)** — `VD.Core.IDamageable`(최소 TakeDamage), `Enemy` 구현(maxHp 30), 투사체 히트=**트리거 콜라이더**(Projectile kinematic RB+트리거), 데미지 튜닝=`PlayerShooter.projectileDamage`. **원뿔 타겟 스냅**(매 발 nearest-in-cone, 락 아님) + 원뿔 기즈모. ✅ **3단계(플레이어 충돌 데미지 + 레이어 분리)** — `PlayerHealth`(Player root, maxHp 100, IDamageable 미구현=아군오사 방지, 적 접촉→HP 감소), `Enemy.contactDamage`. **레이어**: Player(8)/Enemy(9)/PlayerBullet(10) + 물리 매트릭스(Player×Enemy·Enemy×PlayerBullet만 ON), `PlayerShooter.targetMask`=Enemy. **⇒ M1-4 DoD 충족**(스폰·사격파괴·충돌 HP감소). **⚠️ 임시요소**: `[TEMP]` 로그·조준 원뿔 기즈모 잔존 — 원뿔 튜닝 후 정리 예정. 스폰 거리/폭·카메라(잠정 −36)는 사용자 튜닝. **적 속도 가변화 = 볼륨업(M3) 이관**. **▶ 잔존 = 원뿔 튜닝·임시요소 정리**(게임오버/HP UI = M1-9/M1-10). **🆕 새 세션 시작점**: (a) 원뿔 각도(`aimConeHalfAngle`)·사거리(`aimRange`) 튜닝 마무리 → 확정되면 **`[TEMP]` 로그**(Projectile/Enemy/PlayerHealth)·**원뿔 기즈모**(`PlayerShooter.drawAimGizmo`) 제거, 또는 (b) 다음 백로그. **M1-5(오브)·M1-6(경험치/레벨업)·M1-9(HP/게임오버/점수) 완료 → 다음은 M1-10(HUD) / M1-8(강화풀) / M1-7(3choice)** — M1-10은 `GameEvents`의 `HpNormalized`/`Level`/`XpNormalized`/`Score`/`SurvivalTime` 바인딩(uGUI), M1-7은 `LevelUp` 구독→일시정지 팝업(M1-8 풀·M1-10 캔버스 필요). `ExperienceSystem`/`PlayerHealth`/`ScoreSystem`의 `[TEMP]` 로그는 각각 M1-7·결과화면 붙을 때 대체. **작업 전 [backlog.md](Docs/Dev/backlog.md) M1-3~M1-9 상세 + 기술문서 [04_ObjectPooling](Docs/Dev/04_ObjectPooling.md)/[05_ProgressionAndEvents](Docs/Dev/05_ProgressionAndEvents.md) 확인.** GameScene 활성, 레이어/물리 매트릭스 설정됨(에디터 재부팅 시 매트릭스 재적용 주의).
- **입력**: **New Input System API로만** 읽는다(레거시 `Input.*` 금지, `activeInputHandler:1`). `VD.Runtime`이 `Unity.InputSystem` 참조 완료. 이동은 `Pointer.current` 델타 직접 읽기(상대 드래그). **액션 에셋 미사용** — Unity6 기본 템플릿 `InputSystem_Actions.inputactions`는 M1-2에서 **삭제**(에셋 + `EditorBuildSettings` 전역 참조 해제). 이후 액션 에셋이 필요해지면 그때 도입.
- **코드 위치·규칙**: 런타임 = `VD.Runtime`(ns `VD.Core`/`VD.Player`/`VD.Enemy`/`VD.UI`), 에디터 = `VD.Editor`. 총알은 Player/Enemy 내부. 파일/네임스페이스/struct 규칙은 [Docs/Dev/01_AssemblyDefinition.md](Docs/Dev/01_AssemblyDefinition.md) + backlog M0-4.
- **리액티브·비동기**: R3(`ReactiveProperty`/`Subject`) + R3.Unity(`AddTo(this)` 수명 연동) 사용 가능. 비동기는 UniTask.

> ⚠️ **M1 인계 주의**
> - **GameScene**: `GameManager`(+임시 `GameDebugDriver`)·`Player`(프리팹 인스턴스)·`ProjectilePool`(투사체 풀, prewarm 32)·`EnemySpawner`(+EnemyPool, prewarm 16, M1-4)·`OrbPool`(오브 풀, prewarm 16, M1-5)·`ExperienceSystem`(경험치/레벨, M1-6)·`ScoreSystem`(생존시간/점수, M1-9)·Main Camera(고정 Perspective, z **−36 잠정** — M1-4서 프레이밍 조정중, 원래 −26)·Directional Light 배치됨. TitleScene/ResultScene은 아직 빈 상태. 현재 에디터 활성 씬 = GameScene.
> - **레이어(M1-4 신설, 물리 매트릭스)**: `Player`(8)·`Enemy`(9)·`PlayerBullet`(10). 매트릭스 = **Player×Enemy·Enemy×PlayerBullet만 ON**(총알↔플레이어=자살 OFF, 동종 OFF). 프리팹(Player/Enemy/Projectile)·씬 Player에 할당됨. 적 탄환용 `EnemyBullet`은 M3-2에서 추가 예정. 재부팅 후 매트릭스 풀리면 재적용.
> - **태그(M1-5)**: Player 프리팹에 builtin 태그 `Player` 부여 — `OrbPool`이 자석 타깃(플레이어) 탐색에 사용(`FindGameObjectWithTag`). Core→Player 타입 결합 회피용.
> - **마커**: `VDRuntimeMarker`는 M1-1에서 **삭제**(실코드가 참조 검증). `VDEditorMarker`(`Editor/`)만 **유지** — M2 에디터 툴 실코드 전까지 VD.Editor 검증용, 그때 삭제.
> - **임시 `GameDebugDriver`**(`Core/`): 키보드 상태 전이 검증용(P/G/R). M1-2(입력)·M1-9(게임오버)에서 실코드로 대체 시 삭제.
> - `SampleScene`/`SmokeCube`는 M0-2 테스트 잔재(빌드 제외). M1과 무관.
> - **Unity 재부팅 시 루틴**: Unity 서버 Start → Claude에서 MCP 재연결(또는 세션 재시작). 창의 "Client Configure: Not Configured"는 무시(우리는 `.mcp.json` 수동 등록 사용).

**⚠️ 설치 상태 실측(2026-08-17)**: UniTask ✅ / R3 코어 1.3.1 ✅(NuGet) / R3.Unity 통합 ✅설치(`com.cysharp.r3` 1.3.1) / Addressables ❌미설치(M2에서) / MCP ✅설치완료 / Input System ✅New 단독(`com.unity.inputsystem` 1.20.0, handler 1). 상세는 backlog §0.

**Backlog 유지 원칙**: [scope-tiering.md](Docs/Designs/scope-tiering.md)는 티어 수준, backlog는 구현 태스크 단위(DoD 포함). **갱신은 §1-8에 따라 사용자 확인 후.**

## 5. 유지보수

- 규칙·약속 변경 → §1 갱신
- 새 기획 문서 추가 → onepage §0 인덱스 등록 + 필요 시 §2 갱신 / 새 개발·이슈 문서 → `Docs/Dev/`
- 핵심 사실 변경 → §3 갱신
- 진행 상태 변경(단계 완료 등) → §4 갱신
