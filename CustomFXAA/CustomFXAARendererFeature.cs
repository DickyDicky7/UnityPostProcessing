    using UnityEngine;
//  using UnityEngine;
    using UnityEngine.Rendering;
//  using UnityEngine.Rendering;
    using UnityEngine.Rendering.Universal;
//  using UnityEngine.Rendering.Universal;

    public class CustomFXAARendererFeature : ScriptableRendererFeature
//  public class CustomFXAARendererFeature : ScriptableRendererFeature
    {
//  {
        class CustomFXAARenderPass : ScriptableRenderPass
//      class CustomFXAARenderPass : ScriptableRenderPass
        {
//      {
            private Material m_Material;
//          private Material m_Material;
            private CustomFXAAComponent m_FXAAComponent;
//          private CustomFXAAComponent m_FXAAComponent;
            private RTHandle m_TempTexture;
//          private RTHandle m_TempTexture;

            // Define a profiling sampler for the Frame Debugger.
//          // Define a profiling sampler for the Frame Debugger.
            private ProfilingSampler m_ProfilingSampler = new ProfilingSampler("Custom FXAA");
//          private ProfilingSampler m_ProfilingSampler = new ProfilingSampler("Custom FXAA");

            public CustomFXAARenderPass(Material material)
//          public CustomFXAARenderPass(Material material)
            {
//          {
                m_Material = material;
//              m_Material = material;
                // FXAA must run after tonemapping to operate within the 0-1 LDR color space.
//              // FXAA must run after tonemapping to operate within the 0-1 LDR color space.
                renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
//              renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;

                // Ensure URP provides the color target to read from.
//              // Ensure URP provides the color target to read from.
                ConfigureInput(ScriptableRenderPassInput.Color);
//              ConfigureInput(ScriptableRenderPassInput.Color);
            }
//          }

            public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
//          public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
            {
//          {
                var desc = renderingData.cameraData.cameraTargetDescriptor;
//              var desc = renderingData.cameraData.cameraTargetDescriptor;
                desc.depthBufferBits = 0; // Set depth to 0 to conserve memory.
//              desc.depthBufferBits = 0; // Set depth to 0 to conserve memory.

                // Force MSAA to 1, as post-processing must run on resolved targets.
//              // Force MSAA to 1, as post-processing must run on resolved targets.
                desc.msaaSamples = 1;
//              desc.msaaSamples = 1;

                RenderingUtils.ReAllocateIfNeeded(ref m_TempTexture, desc, FilterMode.Bilinear, TextureWrapMode.Clamp, name: "_TempFXAATexture");
//              RenderingUtils.ReAllocateIfNeeded(ref m_TempTexture, desc, FilterMode.Bilinear, TextureWrapMode.Clamp, name: "_TempFXAATexture");
            }
//          }

            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
//          public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
//          {
                if (m_Material == null)
//              if (m_Material == null)
                {
//              {
                    return;
//                  return;
                }
//              }
                var stack = VolumeManager.instance.stack;
//              var stack = VolumeManager.instance.stack;
                m_FXAAComponent = stack.GetComponent<CustomFXAAComponent>();
//              m_FXAAComponent = stack.GetComponent<CustomFXAAComponent>();
                if (m_FXAAComponent == null || !m_FXAAComponent.IsActive())
//              if (m_FXAAComponent == null || !m_FXAAComponent.IsActive())
                {
//              {
                    return;
//                  return;
                }
//              }
                CommandBuffer cmd = CommandBufferPool.Get();
//              CommandBuffer cmd = CommandBufferPool.Get();
                // Wrap the execution in a ProfilingScope so it appears named in the Frame Debugger.
//              // Wrap the execution in a ProfilingScope so it appears named in the Frame Debugger.
                using (new ProfilingScope(cmd, m_ProfilingSampler))
//              using (new ProfilingScope(cmd, m_ProfilingSampler))
                {
//              {
                    // Fetch the camera's color target during execution.
//                  // Fetch the camera's color target during execution.
                    RTHandle cameraColorTarget = renderingData.cameraData.renderer.cameraColorTargetHandle;
//                  RTHandle cameraColorTarget = renderingData.cameraData.renderer.cameraColorTargetHandle;
                    // Execute the blit logic.
//                  // Execute the blit logic.
                    // Note: The Blitter automatically sets the shader's _BlitTexture and _BlitTexture_TexelSize properties.
//                  // Note: The Blitter automatically sets the shader's _BlitTexture and _BlitTexture_TexelSize properties.
                    Blitter.BlitCameraTexture(cmd, cameraColorTarget, m_TempTexture, m_Material, 0);
//                  Blitter.BlitCameraTexture(cmd, cameraColorTarget, m_TempTexture, m_Material, 0);
                    Blitter.BlitCameraTexture(cmd, m_TempTexture, cameraColorTarget);
//                  Blitter.BlitCameraTexture(cmd, m_TempTexture, cameraColorTarget);
                }
//              }
                context.ExecuteCommandBuffer(cmd);
//              context.ExecuteCommandBuffer(cmd);
                cmd.Clear(); // It is good practice to clear the command buffer before releasing it.
//              cmd.Clear(); // It is good practice to clear the command buffer before releasing it.
                CommandBufferPool.Release(cmd);
//              CommandBufferPool.Release(cmd);
            }
//          }

            public void Dispose()
//          public void Dispose()
            {
//          {
                if (m_TempTexture != null)
//              if (m_TempTexture != null)
                {
//              {
                    m_TempTexture.Release();
//                  m_TempTexture.Release();
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
            public Shader shader;
//          public Shader shader;
        }
//      }

        public Settings settings = new Settings();
//      public Settings settings = new Settings();
        private CustomFXAARenderPass m_Pass;
//      private CustomFXAARenderPass m_Pass;
        private Material m_Material;
//      private Material m_Material;

        public override void Create()
//      public override void Create()
        {
//      {
            if (settings.shader == null)
//          if (settings.shader == null)
            {
//          {
                return;
//              return;
            }
//          }

            m_Material = CoreUtils.CreateEngineMaterial(settings.shader);
//          m_Material = CoreUtils.CreateEngineMaterial(settings.shader);
            m_Pass = new CustomFXAARenderPass(m_Material);
//          m_Pass = new CustomFXAARenderPass(m_Material);
        }
//      }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
//      public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
//      {
            if (settings.shader == null || m_Material == null)
//          if (settings.shader == null || m_Material == null)
            {
//          {
                return;
//              return;
            }
//          }

            // Respect the camera's post-processing toggle.
//          // Respect the camera's post-processing toggle.
            if (!renderingData.cameraData.postProcessEnabled)
//          if (!renderingData.cameraData.postProcessEnabled)
            {
//          {
                return;
//              return;
            }
//          }

            // Skip execution for reflection probes and material previews.
//          // Skip execution for reflection probes and material previews.
            if (renderingData.cameraData.cameraType == CameraType.Preview || renderingData.cameraData.cameraType == CameraType.Reflection)
//          if (renderingData.cameraData.cameraType == CameraType.Preview || renderingData.cameraData.cameraType == CameraType.Reflection)
            {
//          {
                return;
//              return;
            }
//          }
            renderer.EnqueuePass(m_Pass);
//          renderer.EnqueuePass(m_Pass);
        }
//      }

        protected override void Dispose(bool disposing)
//      protected override void Dispose(bool disposing)
        {
//      {
            CoreUtils.Destroy(m_Material);
//          CoreUtils.Destroy(m_Material);
            if (m_Pass != null)
//          if (m_Pass != null)
            {
//          {
                m_Pass.Dispose();
//              m_Pass.Dispose();
            }
//          }
        }
//      }
    }
//  }
