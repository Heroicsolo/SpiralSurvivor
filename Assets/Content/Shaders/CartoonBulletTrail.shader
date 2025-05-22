Shader "Custom/CartoonBulletTrail"
{
    Properties
    {
        _Color ("Main Color", Color) = (1, 0.8, 0.2, 1)
        _OutlineColor ("Outline Color", Color) = (0, 0, 0, 1)
        _MainTex ("Texture", 2D) = "white" {}
        _Width ("Width", Float) = 1
        _Softness ("Soft Edge Softness", Range(0.001, 1)) = 0.1
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off
            ZWrite Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _Color;
            float4 _OutlineColor;
            float _Width;
            float _Softness;

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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;

                // Create stylized alpha mask (center = solid, edge = fade)
                float dist = abs(uv.y - 0.5) * 2.0; // 0 at center, 1 at edge
                float alpha = smoothstep(1.0, 1.0 - _Softness, 1.0 - dist);

                float4 col = _Color;
                col.a *= alpha;

                // Optional: fake outline near edge
                if (dist > 1.0 - _Softness)
                {
                    col.rgb = lerp(col.rgb, _OutlineColor.rgb, (dist - (1.0 - _Softness)) / _Softness);
                }

                return col;
            }
            ENDCG
        }
    }
}
