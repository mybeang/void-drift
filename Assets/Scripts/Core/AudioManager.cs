using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

namespace VD.Core
{
    /// <summary>효과음 종류(M5-5). AudioManager 인스펙터의 클립과 1:1 매핑.</summary>
    public enum SfxId
    {
        BasicGun,       // 기관총 발사
        Missile,        // 유도 미사일 발사
        Railgun,        // 레일건 발사
        ChargeAttack,   // 돌진 적 접촉 피격
        SelfExplosion,  // 자폭 적 폭발(적 위치)
        PlayerDead,     // 플레이어 사망 폭발
        EnemyDead,      // 적 실사망(적 위치)
        HitPlayer,      // 적탄/자폭에 의한 피격
        MissileHit,     // 유도 미사일 명중
        RailgunHit,     // 레일건 명중
        ShieldOn,       // 실드 발동
        ButtonClick,    // UI 버튼 클릭
        ButtonHover,    // UI 버튼 호버
    }

    /// <summary>
    /// 오디오 총괄(M5-5). DontDestroyOnLoad 싱글톤 — 첫 씬(TitleScene)에 1개 배치되어 전 씬 유지.
    /// <para><b>BGM</b>: 씬에 따라 자동 전환(게임 씬=gameBgm, 그 외=titleBgm), 2D 루프. <b>SFX</b>: <see cref="SfxId"/>별 클립을
    /// 3D(<see cref="PlaySfx(SfxId, Vector3)"/>) 또는 2D(<see cref="PlayUi"/>)로 재생. 3D는 풀에서 <b>빈 보이스</b>를 골라 재생.</para>
    /// <para>볼륨은 <see cref="AudioMixer"/> 노출 파라미터(MasterVol/BgmVol/SfxVol, dB)로 제어 — M5-10 설정창이 사용.</para>
    /// </summary>
    public sealed class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("믹서 (GameAudioMixer)")]
        [SerializeField] AudioMixer mixer;
        [Tooltip("BGM 그룹 이름(믹서 하위)")]
        [SerializeField] string bgmGroupName = "BGM";
        [Tooltip("SFX 그룹 이름(믹서 하위)")]
        [SerializeField] string sfxGroupName = "SFX";

        [Header("BGM")]
        [SerializeField] AudioClip titleBgm;
        [SerializeField] AudioClip gameBgm;
        [Tooltip("이 이름의 씬에서만 gameBgm, 그 외엔 titleBgm")]
        [SerializeField] string gameSceneName = "GameScene";

        [Header("SFX 클립 (SfxId 순서)")]
        [SerializeField] AudioClip basicGun;
        [SerializeField] AudioClip missile;
        [SerializeField] AudioClip railgun;
        [SerializeField] AudioClip chargeAttack;
        [SerializeField] AudioClip selfExplosion;
        [SerializeField] AudioClip playerDead;
        [SerializeField] AudioClip enemyDead;
        [SerializeField] AudioClip hitPlayer;
        [SerializeField] AudioClip missileHit;
        [SerializeField] AudioClip railgunHit;
        [SerializeField] AudioClip shieldOn;
        [SerializeField] AudioClip buttonClick;
        [SerializeField] AudioClip buttonHover;

        [Header("3D 설정")]
        [Tooltip("이 거리 안에선 최대 음량")]
        [SerializeField] float sfxMinDistance = 6f;
        [Tooltip("이 거리 밖에선 최소 음량")]
        [SerializeField] float sfxMaxDistance = 70f;
        [Tooltip("동시 재생 3D 보이스 수(빈 보이스 우선 선택)")]
        [SerializeField] int sfxVoices = 24;

        AudioSource _bgmSource;
        AudioSource _uiSource;         // 2D UI
        AudioSource[] _sfxPool;        // 3D 보이스(각자 한 클립씩 Play)
        float[] _busyUntil;            // 보이스별 재생 종료 예정 시각(unscaled). 이하이면 빈 보이스
        AudioMixerGroup _sfxGroup;
        Dictionary<SfxId, AudioClip> _clips;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            AudioMixerGroup bgmGroup = FindGroup(bgmGroupName);
            _sfxGroup = FindGroup(sfxGroupName);

            _bgmSource = gameObject.AddComponent<AudioSource>();
            _bgmSource.loop = true;
            _bgmSource.playOnAwake = false;
            _bgmSource.spatialBlend = 0f;
            _bgmSource.outputAudioMixerGroup = bgmGroup;

            _uiSource = gameObject.AddComponent<AudioSource>();
            _uiSource.playOnAwake = false;
            _uiSource.spatialBlend = 0f;
            _uiSource.outputAudioMixerGroup = _sfxGroup;

            int voices = Mathf.Max(1, sfxVoices);
            _sfxPool = new AudioSource[voices];
            _busyUntil = new float[voices];
            for (int i = 0; i < voices; i++)
            {
                var s = gameObject.AddComponent<AudioSource>();
                s.playOnAwake = false;
                s.spatialBlend = 1f;                       // 완전 3D
                s.dopplerLevel = 0f;                       // 보이스 재사용 시 위치 순간이동 → 도플러 아티팩트(끊김) 방지
                s.rolloffMode = AudioRolloffMode.Linear;
                s.minDistance = sfxMinDistance;
                s.maxDistance = sfxMaxDistance;
                s.outputAudioMixerGroup = _sfxGroup;
                _sfxPool[i] = s;
            }

            _clips = new Dictionary<SfxId, AudioClip>
            {
                { SfxId.BasicGun, basicGun },
                { SfxId.Missile, missile },
                { SfxId.Railgun, railgun },
                { SfxId.ChargeAttack, chargeAttack },
                { SfxId.SelfExplosion, selfExplosion },
                { SfxId.PlayerDead, playerDead },
                { SfxId.EnemyDead, enemyDead },
                { SfxId.HitPlayer, hitPlayer },
                { SfxId.MissileHit, missileHit },
                { SfxId.RailgunHit, railgunHit },
                { SfxId.ShieldOn, shieldOn },
                { SfxId.ButtonClick, buttonClick },
                { SfxId.ButtonHover, buttonHover },
            };

            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        void Start() => ApplyBgmForScene(SceneManager.GetActiveScene().name);

        void OnDestroy()
        {
            if (Instance == this) SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        AudioMixerGroup FindGroup(string name)
        {
            if (mixer == null || string.IsNullOrEmpty(name)) return null;
            var groups = mixer.FindMatchingGroups(name);
            return (groups != null && groups.Length > 0) ? groups[0] : null;
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode) => ApplyBgmForScene(scene.name);

        void ApplyBgmForScene(string sceneName)
        {
            AudioClip clip = sceneName == gameSceneName ? gameBgm : titleBgm;
            if (clip == null) { _bgmSource.Stop(); return; }
            if (_bgmSource.clip == clip && _bgmSource.isPlaying) return;   // 같은 곡이면 이어서
            _bgmSource.clip = clip;
            _bgmSource.Play();
        }

        /// <summary>
        /// 3D 위치 기반 효과음(월드 이벤트: 발사·폭발·피격 등).
        /// <b>재생 중이 아닌 빈 보이스만</b> 골라 재생한다 — 아직 울리는 다른 소리의 보이스를 재사용해
        /// 위치를 옮겨버려(거리감쇠로) 끊기는 문제 방지. 빈 보이스가 없으면 가장 먼저 끝나는 보이스를 스틸.
        /// </summary>
        public void PlaySfx(SfxId id, Vector3 position)
        {
            if (_clips == null || !_clips.TryGetValue(id, out var clip) || clip == null) return;

            float now = Time.unscaledTime;
            int pick = 0;
            float earliest = float.MaxValue;
            for (int i = 0; i < _sfxPool.Length; i++)
            {
                if (_busyUntil[i] <= now) { pick = i; break; }   // 빈 보이스 발견 → 즉시 사용
                if (_busyUntil[i] < earliest) { earliest = _busyUntil[i]; pick = i; }  // 전부 사용 중이면 가장 곧 끝날 것을 스틸
            }

            var src = _sfxPool[pick];
            src.transform.position = position;
            src.clip = clip;
            src.Play();
            _busyUntil[pick] = now + clip.length;
        }

        /// <summary>2D 효과음(UI 버튼 등 위치 무관).</summary>
        public void PlayUi(SfxId id)
        {
            if (_clips == null || !_clips.TryGetValue(id, out var clip) || clip == null) return;
            _uiSource.PlayOneShot(clip);
        }

        // ── 볼륨 API(M5-10 설정창) — 선형 0~1 → dB ─────────────────────
        public void SetMasterVolume(float linear01) => SetVolume("MasterVol", linear01);
        public void SetBgmVolume(float linear01) => SetVolume("BgmVol", linear01);
        public void SetSfxVolume(float linear01) => SetVolume("SfxVol", linear01);

        void SetVolume(string exposedParam, float linear01)
        {
            if (mixer == null) return;
            float v = Mathf.Clamp01(linear01);
            float dB = v <= 0.0001f ? -80f : Mathf.Log10(v) * 20f;
            mixer.SetFloat(exposedParam, dB);
        }
    }
}
