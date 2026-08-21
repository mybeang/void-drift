# Void Drift — 개발 Backlog (허브)

> 작업 전 [context.md](../../context.md) → [onepage-design.md](../Designs/onepage-design.md) 확인.
> 이 문서는 `scope-tiering.md`의 Must/Should/Nice를 **실제 구현 작업 단위**로 쪼갠 것.
> 마일스톤/티어 명세가 아니라 **"이 태스크를 하면 뭐가 산출되고, 언제 끝난 걸로 보는지(DoD)"** 를 담는다.
> 규칙: 결정/변경 생기면 즉시 갱신, 완료 시 상태 체크. 세부 수치는 대부분 Day5(밸런싱) 이관.

> 📂 **마일스톤 상세는 파일로 분리**(2026-08-19). 이 허브는 **개요·스냅샷·크로스컷·진행로그**만 담는다.
> 각 마일스톤 파일 **최상단 `⚡ 특이사항` 헤더**만 읽어도 크로스 마일스톤(이전/이후에서 할 것·확인할 것·방침)을 파악할 수 있게 유지한다. 특정 마일스톤 작업 시 **그 파일 하나 + 헤더**만 읽으면 충분.

## 문서 사용법 / 표기

- **티어**: 🔴 Must / 🟡 Should / 🟢 Nice (scope-tiering.md 기준)
- **상태**: `[ ]` 대기 · `[~]` 진행중 · `[x]` 완료 · `[!]` 막힘/확인필요
- **DoD** = Definition of Done (완료 판정 조건). 이게 충족돼야 `[x]`.
- 태스크 ID = `M{마일스톤}-{번호}`. 의존성은 ID로 표기.
- ⚠️ 코드 심볼(클래스/파일명): **미착수 마일스톤은 제안 이름**(실제 생성 시 바뀔 수 있음, 존재 주장 안 함). **완료 마일스톤은 실존 검증된 이름.**

---

## 0. 현재 상태 스냅샷 (2026-08-17 실측, 이후 갱신)

| 항목 | 상태 | 비고 |
|---|---|---|
| Unity 프로젝트 / URP 17.3.0 | ✅ | Unity 6000.3.13f1 |
| 3D 에셋 임포트 | ✅ | `Assets/Imports/`: FREE Low Poly Spaceships, StarSparrow(우주선), Planets of the Solar System 3D, JMO Assets(VFX 계열), Hovl Studio(Magic effects), SUIT 폰트 |
| Scripts | ✅ | `VD.Runtime`/`VD.Editor` asmdef, M1 코어 전체 구현됨 |
| Scene | Title/Game/Result(+SampleScene 빌드제외) | GameScene 활성 |
| **UniTask** | ✅ | git UPM (`com.cysharp.unitask`) |
| **R3 코어 1.3.1** | ✅ | NuGetForUnity (`Assets/Packages/R3.1.3.1`) |
| **R3.Unity 통합** | ✅ 설치 | `com.cysharp.r3` 1.3.1 (git UPM). M0-3에서 설치·검증 |
| **Addressables** | ❌ 미설치 | **M2-1에서 설치** |
| **Unity MCP 브리지** | ✅ 설치 | M0-1 완료(CoplayDev, HTTP 8080) |
| **Input System** | ✅ New 단독 | `com.unity.inputsystem` 1.20.0, `activeInputHandler:1`. M0-3에서 확정 |

---

## 마일스톤 개요

| MS | 이름 | 티어 | 상태 | 상세 파일 | 목표(한 줄) |
|---|---|---|---|---|---|
| **M0** | 부트스트랩 & 스모크 테스트 | 🔴 | ✅ 완료 | [backlog-M0.md](backlog-M0.md) | MCP 연결 + 큐브 Z고정 회전 검증 + 프로젝트 골격 |
| **M1** | 코어 루프 (플레이 가능한 최소 게임) | 🔴 | ✅ 완료 | [backlog-M1.md](backlog-M1.md) | 이동·오토사격·적·오브·레벨업·3choice·게임오버·HUD |
| **M2** | 에디터 커스텀 툴 (핵심 어필) | 🔴 | 🟢 **완료**(M2-1~2-5✅, 다음 M3) | [backlog-M2.md](backlog-M2.md) | SO DB + 적 조합 오서링 + 유효성 경고 + Addressables + 스폰 연결 |
| **M3** | 적 다양성 & 3choice 풀 (Must 완성) | 🔴 | 🔴 미착수 | [backlog-M3.md](backlog-M3.md) | 이동/공격 AI 모듈, 아키타입, 최소 강화 풀 |
| **M4** | 확장 (Should) | 🟡 | 🟡 미착수 | [backlog-M4.md](backlog-M4.md) | 무기 3종·레벨, 실드, 난이도 페이즈, 에디터 툴 2~3층, VFX, 하이스코어 |
| **M5** | 빌드 & 폴리싱 (Must 빌드 + Nice) | 🔴/🟢 | 🔴 미착수 | [backlog-M5.md](backlog-M5.md) | 모바일 가로 Android 빌드 + 데모영상 + (Nice)사운드/특수기능/Firebase |

> 빌드 순서: **M0 → M1 → M2 → M3** 까지가 Must 코어. 이후 M4 Should, M5는 모바일 빌드(Must)를 앞당겨 M3 직후에 1차 실행 권장(빌드 리스크 조기 발견).

---

## 크로스컷 / 미해결 (Day5 밸런싱 이관 수치 포함)

- **수치 미정(Day5)**: 이동 감도/데드존, 발사 간격, 적 스탯 전반, 레벨 임계값 곡선, 페이즈 길이/상승률/점프폭, 처치 점수값, 무기 레벨 수치. → 대부분 **에디터 툴/SO 데이터**로 관리(하드코딩 지양).
- **미해결 결정**: 실드 버튼 좌/우 옵션, 데미지 넘버 도입 여부, 월드스페이스 체력바 도입 여부(빨간 피격 연출로 대체 가능).
- **확인 필요**: 임포트 우주선 에셋 중 적/플레이어 배분. (~~Input System 활성 핸들러~~·~~R3.Unity 설치 여부~~ → M0-3에서 해소: New 단독 / 설치)
- **이슈 트래커**: [issues.md](issues.md) — I-1 이동 관성감(보류).

## 진행 로그

| 날짜 | 변경 |
|---|---|
| 2026-08-17 | Backlog 최초 작성 (M0~M5). 설치/에셋 상태 실측 반영. |
| 2026-08-17 | M0-1 진행: MCP 구현 = CoplayDev `com.coplaydev.unity-mcp` v10.1.2 확정, manifest.json 추가. Unity 서버 기동(127.0.0.1:8080). claude CLI 미검출로 자동설정 실패 → 프로젝트 `.mcp.json` 수동 등록(`UnityMCP`, http). |
| 2026-08-17 | M0-1 왕복 검증 완료(✅). M0-2 1차 시도 후 **재작업으로 정정** — 사용자 지시 없는 임의 구현+해석 오류(§1-7 위반). 재구현은 사용자 기준 지시 후. context.md §1에 구현 승인 규칙(§1-7·§1-8) 추가. |
| 2026-08-17 | M0-2 **재작업 완료(✅)** — 사용자 지시대로 물리(Rigidbody+angularVelocity) Z축 회전, 인스펙터(속도/크기/방향), SmokeCube 재사용. 사용자 육안 확인. context.md §1-9(기능단위 진행·사용자 주도 페이스) 추가. |
| 2026-08-17 | M0-3 **완료(✅)** — 사용자 결정: 입력 = **New Input System**(`com.unity.inputsystem` 1.20.0, `activeInputHandler:1` New 단독), R3.Unity = **설치**(`com.cysharp.r3` 1.3.1 git UPM). MCP로 설치·검증(컴파일 에러 0, 왕복 정상). 실측 결과 시작 상태는 레거시(handler 0)+Input System 미설치였고 `.inputactions`는 템플릿 잔재였음. |
| 2026-08-17 | M0-4 **완료(✅)** — asmdef 2개(`VD.Runtime`/`VD.Editor`, 네임스페이스 `VD.*`) + 폴더 골격(Core/Player/Enemy/UI/Editor) + 씬 3개(Title/Game/Result, build 0~2). 리플렉션 검증·컴파일 0. 기술 문서 [01_AssemblyDefinition.md](01_AssemblyDefinition.md) 신규 작성. 초안 `Combat`/`Progression` 폴더 폐기(총알=Player/Enemy 내부, 진행=Core). |
| 2026-08-17 | M1-1 **완료(✅)** — 사용자 결정: 전역=MonoBehaviour 싱글톤 `GameManager`, 이벤트=별도 `GameEvents` 채널. `VD.Core`에 `GameState`/`GameEvents`/`GameManager`/`GameDebugDriver`(임시) 생성, GameScene 배치·검증(Boot→Playing 로그, timeScale 0↔1, 컴파일 0). 기술 문서 [02_GameStateArchitecture.md](02_GameStateArchitecture.md) 신규. 정리: `VDRuntimeMarker` 삭제, `VDEditorMarker` 유지·재연결. |
| 2026-08-17 | M1-2 **완료(✅)** — `VD.Player.PlayerMovement`(이동 전담) + `Player` 프리팹(StarSparrow_1_LP_Red, root=Rigidbody+PlayerMovement / 자식 `Model`). 상대 드래그(`Pointer.current`)→속도 직접 매핑, 해상도 무관 `dragGain`(현재 5), 뱅킹=자식 `Model` 회전(물리 분리), 뷰포트 선-클램프, 고정 Perspective 카메라(0,0,-26/FOV55). 사용자 튜닝 반복. 기본 InputActions 템플릿 삭제. 기술 문서 [03_PlayerMovementAndCamera.md](03_PlayerMovementAndCamera.md) 신규. 이슈 트래커 [issues.md](issues.md) 신설(`I-1` 이동 관성감 보류). |
| 2026-08-18 | M1-3 **2단계 완료(진행중 `[~]`)** — 신규 `VD.Player.PlayerAim`(FirePoint 부착): 오프셋→pitch/yaw 즉시 정렬(`PlayerBanking` 동일 공식, roll 생략, 독립 필드 28/28), 임시 조준 축 기즈모 포함. 프리팹에 `FirePoint`(root 직속, localPos 0) 추가. 컴파일 0·사용자 육안 확인. 결정: **조준 원뿔 중심축 = 뱅킹 방향(`PlayerAim`)**, **원뿔 내 적 타겟 스냅(기관총·레일건)은 M1-4로 이관**(무타겟이면 축 직사), 유도탄은 별도 호밍(M4). 다음 = 3단계 발사 로직. |
| 2026-08-18 | **M1-4 3단계 완료 → DoD 충족(`[~]` 폴리싱만 잔존)** — 플레이어 충돌 데미지: 신규 `PlayerHealth`(Player root, maxHp 100, IDamageable 미구현=아군오사 방지, OnTriggerEnter로 적 접촉→HP 감소), `Enemy.contactDamage`(10). **레이어 분리**: Player(8)/Enemy(9)/PlayerBullet(10) + 물리 매트릭스(Player×Enemy·Enemy×PlayerBullet만 ON, 자살·동종 OFF), 프리팹·씬 할당, `PlayerShooter.targetMask`=Enemy. 사용자 확인. 게임오버=HP감소만(전이 M1-9). 잔존=원뿔 튜닝·`[TEMP]` 정리. |
| 2026-08-18 | **M1-4 2단계 완료(`[~]`)** — 적 피격·사망: `VD.Core.IDamageable`(최소 TakeDamage) 신설, `Enemy` 구현(maxHp 30, HP≤0 풀 반납). 투사체 히트 = **트리거 콜라이더**(Projectile에 kinematic RB+트리거, OnTriggerEnter). 데미지 튜닝=`PlayerShooter.projectileDamage`(10). **원뿔 타겟 스냅**(매 발 nearest-in-cone, `aimConeHalfAngle`/`aimRange`, 락 아님) + 조준 원뿔 기즈모. 사용자 육안+로그 확인. 남은 것 = 플레이어 충돌 데미지(M1-9). |
| 2026-08-18 | **M1-4 1단계 완료(`[~]`)** — 적 `Enemy`(-Z 직진 접근)·`EnemyPool`(상속형)·`EnemySpawner`(랜덤 위치 스폰). `Enemy.prefab`=spaceship_6(임시 스케일 6, root BoxCollider trigger). GameScene 배치. 사용자 확인. 스폰 거리/폭·카메라(−36)는 사용자 튜닝. 결정: **적 속도 가변화 = 볼륨업(M3) 이관**. 다음 = HP/피격/충돌 데미지/투사체 히트. |
| 2026-08-18 | **M5-8 신설(🟢 Nice)** — 스폰 패턴/포메이션(편대·웨이브 등 공간적 배치). 랜덤 스폰은 M1-4, 패턴화는 후순위. M4-6(시간축 프로파일)과 구분. 사용자 요청 등재. |
| 2026-08-18 | **M1-4 사전 결정** — 적 프리팹 소스 = `FREE Low Poly Spaceships`(spaceship_1~7). M1-4 첫 적 = **spaceship_6 1종**, 나머지 볼륨업은 M3-3. 진행 = **단계 분할**. 아키타입 잠정 매핑 기록 → M3-3. 구현은 착수 지시 후. |
| 2026-08-18 | **M1-8 완료(`[x]`)** — 최소 3choice 강화 풀. `UpgradeType` enum(이동/최대체력/자석범위) + `UpgradeSystem`(LevelUp 구독→`Roll(3)` Fisher–Yates→임시 자동적용), mutator 라우팅. `Roll()`/`Apply()` public(M1-7 재사용). 검증: dragGain5→5.6·maxHp100→120·자석+2, 에러 0. |
| 2026-08-18 | **M1-10 완료(`[x]`)** — HUD(uGUI+TMP). `VD.UI.HudView`가 `GameEvents` 구독→우상단 시간/점수/레벨/HP·경험치바. `VD.Runtime`에 UI/TMP 참조 추가, TMP 에센셜 임포트. 검증 실시간 갱신, 에러 0. |
| 2026-08-18 | **M1-9 완료(`[x]`)** — HP/게임오버/점수. 게임오버=GameScene 정지형(HP0→GameOver+timeScale0+결과값 보관), 점수=생존시간+처치점수. `GameEvents`에 `EnemyKilled`/`HpNormalized`/`Score`/`SurvivalTime` 추가, 신규 `ScoreSystem`. 검증 생존53.1s+처치180=233, 에러 0. 05 문서에 §종료/점수 추가. |
| 2026-08-18 | **M1-6 완료(`[x]`)** — 경험치/레벨업. `GameEvents` 확장(`OrbCollected`/`Level`/`XpNormalized`/`LevelUp`), 신규 `ExperienceSystem`(지수형 임계값 `base×growth^(n-1)`), `Orb`→`PublishOrbCollected`. 검증 5개→Lv2·다음 6.5, 에러 0. 기술 문서 [05_ProgressionAndEvents.md](05_ProgressionAndEvents.md) + [04_ObjectPooling.md](04_ObjectPooling.md) 신규. |
| 2026-08-18 | **M1-5 완료(`[x]`)** — 오브 드랍·자석·습득. 단계 분할: 실사망 드랍(`Enemy.SetDropHandler`/`EnemySpawner.DropOrb`) + `Orb`/`OrbPool : PooledObjectPool<Orb>`(Core); 자석(반경 밖 -Z 드리프트→못 만나면 지나침, 반경 내 캡처·가속 끌림, 타깃=태그"Player"); 거리 기반 습득→`[TEMP]` 로그. 비주얼=Crystal effect green. 경험치 이벤트만 M1-6 이관. 사용자 확인, 에러 0. |
| 2026-08-19 | **M1-4 마감(`[x]`)** — DoD(스폰·직진 접근·사격 파괴·충돌 HP감소·레이어 분리) 충족 상태로 마감. 잔존이던 원뿔 각도/사거리 = 사용자 튜닝(제외), `[TEMP]` 로그·기즈모 = 로그 유지 방침으로 정리 취소. |
| 2026-08-19 | **M1-7 완료(`[x]`)** — 3choice 강화 선택(일시정지 팝업). `VD.UI.LevelUpPopup`(`LevelUp`→`GameManager.Pause()`→`Roll(3)` 3카드→클릭 `Apply`→`Resume()`, 다중 레벨업 큐 순차) + `VD.Core.UpgradeDisplay`(struct) + `UpgradeSystem.Describe`(효과 수치 실제 필드 렌더). M1-8 임시 자동적용 제거. 한글 폰트=**SUIT SDF**. 검증(Play) 정상. **⇒ M1 코어루프 완성.** |
| 2026-08-19 | **문서 구조 개편** — backlog를 마일스톤별 파일(`backlog-M0..M5.md`)로 분리, 각 상단 `⚡ 특이사항` 헤더 추가(크로스 마일스톤 요약). 허브(이 파일)는 개요·스냅샷·크로스컷·진행로그만 유지. `GameDebugDriver`를 `#if UNITY_EDITOR` 가드로 유지 결정. |
| 2026-08-19 | **enemy-design refine** — 아키타입 **복합** 추가(3→4종), **아키타입이 range 자동결정**(탄막=원거리·돌진/자폭=근거리·복합=복합), 모델(비주얼)은 아키타입 고정 안 됨 → `archetype:` **멀티라벨**(적합 집합), 라벨 밖 선택 시 경고(차단X). §2·§6·§8 갱신. |
| 2026-08-19 | **M2-1 완료(`[x]`)** — Addressables. 세부 분할: (a) `com.unity.addressables` 4.0.1 설치; (b) spaceship_1~7 → `Assets/Prefabs/Enemies/`에 비주얼 프리팹 복제(새 GUID, 아키타입 기준 이름); (c) `Enemy` 그룹 등록 + archetype 멀티라벨(모델별 적합 집합, 사용자 지정)+파생 range 라벨; (d) Play 모드 UniTask 비동기 로드 스모크 = `archetype:탄막` 5개 로드 확인. 임시 스크립트 삭제, 에러 0. 다음 = M2-2. |
| 2026-08-19 | **M2-2a 완료(`[~]` M2-2 진행중)** — 적 SO용 enum 3개 `Core/Enum/`(ns VD.Core, 1파일1개): `MoveAIType`(직진/추적/사행/견제) · `AttackAIType`(충돌/조준단발/탄막/자폭) · `Archetype`(탄막/돌진/자폭/**복합**). 컴파일 0. 결정: enum=Core/Enum, SO스크립트=VD.Runtime `Data/`(+Addressables 참조)·에셋=`Assets/ScriptableObjects/Data/`, 스탯=struct, Archetype=명시필드, 유효성메타=M2-4, AI로직=M3. 다음 = M2-2b(스탯 struct). |
| 2026-08-19 | **M2-2 완료(`[x]`)** — b~d: `EnemyStats`(struct 8필드: 공통 maxHp/moveSpeed/damage/killScore + 공격AI별 fireInterval/projectileSpeed/barrageCount/suicideRadius) · `EnemyDefinition`(SO: visual `AssetReferenceGameObject`+moveAI+attackAI+archetype+stats+dropOrb, `RangeLabelOf` 파생) · `OrbDefinition`(SO: xpValue+visual, **오브 결정(a)** 동작 공유·비주얼/xp만 차등). 검증 인스턴스 `Orb_Green`·`Enemy_Sample_Barrage`·`Enemy_Sample_Charger`. `Archetype.Barrage→Shooter` 개명. 컴파일·에러 0. |
| 2026-08-19 | **M2-3 완료(`[x]`)** — ⭐UI Toolkit 오서링 창(공고 1순위). `Assets/Scripts/Editor/Authoring/`: 재사용 베이스 `SoTableEditorView<T>`(VisualElement — 목록 `MultiColumnListView`+상세 `PropertyField`+New/Delete/Reload+선택전환/창닫힘 저장+Name 편집+`CustomizeDetail` 훅) 위에 `EnemyTableEditorView`(컬럼 ⚠·archetype·moveAI·attackAI·Range + 공격AI별 stats 필드 실시간 비활성)+`EnemyAuthoringWindow`(메뉴 `Window/Void Drift/Enemy Authoring`)+`SoTableEditor.uss`. 확장성=도메인별 에디터를 베이스로 저렴하게(별창/탭은 2번째 때 결정). 왕복 검증·에러 0. |
| 2026-08-20 | **M2-4 완료(`[x]`, a~e)** — 유효성 경고(테이블→검증 툴 격상). R1·R2=`VD.Core.EnemyValidation.Validate`→`EnemyWarning` 리스트, R3(비주얼 `archetype:` 라벨 교차)=`VD.Editor`의 `AppendLabelWarning`(에디터 전용 Addressables API라 Editor층 불가피). 표시=경고박스+모순 필드 red(`.so-field-error`)+목록 행 ⚠, 실시간. **차단 아님(비차단 저장)**. e=창 실측(사용자 육안) 통과. 다음 = M2-5(툴 데이터→런타임 스폰). |
| 2026-08-20 | **M2-5 완료(`[x]`, a~f) ⇒ M2 완료** — 툴 데이터→런타임 스폰(**조립형/빌더**). 공통 로직 셸(`Enemy.prefab` 비주얼 분리) + 주입 조립: `EnemyBuilder`가 ①비주얼(`EnemyVisualCache` Addressables 로드→`AttachVisual`) ②effective 스탯(`StatScaler`×`DifficultyProvider` 배율 1.0 스텁→`ApplyStats`). `EnemySpawner` DB=`SpawnEntry[]`(def+weight) **가중 랜덤**+프리로드. 스탯 3층 분리(base RO/배율/effective). **스코프=비주얼+스탯만**(AI=M3, 드랍오브 데이터화·실배율=이후). Play 검증: 22체 3종 모델·가중 분포·SO별 스탯 반영·에러 0(사용자 육안). 기술문서 [06_EnemyPipeline.md](06_EnemyPipeline.md) 작성. 이슈 I-2(플레이어 Aim 어색) 등록(보류). 다음 = M3. |
| 2026-08-21 | **M4-10 완료(`[x]`) ⇒ M4 기능 구현 전부 완료** — 로컬 하이스코어 + 게임오버 결과 화면(프로젝트 첫 씬 전환). 저장=**교체 이음새** `IHighScoreStore`→로컬 `LocalObscureStore`(persistentDataPath `.vdsys.dat`, **AES-256+SHA256 무결성 해시**, 변조 시 0 폴백)→**DB 역할 SO** `HighScoreRepository`(`Best`/`LastScore` 인메모리 씬전달/`Commit`·`LastWasRecord`). **Firebase(M5-7)=store 교체.** `GameOverFlow`(GameScene)=GameOver→폭발 `CFXR Explosion 1`(×1.5, 프리즈 중 unscaled)+플레이어 `SetActive(false)`→1.5s→`Commit`→`SceneTransition`(**이클립스 와이프**=절차적 검은 원 좌→우, DontDestroyOnLoad·unscaled). ResultScene=Cam+EventSystem+Canvas(GAME OVER/신기록!/SCORE/BEST/다시하기·타이틀)+`ResultView`. Play 검증(사용자). 이슈 **I-5**(라인 통과 페이드아웃) 신설. 남은 후속=밸런싱 데이터 일괄+M4-8 검증. |
