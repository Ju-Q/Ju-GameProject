Shader "Custom/LuminousShader"
{
    Properties
    {
        _Color("��ɫ", Color) = (1, 1, 1, 1)
        _EmissionColor("������ɫ", Color) = (1, 1, 1, 1)
        _MainTex("����", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        Pass
        {
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
                float4 color : COLOR;
            };

            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _EmissionColor;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = _Color;
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                half4 tex = tex2D(_MainTex, i.uv);
                return tex * i.color + _EmissionColor; // ��ӷ�����ɫ
            }
            ENDHLSL
        }
    }
    FallBack "Diffuse"
}
