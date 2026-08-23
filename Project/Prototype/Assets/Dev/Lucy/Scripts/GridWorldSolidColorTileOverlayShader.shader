Shader "Custom/ChunkOverlayArray"
{
    Properties
    {
        _MainTex ("Grid Texture Array", 2DArray) = "" {}
        _Layer ("Layer Index", Float) = 0
        _GlobalOverlayAlpha ("Global Overlay Opacity", Range(0,1)) = 1
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        LOD 100

        Cull Off
        ZWrite Off
        ZTest LEqual
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.5 // required for texture arrays
            #pragma multi_compile_instancing

            #include "UnityCG.cginc"

            UNITY_DECLARE_TEX2DARRAY(_MainTex);

            // Per-instance properties so each chunk can use the same
            // material/shader but sample a different array layer.
            UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(float, _Layer)
            UNITY_INSTANCING_BUFFER_END(Props)

            float _GlobalOverlayAlpha;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv  : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                float layer = UNITY_ACCESS_INSTANCED_PROP(Props, _Layer);

                fixed4 c = UNITY_SAMPLE_TEX2DARRAY(_MainTex, float3(i.uv, layer));
                c.a *= _GlobalOverlayAlpha;
                return c;
            }
            ENDCG
        }
    }
}