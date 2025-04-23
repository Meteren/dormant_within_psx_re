Shader "Custom/URP_MultiLight_Shadow"
{
    Properties
    {
        _BaseColor("Base Color", Color) = (1,1,1,1)
    }

    SubShader
    {
        tags{"RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry"}

        pass
        {
            name "ForwardPass"

            tags{"LightMode"="UniversalForward"}

            HLSLPROGRAM

            #define _SPECULAR_COLOR_SPECULAR_COLOR
            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature _FORWARD_PLUS
            #pragma shader_feature_fragment _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma shader_feature_fragment _ADDITIONAL_LIGHT_SHADOWS

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            float4 _BaseColor;
            
            struct Attributes{
                float3 positionLS : POSITION;
                float3 normalLS : NORMAL;

            };

            struct Varyings{
                float4 positionCS : SV_POSITION;
                float3 normalWS : TEXCOORD0;
                float3 positionWS : TEXCOORD1;

            };

            Varyings vert(Attributes i){
                Varyings o;

                o.positionCS = TransformObjectToHClip(i.positionLS);
                o.normalWS = TransformObjectToWorldNormal(i.normalLS);
                o.positionWS = TransformObjectToWorld(i.positionLS);
                return o;
            }

            float4 frag(Varyings i) : SV_TARGET{
                InputData light = (InputData)0;
                light.positionWS = i.positionWS;
                light.normalWS = normalize(i.normalWS);
                light.viewDirectionWS = GetWorldSpaceViewDir(i.positionWS);
                light.shadowCoord = TransformWorldToShadowCoord(i.positionWS); 

                SurfaceData surface = (SurfaceData)0;
                surface.albedo = _BaseColor.rgb;
                surface.alpha = 1;
                surface.smoothness = 0.9;
                surface.specular = 0.9;
             
                return UniversalFragmentBlinnPhong(light,surface) + unity_AmbientSky;
            }

            ENDHLSL
        }
        pass
        {
            name "ShadowCaster"
            tags{"LightMode"="ShadowCaster"}

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag
 
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            inline real LerpWhiteTo(real rgb, real t) {
                return lerp(rgb, 1.0, t);
            }

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"


            float3 _LightDirection;
            
            struct Attributes{
                float3 positionLS : POSITION;
                float3 normalLS : NORMAL;

            };

            float4 GetShadowPositionHClip(Attributes i)
            {
                float3 positionWS = TransformObjectToWorld(i.positionLS);
                float3 normalWS = TransformObjectToWorldNormal(i.normalLS);
                float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS,normalWS,_LightDirection));

                return positionCS;
            }

            struct Varyings{
                float4 positionCS : SV_POSITION;
        
            };

            Varyings vert(Attributes i){
                Varyings o;

                o.positionCS = GetShadowPositionHClip(i);
               
                return o;
            }

            float4 frag(Varyings i) : SV_TARGET{
        
                return 0;
            }
            ENDHLSL
        }

    }
       
}