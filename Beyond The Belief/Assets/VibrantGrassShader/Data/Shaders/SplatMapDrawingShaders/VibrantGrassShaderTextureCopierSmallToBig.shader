Shader "Hidden/VibrantGrassShaderTextureCopierSmallToBig"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_TexToAdd("TexToAdd", 2D) = "white" {}
		_CoordinateX("CoordinateX", Vector) = (0,0,0,0)
		_CoordinateY("CoordinateY", Vector) = (0,0,0,0)
		_UseColorInstead("UseColorInstead", float) = 0
		_ColorVectorToApply("ColorVectorToApply", Vector) = (1,1,1,1)
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

				sampler2D _MainTex, _TexToAdd;
				float4 _MainTex_ST;
				fixed4 _CoordinateX, _CoordinateY;
				float _UseColorInstead;
				fixed4 _ColorVectorToApply;

				v2f vert(appdata v)
				{
					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.uv = TRANSFORM_TEX(v.uv, _MainTex);
					return o;
				}

				fixed4 frag(v2f i) : SV_Target
				{
					fixed4 result = tex2D(_MainTex, i.uv);
					if (i.uv.x >= _CoordinateX.x && i.uv.x <= _CoordinateX.y 
						&& i.uv.y >= _CoordinateY.x && i.uv.y <= _CoordinateY.y)
					{
						if (_UseColorInstead <= 0.5)
						{
							float XuvScaledUp = 1 - (i.uv.x - _CoordinateX.x) / (_CoordinateX.y - _CoordinateX.x);//Inverse Lerp 
							float YuvScaledUp = 1 - (i.uv.y - _CoordinateY.x) / (_CoordinateY.y - _CoordinateY.x);//Inverse Lerp 
							float2 uvScaledUp = float2(XuvScaledUp, YuvScaledUp);
							result = tex2D(_TexToAdd, uvScaledUp);
						}
						else result = _ColorVectorToApply;
					}
					return result;
				}
				ENDCG
			}
		}
}
