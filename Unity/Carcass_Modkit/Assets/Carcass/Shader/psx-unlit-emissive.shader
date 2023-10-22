Shader "psx/unlit/emissive"
{
    Properties
    {
        _Color("Color", Color) = (1, 1, 1, 1)
		_MainTex("Base (RGB)", 2D) = "white" {}
		_EmissiveTex("Emissive Mask Texture (G)", 2D) = "black" {}
		[Toggle] _UseAlbedoAsEmissive("Use Base Texture as Emissve Color", Float) = 0
		[Toggle] _EmissiveReplaces("Emissive Replaces Instead of Adding to Underlying Color", Float) = 0
		_EmissiveColor("Emissive Color (RGB)", Color) = (0.0, 0.0, 0.0, 0.0)
		_EmissiveIntensity("Emissive Strength", Float) = 1
		_VertexWarpScale("Vertex Warping Scalar", Range(0,10)) = 1
		[Toggle] _Outline("Assist Outline", Float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
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
            float4 _MainTex_ST;
            float4 _Color;
            sampler2D _EmissiveTex;
            float4 _EmissiveColor;
            float _EmissiveIntensity;


            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv) * _Color;
                float e = tex2D(_EmissiveTex, i.uv).r;

                col.rgb *= (1-e);

                col.rgb += e * _EmissiveColor;

                return col;
            }
            ENDCG
        }
    }
}
