    Shader "Hidden/CustomSMAA"
//  Shader "Hidden/CustomSMAA"
    {
//  {
        SubShader
//      SubShader
        {
//      {
            Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
//          Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
            Cull Off ZWrite Off ZTest Always
//          Cull Off ZWrite Off ZTest Always

            HLSLINCLUDE
//          HLSLINCLUDE
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
//          #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
//          #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            // SMAA Custom SL port for Unity URP
//          // SMAA Custom SL port for Unity URP
            #define SMAA_CUSTOM_SL
//          #define SMAA_CUSTOM_SL
            #define SMAATexture2D(tex) Texture2D tex
//          #define SMAATexture2D(tex) Texture2D tex
            #define SMAATexturePass2D(tex) tex
//          #define SMAATexturePass2D(tex) tex
            #define SMAASampleLevelZero(tex, coord) tex.SampleLevel(sampler_LinearClamp, coord, 0)
//          #define SMAASampleLevelZero(tex, coord) tex.SampleLevel(sampler_LinearClamp, coord, 0)
            #define SMAASampleLevelZeroPoint(tex, coord) tex.SampleLevel(sampler_PointClamp, coord, 0)
//          #define SMAASampleLevelZeroPoint(tex, coord) tex.SampleLevel(sampler_PointClamp, coord, 0)
            #define SMAASampleLevelZeroOffset(tex, coord, offset) tex.SampleLevel(sampler_LinearClamp, coord, 0, offset)
//          #define SMAASampleLevelZeroOffset(tex, coord, offset) tex.SampleLevel(sampler_LinearClamp, coord, 0, offset)
            #define SMAASample(tex, coord) tex.Sample(sampler_LinearClamp, coord)
//          #define SMAASample(tex, coord) tex.Sample(sampler_LinearClamp, coord)
            #define SMAASamplePoint(tex, coord) tex.Sample(sampler_PointClamp, coord)
//          #define SMAASamplePoint(tex, coord) tex.Sample(sampler_PointClamp, coord)
            #define SMAASampleOffset(tex, coord, offset) tex.Sample(sampler_LinearClamp, coord, offset)
//          #define SMAASampleOffset(tex, coord, offset) tex.Sample(sampler_LinearClamp, coord, offset)
            #define SMAA_FLATTEN [flatten]
//          #define SMAA_FLATTEN [flatten]
            #define SMAA_BRANCH [branch]
//          #define SMAA_BRANCH [branch]
            #define SMAATexture2DMS2(tex) Texture2DMS<float4, 2> tex
//          #define SMAATexture2DMS2(tex) Texture2DMS<float4, 2> tex
            #define SMAALoad(tex, pos, sample) tex.Load(pos, sample)
//          #define SMAALoad(tex, pos, sample) tex.Load(pos, sample)
            #define SMAAGather(tex, coord) tex.Gather(sampler_LinearClamp, coord, 0)
//          #define SMAAGather(tex, coord) tex.Gather(sampler_LinearClamp, coord, 0)

            // Let Unity automatically handle the _BlitTexture texel size.
//          // Let Unity automatically handle the _BlitTexture texel size.
            float4 _BlitTexture_TexelSize;
//          float4 _BlitTexture_TexelSize;
            #define SMAA_RT_METRICS _BlitTexture_TexelSize
//          #define SMAA_RT_METRICS _BlitTexture_TexelSize
            #define SMAA_PRESET_HIGH
//          #define SMAA_PRESET_HIGH

            // Include the SMAA HLSL file. The user must place this file alongside the shader.
//          // Include the SMAA HLSL file. The user must place this file alongside the shader.
            #include "SMAA.hlsl"
//          #include "SMAA.hlsl"

            // Textures required by SMAA.
//          // Textures required by SMAA.
            // _BlitTexture is automatically set by Unity's Blitter.
//          // _BlitTexture is automatically set by Unity's Blitter.
            Texture2D _AreaTex;
//          Texture2D _AreaTex;
            Texture2D _SearchTex;
//          Texture2D _SearchTex;
            Texture2D _ColorTex;
//          Texture2D _ColorTex;

            // ---- Pass 0: Edge Detection ----
//          // ---- Pass 0: Edge Detection ----
            struct VaryingsEdge
//          struct VaryingsEdge
            {
//          {
                float4 positionCS : SV_Position;
//              float4 positionCS : SV_Position;
                float2 texcoord : TEXCOORD0;
//              float2 texcoord : TEXCOORD0;
                float4 offset[3] : TEXCOORD1;
//              float4 offset[3] : TEXCOORD1;
            };
//          };

            VaryingsEdge EdgeVert(Attributes input)
//          VaryingsEdge EdgeVert(Attributes input)
            {
//          {
                VaryingsEdge output;
//              VaryingsEdge output;
                UNITY_SETUP_INSTANCE_ID(input);
//              UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
//              UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                float4 pos = GetFullScreenTriangleVertexPosition(input.vertexID);
//              float4 pos = GetFullScreenTriangleVertexPosition(input.vertexID);
                float2 uv  = GetFullScreenTriangleTexCoord(input.vertexID);
//              float2 uv  = GetFullScreenTriangleTexCoord(input.vertexID);
                output.positionCS = pos;
//              output.positionCS = pos;
                output.texcoord = uv;
//              output.texcoord = uv;
                SMAAEdgeDetectionVS(uv, output.offset);
//              SMAAEdgeDetectionVS(uv, output.offset);
                return output;
//              return output;
            }
//          }

            float4 EdgeFrag(VaryingsEdge input) : SV_Target
//          float4 EdgeFrag(VaryingsEdge input) : SV_Target
            {
//          {
                // Using Luma Edge Detection. Here, _BlitTexture acts as the camera color target.
//              // Using Luma Edge Detection. Here, _BlitTexture acts as the camera color target.
                return float4(SMAALumaEdgeDetectionPS(input.texcoord, input.offset, _BlitTexture), 0.0, 0.0);
//              return float4(SMAALumaEdgeDetectionPS(input.texcoord, input.offset, _BlitTexture), 0.0, 0.0);
            }
//          }

            // ---- Pass 1: Blend Weight Calculation ----
//          // ---- Pass 1: Blend Weight Calculation ----
            struct VaryingsBlend
//          struct VaryingsBlend
            {
//          {
                float4 positionCS : SV_Position;
//              float4 positionCS : SV_Position;
                float2 texcoord : TEXCOORD0;
//              float2 texcoord : TEXCOORD0;
                float2 pixcoord : TEXCOORD1;
//              float2 pixcoord : TEXCOORD1;
                float4 offset[3] : TEXCOORD2;
//              float4 offset[3] : TEXCOORD2;
            };
//          };

            VaryingsBlend BlendVert(Attributes input)
//          VaryingsBlend BlendVert(Attributes input)
            {
//          {
                VaryingsBlend output;
//              VaryingsBlend output;
                UNITY_SETUP_INSTANCE_ID(input);
//              UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
//              UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                float4 pos = GetFullScreenTriangleVertexPosition(input.vertexID);
//              float4 pos = GetFullScreenTriangleVertexPosition(input.vertexID);
                float2 uv  = GetFullScreenTriangleTexCoord(input.vertexID);
//              float2 uv  = GetFullScreenTriangleTexCoord(input.vertexID);
                output.positionCS = pos;
//              output.positionCS = pos;
                output.texcoord = uv;
//              output.texcoord = uv;
                SMAABlendingWeightCalculationVS(uv, output.pixcoord, output.offset);
//              SMAABlendingWeightCalculationVS(uv, output.pixcoord, output.offset);
                return output;
//              return output;
            }
//          }

            float4 BlendFrag(VaryingsBlend input) : SV_Target
//          float4 BlendFrag(VaryingsBlend input) : SV_Target
            {
//          {
                // _BlitTexture acts as the Edge texture from Pass 0.
//              // _BlitTexture acts as the Edge texture from Pass 0.
                return SMAABlendingWeightCalculationPS(input.texcoord, input.pixcoord, input.offset, _BlitTexture, _AreaTex, _SearchTex, float4(0.0, 0.0, 0.0, 0.0));
//              return SMAABlendingWeightCalculationPS(input.texcoord, input.pixcoord, input.offset, _BlitTexture, _AreaTex, _SearchTex, float4(0.0, 0.0, 0.0, 0.0));
            }
//          }

            // ---- Pass 2: Neighborhood Blending ----
//          // ---- Pass 2: Neighborhood Blending ----
            struct VaryingsNeighborhood
//          struct VaryingsNeighborhood
            {
//          {
                float4 positionCS : SV_Position;
//              float4 positionCS : SV_Position;
                float2 texcoord : TEXCOORD0;
//              float2 texcoord : TEXCOORD0;
                float4 offset : TEXCOORD1;
//              float4 offset : TEXCOORD1;
            };
//          };

            VaryingsNeighborhood NeighborhoodVert(Attributes input)
//          VaryingsNeighborhood NeighborhoodVert(Attributes input)
            {
//          {
                VaryingsNeighborhood output;
//              VaryingsNeighborhood output;
                UNITY_SETUP_INSTANCE_ID(input);
//              UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
//              UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                float4 pos = GetFullScreenTriangleVertexPosition(input.vertexID);
//              float4 pos = GetFullScreenTriangleVertexPosition(input.vertexID);
                float2 uv  = GetFullScreenTriangleTexCoord(input.vertexID);
//              float2 uv  = GetFullScreenTriangleTexCoord(input.vertexID);
                output.positionCS = pos;
//              output.positionCS = pos;
                output.texcoord = uv;
//              output.texcoord = uv;
                SMAANeighborhoodBlendingVS(uv, output.offset);
//              SMAANeighborhoodBlendingVS(uv, output.offset);
                return output;
//              return output;
            }
//          }

            float4 NeighborhoodFrag(VaryingsNeighborhood input) : SV_Target
//          float4 NeighborhoodFrag(VaryingsNeighborhood input) : SV_Target
            {
//          {
                // _ColorTex acts as the original camera target.
//              // _ColorTex acts as the original camera target.
                // _BlitTexture acts as the blend weight texture from Pass 1.
//              // _BlitTexture acts as the blend weight texture from Pass 1.
                return SMAANeighborhoodBlendingPS(input.texcoord, input.offset, _ColorTex, _BlitTexture);
//              return SMAANeighborhoodBlendingPS(input.texcoord, input.offset, _ColorTex, _BlitTexture);
            }
//          }
            ENDHLSL
//          ENDHLSL

            Pass
//          Pass
            {
//          {
                Name "SMAA Edge Detection"
//              Name "SMAA Edge Detection"
                HLSLPROGRAM
//              HLSLPROGRAM
                #pragma vertex EdgeVert
//              #pragma vertex EdgeVert
                #pragma fragment EdgeFrag
//              #pragma fragment EdgeFrag
                ENDHLSL
//              ENDHLSL
            }
//          }

            Pass
//          Pass
            {
//          {
                Name "SMAA Blend Weight Calculation"
//              Name "SMAA Blend Weight Calculation"
                HLSLPROGRAM
//              HLSLPROGRAM
                #pragma vertex BlendVert
//              #pragma vertex BlendVert
                #pragma fragment BlendFrag
//              #pragma fragment BlendFrag
                ENDHLSL
//              ENDHLSL
            }
//          }

            Pass
//          Pass
            {
//          {
                Name "SMAA Neighborhood Blending"
//              Name "SMAA Neighborhood Blending"
                HLSLPROGRAM
//              HLSLPROGRAM
                #pragma vertex NeighborhoodVert
//              #pragma vertex NeighborhoodVert
                #pragma fragment NeighborhoodFrag
//              #pragma fragment NeighborhoodFrag
                ENDHLSL
//              ENDHLSL
            }
//          }
        }
//      }
    }
//  }
