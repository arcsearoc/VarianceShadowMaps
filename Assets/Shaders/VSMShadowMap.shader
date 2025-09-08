Shader "Unlit/VSMShadowMap"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float depth : TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                
                // 正确计算线性深度值，范围从0到1
                float4 clipPos = o.pos;
                o.depth = clipPos.z / clipPos.w;
                
                // 将深度从[-1,1]范围转换到[0,1]范围（适用于所有平台）
                #if UNITY_REVERSED_Z
                    o.depth = 1.0 - o.depth;
                #else
                    o.depth = o.depth * 0.5 + 0.5;
                #endif
                
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                // 使用从顶点着色器传递的深度值
                float depth = i.depth;
                
                // 确保深度值在有效范围内
                depth = saturate(depth);
                
                // VSM需要存储深度和深度的平方
                float2 moments = float2(depth, depth * depth);
                
                // 添加小的常数到方差中以减少阴影痤疮(shadow acne)
                moments.y += 0.00002;
                
                return float4(moments.x, moments.y, 0, 1);
            }
            ENDCG
        }
    }
}