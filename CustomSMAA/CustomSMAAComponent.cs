    using UnityEngine;
//  using UnityEngine;
    using UnityEngine.Rendering;
//  using UnityEngine.Rendering;
    using UnityEngine.Rendering.Universal;
//  using UnityEngine.Rendering.Universal;

    [VolumeComponentMenu("Custom Post-processing/Custom SMAA")]
//  [VolumeComponentMenu("Custom Post-processing/Custom SMAA")]
    public class CustomSMAAComponent : VolumeComponent, IPostProcessComponent
//  public class CustomSMAAComponent : VolumeComponent, IPostProcessComponent
    {
//  {
        [Tooltip("Enables Custom SMAA.")]
//      [Tooltip("Enables Custom SMAA.")]
        public BoolParameter isEnabled = new BoolParameter(false);
//      public BoolParameter isEnabled = new BoolParameter(false);

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
