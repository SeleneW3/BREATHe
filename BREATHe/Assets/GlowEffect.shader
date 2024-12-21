Shader "Custom/GlowEffect"
{
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _GlowColor ("Glow Color", Color) = (1, 1, 1, 1)
        _GlowIntensity ("Glow Intensity", Range(0, 10)) = 1
    }
    SubShader
    {
        Tags {"Queue"="Overlay" "RenderType"="Opaque"}
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            float _GlowIntensity;
            float4 _GlowColor;
            sampler2D _MainTex;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }

            half4 frag(v2f i) : COLOR
            {
                half4 texColor = tex2D(_MainTex, i.uv);
                return texColor * (1.0 + _GlowIntensity) + _GlowColor * _GlowIntensity;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
