Shader "Unlit/TestShader"
{
    Properties
    {
        // _OutlineColor ("OutlineColor", Color) = (1, 0.5, 1, 1)
        // _OutlineThickness ("OutlineThickness", float) = 0
        _BaseColor ("BaseColor", Color) = (1, 1, 1, 1)
    }
    SubShader
    {
        // Pass
        // {
        //     Cull Front
        //     CGPROGRAM
        //     #pragma vertex vert
        //     #pragma fragment frag

        //     #include "UnityCG.cginc"

        //     uniform float4 _OutlineColor;
        //     uniform float _OutlineThickness;

        //     struct vertIn
        //     {
        //         float4 vertex : POSITION;
        //         float3 normal : NORMAL;
        //     };

        //     struct vertOut
        //     {
        //         float4 vertex : SV_POSITION;
        //     };

        //     vertOut vert (vertIn v)
        //     {
        //         vertOut o;
        //         o.vertex = UnityObjectToClipPos(v.vertex + v.normal * _OutlineThickness);
        //         return o;
        //     }

        //     fixed4 frag (vertOut i) : SV_Target
        //     {
        //         return _OutlineColor;
        //     }
        //     ENDCG
        // }
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }
        LOD 200

        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            fixed4 _BaseColor;

            #include "UnityCG.cginc"

            struct vertIn
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct vertOut
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            vertOut vert (vertIn v)
            {
                vertOut o;
                o.color = v.color;
                o.uv = v.uv;
                o.vertex = UnityObjectToClipPos(v.vertex);
                
                return o;
            }

            fixed4 frag (vertOut i) : SV_Target
            {
                float2 offset = i.uv - 0.5;
                float d = length(offset) / 0.7071;
                d = pow(d, 2);
                float4 col = float4(d, d / 30, d, 0.5);
                return col * _BaseColor;
            }
            ENDCG
        }
    }
}
