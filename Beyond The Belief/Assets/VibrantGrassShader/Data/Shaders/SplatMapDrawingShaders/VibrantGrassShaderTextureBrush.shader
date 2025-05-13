Shader "Hidden/VibrantGrassShaderTextureBrush"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_BrushTex("Brush", 2D) = "white" {}
		_Coordinate("Coordinate", Vector) = (0,0,0,0)
		_Size("Size", Range(0.01, 1)) = 1
		_Strength("Strength", Range(0.01,1)) = 1
		_SaturateBrush("SaturateBrush", Range(0,1)) = 0
		_SaturateResult("SaturateResult", Range(0,1)) = 0
		_ClampMinTo0("ClampMinTo0", Range(0,1)) = 0
		_MinRedPaintingValue("MinRedPaintingValue", float) = 0
		_MaxRedPaintingValue("MaxRedPaintingValue", float) = 0
		_MinGrassFieldSize("MinGrassFieldSize", float) = 1
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
				sampler2D _BrushTex;
				float4 _MainTex_ST, _MainTex_TexelSize;
				fixed4 _Coordinate;
				half _Size, _Strength, _SaturateBrush, _SaturateResult, _ClampMinTo0;
				float _MinRedPaintingValue, _MaxRedPaintingValue, _MinGrassFieldSize;

				v2f vert(appdata v)
				{
					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.uv = TRANSFORM_TEX(v.uv, _MainTex);
					return o;
				}

				fixed4 frag(v2f i) : SV_Target
				{
					float XMultiplier = _MainTex_TexelSize.z / _MinGrassFieldSize;
					float YMultiplier = _MainTex_TexelSize.w / _MinGrassFieldSize;//Gotta assign with the right GrassFieldBaseSquareSize x)
					float2 UVMultiplied = float2(i.uv.x * XMultiplier, i.uv.y * YMultiplier);
					float2 CoordsMultiplied = float2(_Coordinate.x * XMultiplier, _Coordinate.y * YMultiplier);
					float2 centerToPixel = i.uv - float2(0.5, 0.5);
					float2 centerToPixelFixed = float2(centerToPixel.x * (1 - (YMultiplier / XMultiplier)), centerToPixel.y);
					float2 direction = UVMultiplied - CoordsMultiplied;
					float2 addedUVForBrush = direction / _Size;
					fixed4 firstCol = tex2D(_MainTex, i.uv);
					fixed4 secondCol = tex2D(_BrushTex, saturate(float2(0.5, 0.5) + addedUVForBrush)) * _Strength;

					if (secondCol.r > 0.0 && _MaxRedPaintingValue > 0.0)
					{
						float diffUp = _MaxRedPaintingValue - firstCol.r;
						secondCol = clamp(secondCol, 0.0, diffUp);
					}
					if (secondCol.r < 0.0 && _MinRedPaintingValue > 0.0)
					{
						float diffDown = _MinRedPaintingValue - firstCol.r;
						secondCol = clamp(secondCol, diffDown, 0.0);
					}

					if (_SaturateBrush >= 0.5) secondCol = clamp(secondCol, clamp(-firstCol, -1, 0), clamp(1 - firstCol, 0, 1));
					fixed4 result = firstCol + secondCol;
					if (_SaturateResult > 0.5) result = saturate(result);
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
					return result;
				}
				ENDCG
			}
		}
}
