Shader "Hidden/SimpleDrawerA2"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_Coordinate("Coordinate", Vector) = (0,0,0,0)
		_Color("Draw Color", Color) = (1,0,0,0)
		_Size("Size", Range(1,500)) = 1
		_Strength("Strength", Range(0,10)) = 1
		_OverwriteColor("Overwrite Color", Range(0,1)) = 0
		_SaturateDraw("SaturateDraw", Range(0,1)) = 0
		_SaturateResult("SaturateResult", Range(0,1)) = 0
		_ClampMinTo0("ClampMinTo0", Range(0,1)) = 0
		_MinRedPaintingValue("MinRedPaintingValue", float) = 0
		_MaxRedPaintingValue("MaxRedPaintingValue", float) = 0
		_MinGrassFieldLine("MinGrassFieldLine", float) = 1
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
				fixed4 _Coordinate, _Color;
				half _Size, _Strength, _OverwriteColor, _SaturateDraw, _SaturateResult, _ClampMinTo0;
				float _MinRedPaintingValue, _MaxRedPaintingValue;
				float _MinGrassFieldLine;

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
					float XMultiplier = clamp(_MainTex_TexelSize.y / _MainTex_TexelSize.x, 1, 1000000);//0.5, 0.25, 2
					float YMultiplier = clamp(_MainTex_TexelSize.x / _MainTex_TexelSize.y, 1, 1000000);//2, 0.5, 0.5
					XMultiplier = XMultiplier * _MinGrassFieldLine;
					YMultiplier = YMultiplier * _MinGrassFieldLine;
					float draw = pow(saturate(1 - distance(float2(i.uv.x * XMultiplier, i.uv.y * YMultiplier), float2(_Coordinate.x * XMultiplier, _Coordinate.y * YMultiplier))), 500 / _Size);
					fixed4 result;
					if (_Strength > 0.0 && _MaxRedPaintingValue > 0.0)
					{
						float diffUp = _MaxRedPaintingValue - col.r;
						draw = clamp(draw, 0.0, diffUp);
					}
					if (_Strength < 0.0 && _MinRedPaintingValue > 0.0)
					{
						float diffDown = col.r - _MinRedPaintingValue;
						draw = clamp(draw, 0.0, diffDown);
					}
					if (_OverwriteColor == 0)
					{
						fixed4 drawCol = _Color * (draw * _Strength);
						if (_SaturateDraw >= 0.5) drawCol = clamp(drawCol,clamp(- col,-1,0),clamp(1 - col,0,1));
						if (drawCol.r < 0.001 && drawCol.g < 0.001 && drawCol.b < 0.001
							&& drawCol.r > -0.001 && drawCol.g > -0.001 && drawCol.b > -0.001) drawCol = fixed4(0, 0, 0, 0);
						result = col + drawCol;
					}
					else
					{
						//fixed4 drawCol = _Color * (draw * _Strength);
						fixed4 drawCol = _Color * draw;
						if (_SaturateDraw >= 0.5) drawCol = saturate(drawCol);
						//result = (col * (1 - draw)) + drawCol;
						if (drawCol.r < 0.001 && drawCol.g < 0.001 && drawCol.b < 0.001) drawCol = fixed4(0, 0, 0, 0);
						result = lerp(col, col + drawCol, _Strength);
					}
					if (_SaturateResult >= 0.5) result = saturate(result);
					if (_ClampMinTo0 >= 0.5)
					{
						float Red = result.r;
						float Green = result.g;
						float Blue = result.b;
						float Alpha = result.a;
						if (result.r < 0.0) Red = 0.0;
						if (result.g < 0.0) Green = 0.0;
						if (result.b < 0.0) Blue = 0.0;
						if (result.a < 0.0) Alpha = 0.0;
						result = fixed4(Red, Green, Blue, Alpha);
					}
					//else result = clamp(result, 0, 10);//Capping the maximum because it's useless tho makes painting annoying 
					return result;
				}
				ENDCG
			}
		}
}
