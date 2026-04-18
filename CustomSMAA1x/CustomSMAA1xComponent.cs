    using UnityEngine;
//  using UnityEngine;
    using UnityEngine.Rendering;
//  using UnityEngine.Rendering;
    using UnityEngine.Rendering.Universal;
//  using UnityEngine.Rendering.Universal;

    [VolumeComponentMenu("Custom Post-processing/Custom SMAA 1x")]
//  [VolumeComponentMenu("Custom Post-processing/Custom SMAA 1x")]
    public class CustomSMAA1xComponent : VolumeComponent, IPostProcessComponent
//  public class CustomSMAA1xComponent : VolumeComponent, IPostProcessComponent
    {
//  {
        [Tooltip("Enable Custom SMAA 1x")]
//      [Tooltip("Enable Custom SMAA 1x")]
        public BoolParameter isEnabled = new BoolParameter(false);
//      public BoolParameter isEnabled = new BoolParameter(false);

        [Range(0.01f, 0.2f), Tooltip("Edge detection threshold")]
//      [Range(0.01f, 0.2f), Tooltip("Edge detection threshold")]
        public ClampedFloatParameter threshold = new ClampedFloatParameter(0.1f, 0.01f, 0.2f);
//      public ClampedFloatParameter threshold = new ClampedFloatParameter(0.1f, 0.01f, 0.2f);

        public bool IsActive() => isEnabled.value;
//      public bool IsActive() => isEnabled.value;

        public bool IsTileCompatible() => false;
//      public bool IsTileCompatible() => false;
    }
//  }
