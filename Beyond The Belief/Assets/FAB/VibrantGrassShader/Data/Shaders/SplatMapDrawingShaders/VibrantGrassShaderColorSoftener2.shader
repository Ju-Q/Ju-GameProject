﻿Shader "Hidden/VibrantGrassShaderColorSoftener2"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
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
			float4 _MainTex_TexelSize;
            float4 _MainTex_ST;
			fixed4 _Coordinate;
			half _Size;
			//half _Strength;

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
                fixed4 col = tex2D(_MainTex, i.uv);
				fixed4 rightNeighbourCol = tex2D(_MainTex, i.uv + float2(_MainTex_TexelSize.x, 0));
				fixed4 leftNeighbourCol = tex2D(_MainTex, i.uv - float2(_MainTex_TexelSize.x, 0));
				fixed4 topNeighbourCol = tex2D(_MainTex, i.uv + float2(0, _MainTex_TexelSize.y));
				fixed4 bottomNeighbourCol = tex2D(_MainTex, i.uv - float2(0, _MainTex_TexelSize.y));
				fixed4 averageColor = (col + rightNeighbourCol + leftNeighbourCol + topNeighbourCol + bottomNeighbourCol) / 5;
				return averageColor;
            }
            ENDCG
        }
    }
}
