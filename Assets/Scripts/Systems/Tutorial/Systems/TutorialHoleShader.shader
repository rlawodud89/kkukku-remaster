Shader "UI/TutorialHoleShader"
{
    Properties
    {
        _RingColor ("Ring Color", Color) = (1,1,1,1)
        _HoleCenter ("Hole Center", Vector) = (0.5,0.5,0,0)
        _HoleSize ("Radius", Float) = 0.2
        _RingThickness ("Ring Thickness", Float) = 0.02
        _Softness ("Edge Softness", Float) = 0.005
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Transparent"
            "Queue"="Transparent"
            "IgnoreProjector"="True"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            HLSLPROGRAM
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
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            float4 _RingColor;
            float4 _HoleCenter;
            float _HoleSize;
            float _RingThickness;
            float _Softness;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                float dist = distance(i.uv, _HoleCenter.xy);

                float inner = _HoleSize;
                float outer = _HoleSize + _RingThickness;

                // 안쪽 경계 부드럽게
                float innerFade = smoothstep(inner - _Softness, inner, dist);

                // 바깥 경계 부드럽게
                float outerFade = 1 - smoothstep(outer, outer + _Softness, dist);

                float ring = innerFade * outerFade;

                return half4(_RingColor.rgb, ring * _RingColor.a);
            }

            ENDHLSL
        }
    }
}