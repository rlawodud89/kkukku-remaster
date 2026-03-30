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
                
                // 아웃라인이 꺼져있을 때
                if (_OutlineAlpha <= 0) 
                {
                    c.rgb *= c.a; // 유니티 스프라이트 기본 투명도 계산
                    return c;
                }

                // 텍스처 주변 4방향을 검사 (위, 아래, 왼쪽, 오른쪽)
                float2 texel = _MainTex_TexelSize.xy * _OutlineSize;
                
                float aUp = tex2D(_MainTex, IN.texcoord + fixed2(0, texel.y)).a;
                float aDown = tex2D(_MainTex, IN.texcoord - fixed2(0, texel.y)).a;
                float aRight = tex2D(_MainTex, IN.texcoord + fixed2(texel.x, 0)).a;
                float aLeft = tex2D(_MainTex, IN.texcoord - fixed2(texel.x, 0)).a;

                // 주변 픽셀 중 가장 진한 알파(투명도) 값을 가져옴
                float outlineAlpha = max(max(aUp, aDown), max(aRight, aLeft));
                
                // 아웃라인 색상 준비
                fixed4 outline = _OutlineColor;
                outline.a *= outlineAlpha * _OutlineAlpha;
                outline.rgb *= outline.a; // Premultiply 방식 적용
                
                // ★ 마법의 코드: 원본 이미지 뒤에 아웃라인을 자연스럽게 합성!
                // 원본이 불투명하면(c.a=1) 아웃라인이 숨겨지고, 투명하면(c.a=0) 아웃라인이 보입니다.
                c.rgb = c.rgb * c.a + outline.rgb * (1.0 - c.a);
                c.a = c.a + outline.a * (1.0 - c.a);

                return c;
            }
        ENDCG
        }
    }
}