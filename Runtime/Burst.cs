// These Burst settings can be overridden by a project's pre-processor defines

namespace Imagibee.Pcc {
    public class Burst
    {
#if IMAGIBEE_PCC_FLOATMODE_DEFAULT
        public const Unity.Burst.FloatMode FloatMode = Unity.Burst.FloatMode.Default;
#elif IMAGIBEE_PCC_FLOATMODE_DETERMINISTIC
        public const Unity.Burst.FloatMode FloatMode = Unity.Burst.FloatMode.Deterministic;
#elif IMAGIBEE_PCC_FLOATMODE_STRICT
        public const Unity.Burst.FloatMode FloatMode = Unity.Burst.FloatMode.Strict;
#else // Default to FloatMode.Fast
        public const Unity.Burst.FloatMode FloatMode = Unity.Burst.FloatMode.Fast;
#endif

#if IMAGIBEE_PCC_FLOATPRECISION_STANDARD
        public const Unity.Burst.FloatPrecision FloatPrecision = Unity.Burst.FloatPrecision.Standard;
#elif IMAGIBEE_PCC_FLOATPRECISION_HIGH
        public const Unity.Burst.FloatPrecision FloatPrecision = Unity.Burst.FloatPrecision.High;
#elif IMAGIBEE_PCC_FLOATPRECISION_MEDIUM
        public const Unity.Burst.FloatPrecision FloatPrecision = Unity.Burst.FloatPrecision.Medium;
#else // Default to FloatPrecision.Low
        public const Unity.Burst.FloatPrecision FloatPrecision = Unity.Burst.FloatPrecision.Low;
#endif
    }
}