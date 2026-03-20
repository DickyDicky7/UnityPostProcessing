    Shader "Hidden/Custom/PBRNeutralTonemap"
//  Shader "Hidden/Custom/PBRNeutralTonemap"
    {
//  {
        SubShader
//      SubShader
        {
//      {
            Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}
//          Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}
            LOD 100
//          LOD 100
            ZTest Always ZWrite Off Cull Off
//          ZTest Always ZWrite Off Cull Off
            Pass
//          Pass
            {
//          {
                Name "PBRNeutralTonemap"
//              Name "PBRNeutralTonemap"
                HLSLPROGRAM
//              HLSLPROGRAM
                #pragma vertex Vert
//              #pragma vertex Vert
                #pragma fragment Frag
//              #pragma fragment Frag
                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
//              #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
                #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
//              #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            
                // Khronos PBR Neutral Tonemapping Math
//              // Khronos PBR Neutral Tonemapping Math
                // Source: https://github.com/KhronosGroup/ToneMapping/tree/main/PBR_Neutral
//              // Source: https://github.com/KhronosGroup/ToneMapping/tree/main/PBR_Neutral
                half3 PBRNeutralTonemap(half3 color)
//              half3 PBRNeutralTonemap(half3 color)
                {
//              {
                    // Compression and desaturation constants
//                  // Compression and desaturation constants
                    const half startCompression = 0.8h - 0.04h;
//                  const half startCompression = 0.8h - 0.04h;
                    const half desaturation = 0.15h;
//                  const half desaturation = 0.15h;
                
                    // Find the minimum channel value
//                  // Find the minimum channel value
                    half x = min(color.r, min(color.g, color.b));
//                  half x = min(color.r, min(color.g, color.b));
                
                    // Calculate offset for toe curve
//                  // Calculate offset for toe curve
                    half offset;
//                  half offset;
                    UNITY_FLATTEN
//                  UNITY_FLATTEN
                    if (x < 0.08h)
//                  if (x < 0.08h)
                    {
//                  {
                        offset = x - 6.25h * x * x;
//                      offset = x - 6.25h * x * x;
                    }
//                  }
                    else
//                  else
                    {
//                  {
                        offset = 0.04h;
//                      offset = 0.04h;
                    }
//                  }
                
                    // Apply offset
//                  // Apply offset
                    color -= offset;
//                  color -= offset;
                
                    // Find the peak channel value
//                  // Find the peak channel value
                    half peak = max(color.r, max(color.g, color.b));
//                  half peak = max(color.r, max(color.g, color.b));
                
                    // If peak is below startCompression, return color
//                  // If peak is below startCompression, return color
                    UNITY_FLATTEN
//                  UNITY_FLATTEN
                    if (peak < startCompression)
//                  if (peak < startCompression)
                    {
//                  {
                        return color;
//                      return color;
                    }
//                  }
                
                    // Calculate new peak and compression factor
//                  // Calculate new peak and compression factor
                    half d = 1.0h - startCompression;
//                  half d = 1.0h - startCompression;
                    half newPeak = 1.0h - d * d / (peak + d - startCompression);
//                  half newPeak = 1.0h - d * d / (peak + d - startCompression);
                
                    // Compress color
//                  // Compress color
                    color *= newPeak / peak;
//                  color *= newPeak / peak;
                
                    // Calculate desaturation factor
//                  // Calculate desaturation factor
                    half g = 1.0h - 1.0h / (desaturation * (peak - newPeak) + 1.0h);
//                  half g = 1.0h - 1.0h / (desaturation * (peak - newPeak) + 1.0h);
                
                    // Apply desaturation
//                  // Apply desaturation
                    return lerp(color, newPeak, g);
//                  return lerp(color, newPeak, g);
                }
//              }
            
                half4 Frag(Varyings input) : SV_Target
//              half4 Frag(Varyings input) : SV_Target
                {
//              {
                    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
//                  UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                
                    // Sample the screen texture
//                  // Sample the screen texture
                    half3 color = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, input.texcoord).rgb;
//                  half3 color = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, input.texcoord).rgb;
                
                    // Apply PBR Neutral Tonemap
//                  // Apply PBR Neutral Tonemap
                    color = PBRNeutralTonemap(color);
//                  color = PBRNeutralTonemap(color);
                
                    return half4(color, 1.0h);
//                  return half4(color, 1.0h);
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
