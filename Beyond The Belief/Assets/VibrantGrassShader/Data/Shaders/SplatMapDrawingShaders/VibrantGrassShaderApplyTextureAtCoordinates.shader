Shader "Hidden/VibrantGrassShaderApplyTextureAtCoordinates"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_OutOfBoundColorToApply("OutOfBoundColorToApply", Color) = (0,0,0,0)
		_TexToApplyOffsetX("TexToApplyOffsetX", float) = 0
		_TexToApplyOffsetY("TexToApplyOffsetY", float) = 0
		_TexToApplyScaleX("TexToApplyScaleX", float) = 0
		_TexToApplyScaleY("TexToApplyScaleY", float) = 0
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
				float4 _MainTex_ST;
				fixed4 _OutOfBoundColorToApply;
				float _TexToApplyOffsetX, _TexToApplyOffsetY, _TexToApplyScaleX, _TexToApplyScaleY;

				v2f vert(appdata v)
				{
					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.uv = TRANSFORM_TEX(v.uv, _MainTex);
					return o;
				}

				fixed4 frag(v2f i) : SV_Target
				{
					float XMaxCoordinate = _TexToApplyOffsetX + _TexToApplyScaleX;
					float YMaxCoordinate = _TexToApplyOffsetY + _TexToApplyScaleY;
					fixed4 result = _OutOfBoundColorToApply;
					if (i.uv.x >= _TexToApplyOffsetX && i.uv.x <= XMaxCoordinate
						&& i.uv.y >= _TexToApplyOffsetY && i.uv.y <= YMaxCoordinate)
					{
						float XuvScaledUp = (i.uv.x - _TexToApplyOffsetX) / (XMaxCoordinate - _TexToApplyOffsetX);//Inverse Lerp 
						float YuvScaledUp = (i.uv.y - _TexToApplyOffsetY) / (YMaxCoordinate - _TexToApplyOffsetY);//Inverse Lerp 
						float2 uvScaledUp = float2(XuvScaledUp, YuvScaledUp);
						result = tex2D(_MainTex, uvScaledUp);
					}

					return result;
				}
				ENDCG
			}
		}
}
