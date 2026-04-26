Shader "Custom/FoggyLitSprite"
{
    Properties
    {
        _MainTex ("Fog Texture", 2D) = "white" {}
        _FogColor ("Fog Color", Color) = (1, 1, 1, 1)
        _ScrollSpeed ("Scroll Speed", Vector) = (0.05, 0, 0, 0)
        _Alpha ("Alpha", Range(0, 1)) = 0.5
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        Lighting Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _FogColor;
            float4 _ScrollSpeed;
            float _Alpha;
            float4 _MainTex_ST;

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

            float _TimeY;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                float2 offset = _ScrollSpeed.xy * _Time.y;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex) + offset;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 tex = tex2D(_MainTex, i.uv);
                return _FogColor * tex.a * _Alpha;
            }
            ENDCG
        }
    }
}
