Shader "Unlit/GhostShader"
{
    Properties
    {
        // _OutlineColor ("OutlineColor", Color) = (1, 0.5, 1, 1)
        // _OutlineThickness ("OutlineThickness", float) = 0
        _BaseColor ("Base Color", Color) = (1, 1, 1, 1)
        _BaseTexture ("Base Texture", 2D) = "white" {}
        _Transparency ("Transparency", Range(0.0, 1)) = 0.5
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }
        LOD 200

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            fixed4 _BaseColor;
            float4 _BaseTexture_ST;
            float2 _ScrollSpeed;
            float _Transparency;

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
                float4 col = textureColor * _BaseColor * float4(0.1, 0.15, 1.0, 1);
                col.a = _Transparency;
                return col;
            }
            ENDCG
        }
    }
}
