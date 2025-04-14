Shader "Custom/Plane Shader"
{
    Properties
    {
      
        [Header(Textures)]
        _GrassTexture ("Grass Texture", 2D) = "white" {}
        _PathTexture("Path Texture",2D) = "White" {}
        _RiverTexture("River Texture",2D) = "White" {}
        _PathMask("Path Mask",2D) = "White" {}
        _RiverMask("River Mask",2D) = "White" {}        

        [Header(Spot Light)]
        _SpotLightPos("Spot Light Position",vector) = (0,0,0)
        _SpotLightIntensity("Spot Light Intensity",float) = 0
        _SpotLightDir("Spot Light Direction",vector) = (0,0,0)
        _SpotLightColor("Spot Light Color",COLOR) = (1,1,1,1)
        _SpotLightInnerCos("Spot Light Inner Cos",float) = 0
        _SpotLightOuterCos("Spot Light Outer Cos",float) = 0
        _SpotLightRange("Spot Light Range",float) = 0

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
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            //ZWrite Off
            Cull Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "Lighting.cginc"

            #define TAU 6.283185307179586

            sampler2D _GrassTexture;
            sampler2D _PathTexture;
            sampler2D _RiverTexture;
            sampler2D _PathMask;
            sampler2D _RiverMask;
            float4 _GrassTexture_ST;
            float4 _PathTexture_ST;
            float4 _RiverTexture_ST;
            float4 _PathMask_ST;
            float4 _RiverMask_ST;
            float _WaveAmplitude;
            float _WaveSpeed;
            float _WaveFrequency;
            float _PathIntensity;
            float _RiverIntensity;
            float3 _ChannelSetter;
            float _RiverDepth;
            float _RiverSpeed;

            float3 _SpotLightPos;
            float _SpotLightIntensity;
            float3 _SpotLightDir;
            float4 _SpotLightColor;
            float _SpotLightInnerCos;
            float _SpotLightOuterCos;
            float _SpotLightRange;

            void IncreaseIntensity(inout float value,float intensityAmount){
                value = saturate(value * intensityAmount);
            }

            float GenerateWave(float value){
                return cos((value - _Time.y * _WaveSpeed) * TAU * _WaveFrequency) * 2;
            }

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv_grass : TEXCOORD0;
                float2 uv_pathMask : TEXCOORD1;
                float2 uv_path : TEXCOORD2;
                float2 uv_riverMask : TEXCOORD3;
                float2 uv_river : TEXCOORD4;
                float3 worldPos : TEXCOORD5;
                float3 normals : TEXCOORD6;

                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o; 
                
                o.worldPos = mul(unity_ObjectToWorld,v.vertex);

                o.uv_grass = TRANSFORM_TEX(v.uv,_GrassTexture);
                o.uv_pathMask = TRANSFORM_TEX(v.uv, _PathMask);
                o.uv_path = TRANSFORM_TEX(v.uv,_PathTexture);
                o.uv_riverMask = TRANSFORM_TEX(v.uv,_RiverMask);
                o.uv_river = TRANSFORM_TEX(v.uv,_RiverTexture);

                float river = tex2Dlod(_RiverMask,float4(o.uv_riverMask,0,1)).x;
                v.vertex.z = river * (v.vertex.z - _RiverDepth);
               
                IncreaseIntensity(river,_RiverIntensity);
               
                river = saturate((river - 0.9) * 100);
                river = smoothstep(0.1,0.9,river);
                
                float waveX = GenerateWave(v.vertex.x) ;
                float waveY = GenerateWave(v.vertex.y);
                float displacement = waveX * waveY * _WaveAmplitude;

                v.vertex.z -= displacement * river;

                o.normals = UnityObjectToWorldNormal(v.normal);

                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            float LambertVal(float3 normal, float3 lightDir){
                return max(0,dot(normal,lightDir));
            }

            float AngularAnnetuation(float3 normalizedLightDir){
                
                float cosVal = dot(-normalizedLightDir,normalize(_SpotLightDir));
                float angularAnnet = smoothstep(_SpotLightOuterCos,_SpotLightInnerCos,cosVal);
                return angularAnnet;
            }

            float DistanceImpact(float lenght){
                float distAnnet = saturate(1-lenght/_SpotLightRange);
                return distAnnet * distAnnet;
            }

            float4 frag (v2f i) : SV_Target
            {
                
                float4 grass = tex2D(_GrassTexture, i.uv_grass);

                float4 path = tex2D(_PathTexture,i.uv_path);
                path = float4(path.r * _ChannelSetter.r, path.g * _ChannelSetter.g, path.b * _ChannelSetter.b, 1);

                float pathMask = tex2D(_PathMask,i.uv_pathMask).x;
                IncreaseIntensity(pathMask,_PathIntensity);

                i.uv_river.x += frac(_Time.y * -1 * _RiverSpeed); 
                i.uv_river.y += frac(_Time.y  * _RiverSpeed);

                float4 mixedGround = lerp(grass,path,pathMask);

                float4 river = tex2D(_RiverTexture,i.uv_river);

                float riverMask = tex2D(_RiverMask,i.uv_riverMask).x;
                IncreaseIntensity(riverMask,_RiverIntensity);
                
                float4 mixedGroundRiver = lerp(mixedGround,river, riverMask);

                float3 N = i.normals;               
                float3 L = normalize(_WorldSpaceLightPos0.xyz);   

                //for directional light
                float directionalLightDiff = LambertVal(N,L);

                float overallDirectionalLight = directionalLightDiff * _LightColor0;

                //for spot Lighting
                float3 lightDir = _SpotLightPos - i.worldPos; 
                float distance = length(lightDir);
                float3 normalizedLightDir = normalize(lightDir);

                //--lambert for spot Lighting
                float spotLightLambert = LambertVal(N,normalizedLightDir);

                //other calculations
                float angularAnnet = AngularAnnetuation(normalizedLightDir);
                float distanceImpactVal = DistanceImpact(distance);
                
                float4 spotImpact = spotLightLambert * distanceImpactVal * angularAnnet
                * _SpotLightIntensity * _SpotLightColor;

                return mixedGroundRiver * overallDirectionalLight + mixedGroundRiver * spotImpact;
            }
            ENDCG
        }
    }
}
