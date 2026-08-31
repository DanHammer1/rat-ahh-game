Shader "Unlit/GhostShader"
{
    Properties
    {
        // _OutlineColor ("OutlineColor", Color) = (1, 0.5, 1, 1)
        // _OutlineThickness ("OutlineThickness", float) = 0
        _BaseColor ("Base Color", Color) = (1, 1, 1, 1)
        _BaseTexture ("Base Texture", 2D) = "white" {}
        _Transparency ("Transparency", Range(0.0, 1)) = 0.5
        _CutoutThresh ("Cutout Threshold", Range(0.0, 1.0)) = 0.2
        _Distance("Distance", float) = 1
        // _Amplitude("Amplitude", float) = 1
        _Speed("Speed", float) = 1
        _Amount("Amount", float) = 1
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
            float _Transparency;
            float _CutoutThresh;
            float _Distance;
            // float _Amplitude;
            float _Speed;
            float _Amount;

            sampler2D _BaseTexture;

            float random(float2 seed)
            {
                return frac(sin(dot(seed, float2(12.9898, 78.233))) * 43758.5453);
            }

            #include "UnityCG.cginc"

            struct vertIn
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct vertOut
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            vertOut vert (vertIn v)
            {
                vertOut o;
                float r = random(v.vertex.xy + floor(_Time.y * _Speed));
                v.vertex.z += sin(_Time.y * _Speed * r) * _Distance * _Amount;
                o.uv = TRANSFORM_TEX(v.uv, _BaseTexture);
                o.vertex = UnityObjectToClipPos(v.vertex);
                
                return o;
            }

            fixed4 frag (vertOut i) : SV_Target
            {
                float4 textureColor = tex2D(_BaseTexture, i.uv);
                float4 col = textureColor * _BaseColor * float4(0.4, 0.6, 1.0, 1);
                col.a = _Transparency;
                clip(col.b - _CutoutThresh);
                return col;
            }
            ENDCG
        }
    }
}
