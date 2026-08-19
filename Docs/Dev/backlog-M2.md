# Backlog — M2 · 에디터 커스텀 툴 (핵심 어필)
> 상위 허브: [backlog.md](backlog.md) | 인접: [backlog-M1.md](backlog-M1.md) ← **M2** → [backlog-M3.md](backlog-M3.md)

## ⚡ 특이사항 (이 헤더만 읽어도 크로스 마일스톤 파악)
- **상태**: 🟡 **진행중** — M2-1·M2-2·M2-3·**M2-4 완료**(`[x]`, a~e). **다음 = M2-5**(툴 데이터→런타임 스폰; 의존 M3-1·M3-2 AI 모듈). **포폴 공고 1순위 어필** = UI Toolkit 에디터 커스텀 툴. (M2-2~2-4 통합 기술문서 `06`은 **미작성 폐기** — 내용이 이 backlog에 상세 누적돼 별도 로그 불필요, 정식 기술문서는 후속 정리. 2026-08-20 결정.)
- **범위(Must)**: enemy-design 3층 중 **1층(유효성 경고) + SO DB + Addressables + 스폰 연결**까지. 2·3층 심화(아키타입 프로파일·스폰 타임라인) → **M4-6**.
- **전제(이전 M에서 옴)**:
  - M0-4 asmdef `VD.Editor`(에디터 전용, Runtime 참조) 존재. **`Data` 폴더·ScriptableObject는 여기(M2)서 신설.**
  - **M1-4 하드코딩 `EnemySpawner` → M2-5가 SO DB+Addressables 로드 스폰으로 교체.**
  - 적 프리팹 소스 = `FREE Low Poly Spaceships`(spaceship_1~7). 아키타입 잠정 매핑은 [backlog-M3.md](backlog-M3.md) M3-3 표.
  - **Addressables 미설치** → M2-1에서 설치. R3/UniTask는 설치됨(비동기 로드=UniTask).
- **이후 M이 여기서 확인할 것**:
  - **M2-2 SO enum(이동AI 4·공격AI 4·아키타입 3)** 이 **M3-1(이동AI)·M3-2(공격AI)·M3-3(아키타입)** 실구현과 **정합**해야. enum 값 = enemy-design §2/§3/§7 기준.
  - **M2-5 스폰 연결은 M3-1·M3-2 AI 모듈에도 의존**(SO가 지정한 AI가 실제로 돌아야 완성). M1-4 상태에선 이동만 → M3 이후 완전 동작.
  - 적 스탯(체력/속도/데미지/드랍오브량/처치점수)이 **M1의 하드코딩값(`Enemy.killScore`/`contactDamage`·`Orb.xpValue`)을 데이터로 대체**.
- **핵심 방침/주의**:
  - ⚠️ **이 툴은 적 오서링 전용** — **3choice 업그레이드 풀과 섞지 않음**(관심사 분리, context §1-5).
  - 유효성 경고 = **차단 아님, 경고만**(모순 조합도 저장 허용).
  - 런타임 UI=uGUI지만 **에디터 툴=UI Toolkit**(`.uxml`/`.uss`).
  - 진행은 **세부 분할**(예: M2-1 → a 설치 / b 그룹·등록 / c 라벨 / d 로드 스모크). 구현 착수는 사용자 확인 후.
- **문서**: [enemy-design.md](../Designs/enemy-design.md) §2~§7.

---

### M2-1 · Addressables 설치 & Enemy Group/Label 구성 🔴 `[x]` (2026-08-19)
- **목적**: enemy-design §6. 적 프리팹을 Addressables로 관리 + 라벨로 거친 분류.
- **작업**: Addressables 패키지 설치, `Enemy` Group 생성, 임포트한 우주선 에셋으로 적 프리팹 후보 등록, 라벨 `archetype:탄막/돌진/자폭/복합`·`range:원거리/근거리/복합` 부여.
- **DoD**: Enemy Group에 프리팹 N개 등록·라벨링, 라벨 기준 로드 스모크 테스트 1회 성공(UniTask로 비동기 로드). ✅ **충족**.
- **의존**: M0-4
- **문서**: enemy-design.md §6
- **✅ 완료(2026-08-19, 세부 분할 a~d)**:
  - **a** `com.unity.addressables` **4.0.1** 설치(+의존성, 컴파일 0).
  - **b** spaceship_1~7을 **비주얼 프리팹으로 복제**(`AssetDatabase.CopyAsset`, 새 GUID) → `Assets/Prefabs/Enemies/`. 이름 = 아키타입 기준(`Enemy_Heavy_1`/`Enemy_Barrage_2·3·7`/`Enemy_Charger_4·6`/`Enemy_Bomber_5`). **이름은 식별 편의일 뿐 역할 고정 아님**(모델↔AI 분리, enemy-design §2). Imports 원본과 링크 끊김(수정 안전).
  - **c** `Enemy` Addressable 그룹 생성, 7개 등록(주소=프리팹명), **archetype 멀티라벨**(모델별 적합 집합, 사용자 지정) + **파생 range 라벨**(탄막→원거리·돌진/자폭→근거리·복합→복합) 부여. 멀티라벨·범위는 §2/§6 정합.
    - Heavy_1=[탄막,돌진,복합] · Barrage_2·3=[탄막,돌진] · Charger_4·6=[돌진,자폭] · Bomber_5=[탄막,돌진,자폭] · Barrage_7=[탄막,자폭].
  - **d** Play 모드에서 `[RuntimeInitializeOnLoadMethod]` + **UniTask 비동기 로드** — `archetype:탄막` 라벨로 **5개**(Heavy_1·Barrage_2·3·7·Bomber_5) 로드 확인. (에디트 모드는 콜백 미pump라 Play로 검증.)
  - 임시 셋업/스모크 스크립트는 실행 후 삭제(육안 로그만 남김).
  - **⚠️ 인계**: M1-4 `Enemy.prefab`은 아직 Imports `spaceship_6` 중첩 → **M2-5에서 `Enemy_Charger_6`으로 재배선**. VD.Editor asmdef는 아직 Addressables 미참조(M2-3 툴에서 참조 추가 예정).

### M2-2 · 적 데이터 SO 스키마 (조합 원천 데이터) 🔴 `[x]` (2026-08-19, a~d 완료)
- **목적**: enemy-design §2·§6. 에디터가 편집하는 디자인 원천 데이터 정의.
- **작업**: ScriptableObject 스키마 — AssetReference(비주얼) + 이동AI 종류 + 공격AI 종류 + 스탯(체력/속도/데미지/처치점수 + 공격AI별) + 아키타입 + **드랍 오브 종류(OrbDefinition 참조, 오브 1개 고정)**. enum(이동AI 4·공격AI 4·**아키타입 4(복합 포함)**) 정의.
- **DoD**: SO 인스턴스를 인스펙터로 만들 수 있고, 필드가 enemy-design §2/§3/§7과 일치.
- **의존**: M2-1
- **문서**: enemy-design.md §2·§3·§7
- **✅ 결정(2026-08-19, 사용자)**:
  - **#1 enum 위치** = `Assets/Scripts/Core/Enum/`, **1파일 1enum**, ns `VD.Core`. 기존 인라인 enum 유지.
  - **#2 SO 위치** = **스크립트(.cs)는 VD.Runtime 안**(`Assets/Scripts/Data/`), **에셋(.asset)은 `Assets/ScriptableObjects/Data/`**. VD.Runtime에 **Addressables 참조 추가**(AssetReference용). 이유: `Assets/ScriptableObjects`는 asmdef 밖(기본 어셈블리)이라 스크립트를 두면 VD.Runtime 스포너가 참조 불가·순환.
  - **#3 스탯 = `struct`로 묶기**(EditorTool 제공 전제).
  - **#4 Archetype = SO에 명시 필드**(자동 유추 아님 — 복합 유추 애매·라벨/range/유효성 기준점). 데이터 커플링은 파생 **단일 소스**로 관리.
  - **#5 유효성 메타(공격AI 요구거리·이동AI 거리성향) = M2-4로 미룸.** M2-2 enum은 값만.
  - **#6 M2-2 = 데이터 필드만.** 실제 이동/공격 AI 로직은 M3-1/M3-2(스포너가 SO 읽어 AI 모듈에 주입).
- **세부 분할 (토큰 관리, 순차)**:
  - **✅ M2-2a — enum 3개(2026-08-19)**: `Core/Enum/`에 `MoveAIType`(Straight/Chase/Weave/Hover=직진/추적/사행/견제) · `AttackAIType`(Contact/AimedShot/Barrage/Suicide=충돌/조준단발/탄막/자폭) · `Archetype`(**Shooter**/Charger/Bomber/Hybrid=사격/돌진/자폭/복합). 컴파일 0. (range는 별도 enum 아님 — archetype에서 파생 예정.) **⚠️ Shooter는 원래 `Barrage`였으나 M2-3에서 `AttackAIType.Barrage`(탄막 공격)와 코드 이름 충돌 회피로 개명(2026-08-19). 인덱스 0 유지 → 에셋 무변경.**
  - **✅ M2-2b — 스탯 struct(2026-08-19)**: `Assets/Scripts/Data/EnemyStats.cs`(ns `VD.Core`, `[Serializable] struct`, VD.Runtime). **단일 struct 8필드** — 공통(`maxHp`/`moveSpeed`/`damage`/`killScore`) + 공격AI별(`fireInterval`/`projectileSpeed`/`barrageCount`/`suicideRadius`). 컴파일 0. **결정 반영**:
    - **"드랍오브량" 폐기**(문서 §7 표현, 사용자 미채택). 적은 **오브 1개 고정 드랍** — 스탯 아님. "**어떤 오브를 떨구나**"는 참조 성격이라 스탯 struct 밖, **SO 본체(M2-2c)** 필드로. xp 값은 **오브 쪽에 종류별** 정의(현재 green default, 배선은 이후). **추천 = OrbDefinition SO 3개**(오브별 xp+비주얼), EnemyDefinition이 참조 — M2-2c에서 확정.
    - **`damage` 1필드** — 전달 방식은 공격AI가 해석(충돌/돌진→접촉, 조준단발/탄막→탄별, 자폭→범위). **적 탄환 데미지 실배선(EnemyBullet→PlayerHealth)은 M3-2**로 이관.
    - **`moveSpeed` = 적별 상이** → `EnemySpawner.enemySpeed`(전 적 동일 주입) 폐기 대상, M2-5서 SO값 읽어 주입.
    - M1 하드코딩(`Enemy.maxHp`/`contactDamage`/`killScore`·`EnemySpawner.enemySpeed`)이 흡수 대상(실제 대체는 M2-5). 수치는 전부 0(Day5).
  - **✅ M2-2c — SO 클래스 2종(2026-08-19)**: `Assets/Scripts/Data/`(VD.Runtime, ns `VD.Core`).
    - `EnemyDefinition : ScriptableObject` — `visual`(`AssetReferenceGameObject`)+`moveAI`+`attackAI`+`archetype`+`stats`(`EnemyStats`)+`dropOrb`(`OrbDefinition`), `[CreateAssetMenu(Void Drift/Enemy Definition)]`. `RangeLabelOf(archetype)` **파생 헬퍼**(단일 소스, 결정 #4) — 문자열 라벨(`원거리/근거리/복합`, Addressables `range:` 라벨과 동일; enum 신설 안 함=§주석 "range 별도 enum 아님" 준수). `VD.Runtime.asmdef`에 `Unity.Addressables`/`Unity.ResourceManager` 참조 추가.
    - `OrbDefinition : ScriptableObject` — **드랍 오브 종류**. `xpValue`(int, green baseline 1)+`visual`(**GameObject 크리스탈**), `[CreateAssetMenu(Void Drift/Orb Definition)]`. **결정 (a) 채택**: 오브 동작(자석/습득)은 **공유 `Orb` 하나**, 종류별로 다른 건 **비주얼+xp**뿐 → 드랍 시 공유 Orb에 비주얼 주입(런타임 배선은 이후). **발견**: 사용자 분리 크리스탈 3종(green/blue/red)은 **ParticleSystem 비주얼 전용**(Orb 컴포넌트 없음), Orb 동작은 `Orb.prefab`에만 존재 → 그래서 (a). (b=Orb 프리팹 3종은 미채택.)
    - ⚠️ **런타임 배선 미완**: 오브가 `OrbDefinition.xpValue`로 발행 / 드랍 측 `dropOrb.visual` 주입 / 스포너가 `EnemyStats`·`visual` 로드 스폰은 **이후(M2-5·드랍 데이터화)**. 현 `Orb.xpValue`·`EnemySpawner.enemySpeed`·`Enemy.maxHp/contactDamage/killScore`·단일 OrbPool 유지.
  - **✅ M2-2d — 검증(2026-08-19)**: `Assets/ScriptableObjects/Data/`(결정 #2)에 `Orb_Green`(xp1·green 비주얼) + `Enemy_Sample_Barrage`(탄막/Barrage·Chase, visual→`Enemy_Barrage_2`) + `Enemy_Sample_Charger`(돌진/Charger·Contact, visual→`Enemy_Charger_6`) 생성. 직렬화 확인: enum·`stats` 8필드·**`visual.m_AssetGUID`가 Enemy_* Addressable에 연결**·`dropOrb`→`Orb_Green` 정상, §2/§3/§7 일치, 컴파일·콘솔 에러 0. **⇒ M2-2 DoD 충족.**

### M2-3 · UI Toolkit 오서링 창 — 조합 테이블 🟢 `[x]` (2026-08-19 완료)
- **✅ 완료(2026-08-19, a~g)**: `Assets/Scripts/Editor/Authoring/`에 — **재사용 베이스** `SoTableEditorView<T>`(VisualElement; `MultiColumnListView` 목록 + `PropertyField` 상세 + New/Delete/Reload + 선택전환·창닫힘 시 `SaveAssetIfDirty` 저장 + Name(에셋명) 편집 + 폴드아웃 펼침 + `CustomizeDetail` 훅), **`EnemyTableEditorView`**(EnemyDefinition; 컬럼 archetype·moveAI·attackAI·RangeLabel), **`EnemyAuthoringWindow`**(메뉴 `Window/Void Drift/Enemy Authoring`), **`SoTableEditor.uss`**. **사용자 피드백 반영**: ① 스탯 폴드아웃 기본 펼침 ② Name 최상단 편집 가능(`RenameAsset`) ③ 선택 AttackAI에 안 쓰이는 stats 필드 **비활성(그레이아웃, 실시간)** — Contact=없음/AimedShot=발사간격·탄속/Barrage=+탄막수/Suicide=자폭반경. **검증(창 실측)**: 컬럼 5개·샘플 2개 로드, Name 편집, ③ Barrage→suicideRadius만 비활성·Contact→4필드 비활성, New→편집→디스크 저장→삭제 왕복, 컴파일·콘솔 에러 0. **⇒ DoD 충족.** (`Archetype.Barrage→Shooter` 개명도 이 단계에서 처리 — 위 M2-2a 참조.)
- **목적**: enemy-design §5 1층 토대. 적 조합 목록을 한 창에서 편집.
- **작업**: `EditorWindow`(UI Toolkit) — **재사용 베이스**(제네릭 SO 테이블 편집기) 위에 Enemy 에디터. 목록 = **`MultiColumnListView`(진짜 테이블)**, 상세 편집 = **`PropertyField` 손수 배치**(필드 = 비주얼×이동AI×공격AI×스탯×dropOrb). `.uxml`/`.uss` 레이아웃.
- **DoD**: 창에서 적 조합을 신규 생성/수정/저장(SO에 반영)까지 왕복.
- **의존**: M2-2
- **세부 분할 (a~g, 순차 · 토큰 관리)**:
  - **a** 에디터 asmdef 준비 — `VD.Editor.asmdef` 참조 보강(AssetReference용 `Unity.Addressables`·`Unity.Addressables.Editor`), `VDEditorMarker` 삭제(실코드 진입, 01 문서 방침). DoD 컴파일 0.
  - **b** ⭐ **재사용 베이스** — 제네릭 SO 테이블 편집기 골격(`MultiColumnListView` 목록 + 상세 `PropertyField` 패널 + 신규/삭제/저장 훅), 특정 타입 비의존. **확장성의 실체.**
  - **c** 창 골격(Enemy) — `EnemyAuthoringWindow : EditorWindow` + `[MenuItem]`, 베이스에 `EnemyDefinition` 바인딩, UXML/USS 로드. DoD 메뉴로 창 뜸.
  - **d** 목록(읽기) — 모든 `EnemyDefinition` 수집(AssetDatabase) → 컬럼(이름·archetype·moveAI·attackAI·`RangeLabel`). DoD 샘플 2개 표시.
  - **e** 상세 편집+저장 — 행 선택→`PropertyField` 편집(비주얼/AI/스탯/dropOrb), dirty+SaveAssets. DoD 수정→에셋 반영.
  - **f** 신규/삭제 — 창에서 `EnemyDefinition` 생성(명명)·삭제.
  - **g** 레이아웃/UX + 왕복 검증 → **M2-3 DoD 충족**.
- **설계 결정·근거 (2026-08-19, 사용자 — backlog에 상세 반영됨, 06 미작성)**:
  - **편집 UI = `PropertyField` 직행**(단계적 (i)InspectorElement 생략): 스키마(M2-2) 완성돼 애매함 없음, 뒤 단계(M2-4/2-5/M3)는 데이터를 **소비**만 → 가볍게 먼저 갈 이유 없음(버릴 코드 회피).
  - **목록 = `MultiColumnListView`**: §5 "조합 테이블"의 정공법(컬럼·정렬).
  - **확장성 = "한 창에 다 몰기"가 아니라, 재사용 베이스로 도메인별 에디터(적/플레이어/…)를 별창 또는 탭으로 저렴하게 찍어내기.** 관심사 분리(§1-5) 유지. **동기(사용자)**: 오브젝트를 일일이 찾아다니며 데이터 변경하던 고생 제거.
  - **별창 vs 탭 허브 = 2번째 에디터 만들 때 결정**(베이스가 둘 다 싸게 함).
  - **통합 튜닝 창(비전) = 별도 후속** — orb xp·player state·필요 경험치 등 스칼라(단, **물리/사거리 원뿔 반경 등 구현·물리용 값은 제외**)를 한 창에서 일괄 편집. **전제: 코드에 박힌 스칼라(`ExperienceSystem` 임계값·`PlayerHealth` HP 등)를 먼저 SO로 데이터화** → 그래서 M2-3에 못 얹는 별건. orb xp는 이미 `OrbDefinition.xpValue`라 즉시 물림. (Day5 튜닝/M4쯤.)
  - **Archetype.Barrage → `Shooter`(사격형) 개명 (2026-08-19, 사용자)**: `AttackAIType.Barrage`(탄막 공격)와 코드 이름 충돌 회피. 에셋은 int 인덱스라 데이터 무변경(인덱스 0 유지, `RangeLabelOf(Shooter)="원거리"`). ⚠️ **미정합 잔존(개념/라벨층)**: enemy-design.md 개념어 '탄막형' + Addressables `archetype:탄막` 라벨 + 프리팹명 `Enemy_Barrage_*`는 아직 '탄막' — 이 층까지 '사격'으로 맞출지는 후속 결정(라벨 5개 재부여 수반).
- **문서**: enemy-design.md §5. **▶ 통합 기술문서(`06_EnemyAuthoringTool.md`) = 미작성 폐기** — 한 번 작성했으나 backlog 진행로그 톤이라, 내용이 이 backlog-M2에 이미 상세 누적된 것으로 대체(2026-08-20 결정, 06 삭제). 정식 기술문서(아키텍처·데이터·API 중심)는 후속에 별도 정리.

### M2-4 · 유효성 경고 (교전거리 모순) 🟢 `[x]` (2026-08-20 완료, a~e)
- **✅ 완료(a~e)**: R1·R2 순수 로직 = `VD.Core.EnemyValidation.Validate(EnemyDefinition)`→`List<EnemyWarning>`(메시지+하이라이트 필드; 메타 `TendencyOf`/`RangeOf`). R3(라벨 교차) = `VD.Editor.EnemyTableEditorView.AppendLabelWarning`(비주얼 Addressables `archetype:` 라벨 조회 = **에디터 전용 API라 Editor층 불가피**; `ArchetypeLabel` 딕셔너리로 기대 라벨 ↔ 엔트리 라벨 집합 대조). 표시 = 상세 상단 경고박스(`RenderWarningBox`) + 모순 필드 red 테두리(`ApplyFieldHighlights`, `.so-field-error`) + 목록 행 ⚠(`AddWarnColumn`+`RefreshRows`), moveAI/attackAI/archetype/visual 변경 시 실시간(`RefreshValidation`). **e 검증(창 실측, 사용자 육안)**: R1(Suicide+Hover)·R2(Charger+Barrage·Shooter+Contact)·R3(라벨 밖 archetype) 경고 표시 / 유효 조합 깨끗 / 경고 있어도 저장 계속(비차단) / 유효 복귀 시 즉시 해제 / 컴파일·콘솔 에러 0. **⇒ DoD 충족.** (R3 라벨 사실: `Enemy_Charger_6`={돌진,자폭}·`Enemy_Barrage_2`={돌진,탄막}.)
- **목적**: enemy-design §4·§6. 툴의 격상 포인트(단순 테이블 → 검증 툴). 포폴 핵심 서사.
- **작업**: 규칙 판정 → 경고 표시(행/필드 하이라이트 + 메시지). **차단 아님, 경고만**(모순 조합도 저장 허용).
- **DoD**: 모순 조합 만들면 창에 경고가 뜨고, 유효 조합은 깨끗. 저장은 안 막음.
- **의존**: M2-3
- **판정 규칙 (근거 §3 성향/§4 예시/우리 enum)**:
  - 메타: MoveAI 거리성향(`Straight/Chase/Weave`=접근, `Hover`=거리유지) · AttackAI 요구거리(`Contact/Suicide`=근접필수, `AimedShot`=무관, `Barrage`=원거리선호) · archetype→range(`RangeLabelOf`: Shooter=원거리, Charger/Bomber=근거리, Hybrid=복합).
  - **R1 (AttackAI↔MoveAI)**: 근접필수(Contact/Suicide)+거리유지(Hover) → ⚠️ (§4: 자폭/충돌+견제).
  - **R2 (archetype range↔AttackAI)**: 근거리형(Charger/Bomber)+Barrage → ⚠️ / 원거리형(Shooter)+근접(Contact/Suicide) → ⚠️. Hybrid·AimedShot 통과.
  - **R3 (§6 비주얼 라벨 교차)**: 비주얼 프리팹 Addressables `archetype:` 멀티라벨 집합 밖 archetype → ⚠️ "부자연스러운 조합".
- **세부 분할 (a~e, 순차)**:
  - **a** 유효성 코어(순수 로직) — 메타 + `EnemyValidation.Validate(EnemyDefinition)`→경고 리스트(메시지+관련 필드). **R1·R2**. 위치=**VD.Core**(`RangeLabelOf` 옆). DoD 유닛 판정.
  - **b** 경고 표시 — 상세 상단 경고 박스, moveAI/attackAI/archetype 변경 시 실시간 갱신.
  - **c** 하이라이트 — 모순 필드 red 테두리(USS) + 목록 행 ⚠ 표시.
  - **d** §6 라벨 교차검증(R3) — 비주얼 프리팹 Addressables `archetype:` 라벨 조회(**에디터 전용 API → VD.Editor**), 집합 밖이면 경고.
  - **e** 왕복+비차단 검증 — 모순 조합(Suicide+Hover, Charger+Barrage 등) 경고 확인, 유효는 깨끗, 저장 계속됨 → DoD.
- **결정(2026-08-19, 사용자)**: 코어=VD.Core(R1·R2; **R3는 Addressables 에디터 API라 VD.Editor 불가피**) · R3 포함 · UX=경고박스+필드 red+행 ⚠.
- **문서**: enemy-design.md §4·§6. (통합 기술문서 `06`은 미작성 폐기 — M2-3 문서란 참조.)

### M2-5 · 최소 스폰 연결 (툴 데이터 → 런타임 스폰) 🔴
- **목적**: scope-tiering Must "최소 스폰 연결". 툴로 만든 적이 실제로 게임에 등장.
- **작업**: 런타임 스포너가 SO DB(+Addressables 로드)에서 적을 읽어 스폰하도록 M1-4의 하드코딩 스포너 교체. 최소한 "SO 목록에서 랜덤/가중 스폰".
- **DoD**: 에디터 툴에서 만든 적 조합이 플레이 중 실제로 스폰·동작(비주얼+AI+스탯 반영).
- **의존**: M2-4, M3-1·M3-2(AI 모듈), M1-4
- **문서**: enemy-design.md

> **M2 완료 판정(게이트)**: "**툴로 오서링한 적이 실제 게임에 등장**하고, 모순 조합엔 경고가 뜬다." → 포폴 핵심 데모 성립.
