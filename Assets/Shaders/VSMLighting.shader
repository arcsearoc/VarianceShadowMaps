Shader "Unlit/VSMLighting"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ShadowMap ("Shadow Map", 2D) = "white" {}
        _MinVariance ("Min Variance", Float) = 0.00001
        _ShadowStrength ("Shadow Strength", Range(0,1)) = 0.7
        _LightBleedingReduction ("Light Bleeding Reduction", Range(0,1)) = 0.2
        _DepthBias ("Depth Bias", Float) = 0.01
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            Tags { "LightMode"="ForwardBase" }
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase
            
            #include "UnityCG.cginc"
            #include "Lighting.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
                float3 worldNormal : TEXCOORD1;
                float3 worldPos : TEXCOORD2;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _ShadowMap;
            float4x4 _ShadowMapWorldToLight;
            float _MinVariance;
            float _ShadowStrength;
            float _LightBleedingReduction;
            float _DepthBias;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            float VSMShadow(float2 moments, float depth)
            {
                float mu = moments.x;
                float variance = max(moments.y - mu * mu, _MinVariance);
                
                // 如果当前片段比阴影更近，则不在阴影中
                if (depth <= mu + 0.0001)
                    return 1.0;
                
                // 切比雪夫不等式计算阴影值
                float d = depth - mu;
                float pMax = variance / (variance + d * d);
                
                // 减少光线渗漏（Light Bleeding）
                pMax = saturate((pMax - _LightBleedingReduction) / (1.0 - _LightBleedingReduction));
                
                return pMax;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 基础光照计算
                float3 normal = normalize(i.worldNormal);
                float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
                float ndotl = saturate(dot(normal, lightDir));
                
                // 使用Unity的光照颜色
                float3 lightColor = _LightColor0.rgb;
                float3 ambient = UNITY_LIGHTMODEL_AMBIENT.rgb;
                
                // 阴影计算
                float4 shadowCoord = mul(_ShadowMapWorldToLight, float4(i.worldPos, 1.0));
                float3 shadowUV = shadowCoord.xyz / shadowCoord.w;
                
                float shadow = 1.0;
                if (shadowUV.x >= 0 && shadowUV.x <= 1 && shadowUV.y >= 0 && shadowUV.y <= 1 && shadowUV.z >= 0 && shadowUV.z <= 1)
                {
                    // 使用双线性采样获取更平滑的阴影边缘
                    float2 moments = tex2D(_ShadowMap, shadowUV.xy).xy;
                    
                    // 确保深度值在正确范围内
                    float depth = saturate(shadowUV.z);
                    depth = depth - _DepthBias;
                    
                    shadow = VSMShadow(moments, depth);
                    
                    // 边缘淡出处理，避免阴影贴图边缘的硬切换
                    float2 border = abs(shadowUV.xy * 2.0 - 1.0);
                    float borderMax = max(border.x, border.y);
                    float borderFade = 1.0 - saturate((borderMax - 0.9) * 10.0);
                    shadow = lerp(1.0, shadow, borderFade);
                }
                
                // Apply shadow strength
                shadow = lerp(1.0 - _ShadowStrength, 1.0, shadow);
                
                // Final color
                float3 diffuse = ndotl * lightColor * shadow;
                fixed4 col = tex2D(_MainTex, i.uv);
                col.rgb *= (diffuse + ambient);
                
                return col;
            }
            ENDCG
        }
    }
}