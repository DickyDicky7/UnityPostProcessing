    Shader "Hidden/CustomSMAAT2x"
//  Shader "Hidden/CustomSMAAT2x"
    {
//  {
        Properties
//      Properties
        {
//      {
            _MainTex ("Source", 2D) = "white" {}
//          _MainTex ("Source", 2D) = "white" {}
            _AreaTex ("Area Texture", 2D) = "white" {}
//          _AreaTex ("Area Texture", 2D) = "white" {}
            _SearchTex ("Search Texture", 2D) = "white" {}
//          _SearchTex ("Search Texture", 2D) = "white" {}
            _HistoryTex ("History Texture", 2D) = "black" {}
//          _HistoryTex ("History Texture", 2D) = "black" {}
        }
//      }

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
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
//          #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
//          #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            TEXTURE2D(_EdgeTexture);
//          TEXTURE2D(_EdgeTexture);
            TEXTURE2D(_WeightTexture);
//          TEXTURE2D(_WeightTexture);
            TEXTURE2D(_AreaTex);
//          TEXTURE2D(_AreaTex);
            SAMPLER(sampler_AreaTex);
//          SAMPLER(sampler_AreaTex);
            TEXTURE2D(_SearchTex);
//          TEXTURE2D(_SearchTex);
            SAMPLER(sampler_SearchTex);
//          SAMPLER(sampler_SearchTex);
            TEXTURE2D(_HistoryTex);
//          TEXTURE2D(_HistoryTex);
            SAMPLER(sampler_HistoryTex);
//          SAMPLER(sampler_HistoryTex);
            TEXTURE2D(_MotionVectorTexture);
//          TEXTURE2D(_MotionVectorTexture);
            SAMPLER(sampler_MotionVectorTexture);
//          SAMPLER(sampler_MotionVectorTexture);

            float _SMAAThreshold;
//          float _SMAAThreshold;
            float4 _BlitTexture_TexelSize;
//          float4 _BlitTexture_TexelSize;
            float _TemporalBlendWeight;
//          float _TemporalBlendWeight;

            float GetLuma(float3 color)
//          float GetLuma(float3 color)
            {
//          {
                return dot(color, float3(0.299, 0.587, 0.114));
//              return dot(color, float3(0.299, 0.587, 0.114));
            }
//          }
            ENDHLSL
//          ENDHLSL

            // Pass 0: Edge Detection
//          // Pass 0: Edge Detection
            Pass
//          Pass
            {
//          {
                Name "SMAA_EdgeDetection"
//              Name "SMAA_EdgeDetection"

                HLSLPROGRAM
//              HLSLPROGRAM
                #pragma vertex Vert
//              #pragma vertex Vert
                #pragma fragment frag
//              #pragma fragment frag

                float2 frag (Varyings input) : SV_Target
//              float2 frag (Varyings input) : SV_Target
                {
//              {
                    float2 uv = input.texcoord;
//                  float2 uv = input.texcoord;
                    float2 texelSize = _BlitTexture_TexelSize.xy;
//                  float2 texelSize = _BlitTexture_TexelSize.xy;

                    float lumaM = GetLuma(SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv).rgb);
//                  float lumaM = GetLuma(SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv).rgb);
                    float lumaL = GetLuma(SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + float2(-1, 0) * texelSize).rgb);
//                  float lumaL = GetLuma(SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + float2(-1, 0) * texelSize).rgb);
                    float lumaT = GetLuma(SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + float2(0, -1) * texelSize).rgb);
//                  float lumaT = GetLuma(SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + float2(0, -1) * texelSize).rgb);

                    float2 delta = abs(lumaM - float2(lumaL, lumaT));
//                  float2 delta = abs(lumaM - float2(lumaL, lumaT));
                    float2 edges = step(_SMAAThreshold, delta);
//                  float2 edges = step(_SMAAThreshold, delta);

                    return edges;
//                  return edges;
                }
//              }
                ENDHLSL
//              ENDHLSL
            }
//          }

            // Pass 1: Blending Weight Calculation (Simplified)
//          // Pass 1: Blending Weight Calculation (Simplified)
            Pass
//          Pass
            {
//          {
                Name "SMAA_WeightCalculation"
//              Name "SMAA_WeightCalculation"

                HLSLPROGRAM
//              HLSLPROGRAM
                #pragma vertex Vert
//              #pragma vertex Vert
                #pragma fragment frag
//              #pragma fragment frag

                float4 frag (Varyings input) : SV_Target
//              float4 frag (Varyings input) : SV_Target
                {
//              {
                    float2 uv = input.texcoord;
//                  float2 uv = input.texcoord;
                    float2 edge = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv).rg;
//                  float2 edge = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv).rg;
                    float4 weights = 0;
//                  float4 weights = 0;

                    UNITY_FLATTEN
//                  UNITY_FLATTEN
                    if (edge.x > 0 || edge.y > 0)
//                  if (edge.x > 0 || edge.y > 0)
                    {
//                  {
                        weights = float4(edge.x, edge.x, edge.y, edge.y) * 0.5;
//                      weights = float4(edge.x, edge.x, edge.y, edge.y) * 0.5;
                    }
//                  }

                    return weights;
//                  return weights;
                }
//              }
                ENDHLSL
//              ENDHLSL
            }
//          }

            // Pass 2: Neighborhood Blending
//          // Pass 2: Neighborhood Blending
            Pass
//          Pass
            {
//          {
                Name "SMAA_NeighborhoodBlending"
//              Name "SMAA_NeighborhoodBlending"

                HLSLPROGRAM
//              HLSLPROGRAM
                #pragma vertex Vert
//              #pragma vertex Vert
                #pragma fragment frag
//              #pragma fragment frag

                float4 frag (Varyings input) : SV_Target
//              float4 frag (Varyings input) : SV_Target
                {
//              {
                    float2 uv = input.texcoord;
//                  float2 uv = input.texcoord;
                    float2 texelSize = _BlitTexture_TexelSize.xy;
//                  float2 texelSize = _BlitTexture_TexelSize.xy;
                    float4 weights = SAMPLE_TEXTURE2D_X(_WeightTexture, sampler_LinearClamp, uv);
//                  float4 weights = SAMPLE_TEXTURE2D_X(_WeightTexture, sampler_LinearClamp, uv);

                    UNITY_FLATTEN
//                  UNITY_FLATTEN
                    if (any(weights > 0))
//                  if (any(weights > 0))
                    {
//                  {
                        float4 color = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv);
//                      float4 color = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv);
                        float4 colorL = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + float2(-1, 0) * texelSize);
//                      float4 colorL = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + float2(-1, 0) * texelSize);
                        float4 colorT = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + float2(0, -1) * texelSize);
//                      float4 colorT = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + float2(0, -1) * texelSize);

                        color = lerp(color, colorL, weights.x * 0.5);
//                      color = lerp(color, colorL, weights.x * 0.5);
                        color = lerp(color, colorT, weights.z * 0.5);
//                      color = lerp(color, colorT, weights.z * 0.5);
                        return color;
//                      return color;
                    }
//                  }

                    return SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv);
//                  return SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv);
                }
//              }
                ENDHLSL
//              ENDHLSL
            }
//          }

            // Pass 3: Temporal Resolve
//          // Pass 3: Temporal Resolve
            Pass
//          Pass
            {
//          {
                Name "SMAA_TemporalResolve"
//              Name "SMAA_TemporalResolve"

                HLSLPROGRAM
//              HLSLPROGRAM
                #pragma vertex Vert
//              #pragma vertex Vert
                #pragma fragment frag
//              #pragma fragment frag

                float4 frag (Varyings input) : SV_Target
//              float4 frag (Varyings input) : SV_Target
                {
//              {
                    float2 uv = input.texcoord;
//                  float2 uv = input.texcoord;
                    float2 motionVec = SAMPLE_TEXTURE2D_X(_MotionVectorTexture, sampler_MotionVectorTexture, uv).xy;
//                  float2 motionVec = SAMPLE_TEXTURE2D_X(_MotionVectorTexture, sampler_MotionVectorTexture, uv).xy;

                    float2 historyUV = uv - motionVec;
//                  float2 historyUV = uv - motionVec;

                    float4 currentColor = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv);
//                  float4 currentColor = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv);
                    float4 historyColor = SAMPLE_TEXTURE2D_X(_HistoryTex, sampler_HistoryTex, historyUV);
//                  float4 historyColor = SAMPLE_TEXTURE2D_X(_HistoryTex, sampler_HistoryTex, historyUV);

                    float2 texelSize = _BlitTexture_TexelSize.xy;
//                  float2 texelSize = _BlitTexture_TexelSize.xy;
                    float4 c0 = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + float2(-1, 0) * texelSize);
//                  float4 c0 = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + float2(-1, 0) * texelSize);
                    float4 c1 = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + float2(1, 0) * texelSize);
//                  float4 c1 = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + float2(1, 0) * texelSize);
                    float4 c2 = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + float2(0, -1) * texelSize);
//                  float4 c2 = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + float2(0, -1) * texelSize);
                    float4 c3 = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + float2(0, 1) * texelSize);
//                  float4 c3 = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + float2(0, 1) * texelSize);

                    float4 minColor = currentColor;
//                  float4 minColor = currentColor;
                    float4 maxColor = currentColor;
//                  float4 maxColor = currentColor;
                    float4 neighbors[4] = { c0, c1, c2, c3 };
//                  float4 neighbors[4] = { c0, c1, c2, c3 };

                    UNITY_UNROLL
//                  UNITY_UNROLL
                    for (int i = 0; i < 4; i++)
//                  for (int i = 0; i < 4; i++)
                    {
//                  {
                        minColor = min(minColor, neighbors[i]);
//                      minColor = min(minColor, neighbors[i]);
                        maxColor = max(maxColor, neighbors[i]);
//                      maxColor = max(maxColor, neighbors[i]);
                    }
//                  }
                    historyColor = clamp(historyColor, minColor, maxColor);
//                  historyColor = clamp(historyColor, minColor, maxColor);

                    float outOfBounds = 0.0;
//                  float outOfBounds = 0.0;

                    UNITY_FLATTEN
//                  UNITY_FLATTEN
                    if (historyUV.x < 0.0 || historyUV.x > 1.0 || historyUV.y < 0.0 || historyUV.y > 1.0)
//                  if (historyUV.x < 0.0 || historyUV.x > 1.0 || historyUV.y < 0.0 || historyUV.y > 1.0)
                    {
//                  {
                        outOfBounds = 1.0;
//                      outOfBounds = 1.0;
                    }
//                  }
                    float weight = lerp(_TemporalBlendWeight, 0.0, outOfBounds);
//                  float weight = lerp(_TemporalBlendWeight, 0.0, outOfBounds);

                    return lerp(currentColor, historyColor, weight);
//                  return lerp(currentColor, historyColor, weight);
                }
//              }
                ENDHLSL
//              ENDHLSL
            }
//          }
        }
//      }
    }
//  }
