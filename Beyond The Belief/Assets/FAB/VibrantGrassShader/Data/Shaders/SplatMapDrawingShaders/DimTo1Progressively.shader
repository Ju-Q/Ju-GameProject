﻿Shader "Hidden/DimTo1Progressively"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_Coordinate("Coordinate", Vector) = (0,0,0,0)
		_Color("Draw Color", Color) = (1,0,0,0)
		_Size("Size", Range(1,500)) = 1
		_Strength("Strength", Range(0,10)) = 1
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
				fixed4 _Coordinate, _Color;
				half _Size, _Strength;

				v2f vert(appdata v)
				{
					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.uv = TRANSFORM_TEX(v.uv, _MainTex);
					return o;
				}

				fixed4 frag(v2f i) : SV_Target
				{
					// sample the texture
					fixed4 col = tex2D(_MainTex, i.uv);
					float draw = pow(saturate(1 - distance(i.uv, _Coordinate.xy)), 500 / _Size);
					fixed4 normalizedCol = clamp(col, 0, 1);
					fixed4 result = lerp(col, normalizedCol, draw * _Strength);
					return result;
				}
				ENDCG
			}
		}
}
