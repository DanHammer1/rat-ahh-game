Shader "Unlit/ScrollingShader"
{
    Properties
    {
        // _OutlineColor ("OutlineColor", Color) = (1, 0.5, 1, 1)
        // _OutlineThickness ("OutlineThickness", float) = 0
        _BaseColor ("Base Color", Color) = (1, 1, 1, 1)
        _BaseTexture ("Base Texture", 2D) = "white" {}
        _ScrollSpeed ("Scroll Speed", Vector) = (0, 0, 0, 0)
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
            float4 _BaseTexture_ST;
            float2 _ScrollSpeed;

            sampler2D _BaseTexture;

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
                o.uv = TRANSFORM_TEX(v.uv, _BaseTexture);
                o.vertex = UnityObjectToClipPos(v.vertex);
                
                return o;
            }

            fixed4 frag (vertOut i) : SV_Target
            {
                // float2 offset = i.uv - 0.5;
                // float d = length(offset) / 0.7071;
                // d = pow(d, 2);
                // float scroll = sin(_Time.y * 2);
                // float4 col = float4(d + scroll, (d + scroll) / 30, d + scroll, 1);
                // return col * _BaseColor;
                // if (i.uv.x > frac(_Time.y)) return float4(1, 0.5, 1, 1);
                // else return float4(1, 1, 0.5, 1);
                float2 uv = i.uv + _ScrollSpeed * _Time.y;
                float4 textureColor = tex2D(_BaseTexture, uv);

                float t = _Time.y * 0.5;
                float r = sin(t * 6.2831) * 0.5 + 0.5;
                float g = sin(t * 6.2831 + 2.094) * 0.5 + 0.5;
                float b = sin(t * 6.2831 + 4.188) * 0.5 + 0.5;
                float4 rainbow = float4(r, g, b, 0.5);
                return textureColor * _BaseColor;
            }
            ENDCG
        }
    }
}
