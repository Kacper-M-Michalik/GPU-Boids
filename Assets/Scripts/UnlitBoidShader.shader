Shader "Unlit/UnlitBoidShader"
{
    Properties
    {
        _Color ("Colour", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct FragInput
            {
                float4 position : SV_POSITION;
                float4 velocity : TEXCOORD0;
            };

            struct BoidData
            {
                float3 position;
                float3 velocity;
                float2 padding;
            };
            
            StructuredBuffer<BoidData> BoidBuffer;
            float4 _Color;

            FragInput vert (appdata_full VertexData, uint InstanceID : SV_InstanceID)
            {
                float3 InstancePosition = BoidBuffer[InstanceID].position;

                float3 VertexWorldPosition = VertexData.vertex + InstancePosition;

                FragInput o;
                o.position = mul(UNITY_MATRIX_VP, float4(VertexWorldPosition, 1));
                o.velocity = float4(BoidBuffer[InstanceID].velocity.xyz, 0);
                return o;
            }

            fixed4 frag (FragInput i) : SV_Target
            {                
                return float4(normalize(i.velocity).xyz, 1);
            }
            ENDCG
        }
    }
}
