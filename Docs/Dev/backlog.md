# Void Drift — 개발 Backlog (구현 태스크 상세 명세)

> 작업 전 [context.md](../../context.md) → [onepage-design.md](../Designs/onepage-design.md) 확인.
> 이 문서는 `scope-tiering.md`의 Must/Should/Nice를 **실제 구현 작업 단위**로 쪼갠 것.
> 마일스톤/티어 명세가 아니라 **"이 태스크를 하면 뭐가 산출되고, 언제 끝난 걸로 보는지(DoD)"** 를 담는다.
> 규칙: 결정/변경 생기면 즉시 갱신, 완료 시 상태 체크. 세부 수치는 대부분 Day5(밸런싱) 이관.

## 문서 사용법 / 표기

- **티어**: 🔴 Must / 🟡 Should / 🟢 Nice (scope-tiering.md 기준)
- **상태**: `[ ]` 대기 · `[~]` 진행중 · `[x]` 완료 · `[!]` 막힘/확인필요
- **DoD** = Definition of Done (완료 판정 조건). 이게 충족돼야 `[x]`.
- 태스크 ID = `M{마일스톤}-{번호}`. 의존성은 ID로 표기.
- ⚠️ 코드 심볼(클래스/파일명)은 **아직 존재하지 않는 제안 이름**이다. 실제 생성 시 이름이 바뀔 수 있음. (전역 규칙: 존재를 주장하지 않음)

---

## 0. 현재 상태 스냅샷 (2026-08-17 실측)

| 항목 | 상태 | 비고 |
|---|---|---|
| Unity 프로젝트 / URP 17.3.0 | ✅ | |
| 3D 에셋 임포트 | ✅ | `Assets/Imports/`: FREE Low Poly Spaceships, StarSparrow(우주선), Planets of the Solar System 3D, JMO Assets(VFX 계열) |
| Scripts | ❌ 비어있음 | 코드 0줄. 이 Backlog가 시작점 |
| Scene | `SampleScene`만 | |
| **UniTask** | ✅ | git UPM (`com.cysharp.unitask`) |
| **R3 코어 1.3.1** | ✅ | NuGetForUnity (`Assets/Packages/R3.1.3.1`) |
| **R3.Unity 통합** | ✅ 설치 | `com.cysharp.r3` 1.3.1 (git UPM). M0-3에서 설치·검증 |
| **Addressables** | ❌ 미설치 | M2(에디터 툴)에서 설치 |
| **Unity MCP 브리지** | ✅ 설치 | M0-1 완료(CoplayDev, HTTP 8080) |
| **Input System** | ✅ New 단독 | `com.unity.inputsystem` 1.20.0, `activeInputHandler:1`. M0-3에서 확정 |

---

## 마일스톤 개요

| MS | 이름 | 티어 | 목표(한 줄) |
|---|---|---|---|
| **M0** | 부트스트랩 & 스모크 테스트 | 🔴 | MCP 연결 + 큐브 Z고정 회전 검증 + 프로젝트 골격 |
| **M1** | 코어 루프 (플레이 가능한 최소 게임) | 🔴 | 이동·오토사격·적·오브·레벨업·3choice·게임오버·HUD |
| **M2** | 에디터 커스텀 툴 (핵심 어필) | 🔴 | SO DB + 적 조합 오서링 + 유효성 경고 + Addressables + 스폰 연결 |
| **M3** | 적 다양성 & 3choice 풀 (Must 완성) | 🔴 | 이동/공격 AI 모듈, 아키타입, 최소 강화 풀 |
| **M4** | 확장 (Should) | 🟡 | 무기 3종·레벨, 실드, 난이도 페이즈, 에디터 툴 2~3층, VFX, 하이스코어 |
| **M5** | 빌드 & 폴리싱 (Must 빌드 + Nice) | 🔴/🟢 | 모바일 가로 Android 빌드 + 데모영상 + (Nice)사운드/특수기능/Firebase |

> 빌드 순서: **M0 → M1 → M2 → M3** 까지가 Must 코어. 이후 M4 Should, M5는 모바일 빌드(Must)를 앞당겨 M3 직후에 1차 실행 권장(빌드 리스크 조기 발견).

---

## M0 — 부트스트랩 & 스모크 테스트 🔴

> 목적: 개발 파이프라인(MCP 왕복 + 씬 조작 + 어셈블리 구조)을 가장 단순한 대상으로 검증. 여기서 막히면 이후 전부 막힘.

### M0-1 · Unity MCP 브리지 설치 & Claude 연결 🔴 `[x]`
- **목적**: Claude Code ↔ Unity 에디터 왕복 조작 가능하게.
- **결정(2026-08-17)**: **CoplayDev "MCP for Unity"** (`com.coplaydev.unity-mcp` v10.1.2). 사실상 표준, Claude Code 공식 지원(47 툴). git URL `https://github.com/CoplayDev/unity-mcp.git?path=/MCPForUnity#main` 를 `Packages/manifest.json`에 추가 완료.
- **사전조건 실측**: uv ✅ / Python 3.14 ✅ / claude CLI ❌(PATH 없음 — 자동설정 실패 시 수동 `.mcp.json` 폴백).
- **남은 작업(Unity UI, 사용자)**: ① Unity 포커스→패키지 임포트 대기 ② 셋업 마법사에서 Python/uv 확인→Done ③ **Claude Code 선택→Configure Selected** ④ `Window → MCP for Unity`에서 Bridge가 Stopped면 Start ⑤ Claude Code 세션 재시작(현 세션은 새 MCP 서버 인식 못 함).
- **실제 적용**: 자동 Configure는 claude CLI 미검출로 실패(예상됨) → **프로젝트 `.mcp.json` 수동 등록**. 서버명 `UnityMCP`, `type: http`, `url: http://127.0.0.1:8080/mcp` (Unity 창 Manual Configuration 값 그대로). Unity 측 서버는 `Start Server`로 기동, "Session connected" 확인됨.
- **DoD**: Claude에서 명령 → Unity 씬/콘솔에 반영되는 왕복 1회 성공(예: 콘솔 로그 또는 오브젝트 생성 확인). ✅ 2026-08-17 검증(인스턴스 `void-drift@5152252c...`, Unity 6000.3.13f1, `manage_scene get_active` 응답 확인).
- **의존**: 없음

### M0-2 · 스모크 테스트: 큐브 Z축 고정 회전 🔴 `[x]` (재작업 완료)
- **목적**: 씬 배치 + 스크립트 컴파일 + 플레이 검증 파이프라인 확인. (사용자 지정 첫 태스크)
- **작업**:
  1. `SmokeScene`(또는 SampleScene 재사용)에 Cube 배치.
  2. 회전 스크립트 작성 — **Z축 위치 고정**, 매 프레임 회전(Y 또는 전축 스핀). 파일 제안: `Assets/Scripts/Smoke/CubeSpinner.cs`.
  3. 플레이 모드에서 큐브가 제자리(Z 고정)에서 빙글빙글 도는 것 확인.
- **DoD**: 플레이 시 큐브가 Z 고정으로 회전. 컴파일 에러 0. (가능하면 MCP로 스핀 속도 파라미터 바꿔 왕복까지)
- **의존**: M0-1(선택 — MCP 없이 수동으로도 가능)
- **⚠️ 재작업 사유(2026-08-17)**: 1차 시도 **폐기**. Claude가 **사용자 지시 없이 구현 방식을 임의 결정**(context.md §1-7 위반). "Z축 고정 회전" **해석 오류** — 사용자 의도는 *Z축을 회전 중심축으로 도는 것*인데, Claude 구현은 *X·Y축 회전 + Z 위치 잠금*이라 큐브가 의도와 다르게(제멋대로) 회전함. **재구현은 사용자가 회전축·"Z축 고정"의 정확한 의미·스핀 속도 등 기준을 지시한 뒤 진행.**
- **1차 잔재(참고, git 커밋됨)**: `Assets/Scripts/Smoke/CubeSpinner.cs`(ns `VoidDrift.Smoke`), `SampleScene`의 `SmokeCube`. MCP 파이프라인(스크립트 생성→컴파일→GameObject/컴포넌트→플레이→상태읽기) 자체는 정상 동작 확인됨 — 재작업 시 이 파이프라인 재사용.
- **✅ 재작업 완료(2026-08-17, 사용자 지시대로)**: **물리 엔진(Rigidbody + angularVelocity)** 로 **Z축 중심** 회전 구현. 인스펙터 노출 = 회전 속도(도/초)·큐브 크기(균일 스케일)·회전 방향(CCW/CW enum). Rigidbody `useGravity=off` + 위치3축·회전 X/Y 고정으로 제자리 Z 스핀. **기존 `SmokeCube` 재사용** + Rigidbody 추가. 검증: rotation `(0,0,z)` Z축 전용·위치 고정·90도/초 정확, 컴파일 에러 0, **사용자 육안 확인 완료**. 방식 선택 근거: 사용자가 "물리 엔진 이용"을 명시 지시, 세부 방식은 Dynamic+angularVelocity 선택.

### M0-3 · 입력 백엔드 & R3.Unity 판단 🔴 `[x]`
- **목적**: 이후 이동 구현(M1-2)이 쓸 입력 방식 확정 + R3 사용 범위 확정.
- **작업**:
  1. Project Settings의 **Active Input Handling** 확인(Input System / 둘 다 / 레거시). `.inputactions` 실사용 여부 판단.
  2. R3를 MonoBehaviour 생명주기(구독 자동 해제)와 함께 쓸지 → **R3.Unity 통합 패키지** 설치 필요 여부 결정. (미설치면 `CancellationToken`/수동 Dispose로 커버 가능 여부 메모)
- **DoD**: (a) 이동 입력을 어떤 API로 읽을지 1줄 결론, (b) R3.Unity 설치 여부 결론 — 둘 다 이 문서에 기록.
- **의존**: 없음
- **✅ 결론(2026-08-17, 사용자 결정·실측 검증)**:
  - **실측 시작 상태**: `activeInputHandler: 0`(레거시 단독), `com.unity.inputsystem` 미설치(manifest/packages-lock/PackageCache 전부 없음). `Assets/InputSystem_Actions.inputactions`는 Unity6 기본 **템플릿 잔재**(패키지 미설치라 무력). R3 코어 1.3.1은 NuGet DLL 존재, R3.Unity 통합은 미설치.
  - **(a) 입력 API = New Input System.** 사용자 결정으로 `com.unity.inputsystem` 1.20.0 설치 + **Active Input Handling = New 단독**(`activeInputHandler: 1`, 사용자 UI 전환 후 에디터 재부팅으로 반영 확인). 이동은 포인터/터치 델타로 **상대 드래그**를 읽음 — 구체 InputAction/EnhancedTouch 오서링은 **M1-2**에서 우리 모바일 스킴에 맞게 진행. (기존 `.inputactions` 데스크톱 템플릿은 M1-2에서 교체 예정, 현재는 그대로 둠.)
  - **(b) R3.Unity = 설치함.** `com.cysharp.r3` 1.3.1 (git UPM `https://github.com/Cysharp/R3.git?path=src/R3.Unity/Assets/R3.Unity`). R3 코어(NuGet)는 유지 — R3.Unity는 코어를 참조만 하고 번들하지 않아 충돌 없음(컴파일 에러 0 확인). `AddTo(this)` 수명 연동 구독 해제·Unity 프레임 연산자·에디터 ObservableTracker 누수 창 사용 가능.
  - **검증**: MCP 왕복(telemetry_ping) 정상, 콘솔 에러/경고 0(유일 항목은 무시 대상 "Claude CLI not found" Configure 메시지). 패키지 54→56, packages-lock에 두 패키지 등록 확인.

### M0-4 · 프로젝트 골격 (폴더 · 어셈블리 · 네임스페이스) 🔴 `[x]`
- **목적**: 런타임/에디터 코드 분리 기반 마련(에디터 툴이 핵심이라 asmdef 분리 필수).
- **작업**:
  - 폴더: `Assets/Scripts/{Core,Player,Enemy,Combat,Progression,UI,Data}`, `Assets/Scripts/Editor/`.
  - asmdef 제안: `VoidDrift.Runtime`(Scripts 루트), `VoidDrift.Editor`(Editor 전용, Runtime 참조). 네임스페이스 루트 `VoidDrift.*`.
  - UniTask/R3 참조를 asmdef에 연결.
- **DoD**: 빈 asmdef 2개로 컴파일 통과, Editor 어셈블리가 Runtime을 참조. 스모크 스크립트가 새 구조에 안착.
- **의존**: M0-2
- **✅ 완료(2026-08-17, 사용자 결정 반영)**: 상세 = [01_AssemblyDefinition.md](01_AssemblyDefinition.md)
  - **어셈블리 2개**: `VD.Runtime`(Scripts 루트, `UniTask`·`R3.Unity`·`Unity.InputSystem` 참조 + R3 코어 자동참조) / `VD.Editor`(`Editor/`, `includePlatforms:[Editor]`, `VD.Runtime` 참조). **네임스페이스 루트 `VD.*`** (제안 `VoidDrift.*` → 접두어 `VD` 로 확정). 폴더별 분리 아님(과분리 회피). (`Unity.InputSystem` 참조는 M1 준비 점검에서 누락 발견→추가: autoReferenced라도 커스텀 asmdef엔 수동 참조 필요.)
  - **폴더(확정)**: `Assets/Scripts/{Core,Player,Enemy,UI}` + `Editor/`. 초안의 `Combat`/`Progression`은 폐기 — 총알은 Player/Enemy 내부 구현, 진행 로직은 Core로 흡수. `Data`는 M2에서, `Core/Interface`·`Player/Struct` 등은 필요 시 생성.
  - **파일 규칙(사용자 결정)**: 인터페이스 1파일 1개(`Core/Interface/`), 클래스 1파일 1개(소형 연관클래스 동거 허용), public struct는 별도 파일·`*/Struct` 폴더 몰기.
  - **씬(함께 진행)**: `Assets/Scenes/` 에 TitleScene(build 0)/GameScene(1)/ResultScene(2) 생성·등록. Loading은 별도 씬 아님(GameScene 오버레이+FadeOut 방침, 미구현). SampleScene은 빌드 제외 유지.
  - **검증**: 리플렉션으로 두 어셈블리 로드·Editor→Runtime 참조·R3/UniTask 링크 확인, 컴파일 에러 0. 마커 스크립트 2개(`VDRuntimeMarker`/`VDEditorMarker`)는 참조 검증용 임시 파일(실코드 안착 시 삭제).

---

## M1 — 코어 루프 (플레이 가능한 최소 게임) 🔴

> 목적: scope-tiering "코어 루프" 전체. 이 마일스톤 끝 = **손으로 플레이되는 게임**(에디터 툴 없이 하드코딩 데이터로).
> 관련: [controls-design.md](../Designs/controls-design.md), [progression-design.md](../Designs/progression-design.md), [ui-design.md](../Designs/ui-design.md)

### M1-1 · 게임 부트/상태 관리 골격 🔴 `[x]`
- **목적**: 씬 진입~플레이~게임오버 상태 흐름과 전역 서비스 접근점.
- **작업**: 게임 상태(부팅/플레이/일시정지/게임오버) 관리자, 시간 스케일 제어(3choice 일시정지용), 간단한 서비스 접근(이벤트 버스). R3 `Subject`/`ReactiveProperty` 기반 이벤트 채널 제안(`GameEvents`).
- **DoD**: 상태 전환 로그로 확인. `Time.timeScale=0` 일시정지/재개 동작.
- **의존**: M0-4
- **✅ 완료(2026-08-17, 사용자 결정 반영)**: 상세 = [02_GameStateArchitecture.md](02_GameStateArchitecture.md)
  - **산출(`VD.Core`)**: `GameState`(enum Boot/Playing/Paused/GameOver) · `GameEvents`(별도 pub/sub 채널, R3 `ReactiveProperty<GameState>`를 `ReadOnlyReactiveProperty`로 노출, 갱신은 `internal SetState`로 GameManager만) · `GameManager`(MonoBehaviour **싱글톤**, 씬 한정 수명, 상태 전이+`Time.timeScale` 제어, `StartGame/Pause/Resume/GameOver` 가드 포함) · `GameDebugDriver`(**임시** 키보드 검증: P=일시정지/재개·G=게임오버·R=재진입).
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
    - **조준 "원뿔"의 중심축 = 뱅킹 방향**(= `FirePoint.forward`, `PlayerAim`이 냄). 오프셋 0이면 +Z, 드래그하면 그쪽으로 축이 기욺. (2026-08-18 확인 — "원뿔의 중심은 z축"은 오프셋 0 기준을 임의 용어로 말한 것, 정식은 뱅킹 방향.)
    - **원뿔 내 적 타겟 스냅(기관총·레일건)은 M1-4로 이관.** 적이 있어야 실동작·검증 가능하므로 여기선 축 직사만. 무타겟이면 축(`FirePoint.forward`)으로 직사, 원뿔 안에 적 있으면 그 적으로 조준 스냅 — 이 층은 적 엔티티(M1-4) 붙은 뒤 추가. **유도탄은 축·원뿔과 무관한 별도 호밍(M4-1).**
    - 기체 시각 관성/무게감은 **지금 연출 그대로 유지**(폴리싱 때 손봄, [issues.md](issues.md) I-1).
  - **목표 구조**:
    ```
    Player (root)   ← PlayerMovement (이동만)
    ├── FirePoint   ← 깨끗한 조준 방향 정렬. 발사 원점(2·3단계)
    └── Model       ← Mesh + PlayerBanking (연출: 부드러운 뱅킹)
    ```
  - **✅ 1단계 완료**: 뱅킹을 `PlayerMovement`에서 신규 **`PlayerBanking`**(Model에 부착)으로 분리. `PlayerMovement`=이동만. 동작 동일(컴파일 0, 사용자 플레이 확인). 프리팹 반영.
  - **✅ 2단계 완료(2026-08-18)**: `FirePoint`(root 직속 자식, localPos 0·forward +Z) 생성 + 신규 **`PlayerAim`**(FirePoint 부착)이 오프셋→pitch/yaw를 `LateUpdate`에서 **즉시(보간 없음)** `FirePoint.localRotation`에 적용. 공식은 `PlayerBanking`과 동일하되 **roll 생략**(forward 불변)·**독립 필드**(maxPitch/maxYaw 기본 28/28, 조준 원뿔을 뱅킹 연출과 별도 튜닝 가능). 임시 검증 기즈모(`drawAimGizmo`, 조준 축 레이) 포함 — 발사 로직 안착 후 정리. 컴파일 0, 사용자 육안 확인. 프리팹 반영.
  - **✅ 3단계 완료(2026-08-18)**: 발사 로직 — 발사기 `PlayerShooter`(root, `Playing`에서 `fireInterval`마다 `FirePoint` 방향 발사) + 투사체 `Projectile`(자기 forward 직진 + 수명 만료 시 self-return, 콜라이더 없음) + 풀. **풀은 상속형**: `VD.Core.PooledObjectPool<T>`(추상 MonoBehaviour 베이스, prewarm/Get/Return + `Create`/`OnGet`/`OnReturn` 훅) ← `VD.Player.ProjectilePool : PooledObjectPool<Projectile>`(Get 시 반납 콜백 배선). 이후 EnemyPool(M1-4)·OrbPool(M1-5)이 같은 베이스 상속. **튜닝 한 곳**(사용자 결정): 탄속·수명·발사속도를 `PlayerShooter` 인스펙터에 몰아 발사 시 투사체에 주입(`Projectile.Launch`). 투사체 비주얼 = 임시 프리미티브(`Projectile.prefab`, Unlit 노랑-주황). GameScene에 `ProjectilePool` 오브젝트(prewarm 32). 검증: 조준 축으로 총알 스트림·일시정지(P) 시 정지·컴파일 0, **사용자 육안 확인**. 데미지/충돌은 M1-4.
  - **부수 정리(2026-08-18)**: Player 프리팹의 32-박스 `Collider` 그룹을 **단일 `BoxCollider`(root)로 단순화**(날개 제외·앞쪽 트림·유저 관대 = 작게). 피격 판정용이라 M1-4/M1-9에서 트리거 여부·수치 확정. (사용자가 인스펙터에서 최종 미세조정.)
  - **관련 파일**: `Assets/Scripts/Player/{PlayerMovement,PlayerBanking,PlayerAim,PlayerShooter,Projectile,ProjectilePool}.cs`, `Assets/Scripts/Core/PooledObjectPool.cs`. 프리팹 `Assets/Prefabs/Player.prefab`(root=Rigidbody+PlayerMovement+PlayerShooter+BoxCollider / 자식 `FirePoint`=PlayerAim, `Model`=Mesh+PlayerBanking), `Assets/Prefabs/Projectile.prefab`. GameScene에 `ProjectilePool`. 카메라 리그·이동 상세 = [03_PlayerMovementAndCamera.md](03_PlayerMovementAndCamera.md).

### M1-4 · 적 기본 엔티티 & 스폰(하드코딩) 🔴 `[~]` (DoD 충족 — 스폰·이동·사격파괴·충돌데미지·레이어분리; 잔존 = 원뿔 튜닝·임시요소 정리, 게임오버는 M1-9)
- **목적**: 툴 이전 단계. 코드로 직접 적 1~2종을 스폰해 루프 성립.
- **작업**: 적 컴포넌트(체력/접근 이동/피격→사망), 코스 안쪽에서 플레이어 쪽으로 접근, 간단 스포너(시간/간격 하드코딩). 충돌 시 플레이어 데미지.
- **⤷ M1-3에서 이관 (처리 현황)**: (1) ✅ **데미지 전달 `IDamageable` + 투사체 충돌 감지** 완료(트리거 콜라이더). (2) ✅ **원뿔 내 적 타겟 스냅** 완료(매 발 nearest-in-cone, 무타겟이면 `FirePoint.forward` 축 직사). (3) ✅ 스폰 `EnemyPool`(`PooledObjectPool<T>` 상속) 완료. — 모두 2026-08-18 처리.
- **DoD**: 적이 계속 스폰·접근하고, 사격으로 파괴되며, 플레이어와 충돌 시 HP 감소.
- **의존**: M1-3
- **⚙️ 결정 & 진행 (2026-08-18, 사용자)**:
  - **적 프리팹 소스 = `FREE Low Poly Spaceships`**(`Assets/Imports/FREE Low Poly Spaceships/Prefabs/spaceship_1~7`, 단일 메시 프리팹). Player는 StarSparrow, **적은 이 세트**로 분리.
  - **M1-4 첫 적 = `spaceship_6` 1종만.** 나머지 아키타입 볼륨업은 M3-3. (아키타입 매핑 잠정 = M3-3 참조.)
  - **진행 = 단계 분할**(M1-3처럼). **1단계 = 스폰 + 직진 접근 이동**만 먼저(코스 안쪽 먼 +Z에서 플레이어 쪽/화면 아래로 직진). 이후 단계에서 **HP/피격→사망 + 충돌 데미지 + 투사체 히트(`IDamageable`)** = 위 M1-3 이관분.
  - **스폰 위치 = 랜덤 위치만.** 편대/웨이브 등 **공간 포메이션 패턴은 M5-8로 이관·등재**(볼륨 큼, Nice 후순위 — 2026-08-18 사용자 결정).
  - **✅ 1단계(스폰 + 직진 접근 이동) 완료(2026-08-18, 사용자 확인)** — `Enemy`(-Z 직진, despawn self-return) + `EnemyPool : PooledObjectPool<Enemy>` + `EnemySpawner`(랜덤 위치 스폰, 튜닝 한 곳). `Enemy.prefab`(root=Enemy+BoxCollider trigger / Model=spaceship_6, 임시 스케일 6). GameScene에 `EnemySpawner`(+EnemyPool, prewarm 16). 스폰 거리/폭·카메라 거리(−26→−36)는 **사용자 인스펙터 튜닝**(값은 씬 인스턴스, 프레이밍 계속 조정 중이라 문서에 수치 미고정). **다음 단계 = HP/피격→사망 + 충돌 데미지 + 투사체 히트(`IDamageable`)** (M1-3 이관분).
  - **적 이동 속도 = 현재 단일 고정.** 적/아키타입별 **가변 속도는 볼륨업(M3)으로 이관**(2026-08-18 사용자 결정) — 하나로 고정하지 않음. (M2-2 SO 스탯·M3-3 아키타입에서 데이터화.)
  - **✅ 2단계(적 피격 → HP 감소 → 사망 + 타겟 스냅) 완료(2026-08-18, 사용자 확인)**:
    - 신규 **`VD.Core.IDamageable`**(최소 `TakeDamage(float)`, `Core/Interface/`). `Enemy`가 구현 — `maxHp`(30, 스폰 시 리셋), 피격 HP 감소, HP≤0 사망→풀 반납(오브 드랍 M1-5·파괴 VFX M4-9는 이후).
    - **투사체 히트 = 트리거 콜라이더**(사용자 결정): `Projectile.prefab`에 kinematic Rigidbody + isTrigger 콜라이더, `Projectile.OnTriggerEnter`→부모의 `IDamageable`만 데미지·즉시 풀 반납(`_spent` 중복가드). 데미지 튜닝 = **`PlayerShooter.projectileDamage`**(10, 발사 시 주입).
    - **원뿔 타겟 스냅**: `PlayerShooter`가 **매 발** `Physics.OverlapSphereNonAlloc`로 조준 축(`FirePoint.forward`) 원뿔(반각 `aimConeHalfAngle` 25°·사거리 `aimRange` 90) 내 **가장 가까운** 대상을 골라 그쪽으로 발사(락/캐싱 없음). 원뿔 밖·무타겟이면 축 직사. (`targetMask`는 3단계에서 Enemy 레이어로 정리.)
    - **⚠️ 임시요소 잔존(정리 예정)**: `[TEMP]` 히트/피격/사망 로그(`Projectile`·`Enemy`), 조준 원뿔 기즈모(`PlayerShooter.drawAimGizmo`). **원뿔 각도/사거리 튜닝은 나중에 자연스럽게**, 그때 임시요소 정리(사용자 결정 2026-08-18).
  - **적 구조(구현됨)**: `Enemy.prefab` = root(Enemy + BoxCollider trigger) / 자식 Model(spaceship_6, 임시 스케일 6). 스폰 = 상속형 `EnemyPool`.
  - **✅ 3단계(플레이어 충돌 데미지 + 레이어 분리) 완료(2026-08-18, 사용자 확인)**:
    - 신규 **`PlayerHealth`**(Player root, `maxHp` 100). **`IDamageable` 미구현**(아군 오사 방지 — 플레이어가 damageable이면 발사 순간 자기 총알에 맞음). 스스로 `OnTriggerEnter`로 **적(`Enemy`) 접촉 감지 → HP 감소**. 접촉 데미지 = `Enemy.contactDamage`(10, `ContactDamage` getter). 적은 접촉 후 계속 진행(램/자폭 사망은 M3 아키타입). HP 0 게임오버 전이·HP UI·결과화면은 **M1-9/M1-10**(사용자 결정: 이번은 HP 감소만).
    - **레이어 분리(물리 매트릭스, 사용자 결정)**: `Player`(8)·`Enemy`(9)·`PlayerBullet`(10) 생성. 매트릭스 = Player×Enemy ON·Enemy×PlayerBullet ON·**Player×PlayerBullet OFF**(자살 물리 차단)·PlayerBullet self OFF·Enemy self OFF. 프리팹 3개+씬 Player에 레이어 할당, `PlayerShooter.targetMask`=Enemy. 기존 컴포넌트 필터는 안전용 유지.
  - **✅ DoD 충족**: 스폰·접근 ✓ / 사격 파괴 ✓ / 충돌 시 HP 감소 ✓. **잔존(폴리싱)** = 원뿔 각도·사거리 튜닝 + `[TEMP]` 로그·기즈모 제거(원뿔 튜닝 시), 게임오버/HP UI = M1-9/M1-10.
  - **관련 파일**: `Assets/Scripts/Enemy/{Enemy,EnemyPool,EnemySpawner}.cs`, `Assets/Scripts/Core/Interface/IDamageable.cs`, `Assets/Scripts/Player/{PlayerShooter,Projectile,PlayerHealth}.cs`. 프리팹 `Assets/Prefabs/{Enemy,Projectile}.prefab` + `Player.prefab`(PlayerHealth). 레이어 Player/Enemy/PlayerBullet(물리 매트릭스). GameScene에 `EnemySpawner`(+EnemyPool).

### M1-5 · 오브 드랍 & 자석 습득 🔴 `[x]` (드랍·자석·습득 완료 — 경험치 이벤트 발행만 M1-6 이관)
- **목적**: 적 파괴 → 자원(오브=경험치) 드랍 → 습득.
- **작업**: 적 사망 시 오브 스폰, 일정 반경 내 플레이어로 끌려오는 자석 로직, 접촉 시 습득 이벤트.
- **DoD**: 파괴 시 오브 드랍, 근접 시 빨려와 습득되고 경험치 이벤트 발행.
  - **판정(2026-08-18)**: 드랍·자석 끌림·근접 습득 = **충족**(사용자 육안 확인). "**경험치 이벤트 발행**"은 누적/레벨업 시스템이 있어야 의미 → **M1-6로 이관**(사용자 결정: M1-5는 로그만). 습득 지점에 `[TEMP]` 로그를 두고 M1-6에서 실이벤트로 대체. M1-5는 **드랍→자석→습득 파이프라인** 기준 완료. (M1-3→M1-4 이관 선례와 동일 방식.)
- **의존**: M1-4
- **⚙️ 결정 & 진행 (2026-08-18, 사용자)** — 단계 분할(M1-3/M1-4 방식), 각 단계 사용자 육안 확인 후 진행.
  - **오브 비주얼(사용자 준비)**: `Assets/Imports/Hovl Studio/Magic effects pack/Prefabs/Environment/Crystal effect green/blue/red`(파티클 VFX 크리스탈)를 **MCP 복제**(새 GUID)해 `Assets/Prefabs/Orbs/Orb Crystal {green,blue,red}.prefab` 생성. **일단 green 사용**. 오브 게임플레이 프리팹 `Assets/Prefabs/Orbs/Orb.prefab` = root(`Orb`) / 자식 `Model`(green 크리스탈).
  - **구조 배치**: `Orb`/`OrbPool`은 **`VD.Core`** (진행 로직=Core 흡수, M0-4 결정). `OrbPool : PooledObjectPool<Orb>`(기존 상속 베이스 재사용, M1-3의 ProjectilePool·M1-4의 EnemyPool과 동일 패턴).
  - **✅ 1단계(드랍 + 오브 존재)**: 적 **실사망**(`Enemy.Die`)에만 드랍 훅 — `Enemy`에 `Action<Vector3>` 드랍 콜백 주입(`SetDropHandler`), 화면 밖 `Despawn`은 드랍 안 함. `EnemySpawner`가 `OrbPool` 자동탐색(`FindAnyObjectByType`) 후 스폰 적에 `DropOrb`(사망 위치에 `orbPool.Get()`) 배선. GameScene에 `OrbPool`(prewarm 16). 사용자 확인.
  - **✅ 2단계(자석)** — 거동 **사용자 결정**:
    - **반경 밖** = 전방(월드 -Z, 플레이어 쪽)으로 **일정 속도 드리프트**. 플레이어가 경로 근처(반경 내)에 없으면 **그대로 지나쳐** 뒤로 빠져 despawn(풀 반납, 못 먹음). (호밍 아님 — "일정 이내 아니면 그냥 지나쳐야 한다" 정정 반영.)
    - **반경 안** = 플레이어가 `magnetRadius` 이내로 들어오면 **캡처(래치)** → 플레이어로 **가속 끌림**(경계=driftSpeed → 접촉=magnetMaxSpeed, 오버슛 클램프). 한 번 캡처되면 놓치지 않음.
    - 타깃(플레이어)은 `OrbPool`이 **태그 "Player"** 로 1회 탐색·캐시 후 `Orb.OnSpawned(target, Return)`로 주입(**Core→Player 타입 결합 회피**). Player 프리팹에 builtin 태그 "Player" 부여.
  - **✅ 3단계(습득)** — **거리 기반**(사용자 결정, 콜라이더/레이어 불필요): 캡처된 오브가 `pickupRadius` 이내 도달 시 습득 → **`[TEMP]` 로그 + 풀 반납**. 경험치 이벤트 배선은 M1-6.
  - **튜닝(Orb 프리팹 인스펙터, Day5 잠정)**: `driftSpeed` 6 · `magnetRadius` 8 · `magnetMaxSpeed` 40 · `pickupRadius` 0.6 · `despawnZ` −50. 사용자: "나중에 튜닝".
  - **관련 파일**: `Assets/Scripts/Core/{Orb,OrbPool}.cs`, `Assets/Scripts/Enemy/{Enemy,EnemySpawner}.cs`(드랍 훅·배선). 프리팹 `Assets/Prefabs/Orbs/{Orb,Orb Crystal green,Orb Crystal blue,Orb Crystal red}.prefab`, `Assets/Prefabs/Player.prefab`(태그). GameScene에 `OrbPool`.

### M1-6 · 경험치 / 레벨업 (점증형 임계값) 🔴 `[x]`
- **목적**: progression §1. 오브 누적 → 임계값 도달 → 레벨업.
- **작업**: 경험치 누적, **레벨별 점증 임계값 곡선**(수치 Day5), 레벨업 시 이벤트(→ 3choice 트리거). R3 `ReactiveProperty<int>`(레벨)·`ReactiveProperty<float>`(경험치%) 제안 — HUD 바인딩 대비.
- **DoD**: 오브 습득이 게이지 채우고, 임계값마다 레벨업 이벤트 발생(레벨 오를수록 더 많이 필요). ✅ **충족**(2026-08-18, 사용자 확인).
- **의존**: M1-5
- **문서**: progression-design.md §1, **[05_ProgressionAndEvents.md](05_ProgressionAndEvents.md)**(기술 상세)
- **✅ 완료(2026-08-18, 사용자 결정 반영)** — 상세 = [05_ProgressionAndEvents.md](05_ProgressionAndEvents.md)
  - **사용자 결정**: (1) 상태 배치 = **GameEvents 확장**(별도 시스템 아님, 02 예고 확장 지점), (2) 오브→경험치 = **GameEvents 이벤트 발행→구독**(pub/sub), (3) 임계값 = **지수형** `base×growth^(n-1)`.
  - **산출(`VD.Core`)**: `GameEvents` 확장 — `OrbCollected`(입력 `Observable<int>`) · `Level`(`ReadOnlyReactiveProperty<int>`) · `XpNormalized`(`ReadOnlyReactiveProperty<float>` 0~1, HUD 게이지) · `LevelUp`(`Observable<int>`, 3choice용). 발행/갱신 메서드는 `internal`(`PublishOrbCollected`/`SetLevel`/`SetXpNormalized`/`RaiseLevelUp`). 신규 **`ExperienceSystem`**(GameScene 1개) — `OrbCollected` 구독·누적, 지수 임계값 도달 시 초과분 이월+레벨업 발행. `Orb`는 습득 시 `[TEMP]` 로그 대신 `PublishOrbCollected(xpValue)`(xpValue 기본 1).
  - **튜닝(Day5 잠정, ExperienceSystem 인스펙터)**: `baseThreshold` 5 · `growth` 1.3. `Orb.xpValue` 1(→ M2-2 SO).
  - **검증**: 오브 5개→Lv2·다음 임계 6.5(=5×1.3), 컴파일/런타임 에러 0, 사용자 확인.
  - **⚠️ 임시요소**: `[TEMP] 레벨업` 로그 — **M1-7(3choice)** 가 `LevelUp` 구독해 팝업 띄우면 대체. `Level`/`XpNormalized`는 **M1-10 HUD** 바인딩.
  - **관련 파일**: `Assets/Scripts/Core/{GameEvents,ExperienceSystem,Orb}.cs`. GameScene에 `ExperienceSystem`.

### M1-7 · 3choice 강화 선택 (일시정지 팝업) 🔴 `[x]` (2026-08-19 검증 완료)
- **목적**: progression §1. 레벨업 시 게임 일시정지 + 3택 카드 + 선택 적용 + 재개.
- **작업**: 레벨업 이벤트 수신 → `Time.timeScale=0` → 후보 3개 롤(중복 방지) → uGUI 카드 팝업 → 선택 시 강화 적용 → 재개. 강화 데이터는 M1-8 최소 풀 사용.
- **DoD**: 레벨업 시 프리즈되고 3장 뜸, 하나 고르면 효과 적용 후 게임 재개. ✅ **충족**(2026-08-19, Play 검증).
- **의존**: M1-6, M1-8, M1-10(HUD/캔버스)
- **문서**: progression-design.md, ui-design.md §3
- **✅ 완료(2026-08-19)**:
  - **산출**: `VD.UI.LevelUpPopup`(`GameEvents.LevelUp` 구독 → `GameManager.Pause()`(timeScale 0) → `UpgradeSystem.Roll(3)` 3장 카드 표시 → 카드 클릭 시 `UpgradeSystem.Apply(선택)` → `GameManager.Resume()`, **다중 레벨업 큐 순차**), `VD.Core.UpgradeDisplay`(readonly struct — 카드 표시용), `UpgradeSystem.Describe(UpgradeType)`(제목/설명/**효과 수치를 실제 필드에서 렌더** → UI 하드코딩 회피). M1-8의 **임시 자동적용 제거** — 이제 팝업이 선택·적용을 구동.
  - **씬**: GameScene에 `LevelUp Canvas`(sortOrder 100) + 딤 Panel + 3 Card(Button+TMP) + `EventSystem`(`InputSystemUIInputModule`).
  - **한글 폰트**: 런타임 TMP 텍스트에 **SUIT SDF**(`Assets/Imports/Fonts/SUIT-Regular SDF`·`SUIT-Heavy SDF`) 적용 — 기본 `LiberationSans SDF`엔 한글 글리프 없어 깨지던 문제 해소. (배선 세부 점검은 보류, 문제 시 대응.)
  - **검증**: 레벨업→프리즈·3카드→클릭 시 스탯 변경·재개·큐 순차 정상, 한글 렌더 정상.
  - **⚠️ 임시요소 유지**: `[TEMP]` 로그는 **정리하지 않고 유지**(육안 확인용, 사용자 방침 — 앞으로도 동일).

### M1-8 · 최소 3choice 강화 풀 (공용 스탯) 🔴 `[x]` (풀·적용·롤 완료 — 팝업 UI·선택은 M1-7)
- **목적**: scope-tiering Must "빌드 선택이 성립할 최소". 공격력/이동속도/최대체력 등 몇 종.
- **작업**: 강화 항목 정의(효과 적용 방식: 스탯 배율/가산), 최소 3~5종 하드코딩 또는 소형 데이터. 3choice 롤 대상이 되게 연결.
- **DoD**: 최소 3종 이상이 롤에 등장하고 각각 실제로 스탯을 바꿈. ✅ **충족**(2026-08-18, execute_code 검증).
- **의존**: M1-2/M1-3(스탯 대상 존재)
- **문서**: [upgrade-pool.md](../Designs/upgrade-pool.md) (풀세트는 M4)
- **✅ 완료(2026-08-18, 사용자 결정 반영)**:
  - **사용자 결정**: (1) 정의 = **하드코딩**(enum+로직, SO는 M2), (2) 효과 = **능력치별 상이**(일괄 아님), (3) 항목 3종 = **이동속도/최대체력/자석범위**. 공격력·연사 등은 **무기 스코프**라 무기 개발(M4) 후로 미룸(사용자 지적, upgrade-pool §3 정합).
  - **산출**: `VD.Core.UpgradeType`(enum: MoveSpeed/MaxHp/MagnetRadius) + `VD.Player.UpgradeSystem`(GameScene 1개) — `GameEvents.LevelUp` 구독 → `Roll(3)`(Fisher–Yates, 중복없음) → **임시 자동적용**+`[TEMP]` 로그. 라우팅 mutator: `PlayerMovement.AddMoveSpeedMultiplier`(배율%)·`PlayerHealth.AddMaxHp`(가산+회복)·`OrbPool.AddMagnetRadius`(가산, 스폰 시 `Orb`에 보너스 주입). `Roll()`/`Apply()`는 **public → M1-7 팝업이 재사용**.
  - **효과 방식(능력치별)**: 이동=배율 `dragGain*=(1+pct)`, 최대체력=가산 `maxHp+=n`(현재HP도 +n), 자석범위=가산 `magnetRadius + bonus`.
  - **튜닝(Day5)**: `moveSpeedPct` 0.12 · `maxHpAdd` 20 · `magnetRadiusAdd` 2.
  - **검증(execute_code)**: 롤에 3종 등장, MoveSpeed dragGain 5→5.6(×1.12)·MaxHp 100→120(+20)·MagnetRadius bonus+2, 에러 0.
  - **⚠️ 임시/설계 노트**: 레벨업 시 **무작위 자동적용**(사용자 선택 아님) + `[TEMP]` 로그 → **M1-7 팝업**이 `Roll()` 3장 표시·`Apply(선택)`로 대체. 항목 3개라 롤 3장=항상 전부 등장(다양성은 항목 늘면, M3-4/M4-8).
  - **관련 파일**: `Assets/Scripts/Core/{UpgradeType,OrbPool,Orb}.cs`, `Assets/Scripts/Player/{UpgradeSystem,PlayerMovement,PlayerHealth}.cs`. GameScene에 `UpgradeSystem`.

### M1-9 · HP / 데미지 / 게임오버 🔴 `[x]` (게임오버 전이·정지·점수 확정 — 결과 화면 UI는 M1-10/M2)
- **목적**: 종료 조건. HP 0 → 게임오버 → 결과.
- **작업**: 플레이어 HP, 피격 처리, HP 0 시 게임오버 상태 전환 + 결과값(생존시간/점수) 확정.
- **DoD**: 피격 누적으로 HP 0 되면 게임오버 화면/상태로 전환, 최종 점수 표시. ✅ **충족**(2026-08-18, 사용자 확인) — 상태 전환·점수 확정·임시 로그. **결과 "화면"(ResultScene/HUD 표시)은 M1-10/M2**(정지형 프리즈 + 결과값 보관까지가 이번 범위).
- **의존**: M1-1, M1-4
- **문서**: progression-design.md §3, **[05_ProgressionAndEvents.md](05_ProgressionAndEvents.md) §종료/점수**
- **✅ 완료(2026-08-18, 사용자 결정 반영)**:
  - **사용자 결정**: (1) 게임오버 = **GameScene 정지형**(HP0 → GameOver 상태 + `timeScale 0` 프리즈 + 결과값 보관+임시 로그, ResultScene 전환·결과 UI는 이후), (2) 점수 = **생존시간 + 처치점수**(처치 이벤트 배선, 처치당 점수 하드코딩 → M2-2 SO).
  - **산출**: `GameEvents` 확장 — `EnemyKilled`(입력 `Observable<int>`) · `HpNormalized`·`Score`·`SurvivalTime`(출력 `ReadOnlyReactiveProperty`, HUD/결과용, 발행·갱신 internal). `GameManager.GameOver()` → `timeScale 0`(정지형). 신규 **`ScoreSystem`**(GameScene 1개) — Playing 동안 생존시간 누적 + `EnemyKilled` 합산, `Score=round(생존×rate)+처치점수`, 게임오버 시 최종 로그. `Enemy` 실사망 시 `PublishEnemyKilled(killScore)`(기본 10). `PlayerHealth` — HP% 게시 + HP0 시 `GameManager.GameOver()`.
  - **튜닝(Day5 잠정)**: `ScoreSystem.timeScoreRate` 1(초당 점수) · `Enemy.killScore` 10.
  - **검증(execute_code)**: 생존 53.1s + 처치 180(18×10) = **점수 233**, `GameOver()`→`state=GameOver`·`timeScale=0`, 게임오버 로그 출력, 에러 0. (스모크는 원점 고정이라 실피격 HP0은 사용자 육안 확인 대상 — HP감소는 M1-4서 검증됨, GameOver 분기는 강제 호출로 검증.)
  - **⚠️ 임시요소**: `[TEMP]` 피격·게임오버 로그 — 결과 화면(ResultScene/오버레이)·HUD 붙을 때 정리. `Score`/`SurvivalTime`/`HpNormalized`는 **M1-10 HUD** 바인딩.
  - **관련 파일**: `Assets/Scripts/Core/{GameEvents,GameManager,ScoreSystem}.cs`, `Assets/Scripts/Enemy/Enemy.cs`, `Assets/Scripts/Player/PlayerHealth.cs`. GameScene에 `ScoreSystem`.

### M1-10 · HUD (우측 상단) + 점수/생존시간 🔴 `[x]`
- **목적**: ui-design. 생존시간/점수/HP 최소 표기(uGUI).
- **작업**: uGUI 캔버스, 우측 상단 생존시간·점수, HP 표기. 점수=생존시간+처치점수. R3로 상태→UI 바인딩(설치 결론 반영).
- **DoD**: 플레이 중 생존시간·점수·HP가 실시간 갱신. ✅ **충족**(2026-08-18, 스크린샷+런타임값 확인).
- **의존**: M1-9
- **문서**: ui-design.md §3, progression-design.md §3
- **✅ 완료(2026-08-18, 사용자 결정 반영)**:
  - **사용자 결정**: (1) HUD 구성 = **시간·점수·HP + 레벨·경험치바**(M1-6 값도 표시), (2) HP 표현 = **게이지 바**, (3) 텍스트 = **TextMeshPro**(에센셜 임포트). 레이아웃 = 우측 상단 스택(최소 투자, Day5 조정).
  - **산출**: `VD.UI.HudView`(표시 전용) — `GameEvents` 구독→시간(mm:ss)/점수/레벨/HP바(`HpNormalized`)/경험치바(`XpNormalized`), R3 `AddTo` 수명 연동. GameScene에 **HUD Canvas**(ScreenSpaceOverlay + CanvasScaler 1920×1080 match0.5) + TMP 텍스트 3 + HP/XP 바(built-in UISprite, Image Filled Horizontal).
  - **인프라**: `VD.Runtime` asmdef에 `UnityEngine.UI`·`Unity.TextMeshPro` 참조 추가. **TMP 에센셜 리소스 임포트**(`Assets/TextMesh Pro`, 최초 1회 — 폰트 LiberationSans SDF·TMP Settings).
  - **검증**: 런타임 값 `time 00:58 / SCORE 248 / Lv 2 / hpFill 1.0 / xpFill 0.4` 실시간 갱신, 에러 0.
  - **⚠️ 잔여**: 레이아웃·색·바 스타일 = 최소 기본(Day5 튜닝). HP 숫자 없음(바만, 사용자 선택). `[TEMP]` 로그 잔존.
  - **관련 파일**: `Assets/Scripts/UI/HudView.cs`, `Assets/Scripts/VD.Runtime.asmdef`. GameScene에 `HUD Canvas`. TMP 에센셜 `Assets/TextMesh Pro`.

> **M1 완료 판정(게이트)**: 에디터 툴/Addressables 없이도, 하드코딩 데이터로 **처음~게임오버까지 한 판이 돌아간다.**

---

## M2 — 에디터 커스텀 툴 (핵심 어필) 🔴

> 목적: 공고 1순위. [enemy-design.md](../Designs/enemy-design.md) 3층 중 **1층(유효성 경고) + SO DB + Addressables + 스폰 연결**까지가 Must. 2·3층 심화는 M4.
> ⚠️ 관심사 분리: 이 툴은 **적 오서링 전용**. 3choice 업그레이드 풀과 섞지 않음.

### M2-1 · Addressables 설치 & Enemy Group/Label 구성 🔴
- **목적**: enemy-design §6. 적 프리팹을 Addressables로 관리 + 라벨로 거친 분류.
- **작업**: Addressables 패키지 설치, `Enemy` Group 생성, 임포트한 우주선 에셋으로 적 프리팹 후보 등록, 라벨 `archetype:탄막/돌진/자폭`·`range:원거리/근거리` 부여.
- **DoD**: Enemy Group에 프리팹 N개 등록·라벨링, 라벨 기준 로드 스모크 테스트 1회 성공(UniTask로 비동기 로드).
- **의존**: M0-4
- **문서**: enemy-design.md §6

### M2-2 · 적 데이터 SO 스키마 (조합 원천 데이터) 🔴
- **목적**: enemy-design §2·§6. 에디터가 편집하는 디자인 원천 데이터 정의.
- **작업**: ScriptableObject 스키마 — AssetReference(비주얼) + 이동AI 종류 + 공격AI 종류 + 스탯(체력/속도/데미지/드랍오브량/처치점수) + 아키타입. enum(이동AI 4·공격AI 4·아키타입 3) 정의.
- **DoD**: SO 인스턴스를 인스펙터로 만들 수 있고, 필드가 enemy-design §2/§3/§7과 일치.
- **의존**: M2-1
- **문서**: enemy-design.md §2·§3·§7

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

---

## M3 — 적 다양성 & 3choice 풀 (Must 완성) 🔴

> M2 스폰이 의미 있으려면 AI 모듈과 아키타입이 필요. scope-tiering Must "최소 적 다양성".

### M3-1 · 이동 AI 모듈 (직진/추적) 🔴
- **목적**: enemy-design §3. 재사용 이동 모듈 최소 2종.
- **작업**: 이동AI 인터페이스/전략, 직진(코스 따라 접근)·추적(플레이어 XY 보정 접근) 구현. SO의 이동AI enum으로 선택.
- **DoD**: 같은 적 프리팹이 SO 설정만으로 직진/추적 다르게 움직임.
- **의존**: M2-2
- **문서**: enemy-design.md §3

### M3-2 · 공격 AI 모듈 (충돌/탄막/자폭) 🔴
- **목적**: enemy-design §3. 재사용 공격 모듈 2~3종.
- **작업**: 충돌(몸통 접촉 데미지)·탄막(방사/부채꼴 다발)·자폭(근접 시 범위 폭발) 구현. 발사 간격/탄속/탄막수/자폭반경 파라미터(수치 Day5).
- **DoD**: SO 설정으로 세 공격 방식이 구분 동작, 플레이어에게 데미지.
- **의존**: M2-2, M1-9
- **문서**: enemy-design.md §3

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

---

## M4 — 확장 (Should) 🟡

> 있으면 확실히 강해지는 것들. 마감 압박 시 아래→위로 잘라낸다.
> 문서: [weapon-acquisition.md](../Designs/weapon-acquisition.md), [upgrade-pool.md](../Designs/upgrade-pool.md), progression-design.md

### M4-1 · 무기 3종 (유도 미사일 / 레일건) + 동시 오토발사 🟡
- **작업**: 유도 미사일(호밍)·레일건(관통 라인) 추가, 보유 무기 동시 오토발사. weapon-acquisition 규칙 반영.
- **DoD**: 3무기가 각기 다른 발사 패턴으로 동시 발사.
- **의존**: M1-3 · **문서**: weapon-acquisition.md

### M4-2 · 무기 레벨 Lv1~4 (탄약↑) 🟡
- **작업**: 무기별 레벨업(공격력/연사/탄속/관통수 상승) 데이터·적용.
- **DoD**: 무기 레벨업 시 탄약 파워가 실제로 상승. **의존**: M4-1

### M4-3 · 무기 마일스톤 (플레이어 5레벨마다 무기 카드) 🟡
- **작업**: progression §1. 레벨 5·10·15…에 3choice로 무기 카드 최소 1개 보장.
- **DoD**: 5의 배수 레벨업 시 무기 카드가 반드시 후보에 포함. **의존**: M4-1, M1-7

### M4-4 · 실드 스킬 (전용 버튼) + 강화 3종 🟡
- **작업**: controls-design §4. 코너 전용 버튼, 실드 발동(무적/방어), 쿨다운/지속/HP 강화 3종.
- **DoD**: 버튼 탭으로 실드 발동·쿨다운 동작, 강화가 3choice에 등장. **의존**: M1-7, M1-10 · **문서**: controls-design.md

### M4-5 · 난이도 페이즈 (구간 전환 + 안내 문구) 🟡
- **작업**: progression §2. 시간축 페이즈 분할, 페이즈 내 미세 배율 상승 + 경계에서 스폰 프로파일 교체 + 배율 점프 + "공허 속 적이 더욱 강해졌습니다" HUD 안내.
- **DoD**: 시간 경과로 페이즈가 바뀌며 체감 난도 점프 + 안내 문구 표시. **의존**: M2-5, M1-10 · **문서**: progression-design.md §2

### M4-6 · 에디터 툴 2·3층 (아키타입 프로파일 + 스폰 풀 타임라인) 🟡
- **작업**: enemy-design §5. 2층 아키타입 성향 가중치(반쯤 묶기), 3층 스폰 풀 시간축 타임라인(페이즈별 프로파일·밀도·가중치 큐레이션). M4-5의 데이터 원천이 됨.
- **DoD**: 툴에서 페이즈별 스폰 프로파일을 편집→런타임 스폰에 반영. **의존**: M2-4, M4-5 · **문서**: enemy-design.md §5, progression-design.md §2

### M4-7 · 적 AI 풀세트 (사행/견제 + 조준단발) 🟡
- **작업**: 이동AI 사행·견제(호버), 공격AI 조준단발 추가(§3 4×4 완성). 9개 에셋 버킷 채우기.
- **DoD**: 이동4×공격4 모듈 전부 선택 가능·동작. **의존**: M3-1, M3-2 · **문서**: enemy-design.md §3

### M4-8 · 업그레이드 풀 풀세트 🟡
- **작업**: upgrade-pool. 탄속/최대 관통수/체력재생/오브 획득범위·가치 등 추가.
- **DoD**: 풀세트가 3choice에 반영, 카테고리별 강화 성립. **의존**: M3-4 · **문서**: upgrade-pool.md

### M4-9 · 피격/파괴 VFX (빨간 깜빡) 🟡
- **작업**: onepage 차별점. 파괴 가능 오브젝트 피격 시 빨간 깜빡임, 파괴 이펙트(JMO Assets VFX 활용 검토).
- **DoD**: 적/오브젝트 피격 시 빨간 피드백, 파괴 시 이펙트. **의존**: M1-4

### M4-10 · 로컬 하이스코어 저장 🟡
- **작업**: progression §3. PlayerPrefs 하이스코어 저장/표시.
- **DoD**: 게임오버 점수가 저장되고 최고점 표시. **의존**: M1-9

---

## M5 — 빌드 & 폴리싱 🔴/🟢

### M5-1 · 모바일 가로 Android 빌드 🔴
- **목적**: scope-tiering Must. 포폴 필수(모바일 빌드 1개).
- **작업**: Android 플랫폼 스위치, 가로(landscape) 고정, 터치 입력 실기 확인, 빌드·설치·실행.
- **DoD**: 실제 안드로이드 기기(또는 에뮬)에서 가로로 실행·플레이됨.
- **의존**: M3 완료(코어). **권장: M3 직후 1차 실행**(리스크 조기 발견), 이후 폴리싱 반영해 재빌드.
- **문서**: controls-design.md §1

### M5-2 · 데모 영상 (포폴 제출용) 🟡
- **작업**: 플레이 + **에디터 툴 오서링→게임 반영** 흐름을 담은 데모 캡처.
- **DoD**: 게임플레이 + 툴 데모가 포함된 영상 1본. **의존**: M2, M5-1

### M5-3 · 데미지 넘버 (월드스페이스 UI) 🟢
- **작업**: ui-design §3. 피격 데미지 월드스페이스 표기. **의존**: M3-2 · **문서**: ui-design.md

### M5-4 · 무기 Lv5 특수기능 3종 🟢
- **작업**: 기관총 연사+20% / 유도 범위피해 / 레일건 관통 효율. **의존**: M4-2 · **문서**: weapon-acquisition.md, scope-tiering.md(Nice)

### M5-5 · 사운드 / BGM 🟢
- **작업**: 폴리싱 단계 사운드·BGM(onepage TODO 보류분). **의존**: 없음(폴리싱)

### M5-6 · 우주 로우폴리 비주얼 폴리싱 🟢
- **작업**: 배경(Planets 3D 등)·라이팅·포스트프로세싱 폴리싱. **의존**: M1

### M5-7 · Firebase 리더보드 🟢
- **작업**: progression §3. 온라인 리더보드. **의존**: M4-10 · **문서**: progression-design.md §3

### M5-8 · 스폰 패턴 / 포메이션 (편대·웨이브 형태) 🟢
- **목적**: 적이 **랜덤 위치로만** 나오지 않고, 편대/웨이브 등 **공간적 패턴(모양)** 으로도 등장해 연출·난이도 다양화.
- **작업**: 스폰 시 개별 랜덤 위치(M1-4 기본) 외에 **공간 포메이션**(라인/V/원호/웨이브 등) 패턴 정의·롤. 스폰 위치 배치 로직 + 패턴 선택.
- **DoD**: 랜덤 스폰과 함께 최소 1~2종 포메이션 패턴으로 적이 등장.
- **의존**: M1-4 (기본 스폰) · **문서**: enemy-design.md, progression-design.md
- **비고**: 볼륨 큼 → **Nice 티어**(Firebase 리더보드 M5-7과 비슷한 후순위, 2026-08-18 사용자 요청 등재). ⚠️ **M4-6(에디터 툴 3층 스폰 타임라인)과 구분**: M4-6은 *시간축* 프로파일/밀도/가중치 큐레이션, 이 항목은 *공간적* 배치(포메이션 모양). 연계는 가능하나 별개.

---

## 크로스컷 / 미해결 (Day5 밸런싱 이관 수치 포함)

- **수치 미정(Day5)**: 이동 감도/데드존, 발사 간격, 적 스탯 전반, 레벨 임계값 곡선, 페이즈 길이/상승률/점프폭, 처치 점수값, 무기 레벨 수치. → 대부분 **에디터 툴/SO 데이터**로 관리(하드코딩 지양).
- **미해결 결정**: 실드 버튼 좌/우 옵션, 데미지 넘버 도입 여부, 월드스페이스 체력바 도입 여부(빨간 피격 연출로 대체 가능).
- **확인 필요**: 임포트 우주선 에셋 중 적/플레이어 배분. (~~Input System 활성 핸들러~~·~~R3.Unity 설치 여부~~ → M0-3에서 해소: New 단독 / 설치)

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
| 2026-08-17 | M1-2 **완료(✅)** — `VD.Player.PlayerMovement`(이동 전담) + `Player` 프리팹(StarSparrow_1_LP_Red, root=Rigidbody+PlayerMovement / 자식 `Model`). 상대 드래그(`Pointer.current`)→속도 직접 매핑, 해상도 무관 `dragGain`(현재 5), 뱅킹=자식 `Model` 회전(물리 분리), 뷰포트 선-클램프, 고정 Perspective 카메라(0,0,-26/FOV55). 사용자 튜닝 반복(감도·거리·뱅킹·떨림 수정). 기본 InputActions 템플릿 삭제(전역 참조 해제). 기술 문서 [03_PlayerMovementAndCamera.md](03_PlayerMovementAndCamera.md) 신규. 이슈 트래커 [issues.md](issues.md) 신설(`I-1` 이동 관성감 보류). |
| 2026-08-18 | M1-3 **2단계 완료(진행중 `[~]`)** — 신규 `VD.Player.PlayerAim`(FirePoint 부착): 오프셋→pitch/yaw 즉시 정렬(`PlayerBanking` 동일 공식, roll 생략, 독립 필드 28/28), 임시 조준 축 기즈모 포함. 프리팹에 `FirePoint`(root 직속, localPos 0) 추가. 컴파일 0·사용자 육안 확인. 결정: **조준 원뿔 중심축 = 뱅킹 방향(`PlayerAim`)**, **원뿔 내 적 타겟 스냅(기관총·레일건)은 M1-4로 이관**(무타겟이면 축 직사), 유도탄은 별도 호밍(M4). 다음 = 3단계 발사 로직. |
| 2026-08-18 | **M1-4 3단계 완료 → DoD 충족(`[~]` 폴리싱만 잔존)** — 플레이어 충돌 데미지: 신규 `PlayerHealth`(Player root, maxHp 100, IDamageable 미구현=아군오사 방지, OnTriggerEnter로 적 접촉→HP 감소), `Enemy.contactDamage`(10). **레이어 분리**: Player(8)/Enemy(9)/PlayerBullet(10) + 물리 매트릭스(Player×Enemy·Enemy×PlayerBullet만 ON, 자살·동종 OFF), 프리팹·씬 할당, `PlayerShooter.targetMask`=Enemy. 사용자 확인. 게임오버=HP감소만(전이 M1-9). 잔존=원뿔 튜닝·`[TEMP]` 정리. |
| 2026-08-18 | **M1-4 2단계 완료(`[~]`)** — 적 피격·사망: `VD.Core.IDamageable`(최소 TakeDamage) 신설, `Enemy` 구현(maxHp 30, HP≤0 풀 반납). 투사체 히트 = **트리거 콜라이더**(Projectile에 kinematic RB+트리거, OnTriggerEnter). 데미지 튜닝=`PlayerShooter.projectileDamage`(10). **원뿔 타겟 스냅**(매 발 nearest-in-cone, `aimConeHalfAngle`/`aimRange`, 락 아님) + 조준 원뿔 기즈모. 사용자 육안+로그 확인. 임시 `[TEMP]` 로그·기즈모·원뿔 튜닝은 나중에 정리. 남은 것 = 플레이어 충돌 데미지(M1-9). |
| 2026-08-18 | **M1-4 1단계 완료(`[~]`)** — 적 `Enemy`(-Z 직진 접근)·`EnemyPool`(상속형)·`EnemySpawner`(랜덤 위치 스폰). `Enemy.prefab`=spaceship_6(임시 스케일 6, root BoxCollider trigger). GameScene 배치. 사용자 확인. 스폰 거리/폭·카메라(−36)는 사용자 튜닝(프레이밍 조정 중). 결정: **적 속도 가변화 = 볼륨업(M3) 이관**(단일 고정 안 함). 다음 = HP/피격/충돌 데미지/투사체 히트. |
| 2026-08-18 | **M5-8 신설(🟢 Nice)** — 스폰 패턴/포메이션(편대·웨이브 등 공간적 배치). 랜덤 스폰은 M1-4, 패턴화는 후순위(Firebase M5-7급 시기). M4-6(시간축 프로파일)과 구분. 사용자 요청 등재. |
| 2026-08-18 | **M1-4 사전 결정** — 적 프리팹 소스 = `FREE Low Poly Spaceships`(spaceship_1~7). M1-4 첫 적 = **spaceship_6 1종**, 나머지 볼륨업은 M3-3. 진행 = **단계 분할(1단계 스폰+직진 접근 이동)**, 이후 HP/피격/충돌데미지(M1-3 이관). 아키타입 잠정 매핑 기록(1=고체력 돌진/탄막·대형, 4=범용/돌진, 5=자폭, 2=탄막 등) → M3-3. 구현은 착수 지시 후. |
| 2026-08-18 | M1-3 **3단계 완료 → M1-3 `[x]` 마감(a)** — 발사기 `PlayerShooter` + 투사체 `Projectile` + 상속형 풀(`VD.Core.PooledObjectPool<T>` 베이스 ← `ProjectilePool`). 튜닝(탄속·수명·발사속도) `PlayerShooter` 한 곳에 몰아 `Projectile.Launch`로 주입(사용자 결정). 임시 투사체 프리팹(Unlit) + GameScene `ProjectilePool`(prewarm 32). 사용자 육안 검증(총알 스트림·일시정지 정지). **데미지/히트는 적 필요 → M1-4 이관**(사용자 결정). 부수: Player 32-박스 콜라이더 → 단일 BoxCollider(root) 단순화(피격용, 수치·트리거는 M1-4/M1-9). 다음 = M1-4. |
| 2026-08-18 | **M1-8 완료(`[x]`)** — 최소 3choice 강화 풀. 사용자 결정: 하드코딩(enum), 효과=**능력치별 상이**, 항목 3종=**이동/최대체력/자석범위**(공격력·연사는 무기 스코프→M4). `UpgradeType` enum + `UpgradeSystem`(LevelUp 구독→`Roll(3)` Fisher–Yates→임시 자동적용), mutator 라우팅(이동=배율%, 체력·자석=가산). `Roll()`/`Apply()` public(M1-7 재사용). 검증: dragGain5→5.6·maxHp100→120·자석+2, 에러 0. 레벨업 자동적용+`[TEMP]`는 M1-7 팝업이 대체. |
| 2026-08-18 | **M1-10 완료(`[x]`)** — HUD(uGUI+TMP). 사용자 결정: 구성=**시간·점수·HP+레벨·경험치바**, HP=**게이지 바**, 텍스트=**TMP**(에센셜 임포트). `VD.UI.HudView`가 `GameEvents`(`SurvivalTime`/`Score`/`Level`/`HpNormalized`/`XpNormalized`) 구독→우상단 표시(R3 AddTo). GameScene에 HUD Canvas(스케일러 1920×1080)+TMP텍스트3+HP/XP바. `VD.Runtime` asmdef에 UI/TMP 참조 추가, TMP 에센셜(`Assets/TextMesh Pro`) 임포트. 검증: time00:58/SCORE248/Lv2/바 실시간, 에러 0. 레이아웃·색은 최소(Day5). |
| 2026-08-18 | **M1-9 완료(`[x]`)** — HP/게임오버/점수. 사용자 결정: 게임오버=**GameScene 정지형**(HP0→GameOver+`timeScale 0`+결과값 보관), 점수=**생존시간+처치점수**. `GameEvents`에 `EnemyKilled`/`HpNormalized`/`Score`/`SurvivalTime` 추가, 신규 `ScoreSystem`(시간+처치 집계), `Enemy` 처치점수 발행, `PlayerHealth` HP0→`GameOver()`. `GameManager.GameOver()` timeScale 1→0. 검증(execute_code): 생존53.1s+처치180=점수233, GameOver→ts0 프리즈, 에러 0. `[TEMP]` 로그·결과화면(ResultScene/HUD)은 M1-10/M2. 05 문서에 §종료/점수 추가. (M1-7/M1-10 위해 순서상 M1-9 선행.) |
| 2026-08-18 | **M1-6 완료(`[x]`)** — 경험치/레벨업. 사용자 결정: 상태=**GameEvents 확장**, 오브→XP=**이벤트 발행/구독**, 임계값=**지수형** `base×growth^(n-1)`. `GameEvents`에 `OrbCollected`/`Level`/`XpNormalized`/`LevelUp` 추가(발행·갱신 internal), 신규 `ExperienceSystem`(누적·임계·레벨업), `Orb`는 습득 시 `PublishOrbCollected(xpValue)`. 검증: 5개→Lv2·다음 6.5, 에러 0. `[TEMP] 레벨업` 로그는 M1-7서 대체, `Level`/`XpNormalized`는 M1-10 HUD. 기술 문서 [05_ProgressionAndEvents.md](05_ProgressionAndEvents.md) + 재사용 풀 [04_ObjectPooling.md](04_ObjectPooling.md) 신규 작성. |
| 2026-08-18 | **M1-5 완료(`[x]`)** — 오브 드랍·자석·습득. 단계 분할: (1)적 실사망 위치 드랍(`Enemy.SetDropHandler`/`EnemySpawner.DropOrb`, 화면 밖 despawn 제외) + `Orb`/`OrbPool : PooledObjectPool<Orb>`(VD.Core) + GameScene `OrbPool`(prewarm 16); (2)자석 — 반경 밖 전방(-Z) 일정속도 드리프트(못 만나면 지나쳐 despawn), 반경 내 캡처(래치)→가속 끌림, 타깃=태그"Player"로 `OrbPool` 캐시·주입(Core→Player 결합 회피, Player 프리팹 태그 부여); (3)거리 기반 습득(`pickupRadius`)→`[TEMP]` 로그+반납. 비주얼=Hovl Crystal effect green 복제(`Assets/Prefabs/Orbs/`). 각 단계 사용자 육안 확인, 컴파일/런타임 에러 0. **경험치 이벤트 발행만 M1-6 이관**(M1-5는 로그만). 속도·반경 튜닝은 나중(Day5). |
| 2026-08-19 | **M1-7 완료(`[x]`)** — 3choice 강화 선택(일시정지 팝업). `VD.UI.LevelUpPopup`(`GameEvents.LevelUp`→`GameManager.Pause()`(ts0)→`UpgradeSystem.Roll(3)` 3카드→클릭 `Apply(선택)`→`GameManager.Resume()`, **다중 레벨업 큐 순차**) + `VD.Core.UpgradeDisplay`(readonly struct) + `UpgradeSystem.Describe`(효과 수치 실제 필드 렌더=UI 하드코딩 회피). M1-8 임시 자동적용 제거. GameScene `LevelUp Canvas`(sortOrder100)+딤+3카드+`EventSystem`(InputSystemUIInputModule). **한글 폰트 = SUIT SDF** 적용(기본 LiberationSans 한글 깨짐 해소). 검증(Play): 레벨업→프리즈·3카드→클릭 스탯변경·재개·큐 순차·한글 렌더 정상. stray-close는 실버그 아님(반응 없을 때 수동 닫음). **`[TEMP]` 로그는 유지**(육안 확인용, 앞으로도 동일 방침). ⇒ **M1 코어루프 완성.** |
