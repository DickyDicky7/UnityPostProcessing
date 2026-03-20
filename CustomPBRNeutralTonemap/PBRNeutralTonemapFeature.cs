    using UnityEngine;
//  using UnityEngine;
    using UnityEngine.Rendering;
//  using UnityEngine.Rendering;
    using UnityEngine.Rendering.Universal;
//  using UnityEngine.Rendering.Universal;

    /// <summary>A custom ScriptableRendererFeature to apply a PBR Neutral Tonemap. This runs after the standard URP post-processing stack.</summary>
//  /// <summary>A custom ScriptableRendererFeature to apply a PBR Neutral Tonemap. This runs after the standard URP post-processing stack.</summary>
    public class PBRNeutralTonemapFeature : ScriptableRendererFeature
//  public class PBRNeutralTonemapFeature : ScriptableRendererFeature
    {
//  {
        class TonemapPass : ScriptableRenderPass
//      class TonemapPass : ScriptableRenderPass
        {
//      {
            private Material material;
//          private Material material;
            private PBRNeutralTonemapVolume volumeComponent;
//          private PBRNeutralTonemapVolume volumeComponent;
            private RTHandle cameraColorTarget;
//          private RTHandle cameraColorTarget;
            private RTHandle tempTexture;
//          private RTHandle tempTexture;

            public TonemapPass(Material material)
//          public TonemapPass(Material material)
            {
//          {
                this.material = material;
//              this.material = material;
                // Best place for custom tonemapping is right after URP's default post-processing
//              // Best place for custom tonemapping is right after URP's default post-processing
                renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
//              renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
            }
//          }

            public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
//          public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
            {
//          {
                var descriptor = renderingData.cameraData.cameraTargetDescriptor;
//              var descriptor = renderingData.cameraData.cameraTargetDescriptor;
                descriptor.depthBufferBits = 0; // We don't need depth for this purely color-based pass
//              descriptor.depthBufferBits = 0; // We don't need depth for this purely color-based pass
                // Allocate a temporary render target for blitting operations
//              // Allocate a temporary render target for blitting operations
                RenderingUtils.ReAllocateIfNeeded(ref tempTexture, descriptor, name: "_TempTonemapTexture");
//              RenderingUtils.ReAllocateIfNeeded(ref tempTexture, descriptor, name: "_TempTonemapTexture");
                // Get the camera's active color target to read and write
//              // Get the camera's active color target to read and write
                cameraColorTarget = renderingData.cameraData.renderer.cameraColorTargetHandle;
//              cameraColorTarget = renderingData.cameraData.renderer.cameraColorTargetHandle;
            }
//          }

            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
//          public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
//          {
                // Fetch the volume component to check settings and active state
//              // Fetch the volume component to check settings and active state
                volumeComponent = VolumeManager.instance.stack.GetComponent<PBRNeutralTonemapVolume>();
//              volumeComponent = VolumeManager.instance.stack.GetComponent<PBRNeutralTonemapVolume>();
                // Skip execution if the effect is disabled, material is missing, or not active
//              // Skip execution if the effect is disabled, material is missing, or not active
                if (volumeComponent == null || !volumeComponent.IsActive() || material == null)
//              if (volumeComponent == null || !volumeComponent.IsActive() || material == null)
                {
//              {
                    return;
//                  return;
                }
//              }
                CommandBuffer cmd = CommandBufferPool.Get("PBR Neutral Tonemap");
//              CommandBuffer cmd = CommandBufferPool.Get("PBR Neutral Tonemap");
                // URP 2022+ Blitter API requires doing a blit to a temp texture, then blit back to the camera target
//              // URP 2022+ Blitter API requires doing a blit to a temp texture, then blit back to the camera target
                Blitter.BlitCameraTexture(cmd, cameraColorTarget, tempTexture, material, 0);
//              Blitter.BlitCameraTexture(cmd, cameraColorTarget, tempTexture, material, 0);
                Blitter.BlitCameraTexture(cmd, tempTexture, cameraColorTarget);
//              Blitter.BlitCameraTexture(cmd, tempTexture, cameraColorTarget);
                context.ExecuteCommandBuffer(cmd);
//              context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);
//              CommandBufferPool.Release(cmd);
            }
//          }

            public void Dispose()
//          public void Dispose()
            {
//          {
                // Release the allocated temporary RTHandle to free memory
//              // Release the allocated temporary RTHandle to free memory
                if (tempTexture != null)
//              if (tempTexture != null)
                {
//              {
                    tempTexture.Release();
//                  tempTexture.Release();
                }
//              }
            }
//          }
        }
//      }

        [System.Serializable]
//      [System.Serializable]
        public class Settings
//      public class Settings
        {
//      {
            [Tooltip("The shader required for applying the PBR Neutral Tonemap.")]
//          [Tooltip("The shader required for applying the PBR Neutral Tonemap.")]
            public Shader tonemapShader;
//          public Shader tonemapShader;
        }
//      }

        public Settings settings = new Settings();
//      public Settings settings = new Settings();
        private TonemapPass customPass;
//      private TonemapPass customPass;
        private Material material;
//      private Material material;

        public override void Create()
//      public override void Create()
        {
//      {
            // Cannot initialize without the shader assigned
//          // Cannot initialize without the shader assigned
            if (settings.tonemapShader == null)
//          if (settings.tonemapShader == null)
            {
//          {
                return;
//              return;
            }
//          }
            material = CoreUtils.CreateEngineMaterial(settings.tonemapShader);
//          material = CoreUtils.CreateEngineMaterial(settings.tonemapShader);
            customPass = new TonemapPass(material);
//          customPass = new TonemapPass(material);
        }
//      }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
//      public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
//      {
            // Ensure everything is valid before enqueueing the pass
//          // Ensure everything is valid before enqueueing the pass
            if (settings.tonemapShader == null || customPass == null)
//          if (settings.tonemapShader == null || customPass == null)
            {
//          {
                return;
//              return;
            }
//          }
            // Only run for Game and Scene cameras to prevent applying to reflections or UI cameras improperly
//          // Only run for Game and Scene cameras to prevent applying to reflections or UI cameras improperly
            if (renderingData.cameraData.cameraType == CameraType.Game || renderingData.cameraData.cameraType == CameraType.SceneView)
//          if (renderingData.cameraData.cameraType == CameraType.Game || renderingData.cameraData.cameraType == CameraType.SceneView)
            {
//          {
                renderer.EnqueuePass(customPass);
//              renderer.EnqueuePass(customPass);
            }
//          }
        }
//      }

        protected override void Dispose(bool disposing)
//      protected override void Dispose(bool disposing)
        {
//      {
            // Cleanup the pass resources
//          // Cleanup the pass resources
            if (customPass != null)
//          if (customPass != null)
            {
//          {
                customPass.Dispose();
//              customPass.Dispose();
            }
//          }
            // Destroy the dynamically created material safely
//          // Destroy the dynamically created material safely
            CoreUtils.Destroy(material);
//          CoreUtils.Destroy(material);
        }
//      }
    }
//  }
