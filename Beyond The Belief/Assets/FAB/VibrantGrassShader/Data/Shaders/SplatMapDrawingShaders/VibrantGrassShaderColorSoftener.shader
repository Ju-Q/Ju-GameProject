Shader "Hidden/VibrantGrassShaderColorSoftener"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _TexMask("TexMask", 2D) = "white" {}
		_Coordinate("Coordinate", Vector) = (0,0,0,0)
		_Size("Size", Range(1,500)) = 1
		_Strength("Strength", Range(0,1)) = 1
		_Iterations("Iterations", int) = 1
		_MinGrassFieldLine("MinGrassFieldLine", float) = 1
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

            sampler2D _MainTex, _TexMask;
			int _Iterations;
			float4 _MainTex_TexelSize;
            float4 _MainTex_ST;
			fixed4 _Coordinate;
			half _Size;
			float _Strength, _MinGrassFieldLine;

			v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f IN) : SV_Target
            {
				float2 INuv = IN.uv;
				fixed4 col = tex2D(_MainTex, IN.uv);
				fixed4 result = col;
				for (int i = 0; i < _Iterations; i++)
				{
					//col = tex2D(_MainTex, i.uv);
					//float powerResult = pow(saturate(1 - distance(i.uv, _Coordinate.xy)), 500 / _Size);
					float XMultiplier = clamp(_MainTex_TexelSize.y / _MainTex_TexelSize.x, 1, 1000000);
					float YMultiplier = clamp(_MainTex_TexelSize.x / _MainTex_TexelSize.y, 1, 1000000);
					XMultiplier = XMultiplier * _MinGrassFieldLine;
					YMultiplier = YMultiplier * _MinGrassFieldLine;
					float powerResult = pow(saturate(1 - distance(float2(INuv.x * XMultiplier, INuv.y * YMultiplier), float2(_Coordinate.x * XMultiplier, _Coordinate.y * YMultiplier))), 500 / _Size);
					//Multiplying to get the proper distance when the texture isn't a square 
					float2 rightNeighbourUVs = INuv + float2(_MainTex_TexelSize.x, 0);
					float2 leftNeighbourUVs = INuv - float2(_MainTex_TexelSize.x, 0);
					float2 topNeighbourUVs = INuv + float2(0, _MainTex_TexelSize.y);
					float2 bottomNeighbourUVs = INuv - float2(0, _MainTex_TexelSize.y);
					fixed4 rightNeighbourCol = tex2D(_MainTex, rightNeighbourUVs);
					if (tex2D(_TexMask, rightNeighbourUVs).x < 1) rightNeighbourCol = fixed4(0, 0, 0, 0);
					fixed4 leftNeighbourCol = tex2D(_MainTex, leftNeighbourUVs);
					if (tex2D(_TexMask, leftNeighbourUVs).x < 1) leftNeighbourCol = fixed4(0, 0, 0, 0);
					fixed4 topNeighbourCol = tex2D(_MainTex, topNeighbourUVs);
					if (tex2D(_TexMask, topNeighbourUVs).x < 1) topNeighbourCol = fixed4(0, 0, 0, 0);
					fixed4 bottomNeighbourCol = tex2D(_MainTex, bottomNeighbourUVs);
					if (tex2D(_TexMask, bottomNeighbourUVs).x < 1) bottomNeighbourCol = fixed4(0, 0, 0, 0);
					float averageBright = (result + rightNeighbourCol + leftNeighbourCol + topNeighbourCol + bottomNeighbourCol) / 5;
					fixed4 colResult = fixed4(averageBright, averageBright, averageBright, averageBright);
					fixed4 drawCol = (colResult - result) * powerResult;
					result = result + drawCol;
				}
				fixed4 lerpedResult = lerp(col, result, _Strength);
				return lerpedResult;
            }
            ENDCG
        }
    }
}
