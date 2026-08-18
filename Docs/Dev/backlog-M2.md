# Backlog — M2 · 에디터 커스텀 툴 (핵심 어필)
> 상위 허브: [backlog.md](backlog.md) | 인접: [backlog-M1.md](backlog-M1.md) ← **M2** → [backlog-M3.md](backlog-M3.md)

## ⚡ 특이사항 (이 헤더만 읽어도 크로스 마일스톤 파악)
- **상태**: 🟡 **진행중** — M2-1 완료(`[x]`), **M2-2 진행중(`[~]`, M2-2a enum 완료)**, 다음 = M2-2b. **포폴 공고 1순위 어필** = UI Toolkit 에디터 커스텀 툴.
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

### M2-2 · 적 데이터 SO 스키마 (조합 원천 데이터) 🔴 `[~]` (M2-2a 완료)
- **목적**: enemy-design §2·§6. 에디터가 편집하는 디자인 원천 데이터 정의.
- **작업**: ScriptableObject 스키마 — AssetReference(비주얼) + 이동AI 종류 + 공격AI 종류 + 스탯(체력/속도/데미지/드랍오브량/처치점수) + 아키타입. enum(이동AI 4·공격AI 4·**아키타입 4(복합 포함)**) 정의.
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
  - **✅ M2-2a — enum 3개(2026-08-19)**: `Core/Enum/`에 `MoveAIType`(Straight/Chase/Weave/Hover=직진/추적/사행/견제) · `AttackAIType`(Contact/AimedShot/Barrage/Suicide=충돌/조준단발/탄막/자폭) · `Archetype`(Barrage/Charger/Bomber/Hybrid=탄막/돌진/자폭/복합). 컴파일 0. (range는 별도 enum 아님 — archetype에서 파생 예정.)
  - **M2-2b — 스탯 struct**: 공통(체력/이동속도/데미지/드랍오브량/처치점수) + 공격AI별(발사간격/탄속/탄막수/자폭반경). 수치 Day5.
  - **M2-2c — SO 클래스** `EnemyDefinition : ScriptableObject`: `AssetReferenceGameObject`+MoveAIType+AttackAIType+Archetype+스탯struct, `[CreateAssetMenu]`. VD.Runtime `Data/` + Addressables 참조 추가. archetype→range 파생 헬퍼(단일 소스) 여기서.
  - **M2-2d — 검증**: SO 인스턴스 1~2개 생성, 필드가 §2/§3/§7 일치 + AssetReference에 `Enemy_*` 연결 확인 → DoD.

### M2-3 · UI Toolkit 오서링 창 — 조합 테이블 🔴
- **목적**: enemy-design §5 1층 토대. 적 조합 목록을 한 창에서 편집.
- **작업**: EditorWindow(UI Toolkit), SO DB 목록을 테이블/리스트로 표시·추가·편집, 각 행 = 비주얼×이동AI×공격AI×스탯. `.uxml`/`.uss` 레이아웃.
- **DoD**: 창에서 적 조합을 신규 생성/수정/저장(SO에 반영)까지 왕복.
- **의존**: M2-2
- **문서**: enemy-design.md §5

### M2-4 · 유효성 경고 (교전거리 모순) 🔴
- **목적**: enemy-design §4·§6. 툴의 격상 포인트(단순 테이블 → 검증 툴).
- **작업**: 규칙 판정 — 공격AI 요구 교전거리 ↔ 이동AI 거리 성향 모순(예: 자폭+견제, 충돌+견제) → 경고. 라벨(`range:`) ↔ 부여 AI 성향 모순도 경고. **차단 아님, 경고 표시**(행/필드 하이라이트 + 메시지).
- **DoD**: 모순 조합 만들면 창에 경고가 뜨고, 유효 조합은 깨끗. 저장은 막지 않음(경고만).
- **의존**: M2-3
- **문서**: enemy-design.md §4·§6 (교차검증)

### M2-5 · 최소 스폰 연결 (툴 데이터 → 런타임 스폰) 🔴
- **목적**: scope-tiering Must "최소 스폰 연결". 툴로 만든 적이 실제로 게임에 등장.
- **작업**: 런타임 스포너가 SO DB(+Addressables 로드)에서 적을 읽어 스폰하도록 M1-4의 하드코딩 스포너 교체. 최소한 "SO 목록에서 랜덤/가중 스폰".
- **DoD**: 에디터 툴에서 만든 적 조합이 플레이 중 실제로 스폰·동작(비주얼+AI+스탯 반영).
- **의존**: M2-4, M3-1·M3-2(AI 모듈), M1-4
- **문서**: enemy-design.md

> **M2 완료 판정(게이트)**: "**툴로 오서링한 적이 실제 게임에 등장**하고, 모순 조합엔 경고가 뜬다." → 포폴 핵심 데모 성립.
