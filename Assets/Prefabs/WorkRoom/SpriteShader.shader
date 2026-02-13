Shader "Custom/SpriteOutline"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
        
        // ★ 아웃라인 설정
        _OutlineColor ("Outline Color", Color) = (1,1,1,1)
        _OutlineSize ("Outline Size", Range(0, 10)) = 1
        _OutlineAlpha ("Show Outline", Range(0, 1)) = 0 // 0이면 끔, 1이면 켬
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ PIXELSNAP_ON
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            fixed4 _Color;
            fixed4 _OutlineColor;
            float _OutlineSize;
            float _OutlineAlpha;
            sampler2D _MainTex;
            float4 _MainTex_TexelSize;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;
                #ifdef PIXELSNAP_ON
                OUT.vertex = UnityPixelSnap (OUT.vertex);
                #endif
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 c = tex2D(_MainTex, IN.texcoord) * IN.color;
                
                // 아웃라인이 꺼져있으면(_OutlineAlpha가 0이면) 그냥 원래 색 리턴
                if (_OutlineAlpha == 0) return c;

                // 텍스처 주변 4방향을 검사 (위, 아래, 왼쪽, 오른쪽)
                float2 texel = _MainTex_TexelSize.xy * _OutlineSize;
                
                fixed4 pixelUp = tex2D(_MainTex, IN.texcoord + fixed2(0, texel.y));
                fixed4 pixelDown = tex2D(_MainTex, IN.texcoord - fixed2(0, texel.y));
                fixed4 pixelRight = tex2D(_MainTex, IN.texcoord + fixed2(texel.x, 0));
                fixed4 pixelLeft = tex2D(_MainTex, IN.texcoord - fixed2(texel.x, 0));

                // 주변에 불투명한 픽셀이 하나라도 있으면 아웃라인 그림
                float alphaSum = pixelUp.a + pixelDown.a + pixelRight.a + pixelLeft.a;
                
                // 내 픽셀이 투명하고(0), 주변은 불투명할 때 -> 아웃라인 색상 적용
                if (c.a == 0 && alphaSum > 0)
                {
                    c = _OutlineColor * _OutlineAlpha; // 아웃라인 색상
                }

                c.rgb *= c.a;
                return c;
            }
        ENDCG
        }
    }
}