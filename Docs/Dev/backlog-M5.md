# Backlog — M5 · 빌드 & 폴리싱
> 상위 허브: [backlog.md](backlog.md) | 인접: [backlog-M4.md](backlog-M4.md) ← **M5** → (없음)

## ⚡ 특이사항 (이 헤더만 읽어도 크로스 마일스톤 파악)
- **▶ 진행 순서(사용자 확정 2026-08-22)**: **타이틀 씬(M5-9) → M5-5(사운드/BGM 에셋) → 사운드/BGM 설정창(M5-10) → 밸런싱 패스(M4-5 난이도 페이즈 수치 + M4-6 스폰 프로파일 + 적 SO 튜닝) → M5-4 → M5-3 → M5-8 → M5-7 → M5-1 → M5-2**. **M5-1·M5-2는 사용자가 직접 진행**(진행 중 이슈 시 협의). **M5-6(비주얼 폴리싱)은 이 순서에서 제외**(후순위). M4-8 플레이 검증은 밸런싱 패스와 함께.
- **▶ 진행 상태(2026-08-22)**: **M5-9·M5-5·M5-10 ✅ 완료** (아래 각 항목 참조). 오디오 기술문서 = [07_AudioSystem.md](07_AudioSystem.md). **다음 = 밸런싱 패스**. 사운드 관련 열린 이슈 **[I-7](issues.md#i-7--sfx-끊김-적-파괴피격음-재생-시-기관총음-탁-끊김)**(연사 중 적 파괴음 겹침 시 기관총음 끊김, 보류).
- **상태**: 🔴/🟢 혼합. **M5-1(모바일 가로 Android 빌드) = Must**, 나머지는 대부분 🟢 Nice/데모.
- **전제(이전 M에서 옴)**: M5-1은 **M3 완료(코어)** 전제. M5-2 데모영상은 M2 툴+M5-1. M5-4는 M4-2, M5-7은 M4-10, M5-3은 M3-2.
- **핵심 방침/주의**:
  - **M5-1 모바일 빌드는 M3 직후 1차 실행 권장**(빌드 리스크 조기 발견), 이후 폴리싱 반영해 재빌드. 순서상 M4보다 앞당길 수 있음.
  - ⚠️ **M5-8(*공간적* 포메이션 모양)** 은 **M4-6(*시간축* 스폰 타임라인)과 구분**. 볼륨 커서 Nice 후순위(Firebase M5-7급).
  - 🟢 항목들은 마감 압박 시 잘라내는 폴리싱/부가.
- **문서**: controls-design.md §1(가로/터치), ui-design.md, weapon-acquisition.md, progression-design.md §3, enemy-design.md.

---

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

### M5-5 · 사운드 / BGM ✅ (진행 순서 2번, 완료 2026-08-22)
- **작업**: 폴리싱 단계 사운드·BGM(onepage TODO 보류분). **의존**: 없음(폴리싱)
- **✅ 완료**: 단일 진입점 `AudioManager`(VD.Core, DontDestroyOnLoad 싱글톤·프리팹, 3씬 배치·중복가드) — 씬별 BGM 자동전환(타이틀/결과=`bgm_01`, 게임=`bgm_02`, 2D 루프) + `SfxId`별 SFX(**3D** `PlaySfx(id,pos)` / **2D** `PlayUi(id)`). **AudioMixer** `GameAudioMixer`(Master→BGM/SFX, 노출 `MasterVol/BgmVol/SfxVol` dB). SFX 13종 배선(무기 발사·명중, 적 사망·자폭, 피격[돌진=chargeAttack / 적탄·자폭=hitPlayer, `DamageSource`로 분기·실드 흡수 시 무음], 실드, 플레이어 사망, UI 버튼[`UiButtonSfx` 호버/클릭]) + 적 사망 VFX(`Enemy.deathVfx`=CFXR3 Fire Explosion B ×1.2). 3D 재생=**빈 보이스 선택**(보이스 재배치 끊김 방지). 임포트=BGM CompressedInMemory·SFX DecompressOnLoad, 무기음 트리밍(원본 백업 `Assets/Imports/SFX_original/`). **기술문서 = [07_AudioSystem.md](07_AudioSystem.md).** 잔여 이슈 = **[I-7](issues.md#i-7--sfx-끊김-적-파괴피격음-재생-시-기관총음-탁-끊김)**(보류).

### M5-6 · 우주 로우폴리 비주얼 폴리싱 🟢
- **작업**: 배경(Planets 3D 등)·라이팅·포스트프로세싱 폴리싱. **의존**: M1

### M5-7 · Firebase 리더보드 🟢
- **작업**: progression §3. 온라인 리더보드. **의존**: M4-10 · **문서**: progression-design.md §3

### M5-9 · 타이틀 씬 ✅ (진행 순서 1번, 완료 2026-08-22)
- **목적**: TitleScene(현재 **빈 씬**)을 구성 — 게임 시작 진입점. 게임 루프(타이틀→게임→결과→타이틀) 완성.
- **작업**: 타이틀 UI(타이틀 로고/시작 버튼 등) + 시작 시 GameScene 전환(M4-10 `SceneTransition` 이클립스 와이프 재사용). 타이틀 BGM(M5-5) 연동. ResultScene의 "타이틀" 복귀 대상.
- **DoD**: 타이틀에서 시작 → 게임 플레이 → 결과 → 타이틀 복귀 루프 성립.
- **의존**: M4-10(SceneTransition·씬 흐름·HighScoreRepository). **문서**: ui-design.md
- **✅ 완료**: `TitleController`(VD.UI) — 배경 `voidDriftTitle`(전체) + 로고 `logo`(우상단 ~1/4) + `HighScoreRepository.Best` 표기(BEST SCORE 라벨 + `BestValue` 값). 하단 우측 버튼 = **게임 시작 / 환경설정(M5-10) / 게임 종료**(Sci-Fi UI `button1` 스프라이트 스왑, SUIT 폰트). 시작→`SceneTransition.TransitionTo("GameScene")`(이클립스 와이프), 종료→`Application.Quit`. `AudioManager`(M5-5)로 타이틀 BGM. **⇒ 타이틀→게임→결과→타이틀 루프 성립.** Play 검증 정상.

### M5-10 · 사운드/BGM 설정창 ✅ (진행 순서 3번, 완료 2026-08-22)
- **목적**: 볼륨 조절 설정 UI. (M5-5 사운드 시스템의 사용자 제어면.)
- **작업**: 마스터/BGM/SFX 볼륨 슬라이더 + 저장(PlayerPrefs 등). M5-5 오디오 믹서/시스템 연동. 노출 위치(타이틀 or 일시정지)는 착수 시 결정.
- **의존**: M5-5. **문서**: ui-design.md
- **✅ 완료(노출=타이틀+인게임 둘 다, 사용자 확정)**: `SettingsPanel`(VD.UI) 프리팹 1개(`Assets/Prefabs/SettingsPanel.prefab`) — Sci-Fi UI `window_transparent` + 딤 배경, **마스터/BGM/SFX 슬라이더 3종**(트랙=`progress_bar_background`·시안 필·`bar_blue` 핸들) + **퍼센트 표기** + `button1` 닫기. 값=`AudioManager.SetMaster/Bgm/SfxVolume`(실시간) + **PlayerPrefs 영속**(닫을 때 Save), 시작 시 `AudioManager`가 저장값 로드·적용. **타이틀**: 게임시작·게임종료 사이 "환경설정" 버튼(정지 없음). **인게임**: HUD 좌하단 **기어 버튼**(`button1` + `Settings_Simple_Icons_UI` 기어) → 열면 `GameManager.Pause()`(timeScale 0), 닫으면 `Resume()`. 버튼→패널 연결 = `SettingsOpener`(VD.UI, `pauseGameWhileOpen` 인스턴스별). Play 검증: 타이틀/인게임 열기·동기화·%·일시정지·재개 정상. **세부(딤 농도·기어 크기/색·슬라이더 두께)=사용자 조정.**

### M5-8 · 스폰 패턴 / 포메이션 (편대·웨이브 형태) 🟢
- **목적**: 적이 **랜덤 위치로만** 나오지 않고, 편대/웨이브 등 **공간적 패턴(모양)** 으로도 등장해 연출·난이도 다양화.
- **작업**: 스폰 시 개별 랜덤 위치(M1-4 기본) 외에 **공간 포메이션**(라인/V/원호/웨이브 등) 패턴 정의·롤. 스폰 위치 배치 로직 + 패턴 선택.
- **DoD**: 랜덤 스폰과 함께 최소 1~2종 포메이션 패턴으로 적이 등장.
- **의존**: M1-4 (기본 스폰) · **문서**: enemy-design.md, progression-design.md
- **비고**: 볼륨 큼 → **Nice 티어**(Firebase 리더보드 M5-7과 비슷한 후순위, 2026-08-18 사용자 요청 등재). ⚠️ **M4-6(에디터 툴 3층 스폰 타임라인)과 구분**: M4-6은 *시간축* 프로파일/밀도/가중치 큐레이션, 이 항목은 *공간적* 배치(포메이션 모양). 연계는 가능하나 별개.
