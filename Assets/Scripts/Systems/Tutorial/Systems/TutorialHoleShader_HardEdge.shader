Shader "UI/TutorialHoleShader_HardEdge"
{
    Properties
    {
        _RingColor ("Ring Color", Color) = (1,1,1,1)
        _HoleCenter ("Hole Center", Vector) = (0.5,0.5,0,0)
        _HoleSize ("Radius", Float) = 0.2
        _RingThickness ("Ring Thickness", Float) = 0.02
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

                // 딱 잘리는 링
                float ring = step(inner, dist) * step(dist, outer);

                return half4(_RingColor.rgb, ring * _RingColor.a);
            }

            ENDHLSL
        }
    }
}