Shader "Unlit/GhostShader"
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
                float2 uv = i.uv + _ScrollSpeed * _Time.y;
                float4 textureColor = tex2D(_BaseTexture, uv);
                return textureColor * _BaseColor * float4(0.1, 0.15, 1.0, 0.65);
            }
            ENDCG
        }
    }
}
