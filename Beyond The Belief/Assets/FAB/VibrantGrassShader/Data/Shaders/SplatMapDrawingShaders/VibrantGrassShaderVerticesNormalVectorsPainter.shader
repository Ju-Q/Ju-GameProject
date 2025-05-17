Shader "Hidden/VibrantGrassShaderVerticesNormalVectorsPainter"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_DistForEachVector("DistForEachVector", float) = 0
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
				float _DistForEachVector;
				int _ArraysLength;
				float4 _normalVectors[2000];
				float2 _uvCoordinates[2000];

				v2f vert(appdata v)
				{
					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.uv = TRANSFORM_TEX(v.uv, _MainTex);
					return o;
				}

				fixed4 frag(v2f IN) : SV_Target
				{
					float2 currentUV = IN.uv;
					fixed4 origCol = tex2D(_MainTex, currentUV);
					float4 resultCol = origCol;
					if (origCol.r == 0.0f && origCol.g == 0.0f && origCol.b == 0.0f)
					{
						for (int i = 0; i < _ArraysLength; i++)
						{
							float2 DistXY = abs(currentUV - _uvCoordinates[i]);
							if (DistXY.x < _DistForEachVector && DistXY.y < _DistForEachVector)
							{
								resultCol = _normalVectors[i];
							}
						}
					}
					fixed4 result = fixed4(resultCol.r, resultCol.g, resultCol.b, resultCol.a);
					return result;
				}
				//fixed4 frag(v2f IN) : SV_Target
				//{
				//	float2 currentUV = IN.uv;
				//	fixed4 origCol = tex2D(_MainTex, currentUV);
				//	float4 resultCol;
				//	if (origCol.r <= 0.0f && origCol.g <= 0.0f && origCol.b <= 0.0f)
				//	{
				//		for (int i = 0; i < _ArraysLength; i++)
				//		{
				//			float2 DistXY = abs(currentUV - _uvCoordinates[i]);
				//			if (DistXY.x < _DistForEachVector && DistXY.y < _DistForEachVector)
				//			{
				//				resultCol = _normalVectors[i];
				//			}
				//		}
				//	}
				//	fixed4 result = fixed4(resultCol.r, resultCol.g, resultCol.b, resultCol.a);
				//	return result;
				//}
				ENDCG
			}
		}
}
