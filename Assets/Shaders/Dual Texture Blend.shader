Shader "Custom/DualTextureBlendURP"
{
    Properties
    {
        _MainTex ("Texture 1", 2D) = "white" {}
        _BlendTex1 ("Texture 2", 2D) = "white" {}
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalRenderPipeline" }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // Declare the textures and samplers
            TEXTURE2D(_MainTex);
            TEXTURE2D(_BlendTex1);
            SAMPLER(sampler_MainTex);
            SAMPLER(sampler_BlendTex1);

            // Structure for input attributes (vertex data)
            struct Attributes
            {
                float4 positionOS : POSITION;  // Object space position
                float2 uv_MainTex : TEXCOORD0; // UV for main texture
                float2 uv_BlendTex1 : TEXCOORD1; // UV for blend texture
                float4 color : COLOR; // Vertex color (for alpha blending)
            };

            // Structure for passing data between shaders
            struct Varyings
            {
                float4 positionHCS : SV_POSITION; // Clip space position
                float2 uv_MainTex : TEXCOORD0; // UV for main texture
                float2 uv_BlendTex1 : TEXCOORD1; // UV for blend texture
                float4 color : TEXCOORD2; // Vertex color for alpha
            };

            // Vertex shader: Transforms object space position to clip space and passes UVs and color
            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv_MainTex = IN.uv_MainTex;
                OUT.uv_BlendTex1 = IN.uv_BlendTex1;
                OUT.color = IN.color;
                return OUT;
            }

            // Fragment shader: Blends the two textures based on the vertex color alpha
            half4 frag(Varyings IN) : SV_Target
            {
                // Sample both textures
                half4 mainTexColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv_MainTex);
                half4 blendTexColor = SAMPLE_TEXTURE2D(_BlendTex1, sampler_BlendTex1, IN.uv_BlendTex1);

                // If the alpha channel is used, perform blending, otherwise use the main texture
                half alphaFactor = IN.color.w;
                half4 finalColor = mainTexColor * alphaFactor + blendTexColor * (1.0 - alphaFactor);

                // Return the final blended color
                return finalColor;
            }

            ENDHLSL
        }
    }
}
