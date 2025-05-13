Shader "Custom/NPRShader"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (1,1,1,1)    // Editable Base Color
        _StrokeColor ("Stroke Color", Color) = (0,0,0,1)  // Editable Stroke Color
        _Specular ("Specular", Range(0,1)) = 0.5      // Specular Intensity
        _Smoothness ("Smoothness", Range(0,1)) = 0.5  // Surface Smoothness
        _OutlineThickness ("Outline Thickness", Range(0,1)) = 0.03 // Outline Thickness
        _LightDirection ("Light Direction", Vector) = (0, 1, 0, 0)  // Editable Light Direction
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldNormal : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
            };

            fixed4 _BaseColor;
            fixed4 _StrokeColor;
            float _Specular;
            float _Smoothness;
            float _OutlineThickness;
            float4 _LightDirection;  // Editable Light Direction from Inspector

            // Vertex Shader
            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);  // Transform vertex position to clip space
                o.worldNormal = normalize(mul(v.normal, (float3x3)unity_ObjectToWorld));  // Get world space normal
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;  // Get world space position
                return o;
            }

            // Fragment Shader
            fixed4 frag(v2f i) : SV_Target
            {
                // Use the user-defined light direction (normalize it to avoid any unintended scale)
                float3 lightDir = normalize(_LightDirection.xyz);

                // Base lighting calculations
                float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);  // Camera direction
                float NdotL = max(0.0, dot(i.worldNormal, lightDir));           // Lambertian diffuse term

                // Specular lighting calculation
                float3 reflectDir = reflect(-lightDir, i.worldNormal);
                float specFactor = pow(max(dot(reflectDir, viewDir), 0.0), _Smoothness * 128.0) * _Specular;

                // Combine base color and lighting (Diffuse + Specular)
                fixed4 color = _BaseColor * NdotL + specFactor;

                // Outline effect
                float edgeFactor = dot(i.worldNormal, viewDir);
                if (edgeFactor < _OutlineThickness)
                {
                    color = _StrokeColor;  // Apply stroke color on outline areas
                }

                return color;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
