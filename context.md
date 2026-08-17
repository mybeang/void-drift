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
   - 개발·이슈(`Docs/Dev/`): [backlog.md](Docs/Dev/backlog.md)(구현 태스크·이슈 트래킹), [01_AssemblyDefinition.md](Docs/Dev/01_AssemblyDefinition.md)(asmdef 어셈블리 구조·이유) 등 기술 설계 문서

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

**현재 상태**: 기획 완료(`Docs/Designs/` 세트). Unity 프로젝트·에셋 셋업 완료(커밋 `에셋 선정 및 기본 셋팅 완료`). **Backlog 작성 완료** → [Docs/Dev/backlog.md](Docs/Dev/backlog.md) (M0~M5). 아직 게임 코드 0줄.

**개발 전 순서** (전부 완료):
1. ~~Unity 프로젝트 생성~~ ✅ (사용자)
2. ~~3D 로우폴리 에셋 소싱·삽입~~ ✅ (사용자, `Assets/Imports/`)
3. ~~Backlog + 상세 명세 ListUp~~ ✅ → [Docs/Dev/backlog.md](Docs/Dev/backlog.md)

**M0 진행**:
- **M0-1 (Unity MCP 연결)** ✅ 완료 — CoplayDev MCP for Unity, HTTP `127.0.0.1:8080`, `.mcp.json` 등록·왕복 검증.
- **M0-2 (큐브 회전 스모크)** ✅ **완료(재작업)** — 물리(Rigidbody+angularVelocity) Z축 회전, 인스펙터(속도/크기/방향), `SmokeCube` 재사용. 사용자 육안 확인. `Assets/Scripts/Smoke/CubeSpinner.cs`. (1차 임의구현은 폐기 → 재작업, 사유는 backlog M0-2 참조.)
- **M0-3 (입력 백엔드 & R3.Unity 판단)** ✅ **완료** — 사용자 결정: 입력 = **New Input System**(`com.unity.inputsystem` 1.20.0, `activeInputHandler:1` New 단독), R3.Unity = **설치**(`com.cysharp.r3` 1.3.1 git UPM). MCP로 설치·검증(컴파일 에러 0, 왕복 정상). 구체 입력 액션 오서링은 M1-2에서. 상세는 backlog M0-3 결론.
- **M0-4 (프로젝트 골격)** ✅ **완료** — asmdef 2개 `VD.Runtime`/`VD.Editor`(네임스페이스 루트 `VD.*`), 폴더 `Scripts/{Core,Player,Enemy,UI,Editor}`, 씬 3개(Title/Game/Result). 리플렉션 검증·컴파일 0. 기술 문서 [Docs/Dev/01_AssemblyDefinition.md](Docs/Dev/01_AssemblyDefinition.md). 파일/네임스페이스 규칙은 해당 문서·backlog M0-4 참조.
- **다음(대기)**: **M1 코어 루프** (M1-1 게임 상태 골격부터). **§1-9에 따라 사용자 지시 후 진행** — 자동 착수 금지. **M0 전부 완료.**

> ⚠️ 인계 주의: `SampleScene`의 `SmokeCube`에 **Rigidbody가 에디터에서 추가된 상태(씬 미저장일 수 있음)**. 다음 세션에서 SmokeCube 다룰 때 씬 저장 여부 확인. CubeSpinner는 `[RequireComponent(typeof(Rigidbody))]`.

**⚠️ 설치 상태 실측(2026-08-17)**: UniTask ✅ / R3 코어 1.3.1 ✅(NuGet) / R3.Unity 통합 ✅설치(`com.cysharp.r3` 1.3.1) / Addressables ❌미설치(M2에서) / MCP ✅설치완료 / Input System ✅New 단독(`com.unity.inputsystem` 1.20.0, handler 1). 상세는 backlog §0.

**Backlog 유지 원칙**: [scope-tiering.md](Docs/Designs/scope-tiering.md)는 티어 수준, backlog는 구현 태스크 단위(DoD 포함). **갱신은 §1-8에 따라 사용자 확인 후.**

## 5. 유지보수

- 규칙·약속 변경 → §1 갱신
- 새 기획 문서 추가 → onepage §0 인덱스 등록 + 필요 시 §2 갱신 / 새 개발·이슈 문서 → `Docs/Dev/`
- 핵심 사실 변경 → §3 갱신
- 진행 상태 변경(단계 완료 등) → §4 갱신
