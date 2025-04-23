Shader "Custom/PlaneSurfaceShader"
{
    Properties
    {

        _Grass ("Grass Texture", 2D) = "white" {}
        _Path ("Path Texture",2D) = "white" {}
        _PathMask ("Path Mask", 2D) = "white" {}
        _River ("River Tex",2D) = "white" {}
        _RiverMask ("River Mask", 2D) = "white" {}
       
        _ChannelSetter("Channel Setter",float) = (0,0,0,0)

        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0

        [Header(Wave Features)]
        _WaveSpeed("Wave Speed",Range(0,1)) = 0
        _WaveFrequency("Wave Frequency",Range(0,5000)) = 0
        _WaveAmplitude("Wave Amplitude",Range(0,2)) = 0
        _RiverDepth("RiverDepth",Range(0,10)) = 0
        _RiverSpeed("River Speed",Range(0,10)) = 0

    }
    SubShader
    {
        Cull Off
        Tags { "RenderType"="Opaque" }
        LOD 200
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows vertex:vert
    
        #pragma target 3.0
        #define TAU 6.283185307179586

        sampler2D _Grass;
        float4 _Grass_ST;

        sampler2D _Path;
        float4 _Path_ST;

        sampler2D _River;
        float4 _River_ST;

        sampler2D _RiverMask;
        float4 _RiverMask_ST;

        sampler2D _PathMask;
        float4 _PathMask_ST;

        float4 _ChannelSetter;

        float _WaveSpeed;
        float _WaveFrequency;
        float _WaveAmplitude;
        float _RiverDepth;
        float _RiverSpeed;

        void Intensify(inout float target, float amount){
            target = saturate(target * amount);
        }

        float GenerateWave(float value){
            float waveFactor = cos((value - _Time.y * _WaveSpeed) * TAU * _WaveFrequency);
            return waveFactor;
        }

        struct Input
        {
            float2 base_uv;         
            float4 worldPos;
            float2 riverMask_uv;
        };

        half _Glossiness;
        half _Metallic;

        void vert(inout appdata_full v, out Input o)
        {              
            o.base_uv = v.texcoord.xy;
            o.worldPos = mul(unity_ObjectToWorld,v.vertex);   
            o.riverMask_uv = TRANSFORM_TEX(o.base_uv,_RiverMask);
            float riverMask = tex2Dlod(_RiverMask,float4(o.riverMask_uv,0,1)).r;

            v.vertex.z = riverMask * (v.vertex.z - _RiverDepth);

            Intensify(riverMask,3);
            riverMask = saturate(((riverMask - 0.9) * 100));
            riverMask = smoothstep(0.1,0.9,riverMask);

            float waveFactorX = GenerateWave(v.vertex.x);
            float waveFactorY = GenerateWave(v.vertex.y);

            v.vertex.z -= waveFactorX * waveFactorY * _WaveAmplitude * riverMask;  

        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
           
            float2 grass_uv = TRANSFORM_TEX(IN.base_uv, _Grass);
            float2 path_uv = TRANSFORM_TEX(IN.base_uv, _Path);
            float2 pathMask_uv = TRANSFORM_TEX(IN.base_uv, _PathMask);
            float2 river_uv = TRANSFORM_TEX(IN.base_uv, _River);

            float4 grassTex = tex2D(_Grass, grass_uv);
            float4 pathTex = tex2D(_Path, path_uv);

            pathTex *= _ChannelSetter;

            river_uv.x -= _Time.y * _RiverSpeed;
            river_uv.y += _Time.y * _RiverSpeed;

            float pathMask = tex2D(_PathMask, pathMask_uv).r;
            Intensify(pathMask, 3);

            float4 riverTex = tex2D(_River, river_uv);
            float riverMask = tex2D(_RiverMask, IN.riverMask_uv).r;
            Intensify(riverMask,3);

            float4 mixedPlane = lerp(grassTex, pathTex, pathMask); 
            float4 mixedPlaneWithRiver = lerp(mixedPlane, riverTex, riverMask);
    
            o.Albedo = mixedPlaneWithRiver.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = mixedPlaneWithRiver.a;
        }
        ENDCG

        pass
        {
            Name "ShadowCaseter"
            Tags { "LightMode"="ShadowCaster"}
            ZWrite On

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
            };


            struct v2f
            {
                float4 vertex : SV_POSITION;
            };


            v2f vert(appdata v)
            {
                v2f o;

                o.vertex = UnityWorldToClipPos(v.vertex);

                return o;

            }

            float4 frag(v2f i) : SV_TARGET
            {
                return 0;
            }


            ENDCG
        }
    }
    FallBack "Diffuse"
}
