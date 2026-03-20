    using UnityEngine;
//  using UnityEngine;
    using UnityEngine.Rendering;
//  using UnityEngine.Rendering;
    using UnityEngine.Rendering.Universal;
//  using UnityEngine.Rendering.Universal;

    [VolumeComponentMenu("Custom Post-processing/Custom FXAA")]
//  [VolumeComponentMenu("Custom Post-processing/Custom FXAA")]
    public class CustomFXAAComponent : VolumeComponent, IPostProcessComponent
//  public class CustomFXAAComponent : VolumeComponent, IPostProcessComponent
    {
//  {
        [Tooltip("Enable Custom FXAA")]
//      [Tooltip("Enable Custom FXAA")]
        public BoolParameter isEnabled = new BoolParameter(false);
//      public BoolParameter isEnabled = new BoolParameter(false);

        public bool IsActive() => isEnabled.value;
//      public bool IsActive() => isEnabled.value;
        public bool IsTileCompatible() => false;
//      public bool IsTileCompatible() => false;
    }
//  }
