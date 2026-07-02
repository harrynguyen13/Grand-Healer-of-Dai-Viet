Shader "UI/CircleMinimap"
{
    Properties
    {
        [PerRendererData] _MainTex ("Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        _Radius ("Radius", Range(0.1, 0.5)) = 0.49
        _BorderSize ("Border Size", Range(0, 0.2)) = 0.035
        _BorderColor ("Border Color", Color) = (0,0,0,1)
        _Feather ("Edge Feather", Range(0.001, 0.05)) = 0.004
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
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            fixed4 _Color;

            float _Radius;
            float _BorderSize;
            fixed4 _BorderColor;
            float _Feather;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                o.color = v.color * _Color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 centerUv = i.uv - float2(0.5, 0.5);
                float dist = length(centerUv);

                float outerAlpha = 1.0 - smoothstep(_Radius, _Radius + _Feather, dist);

                if (outerAlpha <= 0.001)
                {
                    discard;
                }

                fixed4 mapColor = tex2D(_MainTex, i.uv) * i.color;

                float borderStart = _Radius - _BorderSize;
                float borderMask = smoothstep(borderStart - _Feather, borderStart + _Feather, dist);

                fixed4 finalColor = lerp(mapColor, _BorderColor, borderMask);
                finalColor.a *= outerAlpha;

                return finalColor;
            }
            ENDCG
        }
    }
}