Shader "Custom/SpriteOutline"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _OutlineColor ("Outline Color", Color) = (1,1,1,1)
        _OutlineWidth ("Outline Width", Range(0, 10)) = 1
    }
    SubShader
    {
        Tags { 
            "RenderType"="Transparent" 
            "Queue"="Transparent"
        }

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off
            ZWrite Off

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
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;
            float4 _OutlineColor;
            float _OutlineWidth;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                
                fixed leftPixel = tex2D(_MainTex, i.uv + float2(-_MainTex_TexelSize.x * _OutlineWidth, 0)).a;
                fixed upPixel = tex2D(_MainTex, i.uv + float2(0, _MainTex_TexelSize.y * _OutlineWidth)).a;
                fixed rightPixel = tex2D(_MainTex, i.uv + float2(_MainTex_TexelSize.x * _OutlineWidth, 0)).a;
                fixed bottomPixel = tex2D(_MainTex, i.uv + float2(0, -_MainTex_TexelSize.y * _OutlineWidth)).a;

                fixed outline = max(max(leftPixel, upPixel), max(rightPixel, bottomPixel)) - col.a;

                return lerp(col, _OutlineColor, outline);
            }
            ENDCG
        }
    }
} 