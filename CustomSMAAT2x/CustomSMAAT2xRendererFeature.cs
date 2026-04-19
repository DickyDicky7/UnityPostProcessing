    using UnityEngine;
//  using UnityEngine;
    using UnityEngine.Rendering;
//  using UnityEngine.Rendering;
    using UnityEngine.Rendering.Universal;
//  using UnityEngine.Rendering.Universal;

    public class CustomSMAAT2xRendererFeature : ScriptableRendererFeature
//  public class CustomSMAAT2xRendererFeature : ScriptableRendererFeature
    {
//  {
        class CustomSMAAT2xRenderPass : ScriptableRenderPass
//      class CustomSMAAT2xRenderPass : ScriptableRenderPass
        {
//      {
            private Material m_Material;
//          private Material m_Material;
            private CustomSMAAT2xComponent m_SMAAComponent;
//          private CustomSMAAT2xComponent m_SMAAComponent;
            private RTHandle m_EdgeTexture;
//          private RTHandle m_EdgeTexture;
            private RTHandle m_WeightTexture;
//          private RTHandle m_WeightTexture;
            private RTHandle m_TempTexture;
//          private RTHandle m_TempTexture;
            private RTHandle m_HistoryTexture;
//          private RTHandle m_HistoryTexture;
            private RTHandle m_ColorTexture;
//          private RTHandle m_ColorTexture;

            private static readonly int s_EdgeTexID = Shader.PropertyToID("_EdgeTexture");
//          private static readonly int s_EdgeTexID = Shader.PropertyToID("_EdgeTexture");
            private static readonly int s_WeightTexID = Shader.PropertyToID("_WeightTexture");
//          private static readonly int s_WeightTexID = Shader.PropertyToID("_WeightTexture");
            private static readonly int s_AreaTexID = Shader.PropertyToID("_AreaTex");
//          private static readonly int s_AreaTexID = Shader.PropertyToID("_AreaTex");
            private static readonly int s_SearchTexID = Shader.PropertyToID("_SearchTex");
//          private static readonly int s_SearchTexID = Shader.PropertyToID("_SearchTex");
            private static readonly int s_SMAAThresholdID = Shader.PropertyToID("_SMAAThreshold");
//          private static readonly int s_SMAAThresholdID = Shader.PropertyToID("_SMAAThreshold");
            private static readonly int s_HistoryTexID = Shader.PropertyToID("_HistoryTex");
//          private static readonly int s_HistoryTexID = Shader.PropertyToID("_HistoryTex");
            private static readonly int s_TemporalBlendWeightID = Shader.PropertyToID("_TemporalBlendWeight");
//          private static readonly int s_TemporalBlendWeightID = Shader.PropertyToID("_TemporalBlendWeight");

            private Texture2D m_AreaTex;
//          private Texture2D m_AreaTex;
            private Texture2D m_SearchTex;
//          private Texture2D m_SearchTex;
            private int m_FrameCount = 0;
//          private int m_FrameCount = 0;

            private ProfilingSampler m_ProfilingSampler = new ProfilingSampler("Custom SMAA T2x");
//          private ProfilingSampler m_ProfilingSampler = new ProfilingSampler("Custom SMAA T2x");

            public CustomSMAAT2xRenderPass(Material material, Texture2D areaTex, Texture2D searchTex)
//          public CustomSMAAT2xRenderPass(Material material, Texture2D areaTex, Texture2D searchTex)
            {
//          {
                m_Material = material;
//              m_Material = material;
                m_AreaTex = areaTex;
//              m_AreaTex = areaTex;
                m_SearchTex = searchTex;
//              m_SearchTex = searchTex;
                renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
//              renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
            }
//          }

            public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
//          public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
            {
//          {
                ConfigureInput(ScriptableRenderPassInput.Motion);
//              ConfigureInput(ScriptableRenderPassInput.Motion);

                var desc = renderingData.cameraData.cameraTargetDescriptor;
//              var desc = renderingData.cameraData.cameraTargetDescriptor;
                desc.depthBufferBits = 0;
//              desc.depthBufferBits = 0;

                var edgeDesc = desc;
//              var edgeDesc = desc;
                edgeDesc.graphicsFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8_UNorm;
//              edgeDesc.graphicsFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8_UNorm;
                RenderingUtils.ReAllocateIfNeeded(ref m_EdgeTexture, edgeDesc, FilterMode.Bilinear, TextureWrapMode.Clamp, name: "_EdgeTexture");
//              RenderingUtils.ReAllocateIfNeeded(ref m_EdgeTexture, edgeDesc, FilterMode.Bilinear, TextureWrapMode.Clamp, name: "_EdgeTexture");

                var weightDesc = desc;
//              var weightDesc = desc;
                weightDesc.graphicsFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_UNorm;
//              weightDesc.graphicsFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_UNorm;
                RenderingUtils.ReAllocateIfNeeded(ref m_WeightTexture, weightDesc, FilterMode.Bilinear, TextureWrapMode.Clamp, name: "_WeightTexture");
//              RenderingUtils.ReAllocateIfNeeded(ref m_WeightTexture, weightDesc, FilterMode.Bilinear, TextureWrapMode.Clamp, name: "_WeightTexture");

                RenderingUtils.ReAllocateIfNeeded(ref m_TempTexture, desc, FilterMode.Bilinear, TextureWrapMode.Clamp, name: "_TempSMAATexture");
//              RenderingUtils.ReAllocateIfNeeded(ref m_TempTexture, desc, FilterMode.Bilinear, TextureWrapMode.Clamp, name: "_TempSMAATexture");
                RenderingUtils.ReAllocateIfNeeded(ref m_HistoryTexture, desc, FilterMode.Bilinear, TextureWrapMode.Clamp, name: "_HistoryTexture");
//              RenderingUtils.ReAllocateIfNeeded(ref m_HistoryTexture, desc, FilterMode.Bilinear, TextureWrapMode.Clamp, name: "_HistoryTexture");
                RenderingUtils.ReAllocateIfNeeded(ref m_ColorTexture, desc, FilterMode.Bilinear, TextureWrapMode.Clamp, name: "_ColorTexture");
//              RenderingUtils.ReAllocateIfNeeded(ref m_ColorTexture, desc, FilterMode.Bilinear, TextureWrapMode.Clamp, name: "_ColorTexture");

                m_FrameCount++;
//              m_FrameCount++;
                float jitterX;
//              float jitterX;
                float jitterY;
//              float jitterY;

                if (m_FrameCount % 2 == 0)
//              if (m_FrameCount % 2 == 0)
                {
//              {
                    jitterX = 0.25f / desc.width;
//                  jitterX = 0.25f / desc.width;
                    jitterY = 0.25f / desc.height;
//                  jitterY = 0.25f / desc.height;
                }
//              }
                else
//              else
                {
//              {
                    jitterX = -0.25f / desc.width;
//                  jitterX = -0.25f / desc.width;
                    jitterY = -0.25f / desc.height;
//                  jitterY = -0.25f / desc.height;
                }
//              }

                Matrix4x4 jitterMat = Matrix4x4.Translate(new Vector3(jitterX, jitterY, 0));
//              Matrix4x4 jitterMat = Matrix4x4.Translate(new Vector3(jitterX, jitterY, 0));
                renderingData.cameraData.camera.nonJitteredProjectionMatrix = renderingData.cameraData.camera.projectionMatrix;
//              renderingData.cameraData.camera.nonJitteredProjectionMatrix = renderingData.cameraData.camera.projectionMatrix;
                renderingData.cameraData.camera.projectionMatrix = jitterMat * renderingData.cameraData.camera.projectionMatrix;
//              renderingData.cameraData.camera.projectionMatrix = jitterMat * renderingData.cameraData.camera.projectionMatrix;
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
                m_SMAAComponent = stack.GetComponent<CustomSMAAT2xComponent>();
//              m_SMAAComponent = stack.GetComponent<CustomSMAAT2xComponent>();

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

                    m_Material.SetFloat(s_SMAAThresholdID, m_SMAAComponent.threshold.value);
//                  m_Material.SetFloat(s_SMAAThresholdID, m_SMAAComponent.threshold.value);
                    m_Material.SetFloat(s_TemporalBlendWeightID, m_SMAAComponent.temporalBlendWeight.value);
//                  m_Material.SetFloat(s_TemporalBlendWeightID, m_SMAAComponent.temporalBlendWeight.value);

                    if (m_AreaTex != null)
//                  if (m_AreaTex != null)
                    {
//                  {
                        m_Material.SetTexture(s_AreaTexID, m_AreaTex);
//                      m_Material.SetTexture(s_AreaTexID, m_AreaTex);
                    }
//                  }
                    if (m_SearchTex != null)
//                  if (m_SearchTex != null)
                    {
//                  {
                        m_Material.SetTexture(s_SearchTexID, m_SearchTex);
//                      m_Material.SetTexture(s_SearchTexID, m_SearchTex);
                    }
//                  }

                    Blitter.BlitCameraTexture(cmd, cameraColorTarget, m_EdgeTexture, m_Material, 0);
//                  Blitter.BlitCameraTexture(cmd, cameraColorTarget, m_EdgeTexture, m_Material, 0);

                    cmd.SetGlobalTexture(s_EdgeTexID, m_EdgeTexture);
//                  cmd.SetGlobalTexture(s_EdgeTexID, m_EdgeTexture);
                    Blitter.BlitCameraTexture(cmd, m_EdgeTexture, m_WeightTexture, m_Material, 1);
//                  Blitter.BlitCameraTexture(cmd, m_EdgeTexture, m_WeightTexture, m_Material, 1);

                    cmd.SetGlobalTexture(s_WeightTexID, m_WeightTexture);
//                  cmd.SetGlobalTexture(s_WeightTexID, m_WeightTexture);
                    Blitter.BlitCameraTexture(cmd, cameraColorTarget, m_TempTexture, m_Material, 2);
//                  Blitter.BlitCameraTexture(cmd, cameraColorTarget, m_TempTexture, m_Material, 2);

                    cmd.SetGlobalTexture(s_HistoryTexID, m_HistoryTexture);
//                  cmd.SetGlobalTexture(s_HistoryTexID, m_HistoryTexture);
                    Blitter.BlitCameraTexture(cmd, m_TempTexture, m_ColorTexture, m_Material, 3);
//                  Blitter.BlitCameraTexture(cmd, m_TempTexture, m_ColorTexture, m_Material, 3);

                    Blitter.BlitCameraTexture(cmd, m_ColorTexture, m_HistoryTexture);
//                  Blitter.BlitCameraTexture(cmd, m_ColorTexture, m_HistoryTexture);
                    Blitter.BlitCameraTexture(cmd, m_ColorTexture, cameraColorTarget);
//                  Blitter.BlitCameraTexture(cmd, m_ColorTexture, cameraColorTarget);
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

            public override void OnCameraCleanup(CommandBuffer cmd)
//          public override void OnCameraCleanup(CommandBuffer cmd)
            {
//          {
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
                if (m_WeightTexture != null)
//              if (m_WeightTexture != null)
                {
//              {
                    m_WeightTexture.Release();
//                  m_WeightTexture.Release();
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
                if (m_HistoryTexture != null)
//              if (m_HistoryTexture != null)
                {
//              {
                    m_HistoryTexture.Release();
//                  m_HistoryTexture.Release();
                }
//              }
                if (m_ColorTexture != null)
//              if (m_ColorTexture != null)
                {
//              {
                    m_ColorTexture.Release();
//                  m_ColorTexture.Release();
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
            public Texture2D areaTex;
//          public Texture2D areaTex;
            public Texture2D searchTex;
//          public Texture2D searchTex;
        }
//      }

        public Settings settings = new Settings();
//      public Settings settings = new Settings();
        private CustomSMAAT2xRenderPass m_Pass;
//      private CustomSMAAT2xRenderPass m_Pass;
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
            m_Pass = new CustomSMAAT2xRenderPass(m_Material, settings.areaTex, settings.searchTex);
//          m_Pass = new CustomSMAAT2xRenderPass(m_Material, settings.areaTex, settings.searchTex);
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
