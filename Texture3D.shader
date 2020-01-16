Shader "Ball/Texture3D"
{
    Properties
    {
        _MainTex ("Albedo (RGB)", 3D) = "" {}
        _AlphaRange("Alpha Range",  Range(0,1))=0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 200

        Cull Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass{
        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma vertex vert
        #pragma fragment frag
        #include "UnityCG.cginc"

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler3D _MainTex;
        float _AlphaRange;

        struct vertInput {
           float4 pos : POSITION;
           float3 texcoord : TEXCOORD0; 
        };
    
        struct vertOutput {
            float4 pos : SV_POSITION;
            float3 texcoord : TEXCOORD0;
        };

        vertOutput vert(vertInput input)
        {
            vertOutput o;
            o.pos = UnityObjectToClipPos(input.pos);
            o.texcoord = input.texcoord;
            return o;
        }

        //vertext shader output is fragment shader input
        half4 frag(vertOutput o) : SV_Target
        {
            half4 mainColor = tex3D(_MainTex,o.texcoord);
            if(mainColor.g < _AlphaRange)
            {
                mainColor.a = 0.0f;
            }
            return mainColor;
        }

        ENDCG
        }
    }
    FallBack "Diffuse"
}
