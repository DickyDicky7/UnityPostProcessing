    using System;
//  using System;
    using UnityEngine;
//  using UnityEngine;
    using UnityEngine.Rendering;
//  using UnityEngine.Rendering;
    using UnityEngine.Rendering.Universal;
//  using UnityEngine.Rendering.Universal;

    [Serializable, VolumeComponentMenuForRenderPipeline("Custom/PBR Neutral Tonemap", typeof(UniversalRenderPipeline))]
//  [Serializable, VolumeComponentMenuForRenderPipeline("Custom/PBR Neutral Tonemap", typeof(UniversalRenderPipeline))]
    public class PBRNeutralTonemapVolume : VolumeComponent, IPostProcessComponent
//  public class PBRNeutralTonemapVolume : VolumeComponent, IPostProcessComponent
    {
//  {
        [Tooltip("Enable or disable the PBR Neutral Tonemap effect in the scene.")]
//      [Tooltip("Enable or disable the PBR Neutral Tonemap effect in the scene.")]
        public BoolParameter isEnabled = new BoolParameter(false);
//      public BoolParameter isEnabled = new BoolParameter(false);

        /// <summary>Tells URP if the effect is currently active and should be injected into the render pipeline.</summary>
//      /// <summary>Tells URP if the effect is currently active and should be injected into the render pipeline.</summary>
        /// <returns>True if the effect is active, otherwise false.</returns>
//      /// <returns>True if the effect is active, otherwise false.</returns>
        public bool IsActive()
//      public bool IsActive()
        {
//      {
            if (isEnabled.value)
//          if (isEnabled.value)
            {
//          {
                return true;
//              return true;
            }
//          }
            
//          
            return false;
//          return false;
        }
//      }

        /// <summary>Indicates if the effect supports tile-based deferred rendering.</summary>
//      /// <summary>Indicates if the effect supports tile-based deferred rendering.</summary>
        /// <returns>False, as extra work is needed for tile-based compatibility.</returns>
//      /// <returns>False, as extra work is needed for tile-based compatibility.</returns>
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
