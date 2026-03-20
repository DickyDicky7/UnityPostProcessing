    Shader "Hidden/CustomFXAA"
//  Shader "Hidden/CustomFXAA"
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

            Pass
//          Pass
            {
//          {
                Name "CustomFXAA"
//              Name "CustomFXAA"
                
                HLSLPROGRAM
//              HLSLPROGRAM
                #pragma vertex Vert
//              #pragma vertex Vert
                #pragma fragment frag
//              #pragma fragment frag
                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
//              #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
                #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
//              #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

                // ADDED: Explicitly declare the texel size variable so Unity can populate it.
//              // ADDED: Explicitly declare the texel size variable so Unity can populate it.
                float4 _BlitTexture_TexelSize;
//              float4 _BlitTexture_TexelSize;

                #define FXAA_SPAN_MAX 8.0
//              #define FXAA_SPAN_MAX 8.0
                #define FXAA_REDUCE_MUL (1.0 /   8.0)
//              #define FXAA_REDUCE_MUL (1.0 /   8.0)
                #define FXAA_REDUCE_MIN (1.0 / 128.0)
//              #define FXAA_REDUCE_MIN (1.0 / 128.0)
                #define LUMA_WEIGHTS float3(0.299, 0.587, 0.114)
//              #define LUMA_WEIGHTS float3(0.299, 0.587, 0.114)

                float4 frag (Varyings input) : SV_Target
//              float4 frag (Varyings input) : SV_Target
                {
//              {
                    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
//                  UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                    float2 uv = input.texcoord;
//                  float2 uv = input.texcoord;
                
                    // Now this will correctly reference the declared variable above
//                  // Now this will correctly reference the declared variable above
                    float4 texelSize = _BlitTexture_TexelSize; 
//                  float4 texelSize = _BlitTexture_TexelSize; 
                    float2 texCoordOffset = texelSize.xy;
//                  float2 texCoordOffset = texelSize.xy;

                    float3 luma = LUMA_WEIGHTS;
//                  float3 luma = LUMA_WEIGHTS;
                    float lumaTL = dot(luma, SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + float2(-1.0, -1.0) * texCoordOffset).rgb);
//                  float lumaTL = dot(luma, SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + float2(-1.0, -1.0) * texCoordOffset).rgb);
                    float lumaTR = dot(luma, SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + float2( 1.0, -1.0) * texCoordOffset).rgb);
//                  float lumaTR = dot(luma, SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + float2( 1.0, -1.0) * texCoordOffset).rgb);
                    float lumaBL = dot(luma, SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + float2(-1.0,  1.0) * texCoordOffset).rgb);
//                  float lumaBL = dot(luma, SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + float2(-1.0,  1.0) * texCoordOffset).rgb);
                    float lumaBR = dot(luma, SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + float2( 1.0,  1.0) * texCoordOffset).rgb);
//                  float lumaBR = dot(luma, SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + float2( 1.0,  1.0) * texCoordOffset).rgb);
                    float lumaM  = dot(luma, SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv).rgb);
//                  float lumaM  = dot(luma, SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv).rgb);

                    float lumaMin = min(lumaM, min(min(lumaTL, lumaTR), min(lumaBL, lumaBR)));
//                  float lumaMin = min(lumaM, min(min(lumaTL, lumaTR), min(lumaBL, lumaBR)));
                    float lumaMax = max(lumaM, max(max(lumaTL, lumaTR), max(lumaBL, lumaBR)));
//                  float lumaMax = max(lumaM, max(max(lumaTL, lumaTR), max(lumaBL, lumaBR)));

                    float2 dir;
//                  float2 dir;
                    dir.x = -((lumaTL + lumaTR) - (lumaBL + lumaBR));
//                  dir.x = -((lumaTL + lumaTR) - (lumaBL + lumaBR));
                    dir.y =  ((lumaTL + lumaBL) - (lumaTR + lumaBR));
//                  dir.y =  ((lumaTL + lumaBL) - (lumaTR + lumaBR));

                    float dirReduce = max((lumaTL + lumaTR + lumaBL + lumaBR) * (0.25 * FXAA_REDUCE_MUL), FXAA_REDUCE_MIN);
//                  float dirReduce = max((lumaTL + lumaTR + lumaBL + lumaBR) * (0.25 * FXAA_REDUCE_MUL), FXAA_REDUCE_MIN);

                    float rcpDirMin = 1.0 / (min(abs(dir.x), abs(dir.y)) + dirReduce);
//                  float rcpDirMin = 1.0 / (min(abs(dir.x), abs(dir.y)) + dirReduce);

                    dir = min(float2(FXAA_SPAN_MAX, FXAA_SPAN_MAX), max(float2(-FXAA_SPAN_MAX, -FXAA_SPAN_MAX), dir * rcpDirMin)) * texCoordOffset;
//                  dir = min(float2(FXAA_SPAN_MAX, FXAA_SPAN_MAX), max(float2(-FXAA_SPAN_MAX, -FXAA_SPAN_MAX), dir * rcpDirMin)) * texCoordOffset;

                    float3 rgbA = (1.0 / 2.0) * (
//                  float3 rgbA = (1.0 / 2.0) * (
                        SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + dir * (1.0 / 3.0 - 0.5)).rgb +
//                      SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + dir * (1.0 / 3.0 - 0.5)).rgb +
                        SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + dir * (2.0 / 3.0 - 0.5)).rgb);
//                      SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + dir * (2.0 / 3.0 - 0.5)).rgb);

                    float3 rgbB = rgbA * (1.0 / 2.0) + (1.0 / 4.0) * (
//                  float3 rgbB = rgbA * (1.0 / 2.0) + (1.0 / 4.0) * (
                        SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + dir * (0.0 / 3.0 - 0.5)).rgb +
//                      SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + dir * (0.0 / 3.0 - 0.5)).rgb +
                        SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + dir * (3.0 / 3.0 - 0.5)).rgb);
//                      SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + dir * (3.0 / 3.0 - 0.5)).rgb);

                    float lumaB = dot(rgbB, luma);
//                  float lumaB = dot(rgbB, luma);

                    float3 resultColor;
//                  float3 resultColor;
                    UNITY_FLATTEN
//                  UNITY_FLATTEN
                    if ((lumaB < lumaMin) || (lumaB > lumaMax)) {
//                  if ((lumaB < lumaMin) || (lumaB > lumaMax)) {
                        resultColor = rgbA;
//                      resultColor = rgbA;
                    } else {
//                  } else {
                        resultColor = rgbB;
//                      resultColor = rgbB;
                    }
//                  }

                    return float4(resultColor, 1.0);
//                  return float4(resultColor, 1.0);
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
