Shader "Hidden/VibrantGrassShaderLightDrawer"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_ArraysLength("ArraysLength", int) = 0
	}
		SubShader
		{
			Tags { "RenderType" = "Opaque" }
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
				fixed4 _Coordinate[1000], _Color[1000];
				float _Size[1000], _Strength[1000];
				int _ArraysLength;

				v2f vert(appdata v)
				{
					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.uv = TRANSFORM_TEX(v.uv, _MainTex);
					return o;
				}


				fixed4 frag(v2f IN) : SV_Target
				{
					float2 INuv = IN.uv;
					fixed4 result;
					for (int i = 0; i < _ArraysLength; i++)
					{
						float XMultiplier = max(_MainTex_TexelSize.y / _MainTex_TexelSize.x, 1);
						float YMultiplier = clamp(_MainTex_TexelSize.x / _MainTex_TexelSize.y, 1, 1000000);
						float draw = pow(saturate(1 - distance(float2(INuv.x * XMultiplier, INuv.y * YMultiplier), float2(_Coordinate[i].x * XMultiplier, _Coordinate[i].y * YMultiplier))), 500 / _Size[i]);
						fixed4 drawCol = _Color[i] * draw * _Strength[i];
						result += drawCol;
					}
					return result;
				}
				ENDCG
			}
		}
}
