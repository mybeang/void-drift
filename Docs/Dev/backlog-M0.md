# Backlog — M0 · 부트스트랩 & 스모크 테스트
> 상위 허브: [backlog.md](backlog.md) | 인접: (없음) ← **M0** → [backlog-M1.md](backlog-M1.md)

## ⚡ 특이사항 (이 헤더만 읽어도 크로스 마일스톤 파악)
- **상태**: ✅ **M0-1~M0-4 전부 완료**(2026-08-17).
- **전제(이전 M에서 옴)**: 없음(첫 마일스톤). 사용자 사전작업 = Unity 프로젝트 생성 + 3D 로우폴리 에셋 소싱(`Assets/Imports/`) 완료가 전제.
- **이후로 이관**:
  - 구체 입력 액션/터치 오서링 → **M1-2**(M0-3은 "New Input System 단독"까지만 결정).
  - `Data` 폴더·ScriptableObject → **M2**. `Core/Interface`·`*/Struct` 등 세부 폴더는 필요 시 생성.
  - Loading = 별도 씬 아님 → GameScene 오버레이+FadeOut(방침만, 구현은 **M2 Addressables 이후**).
- **이후 M이 여기서 확인할 것**:
  - 어셈블리 구조 `VD.Runtime`/`VD.Editor`(ns 루트 `VD.*`, 총알=Player/Enemy 내부) = 상세 [01_AssemblyDefinition.md](01_AssemblyDefinition.md). 커스텀 asmdef엔 autoReferenced 패키지도 **수동 참조** 필요.
  - 씬 3개 = TitleScene(build 0)/GameScene(1)/ResultScene(2). `SampleScene`은 빌드 제외(테스트 전용).
  - `VDEditorMarker`(`Editor/`)는 **M2 에디터 실코드 붙을 때 삭제** 예정(그전까지 VD.Editor 검증 유일 수단).
- **핵심 방침/주의**:
  - 입력 = **New Input System 단독**(`activeInputHandler:1`, 레거시 `Input.*` 금지).
  - 리액티브/비동기 스택 = R3 코어 + R3.Unity(`AddTo`) + UniTask **확정·설치**.
  - MCP = CoplayDev `com.coplaydev.unity-mcp`, HTTP `127.0.0.1:8080`, `.mcp.json` 수동 등록. **Unity 재부팅 시** 서버 Start → Claude MCP 재연결.

---

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
