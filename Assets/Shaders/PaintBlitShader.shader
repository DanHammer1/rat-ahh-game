Shader "Custom/PaintBlitShader"
{
    Properties
    {
        _MainTex ("Existing Paint", 2D) = "white" {}
        _PixelUV ("Paint UV", Vector) = (0,0,0,0)
        _Color ("Paint Color", Color) = (1,1,1,1)
        _BrushSize ("Brush Size", Float) = 0.01
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            sampler2D _MainTex;

            float2 _PixelUV;
            float4 _Color;
            float _BrushSize;

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

            v2f vert(appdata v)
            {
                v2f o;

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;

                return o;
            }


            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 existing = tex2D(_MainTex, i.uv);

                float distance = length(i.uv - _PixelUV);

                if (distance < _BrushSize)
                {
                    return _Color;
                }

                return existing;
            }

            ENDCG
        }
    }
}