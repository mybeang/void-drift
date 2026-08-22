using System;
using UnityEngine;

namespace VD.Core
{
    /// <summary>스케일 대상 스탯 종류(밸런싱 패스 3-2). 체력=healthClass, 데미지=공격AI에 매핑(3-2/3-3에서 배선).</summary>
    public enum WaveStatKind { LowHp, HighHp, DmgSingle, DmgBarrage, DmgCharge, DmgSuicide }

    /// <summary>스탯 한 종류의 웨이브 곡선 — base→max를 [startWave, endWave] 구간에 지수(복리) 보간.</summary>
    [Serializable]
    public struct WaveStatCurve
    {
        public WaveStatKind kind;
        [Min(0f)] public float baseValue;
        [Min(0f)] public float maxValue;
        [Min(1)] public int startWave;
        [Min(1)] public int endWave;
    }

    /// <summary>
    /// 웨이브 난이도 설정(밸런싱 패스 3-1·3-2). <b>모든 밸런스 수치를 데이터로</b> — 코드엔 공식만.
    /// Difficulty Authoring 툴이 편집하고 <see cref="DifficultyProvider"/>가 런타임에 읽는다.
    /// (구 <c>DifficultyPhaseDefinition</c>(페이즈 배율)을 폐기하고 대체.)
    /// </summary>
    [CreateAssetMenu(fileName = "WaveDifficultyConfig", menuName = "Void Drift/Wave Difficulty Config")]
    public sealed class WaveDifficultyConfig : ScriptableObject
    {
        [Header("웨이브 타이밍")]
        [Tooltip("1웨이브 길이(초). wave = floor(경과/이 값)+1")]
        [Min(1)] public int secondsPerWave = 60;
        [Tooltip("스탯 스케일 상한 웨이브. 이후는 성장식(post60)으로 전환")]
        [Min(1)] public int scaledWaveMax = 60;

        [Header("동시 상한 (min(base + inc×floor((wave-1)/every), max))")]
        [Min(0)] public int capBase = 10;
        [Min(0)] public int capIncrement = 1;
        [Min(1)] public int capIncEveryWaves = 2;
        [Min(1)] public int capMax = 30;

        [Header("소환 주기(초)")]
        [Tooltip("wave ≤ 전환 웨이브")]
        [Min(0.05f)] public float intervalEarly = 2f;
        [Tooltip("wave > 전환 웨이브")]
        [Min(0.05f)] public float intervalLate = 1f;
        [Min(1)] public int intervalSwitchWave = 60;

        [Header("60웨이브 후 성장 (주기마다 곱)")]
        [Min(0f)] public float post60HpGrowthPct = 0.05f;
        [Min(0f)] public float post60DmgGrowthPct = 0.02f;
        [Min(1f)] public float post60GrowthPeriodSec = 30f;

        [Header("스탯 곡선 (base→max, 구간, 복리)")]
        public WaveStatCurve[] statCurves =
        {
            new WaveStatCurve { kind = WaveStatKind.LowHp,      baseValue = 30f,  maxValue = 300f,  startWave = 1,  endWave = 60 },
            new WaveStatCurve { kind = WaveStatKind.HighHp,     baseValue = 500f, maxValue = 1000f, startWave = 40, endWave = 60 },
            new WaveStatCurve { kind = WaveStatKind.DmgSingle,  baseValue = 5f,   maxValue = 20f,   startWave = 1,  endWave = 60 },
            new WaveStatCurve { kind = WaveStatKind.DmgBarrage, baseValue = 2f,   maxValue = 10f,   startWave = 1,  endWave = 60 },
            new WaveStatCurve { kind = WaveStatKind.DmgCharge,  baseValue = 8f,   maxValue = 25f,   startWave = 1,  endWave = 60 },
            new WaveStatCurve { kind = WaveStatKind.DmgSuicide, baseValue = 10f,  maxValue = 40f,   startWave = 1,  endWave = 60 },
        };

        [Header("웨이브 승급 배너 (PhaseBannerView 재사용)")]
        [Tooltip("wave 1~60: 승급마다 순환")]
        [TextArea] public string[] bannerRotation =
        {
            "공허 속 적이 더욱 강해졌습니다",
            "어둠이 짙어지고 적이 사나워집니다",
            "공허가 새로운 적을 벼려냅니다",
            "심연에서 더 강한 것들이 다가옵니다",
            "적의 밀도가 높아집니다",
        };
        [Tooltip("wave 61+: 고정")]
        [TextArea] public string bannerEndless = "공허 속 적은 끊임없이 강해집니다";

        // ── 공식 헬퍼 (런타임 + 툴 프리뷰 공용) ──

        /// <summary>경과(초) → 웨이브. 1웨이브=secondsPerWave.</summary>
        public int WaveAt(float elapsedSeconds) => Mathf.FloorToInt(elapsedSeconds / Mathf.Max(1, secondsPerWave)) + 1;

        /// <summary>웨이브 → 동시 상한.</summary>
        public int CapAt(int wave)
        {
            int inc = capIncrement * ((Mathf.Max(1, wave) - 1) / Mathf.Max(1, capIncEveryWaves));
            return Mathf.Min(capBase + inc, capMax);
        }

        /// <summary>웨이브 → 소환 주기(초).</summary>
        public float IntervalAt(int wave) => wave > intervalSwitchWave ? intervalLate : intervalEarly;

        /// <summary>웨이브 승급 배너 문구. wave≤60=로테이션, 61+=고정, wave≤1=없음.</summary>
        public string BannerFor(int wave)
        {
            if (wave <= 1) return null;
            if (wave > scaledWaveMax) return bannerEndless;
            if (bannerRotation == null || bannerRotation.Length == 0) return null;
            return bannerRotation[(wave - 2) % bannerRotation.Length];   // wave2 = 첫 문구
        }

        /// <summary>스탯 값 계산. 해당 스탯이 아직 등장 전(wave &lt; startWave)이면 false.</summary>
        public bool TryStatValue(WaveStatKind kind, int wave, float elapsedSeconds, out float value)
        {
            value = 0f;
            int idx = IndexOf(kind);
            if (idx < 0) return false;
            var c = statCurves[idx];
            if (wave < c.startWave) return false;   // 아직 미등장

            // 구간 내 복리 보간: base × (max/base)^((clamp(wave)-start)/(end-start))
            int span = Mathf.Max(1, c.endWave - c.startWave);
            float t = Mathf.Clamp01((Mathf.Min(wave, c.endWave) - c.startWave) / (float)span);
            float v = (c.baseValue > 0f) ? c.baseValue * Mathf.Pow(c.maxValue / c.baseValue, t) : Mathf.Lerp(c.baseValue, c.maxValue, t);

            // 60웨이브 후: max에서 주기마다 +성장%
            if (wave > scaledWaveMax)
            {
                float extra = elapsedSeconds - (float)scaledWaveMax * secondsPerWave;
                int steps = Mathf.Max(0, Mathf.FloorToInt(extra / Mathf.Max(1f, post60GrowthPeriodSec)));
                float pct = IsHp(kind) ? post60HpGrowthPct : post60DmgGrowthPct;
                v = c.maxValue * Mathf.Pow(1f + pct, steps);
            }
            value = v;
            return true;
        }

        int IndexOf(WaveStatKind kind)
        {
            if (statCurves == null) return -1;
            for (int i = 0; i < statCurves.Length; i++)
                if (statCurves[i].kind == kind) return i;
            return -1;
        }

        static bool IsHp(WaveStatKind k) => k == WaveStatKind.LowHp || k == WaveStatKind.HighHp;
    }
}
