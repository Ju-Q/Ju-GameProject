Shader "Hidden/VibrantGrassShaderNoMeshRootsHeightsPainter"
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
				int _ArraysLength, _TexSize;
				float _heightFloats[1000];
				float2 _uvCoordinates[1000];

				v2f vert(appdata v)
				{
					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.uv = TRANSFORM_TEX(v.uv, _MainTex);
					return o;
				}

				float4 frag(v2f IN) : SV_Target
				{
					float2 currentUV = IN.uv;
					float4 resultCol = tex2D(_MainTex, currentUV);
					for (int i = 0; i < _ArraysLength; i++)
					{
						float dist = distance(currentUV * (float)_TexSize, _uvCoordinates[i] * (float)_TexSize);
						//float dist2 = distance(currentUV, _uvCoordinates[i]);
						if (dist < 1)
						{resultCol.r = _heightFloats[i];}
					}
					float4 result = float4(resultCol.r, 0,  0, 0);
					return result;
				}
				ENDCG
			}
		}
}
