Shader "Hidden/VibrantGrassShaderTextureCopierBigToSmall"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_OriginalTex("OriginalTex", 2D) = "white" {}
		_CoordinateX("CoordinateX", Vector) = (0,0,0,0)
		_CoordinateY("CoordinateY", Vector) = (0,0,0,0)
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

				sampler2D _MainTex, _OriginalTex;
				float4 _MainTex_ST;
				fixed4 _CoordinateX, _CoordinateY;

				v2f vert(appdata v)
				{
					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.uv = TRANSFORM_TEX(v.uv, _MainTex);
					return o;
				}

				fixed4 frag(v2f i) : SV_Target
				{
					float XuvScaledDown = lerp(_CoordinateX.x, _CoordinateX.y, i.uv.x);
					float YuvScaledDown = lerp(_CoordinateY.x, _CoordinateY.y, i.uv.y);
					float2 uvScaledDown = float2(XuvScaledDown, YuvScaledDown);
					fixed4 result = tex2D(_OriginalTex, uvScaledDown);
					return result;
				}
				ENDCG
			}
		}
}
