    using UnityEngine;
//  using UnityEngine;
    using UnityEngine.Rendering;
//  using UnityEngine.Rendering;
    using UnityEngine.Rendering.Universal;
//  using UnityEngine.Rendering.Universal;

    [VolumeComponentMenu("Custom Post-processing/Custom SMAA 4x")]
//  [VolumeComponentMenu("Custom Post-processing/Custom SMAA 4x")]
    public class CustomSMAA4xComponent : VolumeComponent, IPostProcessComponent
//  public class CustomSMAA4xComponent : VolumeComponent, IPostProcessComponent
    {
//  {
        [Tooltip("Enable Custom SMAA 4x")]
//      [Tooltip("Enable Custom SMAA 4x")]
        public BoolParameter isEnabled = new BoolParameter(false);
//      public BoolParameter isEnabled = new BoolParameter(false);

        [Range(0.01f, 0.2f), Tooltip("Edge detection threshold")]
//      [Range(0.01f, 0.2f), Tooltip("Edge detection threshold")]
        public ClampedFloatParameter threshold = new ClampedFloatParameter(0.1f, 0.01f, 0.2f);
//      public ClampedFloatParameter threshold = new ClampedFloatParameter(0.1f, 0.01f, 0.2f);

        [Range(0.0f, 1.0f), Tooltip("Temporal Blend Weight")]
//      [Range(0.0f, 1.0f), Tooltip("Temporal Blend Weight")]
        public ClampedFloatParameter temporalBlendWeight = new ClampedFloatParameter(0.8f, 0.0f, 1.0f);
//      public ClampedFloatParameter temporalBlendWeight = new ClampedFloatParameter(0.8f, 0.0f, 1.0f);

        public bool IsActive()
//      public bool IsActive()
        {
//      {
            return isEnabled.value;
//          return isEnabled.value;
        }
//      }

        public bool IsTileCompatible()
//      public bool IsTileCompatible()
        {
//      {
            return false;
//          return false;
        }
//      }
    }
//  }