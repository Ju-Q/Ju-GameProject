Shader "Custom/FrostedGlassShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _Opacity ("Opacity", Range(0, 1)) = 0.5
        _BlurSize ("Blur Size", Range(0, 10)) = 1
        _EdgeSoftness ("Edge Softness", Range(0, 10)) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" }
        LOD 200

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Front
            ZWrite Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
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
            float4 _MainTex_TexelSize;
            float4 _Color;
            float _Opacity;
            float _BlurSize;
            float _EdgeSoftness;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                // 模糊效果
                float4 col = tex2D(_MainTex, i.uv);
                float4 blurred = col * _Opacity * _Color;

                // 简单模糊算法
                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        float2 offset = float2(x, y) * _BlurSize * _MainTex_TexelSize.xy;
                        blurred += tex2D(_MainTex, i.uv + offset) * _Opacity * _Color;
                    }
                }
                blurred /= 9.0;

                return blurred;
            }
            ENDHLSL
        }
    }
    FallBack "Diffuse"
}
