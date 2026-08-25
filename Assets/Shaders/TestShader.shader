Shader "Unlit/TestShader"
{
    Properties
    {
        _OutlineColor ("OutlineColor", Color) = (1, 0.5, 1, 1)
        _OutlineThickness ("OutlineThickness", float) = 0
    }
    SubShader
    {
        Pass
        {
            Cull Front
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            uniform float4 _OutlineColor;
            uniform float _OutlineThickness;

            struct vertIn
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct vertOut
            {
                float4 vertex : SV_POSITION;
            };

            vertOut vert (vertIn v)
            {
                vertOut o;
                o.vertex = UnityObjectToClipPos(v.vertex + v.normal * _OutlineThickness);
                return o;
            }

            fixed4 frag (vertOut i) : SV_Target
            {
                return _OutlineColor;
            }
            ENDCG
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct vertIn
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
            };

            struct vertOut
            {
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
            };

            vertOut vert (vertIn v)
            {
                vertOut o;
                o.color = v.color;
                o.vertex = UnityObjectToClipPos(v.vertex);
                
                return o;
            }

            fixed4 frag (vertOut i) : SV_Target
            {
                return i.color;
            }
            ENDCG
        }
    }
}
