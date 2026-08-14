# Void Drift (공허 속으로) — 원페이지 기획서

> 포트폴리오용 프로젝트. 그라비티 [판교] DevHub Unity 클라이언트 공고 대응 목적.
> Claude + UnityMCP를 개발 가속 도구로 활용(비교실험 없음). 본 문서는 브레인스토밍 정리본.

## 0. 문서 인덱스 (빠른 탐색)

> ⚠️ 작업 진행 전 **[context.md](../../context.md) (레포 루트) 먼저 확인**. 아래는 `Docs/Designs/` 세부 문서 맵.

| 문서 | 한 줄 요약 |
|---|---|
| [context.md](../../context.md) (레포 루트) | **최우선 확인** — 작업 규칙·약속, 문서 우선순위, 핵심 사실 |
| onepage-design.md | (이 문서) 허브 — 개요·컨셉·문서 인덱스·TODO |
| [upgrade-pool.md](upgrade-pool.md) | 3choice 업그레이드 풀 목록 (무기/탄약파워/기동/생존/유틸/실드) |
| [weapon-acquisition.md](weapon-acquisition.md) | 무기 획득·레벨(Lv1~5)·동시 오토발사·3choice 등장 자격 규칙 |
| [enemy-design.md](enemy-design.md) | 조합형 적(비주얼×이동AI×공격AI×스탯)·유효성 경고·에디터 3층·Addressables |
| [controls-design.md](controls-design.md) | 모바일 가로·상대드래그 이동·오토사격·실드 전용버튼 |
| [ui-design.md](ui-design.md) | 런타임=uGUI / 에디터=UI Toolkit 분리 채택 |
| [progression-design.md](progression-design.md) | 3choice 리듬(점증형·일시정지)·난이도 곡선(시간/페이즈)·종료/점수 |
| [scope-tiering.md](scope-tiering.md) | MVP 티어링 (Must / Should / Nice) |

## 1. 개요

- **타이틀**: **Void Drift** (한글 부제: "공허 속으로")
- **장르**: 3D 로우폴리 모바일 비행슈팅 + 로그라이트 즉석 성장(3choice)
- **플랫폼**: 모바일 (터치, 가로/landscape) — [controls-design.md](controls-design.md)
- **개발 기간**: 이상적 스프린트 5일 + 문서화 1일. **실제 마감: 2026-08-31** (그 전까지 포폴 정리 수준이면 OK). → 기획 풍부함 ≠ 전부 구현, **스코프 티어링**으로 조절 → [scope-tiering.md](scope-tiering.md)
- **목적**: 공고 업무내용 1순위인 "유니티 에디터 커스텀 툴 개발"의 포트폴리오 공백을 메우고, 우대요건 "AI 활용 효율화 경험"을 직접 증명

## 2. 한 줄 컨셉

무한히 전진하는 우주 코스를 자유롭게 기동하며 사격하고, 적을 처치해 얻은 자원으로 즉석 3택 강화를 골라 성장하는 모바일 로그라이트 비행슈팅.

## 3. 핵심 재미 *(초안)*

정면 사격 + 자유 기동의 슈팅 손맛과, 자원을 모을 때마다 터지는 3택 강화 선택의 로그라이트 성장감이 교차하는 것. 두 재미 축(조작 스킬 vs 빌드 선택)이 동시에 굴러가는 게 핵심.

## 4. 게임플레이 루프

이동(자유 XY, Z축 고정) + 정면 **오토 사격** → 적(조합형: 탄막형·돌진형·자폭형)과 조우 → 파괴 시 빨간 피격 연출 + 자원(오브) 드랍 → 오브 일정량 습득(경험치) → 레벨업마다 3choice 강화 선택(일시정지) → 스탯/무기 성장 → 시간 기반 난이도 상승(페이즈, 무한 반복) → 종료(HP 0) 시 생존시간/점수 기록.

## 5. 세계관

우주 공간을 뚫고 전진하는 소형 우주선 시점. 별도 스토리텔링 없이 "코스를 헤쳐나가는 서바이벌" 자체가 컨셉. (스토리 확장은 스코프 아웃)

## 6. 핵심 시스템 / 차별점

- **자유기동 + 원근 코스 슈팅**: 상하좌우 자유이동, Z축 고정 무한 전진 — 일반적인 탑다운/스크롤 슈팅과 다른 구도
- **3choice 즉석 성장**: 오브 습득 시 3개 강화 선택지 중 1개 즉시 적용 (뱀서라이크 문법 차용)
- **파괴 연출 구분**: 파괴 가능 오브젝트만 피격 시 빨간 깜빡임 → 플레이어에게 명확한 피드백
- **에디터 커스텀 툴** (핵심 어필): UI Toolkit 기반. **적 조합 오서링**(비주얼×이동AI×공격AI×스탯) + **유효성 경고**(교전거리 모순·라벨-AI 모순) + 아키타입 프로파일 + 스폰 풀(시간축). ScriptableObject DB + Addressables 연동. 공고 1순위 정면 대응 + 이전 인프라 자동화(CMDB) 검증 경력과 서사 연결 → [enemy-design.md](enemy-design.md)
  - 주의: **3choice 업그레이드 풀은 이 툴과 분리**(다른 도메인, 관심사 분리). 필요 시 별도 소형 데이터 → [weapon-acquisition.md](weapon-acquisition.md)

## 7. 콘텐츠 카테고리 *(확정본은 세부 문서 참조)*

- **업그레이드 풀**: 무기(기관총/유도미사일/레일건, 레벨형 Lv1~5) + 탄약 파워(공격력/연사/탄속/관통수) + 기동(이동속도) + 생존(최대체력/재생) + 유틸(오브 범위/가치) + 실드 스킬 → [upgrade-pool.md](upgrade-pool.md)
- **적 유형**: 조합형 — 아키타입(탄막/돌진/자폭) × 이동 AI 4 × 공격 AI 4 → [enemy-design.md](enemy-design.md)

> ⚠️ 초안(구버전)의 방사탄·피격판정 축소·탄수(멀티샷) 파워 등은 **폐기/변경**됨. 위 문서가 최신.

## 8. 개발 일정 *(초안, Day 단위 개략 — 우선순위는 [scope-tiering.md](scope-tiering.md) 기준)*

| Day | 내용 |
|---|---|
| 1 | 코어 무브먼트·사격·적/장애물 스폰 프로토타입, 3choice 기본 구조 |
| 2 | 에디터 커스텀 툴(UI Toolkit 테이블) 1차 구현 + 밸런싱 데이터 연동 |
| 3 | 적 유형/패턴 확장, 업그레이드 풀 확장, 피격 VFX |
| 4 | 우주 로우폴리 에셋 적용, HUD(생존시간/점수), 비주얼 폴리싱 |
| 5 | 버그 수정, 밸런스 튜닝, 모바일 빌드 테스트, 데모 영상 촬영 |
| 6(문서) | README, 기술문서, 포트폴리오 정리 |

## 9. 기술 스택

- Unity (모바일 빌드/Android, 가로), C#
- Claude + UnityMCP (개발 가속 도구, A/B 비교실험 없음)
- **UI Toolkit**(에디터 커스텀 툴) / **uGUI**(런타임 인게임 UI) — 분리 채택 → [ui-design.md](ui-design.md)
- **Addressables** (적 에셋 관리: Group/Label + ScriptableObject)
- 로컬 저장(PlayerPrefs) / Firebase 리더보드(Nice 티어)
- 3D 로우폴리 에셋 (사전 소싱 예정)

## 10. 레퍼런스

없음 — 직접 구상. (구도는 유사 3D 전방 슈팅류, 성장 시스템은 로그라이트류 문법 차용)

## 11. 남은 TODO

- [x] 업그레이드 풀 최종 목록 확정 → [upgrade-pool.md](upgrade-pool.md) (수치는 Day5 튜닝으로 이관)
- [x] 적 유형 최종 목록/패턴 확정 → [enemy-design.md](enemy-design.md) (조합형 적 + 에디터 오서링, 수치는 Day5)
- [x] 조작 디테일(터치 드래그 방식) 확정 → [controls-design.md](controls-design.md) (모바일/가로/상대드래그/오토사격/스킬버튼)
- [x] 무기 "추가" 획득 방식 확정 → [weapon-acquisition.md](weapon-acquisition.md) (동시 오토발사 + 3choice 등장 자격 규칙)
- [x] ~~더블탭 입력 시스템~~ → 실드를 전용 버튼으로 변경하여 해소 (controls-design.md)
- [x] 인게임 UI 방식 결정 → [ui-design.md](ui-design.md) (런타임=uGUI, 에디터=UI Toolkit)
- [x] **레벨/진행 디자인** → [progression-design.md](progression-design.md) (3choice 리듬·난이도 곡선·종료/점수)
- [~] 3D 로우폴리 에셋 소싱 — **보류(나중)**: 사용자가 Unity 프로젝트에 직접 추가 후 논의
- [~] 사운드/BGM 방향 — **보류(나중)**: 폴리싱 단계에서
- [x] 타이틀 확정 → **Void Drift** (한글 부제 "공허 속으로")
- [ ] 레포 생성 후 본 문서를 시작점으로 누적 기획 진행