namespace AdaptableCrtEffect
{
    public enum BloomPresets
    {
        Wide,
        Focussed,
        Small,
        SuperWide,
        Cheap,
        One
    };

    public enum CrtModeOption
    {
        Full,
        SoftScans,
        NoScans,
        NoScansNoCurvature,
        Disabled
    }

    public class BloomSettings
    {
        public BloomPresets Preset = 0;
        public float Threshold = .6f;
        public float StrengthMultiplier = 1f;
        public float RadiusMultiplier = 1f;
        public bool UseLuminance = false;
        public float Intensity = 0.25f;
        public float Saturation = 1.3f;
        public float Quality = .5f;
        public bool PreserveContents = true;
    }

    public static class PostProcessingSettings
    {
        public static bool IsBloomEnabled = true;
        public static bool IsSmoothingFilterEnabled = true;
        public static CrtModeOption CrtMode = CrtModeOption.Full;
        public static bool IsChromaticAberrationEnabled = true;

        public static BloomSettings BloomSettings { get; } = new BloomSettings();

        public static float Exposure = 1.00f;
        public static float Vibrance = 0.18f;
        public static float ScanBrightnessBoost = 1.11f;
    }
}
