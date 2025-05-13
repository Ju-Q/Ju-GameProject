Shader "Hidden/VibrantGrassShaderAddTexture"
{
    Properties
    {
		_MainTex("Texture", 2D) = "white" {}
		_AddedTex("AddedTex", 2D) = "white" {}
		_Color("Color", Color) = (0, 0, 0, 0)
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

            sampler2D _MainTex, _AddedTex;
			fixed4 _Color;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
				fixed4 addedColor = tex2D(_AddedTex, i.uv) * _Color;
				fixed4 colResult = tex2D(_MainTex, i.uv) + addedColor;
				return colResult;
            }
            ENDCG
        }
    }
}
