    using UnityEngine;
//  using UnityEngine;
    using UnityEngine.Rendering;
//  using UnityEngine.Rendering;
    using UnityEngine.Rendering.Universal;
//  using UnityEngine.Rendering.Universal;

    public class CustomSMAARendererFeature : ScriptableRendererFeature
//  public class CustomSMAARendererFeature : ScriptableRendererFeature
    {
//  {
        class CustomSMAARenderPass : ScriptableRenderPass
//      class CustomSMAARenderPass : ScriptableRenderPass
        {
//      {
            private Material m_Material;
//          private Material m_Material;
            private CustomSMAAComponent m_SMAAComponent;
//          private CustomSMAAComponent m_SMAAComponent;
            private RTHandle m_EdgeTexture;
//          private RTHandle m_EdgeTexture;
            private RTHandle m_BlendTexture;
//          private RTHandle m_BlendTexture;
            private RTHandle m_TempTexture;
//          private RTHandle m_TempTexture;
            private Settings m_Settings;
//          private Settings m_Settings;

            private ProfilingSampler m_ProfilingSampler = new ProfilingSampler("Custom SMAA");
//          private ProfilingSampler m_ProfilingSampler = new ProfilingSampler("Custom SMAA");

            public CustomSMAARenderPass(Material material, Settings settings)
//          public CustomSMAARenderPass(Material material, Settings settings)
            {
//          {
                m_Material = material;
//              m_Material = material;
                m_Settings = settings;
//              m_Settings = settings;
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
                // SMAA does not require depth here, which saves memory.
//              // SMAA does not require depth here, which saves memory.
                desc.depthBufferBits = 0;
//              desc.depthBufferBits = 0;

                // Force MSAA to 1, as post-processing must run on resolved targets.
//              // Force MSAA to 1, as post-processing must run on resolved targets.
                desc.msaaSamples = 1;
//              desc.msaaSamples = 1;

                // We use the default descriptor for intermediate textures (usually the standard color format).
//              // We use the default descriptor for intermediate textures (usually the standard color format).
                // The SMAA Edge pass natively outputs an RG texture. Using standard ARGB retains sufficient precision.
//              // The SMAA Edge pass natively outputs an RG texture. Using standard ARGB retains sufficient precision.
                RenderingUtils.ReAllocateIfNeeded(ref m_EdgeTexture, desc, FilterMode.Bilinear, TextureWrapMode.Clamp, name: "_EdgeTex");
//              RenderingUtils.ReAllocateIfNeeded(ref m_EdgeTexture, desc, FilterMode.Bilinear, TextureWrapMode.Clamp, name: "_EdgeTex");
                RenderingUtils.ReAllocateIfNeeded(ref m_BlendTexture, desc, FilterMode.Bilinear, TextureWrapMode.Clamp, name: "_BlendTex");
//              RenderingUtils.ReAllocateIfNeeded(ref m_BlendTexture, desc, FilterMode.Bilinear, TextureWrapMode.Clamp, name: "_BlendTex");
                RenderingUtils.ReAllocateIfNeeded(ref m_TempTexture, desc, FilterMode.Bilinear, TextureWrapMode.Clamp, name: "_TempSMAATexture");
//              RenderingUtils.ReAllocateIfNeeded(ref m_TempTexture, desc, FilterMode.Bilinear, TextureWrapMode.Clamp, name: "_TempSMAATexture");
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
                m_SMAAComponent = stack.GetComponent<CustomSMAAComponent>();
//              m_SMAAComponent = stack.GetComponent<CustomSMAAComponent>();
                if (m_SMAAComponent == null || !m_SMAAComponent.IsActive())
//              if (m_SMAAComponent == null || !m_SMAAComponent.IsActive())
                {
//              {
                    return;
//                  return;
                }
//              }

                CommandBuffer cmd = CommandBufferPool.Get();
//              CommandBuffer cmd = CommandBufferPool.Get();
                using (new ProfilingScope(cmd, m_ProfilingSampler))
//              using (new ProfilingScope(cmd, m_ProfilingSampler))
                {
//              {
                    RTHandle cameraColorTarget = renderingData.cameraData.renderer.cameraColorTargetHandle;
//                  RTHandle cameraColorTarget = renderingData.cameraData.renderer.cameraColorTargetHandle;

                    // Setup precomputed textures required by SMAA.
//                  // Setup precomputed textures required by SMAA.
                    if (m_Settings.areaTexture != null)
//                  if (m_Settings.areaTexture != null)
                    {
//                  {
                        cmd.SetGlobalTexture("_AreaTex", m_Settings.areaTexture);
//                      cmd.SetGlobalTexture("_AreaTex", m_Settings.areaTexture);
                    }
//                  }
                    if (m_Settings.searchTexture != null)
//                  if (m_Settings.searchTexture != null)
                    {
//                  {
                        cmd.SetGlobalTexture("_SearchTex", m_Settings.searchTexture);
//                      cmd.SetGlobalTexture("_SearchTex", m_Settings.searchTexture);
                    }
//                  }

                    // Pass 0: Edge Detection
//                  // Pass 0: Edge Detection
                    // SMAA uses 'discard' for non-edge pixels, so we MUST clear the target beforehand.
//                  // SMAA uses 'discard' for non-edge pixels, so we MUST clear the target beforehand.
                    CoreUtils.SetRenderTarget(cmd, m_EdgeTexture, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, ClearFlag.Color, Color.clear);
//                  CoreUtils.SetRenderTarget(cmd, m_EdgeTexture, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, ClearFlag.Color, Color.clear);
                    Blitter.BlitCameraTexture(cmd, cameraColorTarget, m_EdgeTexture, m_Material, 0);
//                  Blitter.BlitCameraTexture(cmd, cameraColorTarget, m_EdgeTexture, m_Material, 0);

                    // Pass 1: Blend Weight Calculation
//                  // Pass 1: Blend Weight Calculation
                    // SMAA uses 'discard' for non-edge pixels, so we MUST clear the target beforehand.
//                  // SMAA uses 'discard' for non-edge pixels, so we MUST clear the target beforehand.
                    CoreUtils.SetRenderTarget(cmd, m_BlendTexture, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, ClearFlag.Color, Color.clear);
//                  CoreUtils.SetRenderTarget(cmd, m_BlendTexture, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, ClearFlag.Color, Color.clear);
                    Blitter.BlitCameraTexture(cmd, m_EdgeTexture, m_BlendTexture, m_Material, 1);
//                  Blitter.BlitCameraTexture(cmd, m_EdgeTexture, m_BlendTexture, m_Material, 1);

                    // Pass 2: Neighborhood Blending
//                  // Pass 2: Neighborhood Blending
                    // Set the original color texture. The _BlitTexture (blend weights) will be set by BlitCameraTexture.
//                  // Set the original color texture. The _BlitTexture (blend weights) will be set by BlitCameraTexture.
                    cmd.SetGlobalTexture("_ColorTex", cameraColorTarget);
//                  cmd.SetGlobalTexture("_ColorTex", cameraColorTarget);
                    Blitter.BlitCameraTexture(cmd, m_BlendTexture, m_TempTexture, m_Material, 2);
//                  Blitter.BlitCameraTexture(cmd, m_BlendTexture, m_TempTexture, m_Material, 2);

                    // Finally, copy the resolved anti-aliased image back to the camera target.
//                  // Finally, copy the resolved anti-aliased image back to the camera target.
                    Blitter.BlitCameraTexture(cmd, m_TempTexture, cameraColorTarget);
//                  Blitter.BlitCameraTexture(cmd, m_TempTexture, cameraColorTarget);
                }
//              }
                context.ExecuteCommandBuffer(cmd);
//              context.ExecuteCommandBuffer(cmd);
                cmd.Clear();
//              cmd.Clear();
                CommandBufferPool.Release(cmd);
//              CommandBufferPool.Release(cmd);
            }
//          }

            public void Dispose()
//          public void Dispose()
            {
//          {
                if (m_EdgeTexture != null)
//              if (m_EdgeTexture != null)
                {
//              {
                    m_EdgeTexture.Release();
//                  m_EdgeTexture.Release();
                }
//              }
                if (m_BlendTexture != null)
//              if (m_BlendTexture != null)
                {
//              {
                    m_BlendTexture.Release();
//                  m_BlendTexture.Release();
                }
//              }
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
            [Tooltip("The Custom SMAA shader.")]
//          [Tooltip("The Custom SMAA shader.")]
            public Shader shader;
//          public Shader shader;

            [Tooltip("Ensure 'sRGB (Color Texture)' is unchecked in the Unity import settings!")]
//          [Tooltip("Ensure 'sRGB (Color Texture)' is unchecked in the Unity import settings!")]
            public Texture2D areaTexture;
//          public Texture2D areaTexture;

            [Tooltip("Ensure 'sRGB (Color Texture)' is unchecked in the Unity import settings!")]
//          [Tooltip("Ensure 'sRGB (Color Texture)' is unchecked in the Unity import settings!")]
            public Texture2D searchTexture;
//          public Texture2D searchTexture;
        }
//      }

        public Settings settings = new Settings();
//      public Settings settings = new Settings();
        private CustomSMAARenderPass m_Pass;
//      private CustomSMAARenderPass m_Pass;
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
            m_Pass = new CustomSMAARenderPass(m_Material, settings);
//          m_Pass = new CustomSMAARenderPass(m_Material, settings);
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

            // Do not execute for preview cameras or reflection probes.
//          // Do not execute for preview cameras or reflection probes.
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
