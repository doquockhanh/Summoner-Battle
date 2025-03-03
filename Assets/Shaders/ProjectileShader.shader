Shader "Custom/ProjectileShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ProjectileColor ("Projectile Color", Color) = (1,1,1,1)
        _GlowIntensity ("Glow Intensity", Range(0,3)) = 1.0
        _GlowSpeed ("Glow Speed", Range(0,5)) = 1.0
        _GlowSize ("Glow Size", Range(0,1)) = 0.1
    }
    
    SubShader
    {
        Tags 
        { 
            "RenderType"="Transparent" 
            "Queue"="Transparent"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off
        
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

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _ProjectileColor;
            float _GlowIntensity;
            float _GlowSpeed;
            float _GlowSize;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // Lấy màu từ texture
                fixed4 col = tex2D(_MainTex, i.uv);
                
                // Tạo hiệu ứng lấp lánh
                float glow = sin(_Time.y * _GlowSpeed) * 0.5 + 0.5;
                
                // Tính khoảng cách từ tâm
                float2 center = float2(0.5, 0.5);
                float dist = distance(i.uv, center);
                
                // Tạo hiệu ứng phát sáng từ tâm
                float glowMask = 1 - saturate(dist / _GlowSize);
                
                // Kết hợp màu và hiệu ứng
                fixed4 finalColor = _ProjectileColor * col;
                finalColor.rgb += glow * _GlowIntensity * glowMask;
                
                return finalColor;
            }
            ENDCG
        }
    }
} 