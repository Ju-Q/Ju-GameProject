Shader "Unlit/VibrantGrassShaderInteractionDrawer"
{
    Properties
    {
		_MainTex("Texture", 2D) = "white" {}
		_ArraysLength("ArraysLength", int) = 0
		_ErasingSpeed("Erasing Speed", Range(0, 1)) = 0.01
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
            float4 _MainTex_ST, _MainTex_TexelSize;
			fixed4 _Coordinate[1000];
			float _Size[1000], _Strength[1000];
			float _ErasingSpeed;
			int _ArraysLength;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }
			
			float InverseLerp(float A, float B, float T)
			{
				float Out = (T - A) / (B - A);
				return Out;
			}

            fixed4 frag (v2f IN) : SV_Target
            {
					float2 INuv = IN.uv;
					fixed4 col = tex2D(_MainTex, INuv);
					fixed4 neutralCol = fixed4(0.5, 0.5, 0.0, 0.0);
					if (length(col) <= 0.0) col = neutralCol;
					fixed4 colErased = lerp(col, neutralCol, _ErasingSpeed);
					fixed4 result = colErased;
					for (int i = 0; i < _ArraysLength; i++)
					{
						float draw = saturate(1 - InverseLerp(0, _Size[i], distance(INuv, _Coordinate[i])));
						float2 direction = lerp(fixed4(0,0,0,0), (normalize(INuv - _Coordinate[i]) / 2), draw * _Strength[i]);
						fixed4 drawCol = fixed4(direction.x, direction.y, 0.0, 0.0);
						result += drawCol;
					}
					return saturate(result);
            }
            ENDCG
        }
    }
}
