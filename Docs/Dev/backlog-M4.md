# Backlog — M4 · 확장 (Should)
> 상위 허브: [backlog.md](backlog.md) | 인접: [backlog-M3.md](backlog-M3.md) ← **M4** → [backlog-M5.md](backlog-M5.md)

## ⚡ 특이사항 (이 헤더만 읽어도 크로스 마일스톤 파악)
- **상태**: 🟡 Should. **있으면 확실히 강해지는 것들. 마감 압박 시 아래→위로 잘라낸다.**
- **진행(2026-08-21)**: ✅ **M4-1 완료**(무기 3종 전략모듈+전용풀+동시발사). ✅ **M4-2 완료**(무기 레벨 Lv1~4 = 탄약↑ — 공통 `WeaponBase` 레벨 머신). ✅ **M4-3 완료**(시작 로드아웃 기관총만 + 5레벨 마일스톤 3choice 무기 카드 — `PlayerShooter` 무기 슬롯 API + `UpgradeSystem` 마일스톤 롤 + 무기 SO 3종). 다음 = **M4-4**(실드 스킬). 이슈 **I-4**(탄막 무제한 발사) 열림.
- **전제(이전 M에서 옴)**: M1 코어(사격 M1-3·3choice M1-7·HUD M1-10·게임오버 M1-9), M2 툴(M2-4/M2-5), **M3 전부 완료**(이동/공격 AI M3-1/M3-2 · 적 12종 로스터 M3-3 · 3choice 데이터화+오서링 툴 M3-4). 각 태스크 **의존** 참조.
- ⚠️ **M3에서 M4 일부 선구현됨(재작업 금지)**:
  - **M4-7**: 사행(`WeaveMove`)·조준단발(`AimedShot`)은 **M3-3에서 선반영** → **M4-7 잔여 = 견제(Hover)뿐**(현재 직진 폴백).
  - **M4-8**: **공격력(기초 공격력)은 M3-4에서 공용 강화로 구현**(`PlayerShooter.AddAttackPower`) → M4-8은 무기 3종 + 무기별 배율/파워(연사/탄속/관통)·실드부터. `UpgradeDefinition` SO + Upgrade Authoring 툴도 이미 있음(풀세트 확장만).
  - **M4-5/M4-6 데이터 원천**: 적 로스터=티어(`Enemy_{라인}_T{1-3}`), 시간 게이팅=M4-6(현재 스폰 정적·티어 가중). 밸런싱 수치 전부 시작점(M4-5 난이도 그래프 뒤 튜닝).
- **이후로 이관 / 인접 구분**:
  - 무기 **Lv5 특수기능 3종** → **M5-4**(Nice). M4-2는 Lv1~4까지.
  - **M4-6(에디터 툴 3층 = *시간축* 스폰 프로파일/밀도/가중치)** ⚠️ **M5-8(*공간적* 포메이션 모양)과 구분.** 별개.
  - 데미지 넘버 → M5-3(Nice), Firebase 리더보드 → M5-7(M4-10 로컬 하이스코어 위에).
- **이후 M이 여기서 확인할 것**:
  - M4-1 무기 종류·M4-2 무기 레벨 → **M5-4 Lv5 특수기능**의 전제.
  - M4-10 로컬 하이스코어(PlayerPrefs) → **M5-7 Firebase 리더보드**가 확장.
  - M4-6 스폰 타임라인은 M2-4 툴 + M4-5 난이도 페이즈에 의존/연동.
- **핵심 방침/주의**:
  - 무기: 유도(호밍)는 조준 원뿔과 무관한 별도 로직. 동시 오토발사(weapon-acquisition 규칙).
  - 수치(무기 레벨·페이즈 길이/상승/점프폭) = Day5, 가능하면 SO/툴 데이터.
- **문서**: [weapon-acquisition.md](../Designs/weapon-acquisition.md), [upgrade-pool.md](../Designs/upgrade-pool.md), [controls-design.md](../Designs/controls-design.md), progression-design.md.

---

### M4-1 · 무기 3종 (유도 미사일 / 레일건) + 동시 오토발사 ✅ 완료(2026-08-20)
- **작업**: 유도 미사일(호밍)·레일건(관통 라인) 추가, 보유 무기 동시 오토발사. weapon-acquisition 규칙 반영.
- **DoD**: 3무기가 각기 다른 발사 패턴으로 동시 발사. → **충족**(기관총+유도+레일건 동시 발사, Play 검증).
- **의존**: M1-3 · **문서**: weapon-acquisition.md
- **구현 요약**(기술문서는 M4 완료 시 정리):
  - **전략 모듈 패턴**(적 AI와 동형): `IWeapon.Tick(dt, WeaponContext)` — `PlayerShooter`가 오케스트레이터로 보유 무기 리스트를 매 프레임 틱. 공용(발사원점·base데미지·타겟질의·풀)은 `WeaponContext`. 위치=`Assets/Scripts/Player/Weapons/`.
  - **무기 3종**: `StraightGun`(원뿔 스냅 직진 단일히트) · `HomingMissile`(가속·선회 유도) · `Railgun`(원뿔 스냅 초고속 **관통**+데미지감쇠). 각자 전용 풀(`ProjectilePool`/`HomingProjectilePool`/`RailProjectilePool`, 프리팹 분리).
  - **유도 조준 = 타입 1순위(원거리 Shooter 우선)→동급 중 가장 먼 것**(사용자 결정, 원뿔 무관). `Enemy.Archetype` 노출+빌더 주입으로 분류. Aim 사거리=인스펙터(후일 플레이어 셋팅 툴). 발사=**날개 4발사대**(`homingHardpoints`), **탄약 수만큼 동시**(순환 아님). 미사일 모델=`MissileViking`.
  - **레일건 궤적 VFX**=프리팹 TrailRenderer(청록, `RailTrail_Mat`=Sprites/Default). 관통수(`maxPierce`)·감쇠(`damageDecay`) 인스펙터. 탄약=수직 병렬 줄기.
  - **탄약(동시 발사 줄기 수)** = `HomingMissile.Ammo`/`Railgun.Ammo` 프로퍼티로 준비, **레벨 연동은 M4-2**. 현재 인스펙터 수동값(`homingAmmo`/`railAmmo`).
  - **현재 데모 상태 = 3종 전부 보유**(패턴 시연용). **시작 로드아웃(기관총만)+마일스톤 획득 = 구 Step5 폐기 → M4-3 흡수**(사용자 결정 2026-08-20 — 지금 임시구현 후 M4-3서 걷어내는 낭비 회피).
  - VFX **UnityMCP 생성·부착 검증됨**(manage_components add + manage_vfx trail_set_* / 파티클 프리팹 자식 부착).
  - **연계 이슈**: [issues.md](issues.md) **I-4**(탄막 적 무제한 발사 → 교전 라인 게이팅, 열림).

### M4-2 · 무기 레벨 Lv1~4 (탄약↑) ✅ 완료(2026-08-21)
- **작업**: 무기별 레벨업(Lv1~4 = 탄약=동시 발사 줄기 수 상승) 데이터·적용. → **충족**(Play 검증, 특이사항 없음).
- **DoD**: 무기 레벨업 시 탄약 파워가 실제로 상승. **의존**: M4-1
- **결정(사용자, 2026-08-21)**: ①레벨 모델 = **명시 `Level` 필드 + 레벨→탄약 매핑**(향후 M4-3 재픽업·M5-4 특수기능 판정용). ②설계 §3대로 **기관총도 Lv1~4 멀티샷** — 배치=**평행 오프셋**(레일건식, 조준 축에 수직 나란히). ③검증 트리거 = **인스펙터 시작 레벨 필드**(런타임 레벨업 흐름은 M4-3).
- **구현 요약**:
  - **공통 레벨 머신** `WeaponBase : IWeapon`(`Assets/Scripts/Player/Weapons/WeaponBase.cs`) — `Level`(1~`MaxLevel`)·`MaxLevel`(=4, Lv5 특수는 M5-4서 5로)·`IsMaxLevel`·`LevelUp()`(포화 클램프), `Ammo => min(Level, MaxAmmo=4)`. 3무기가 상속(`sealed override Tick`).
  - `IWeapon`에 레벨 API(`Level`/`MaxLevel`/`IsMaxLevel`/`LevelUp`) 추가 → M4-3가 `List<IWeapon>` 폴리모픽으로 최대치 판정·레벨업 구동.
  - **기관총**(`StraightGun`): 평행 오프셋 멀티샷(`streamSpacing`, 가운데 정렬). Lv1=1발이라 **기존 동작 무변화**(회귀 없음).
  - **유도/레일**: 기존 수동 `_ammo`/`public Ammo` 제거 → 레벨=탄약(유도=발사대 수, 레일=레일 줄기 수).
  - **PlayerShooter**: 인스펙터 `homingAmmo`/`railAmmo`(수동 탄약) → **`straightStartLevel`/`homingStartLevel`/`railStartLevel`**(1~4) + 기관총 `straightStreamSpacing`. 생성자에 시작 레벨 주입.
  - **M4-3 레벨업 훅** = `IWeapon.LevelUp()`(구 "Ammo 세터" 대체). 무기카드 3choice가 미보유=추가·보유=`LevelUp`, `IsMaxLevel`이면 풀 제외.

### M4-3 · 무기 마일스톤 (플레이어 5레벨마다 무기 카드) ✅ 완료(2026-08-21)
- **작업**: progression §1. 레벨 5·10·15…에 3choice로 무기 카드 최소 1개 보장. **+ 시작 로드아웃(기관총만)으로 전환**(구 M4-1 Step5 흡수). → **충족**(Play 검증, 특이사항 없음).
- **DoD**: 5의 배수 레벨업 시 무기 카드가 반드시 후보에 포함. **의존**: M4-1, M1-7
- **결정(사용자, 2026-08-21)**: 무기 카드 통합 = **기존 `UpgradeDefinition` 풀에 무기 타입 추가**(별도 추상화 아님) — 팝업 계약 무변화, 오서링 툴에서 가중치 편집, 마일스톤·최대치·획득/Lv업 표시는 무기 타입에 한해 코드 특수 처리.
- **구현 요약**:
  - **①시작 로드아웃**: `PlayerShooter.Awake`가 **기관총만** `Acquire`(weapon-acquisition §2). 무기 슬롯 API 신설 — `_owned`(Dictionary<WeaponId,IWeapon>)·`BuildWeapon` 팩토리·`HasWeapon`/`WeaponLevel`/`IsWeaponMaxed`/`AcquireOrLevelUp`. `WeaponId`(Straight/Homing/Railgun) enum 신규. (죽은 `homingStartLevel`/`railStartLevel` 인스펙터 필드 제거, `straightStartLevel`·`straightStreamSpacing`는 유지.)
  - **②마일스톤 카드**: `UpgradeSystem.Roll(count, playerLevel)` — `playerLevel % 5 == 0`이면 **무기 카드 1개 가중 보장 + 나머지 일반 롤**, 아니면 무기 카드 배제. `IsEligible`(무기=마일스톤 전용·Lv4 제외 / 스탯=maxStacks), `PickWeighted` 헬퍼, `TryWeaponId` 매핑. `Apply`→`shooter.AcquireOrLevelUp`, `Describe`→미보유 "획득 (Lv1)"/보유 "Lv{n}→Lv{n+1}" 동적.
  - **UpgradeType**: `WeaponStraight/Homing/Railgun`(6/7/8) 추가. **LevelUpPopup**: `Queue<int>` 레벨 큐로 전환, 레벨값을 `Roll`에 전달(기존엔 버렸음).
  - **데이터/씬**: `Upgrade_Weapon_{Straight,Homing,Railgun}` SO 3종(`ScriptableObjects/Data/`, weight 1, 값 필드 미사용) + `UpgradeSystem.pool` **9개**(스탯 6 + 무기 3) 배선.
  - **잔여**: 무기 카드 가중치 초기값·마일스톤 자격 3개 미만 엣지(§8)는 Day5. 무기 파워(연사/탄속/관통) 별도 카드 = M4-8.

### M4-4 · 실드 스킬 (전용 버튼) + 강화 3종 🟡
- **작업**: controls-design §4. 코너 전용 버튼, 실드 발동(무적/방어), 쿨다운/지속/HP 강화 3종.
- **DoD**: 버튼 탭으로 실드 발동·쿨다운 동작, 강화가 3choice에 등장. **의존**: M1-7, M1-10 · **문서**: controls-design.md

### M4-5 · 난이도 페이즈 (구간 전환 + 안내 문구) 🟡
- **작업**: progression §2. 시간축 페이즈 분할, 페이즈 내 미세 배율 상승 + 경계에서 스폰 프로파일 교체 + 배율 점프 + "공허 속 적이 더욱 강해졌습니다" HUD 안내.
- **DoD**: 시간 경과로 페이즈가 바뀌며 체감 난도 점프 + 안내 문구 표시. **의존**: M2-5, M1-10 · **문서**: progression-design.md §2
- **아키텍처(M2-5서 확정)**: 이 시스템이 **전역 스탯 배율(체력/속도/데미지)의 소스** — M2-5 적 빌더의 `StatScaler`가 스폰 시 여기에 "현재 배율?"을 질의해 base 스탯에 곱함(effective 주입). base=적 테이블(RO), 배율=여기 곡선 데이터, effective=런타임. killScore는 배율 밖. M2-5에선 이 소스가 **스텁(배율 1.0)**, 실배율·페이즈는 여기서 구현.
- **🆕 아이디어 등재(2026-08-20, 사용자)**: **배율 곡선 데이터(페이즈 길이·내부 상승률·경계 점프폭 등)를 에디터 툴로 편집** — 밸런싱 데이터이므로. 재사용 베이스 `SoTableEditorView<T>`(M2-3) 위에 도메인 에디터로 올리고, **"통합 튜닝 창" 비전**([backlog-M2.md](backlog-M2.md) M2-3 문서란)에 난이도 배율 곡선도 포함. M4-6(스폰 프로파일 툴)과 인접·연계. 후속(난이도 시스템과 함께), M2-5 아님.

### M4-6 · 에디터 툴 2·3층 (아키타입 프로파일 + 스폰 풀 타임라인) 🟡
- **작업**: enemy-design §5. 2층 아키타입 성향 가중치(반쯤 묶기), 3층 스폰 풀 시간축 타임라인(페이즈별 프로파일·밀도·가중치 큐레이션). M4-5의 데이터 원천이 됨.
- **DoD**: 툴에서 페이즈별 스폰 프로파일을 편집→런타임 스폰에 반영. **의존**: M2-4, M4-5 · **문서**: enemy-design.md §5, progression-design.md §2
- **인접**: 난이도 **배율 곡선 데이터**를 툴로 편집하는 아이디어는 M4-5에 등재(둘 다 `SoTableEditorView<T>` 재사용·통합 튜닝 창 비전 계열).

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
