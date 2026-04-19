    using UnityEngine;
//  using UnityEngine;
    using UnityEngine.Rendering;
//  using UnityEngine.Rendering;
    using UnityEngine.Rendering.Universal;
//  using UnityEngine.Rendering.Universal;

    [VolumeComponentMenu("Custom Post-processing/Custom SMAA S2x")]
//  [VolumeComponentMenu("Custom Post-processing/Custom SMAA S2x")]
    public class CustomSMAAS2xComponent : VolumeComponent, IPostProcessComponent
//  public class CustomSMAAS2xComponent : VolumeComponent, IPostProcessComponent
    {
//  {
        [Tooltip("Enable Custom SMAA S2x")]
//      [Tooltip("Enable Custom SMAA S2x")]
        public BoolParameter isEnabled = new BoolParameter(false);
//      public BoolParameter isEnabled = new BoolParameter(false);

        [Range(0.01f, 0.2f), Tooltip("Edge detection threshold")]
//      [Range(0.01f, 0.2f), Tooltip("Edge detection threshold")]
        public ClampedFloatParameter threshold = new ClampedFloatParameter(0.1f, 0.01f, 0.2f);
//      public ClampedFloatParameter threshold = new ClampedFloatParameter(0.1f, 0.01f, 0.2f);

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
