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

            // ADDED: Profiling sampler for the Frame Debugger
//          // ADDED: Profiling sampler for the Frame Debugger
            private ProfilingSampler m_ProfilingSampler = new ProfilingSampler("Custom FXAA");
//          private ProfilingSampler m_ProfilingSampler = new ProfilingSampler("Custom FXAA");

            public CustomFXAARenderPass(Material material)
//          public CustomFXAARenderPass(Material material)
            {
//          {
                m_Material = material;
//              m_Material = material;
                // FIXED: FXAA must run after Tonemapping so it operates on 0-1 LDR color space!
//              // FIXED: FXAA must run after Tonemapping so it operates on 0-1 LDR color space!
                renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
//              renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
            }
//          }

            public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
//          public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
            {
//          {
                var desc = renderingData.cameraData.cameraTargetDescriptor;
//              var desc = renderingData.cameraData.cameraTargetDescriptor;
                desc.depthBufferBits = 0; // Good: saves memory
//              desc.depthBufferBits = 0; // Good: saves memory
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
                // ADDED: Wrap in ProfilingScope so it shows up named in the Frame Debugger
//              // ADDED: Wrap in ProfilingScope so it shows up named in the Frame Debugger
                using (new ProfilingScope(cmd, m_ProfilingSampler))
//              using (new ProfilingScope(cmd, m_ProfilingSampler))
                {
//              {
                    // Fetch the camera color target here during execution!
//                  // Fetch the camera color target here during execution!
                    RTHandle cameraColorTarget = renderingData.cameraData.renderer.cameraColorTargetHandle;
//                  RTHandle cameraColorTarget = renderingData.cameraData.renderer.cameraColorTargetHandle;
                    // Execute blit logic
//                  // Execute blit logic
                    // Note: Blitter automatically sets your shader's _BlitTexture and _BlitTexture_TexelSize!
//                  // Note: Blitter automatically sets your shader's _BlitTexture and _BlitTexture_TexelSize!
                    Blitter.BlitCameraTexture(cmd, cameraColorTarget, m_TempTexture, m_Material, 0);
//                  Blitter.BlitCameraTexture(cmd, cameraColorTarget, m_TempTexture, m_Material, 0);
                    Blitter.BlitCameraTexture(cmd, m_TempTexture, cameraColorTarget);
//                  Blitter.BlitCameraTexture(cmd, m_TempTexture, cameraColorTarget);
                }
//              }
                context.ExecuteCommandBuffer(cmd);
//              context.ExecuteCommandBuffer(cmd);
                cmd.Clear(); // Good practice to clear before releasing
//              cmd.Clear(); // Good practice to clear before releasing
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
            // Good: Don't run this on reflection probes or material previews
//          // Good: Don't run this on reflection probes or material previews
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
