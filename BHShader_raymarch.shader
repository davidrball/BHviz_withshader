// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Ball/BHShader_raymarch"
{
    Properties
    {
        _MainTex ("Albedo (RGB)", 3D) = "" {}
        _Center ("Center", float)=0
        _Radius ("Radius",float)=1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200
        Pass{
        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma vertex vert
        #pragma fragment frag
        #include "UnityCG.cginc"



        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler3D _MainTex;

        float _Radius;
        float _Center;
        
        #define STEPS 64
        #define STEP_SIZE 0.01

        struct appdata {
            float4 vertex : POSITION;
        };

        struct v2f {
            float4 vertex : SV_POSITION;
            float3 wPos : TEXCOORD1; //world position
        };


        v2f vert(appdata v)
        {
            v2f o;
            o.vertex = UnityObjectToClipPos(v.vertex);
            o.wPos = mul(unity_ObjectToWorld, v.vertex).xyz;
            return o;
        }

        bool sphereHit(float3 p){
            return distance(p,_Center)<_Radius;
        }


        bool raymarchHit (float3 position, float3 direction)
        {
            for (int i=0; i<STEPS;i++)
            {
                if (sphereHit(position))
                return true;
            }
            return false;
        }

        fixed4 frag (v2f i) : SV_Target
        {
            float3 worldPosition = i.wPos;
            float3 viewDirection = normalize(i.wPos - _WorldSpaceCameraPos);
            if(raymarchHit(worldPosition,viewDirection))
            {
                return fixed4(1,0,0,1);
            }
            else
                return fixed4(1,1,1,1) 
        }
        

        /*
        struct vertInput
        {
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

        half4 frag(vertOutput o) : SV_Target
        {
            half4 mainColor = tex3D(_MainTex, o.texcoord);
            return mainColor;
        }
        */


        ENDCG
        }
    }
    //FallBack "Diffuse"
}
