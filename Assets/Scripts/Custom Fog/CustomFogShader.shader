Shader "PostEffect/CustomFogShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _FogStart("Fog Start",float) = 0
        _FogEnd("Fog End",float) = 0
        _FogColor("Fog Color",COLOR) = (1,1,1,1)
        _FogDensity("Fog Density",float) = 0
        _FogDistance("Fog Distance",float) = 0
        _FogMethod("Fog Method",float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque"}
        LOD 100

        Pass
        {
            Cull Off ZWrite Off ZTest Always 

            CGPROGRAM  
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            sampler2D _CameraDepthTexture;
            float _FogStart;
            float _FogEnd;
            float4 _FogColor;
            float _FogDensity;
            float _FogDistance;
            float _FogMethod;
              
            float ComputeDistance(float Depth)
            {  
                float dist = LinearEyeDepth(Depth);
                dist -=  _ProjectionParams.y * _FogDistance;
                return dist;
            }
        
            float ComputeFog(float z)
            {
                float fogFactor = exp2(-_FogDensity * z);
                return saturate(fogFactor);
            }

            float ExponentialFog(float Depth){
                float dist = ComputeDistance(Depth);
                float fogFactor = 1 -  ComputeFog(dist);
                return fogFactor;
            }        

            float LinearFog(float linearDepth){
                float fogFactor = (linearDepth - _FogStart) / (_FogEnd - _FogStart);         
                return fogFactor;
            }            

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float4 col = tex2D(_MainTex, i.uv);

                float Depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture,i.uv);
                float linearDepth = LinearEyeDepth(Depth);
                           
                float fogFactor = lerp(LinearFog(linearDepth),ExponentialFog(Depth),_FogMethod);

                return lerp(col,_FogColor,saturate(fogFactor));
            }
            ENDCG

        }

   
    }
}
