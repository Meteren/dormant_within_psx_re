Shader "Custom/URP_MultiLight_Shadow"
{
    Properties
    {
         [Header(Textures)]
        _GrassTexture ("Grass Texture", 2D) = "white" {}
        _PathTexture("Path Texture",2D) = "White" {}
        _RiverTexture("River Texture",2D) = "White" {}
        _PathMask("Path Mask",2D) = "White" {}
        _RiverMask("River Mask",2D) = "White" {}            

         [Header(Wave Adjustments)]
        _WaveAmplitude("Wave Amplitude",Range(0,20)) = 0
        _WaveSpeed("Wave Speed",float) = 0
        _WaveFrequency("Wave Frequency",float) = 0
        _RiverDepth("River Depth",Range(0,0.1)) = 0
        _RiverSpeed("River Speed",float) = 0

         [Header(Path Intensity)]
        _PathIntensity("Path Intensity", float) = 0

         [Header(River Intensity)]
        _RiverIntensity("River Intensity",float) = 0

         [Header(Path Channel Setter)]
        _ChannelSetter("Path Channel Mult Vals",FLOAT) = (0,0,0)
    }

    SubShader
    {
        tags{"RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry"}

        pass
        {
            name "ForwardPass"
            tags{"LightMode"="UniversalForward"}

            Cull Off

            HLSLPROGRAM

            #define TAU 6.283185307179586
            #define _SPECULAR_COLOR_SPECULAR_COLOR
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH 

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_GrassTexture); SAMPLER(sampler_GrassTexture);
            float4 _GrassTexture_ST;

           
            TEXTURE2D(_PathTexture); SAMPLER(sampler_PathTexture);
            float4 _PathTexture_ST;

            
            TEXTURE2D(_RiverTexture); SAMPLER(sampler_RiverTexture);
            float4 _RiverTexture_ST;

            
            TEXTURE2D(_PathMask); SAMPLER(sampler_PathMask);
            float4 _PathMask_ST;
     
            
            TEXTURE2D(_RiverMask); SAMPLER(sampler_RiverMask);
            float4 _RiverMask_ST;


            float _PathIntensity;

            float _WaveAmplitude;
            float _WaveSpeed;
            float _WaveFrequency;
            float _RiverIntensity;
            float3 _ChannelSetter;
            float _RiverDepth;
            float _RiverSpeed;

            
            void IncreaseIntensity(inout float value,float intensityAmount){
                value = saturate(value * intensityAmount);
            }

            
            float GenerateWave(float value){
                return cos((value - _Time.y * _WaveSpeed) * TAU * _WaveFrequency) * 2;
            }
            
            struct Attributes{
                float2 uv : TEXCOORD0;
                float3 positionLS : POSITION;
                float3 normalLS : NORMAL;

            };

            struct Varyings{
                float4 positionCS : SV_POSITION;
                float3 normalWS : TEXCOORD0;
                float3 positionWS : TEXCOORD1;

                float2 uv_grass : TEXCOORD3;
                float2 uv_pathMask : TEXCOORD4;
                float2 uv_path : TEXCOORD5;
                float2 uv_riverMask : TEXCOORD6;
                float2 uv_river : TEXCOORD7;

            };

            Varyings vert(Attributes i){

                Varyings o;
        
                o.uv_grass = TRANSFORM_TEX(i.uv, _GrassTexture);
                o.uv_pathMask = TRANSFORM_TEX(i.uv, _PathMask);
                o.uv_path = TRANSFORM_TEX(i.uv,_PathTexture);
                o.uv_riverMask = TRANSFORM_TEX(i.uv,_RiverMask);
                o.uv_river = TRANSFORM_TEX(i.uv,_RiverTexture);


                float riverMask = SAMPLE_TEXTURE2D_LOD(_RiverMask,sampler_RiverMask,o.uv_riverMask,1).r;
                IncreaseIntensity(riverMask,_RiverIntensity);

                i.positionLS.z = (i.positionLS.z - _RiverDepth) * riverMask;

                riverMask = saturate((riverMask - 0.9) * 100);
                riverMask = smoothstep(0.1,0.9,riverMask);

                float waveX = GenerateWave(i.positionLS.x);
                float waveY = GenerateWave(i.positionLS.y);

                i.positionLS.z -= waveX * waveY * _WaveAmplitude * riverMask;

                o.positionCS = TransformObjectToHClip(i.positionLS);
                o.normalWS = TransformObjectToWorldNormal(i.normalLS);
                o.positionWS = TransformObjectToWorld(i.positionLS);


                return o;
            }

            float4 frag(Varyings i) : SV_TARGET{
                
                float4 grass = SAMPLE_TEXTURE2D(_GrassTexture,sampler_GrassTexture,i.uv_grass);
                float4 path = SAMPLE_TEXTURE2D(_PathTexture,sampler_PathTexture,i.uv_path);
                float pathMask = SAMPLE_TEXTURE2D(_PathMask,sampler_PathMask,i.uv_pathMask).r;

                path = float4(path.r * _ChannelSetter.r, path.g * _ChannelSetter.g, path.b * _ChannelSetter.b, 1);

                IncreaseIntensity(pathMask,_PathIntensity);

                float4 pathGrassMixed = lerp(grass,path,pathMask);

                i.uv_river.x += _Time.y * -1 * _RiverSpeed; 
                i.uv_river.y += _Time.y  * _RiverSpeed;

                float4 river = SAMPLE_TEXTURE2D(_RiverTexture,sampler_RiverTexture,i.uv_river);
                float riverMask = SAMPLE_TEXTURE2D(_RiverMask,sampler_RiverMask,i.uv_riverMask).r;

                IncreaseIntensity(riverMask,_RiverIntensity);            
                
                float4 pathGrassMixedWithRiver = lerp(pathGrassMixed,river,riverMask);

                InputData light = (InputData)0;
                light.positionWS = i.positionWS;
                light.normalWS = normalize(i.normalWS);
                light.viewDirectionWS = GetWorldSpaceViewDir(i.positionWS);
                light.shadowCoord = TransformWorldToShadowCoord(i.positionWS); 
                light.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(i.positionCS);

                SurfaceData surface = (SurfaceData)0;
                surface.albedo = pathGrassMixedWithRiver.rgb;
                surface.alpha = pathGrassMixedWithRiver.a;
                //surface.normal = i.normalWS; 
                surface.occlusion = 1.0;
                surface.smoothness = 0.5;
                surface.specular = 0.5;
             
                return UniversalFragmentBlinnPhong(light,surface);
            }

            ENDHLSL
        }   

    }
       
}