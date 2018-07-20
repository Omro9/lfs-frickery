/*
 * Shader which can blend alpha values of two skyboxes, as well as make LOCAL rotation along three axes. 
 * Alpha blending code taken from StackExchange (I'll find the URL later).
 */
 Shader "Skybox/TriRotateBlend" {
     Properties {
         _Tint ("Tint Color", Color) = (.5, .5, .5, 1)
         _Tint2 ("Tint Color 2", Color) = (.5, .5, .5, 1)
         [Gamma] _Exposure ("Exposure", Range(0, 8)) = 1.0
         _XRotation ("X-axis Rotation", Range(0, 359)) = 0
         _YRotation ("Y-axis Rotation", Range(0, 359)) = 0
         _ZRotation ("Z-axis Rotation", Range(0, 359)) = 0

         _BlendCubemaps ("Blend Cubemaps", Range(0, 1)) = 0.5
         [NoScaleOffset] _Tex ("Cubemap (HDR)", Cube) = "grey" {}
         [NoScaleOffset] _Tex2 ("Cubemap (HDR) 2", Cube) = "grey" {}
     }
     SubShader {
         Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
         Cull Off ZWrite Off
         Blend SrcAlpha OneMinusSrcAlpha
      
         Pass {
          
             CGPROGRAM
             #pragma vertex vert
             #pragma fragment frag
      
             #include "UnityCG.cginc"
      
             samplerCUBE _Tex;
             samplerCUBE _Tex2;
             float _BlendCubemaps;
             half4 _Tex_HDR;
             half4 _Tint;
             half4 _Tint2;
             half _Exposure;
             float _XRotation;
             float _YRotation;
             float _ZRotation;
      
             float4 TriAxisRotateInDegrees (float4 vertex, float xDegrees, float yDegrees, float zDegrees)
             {
                 float4 progVert = vertex;
                 float alpha = xDegrees * UNITY_PI / 180.0;
                 float beta = yDegrees * UNITY_PI / 180.0;
                 float gamma = zDegrees * UNITY_PI / 180.0;

                 // Z-axis rotation
                 float sing, cosg;
                 sincos(gamma, sing, cosg);
                 float3x3 zMat = float3x3(cosg, 0, sing, 0, 1, 0, -sing, 0, cosg);
                 progVert = float4(mul(zMat, progVert.xzy), progVert.w).xzyw;

                 // X-axis rotation
                 float sina, cosa;
                 sincos(alpha, sina, cosa);
                 float3x3 xMat = float3x3(1, 0, 0, 0, cosa, -sina, 0, sina, cosa);
                 progVert = float4(mul(xMat, progVert.xzy), progVert.w).xzyw;

                 // Y-axis (unity) rotation
                 float sinb, cosb;
                 sincos(beta, sinb, cosb);
                 float3x3 yMat = float3x3(cosb, -sinb, 0, sinb, cosb, 0, 0, 0, 1);
                 progVert = float4(mul(yMat, progVert.xzy), progVert.w).xzyw;

                 return progVert;
             }
          
             struct appdata_t {
                 float4 vertex : POSITION;
                 float3 normal : NORMAL;
             };
      
             struct v2f {
                 float4 vertex : SV_POSITION;
                 float3 texcoord : TEXCOORD0;
             };
      
             v2f vert (appdata_t v)
             {
                 v2f o;
                 o.vertex = UnityObjectToClipPos(TriAxisRotateInDegrees(v.vertex, _XRotation, _YRotation, _ZRotation));
                 o.texcoord = v.vertex;
                 return o;
             }
      
             fixed4 frag (v2f i) : SV_Target
             {
                 float4 env1 = texCUBE (_Tex, i.texcoord);
                 float4 env2 = texCUBE (_Tex2, i.texcoord); 
                 float4 env = lerp( env2, env1, _BlendCubemaps );
                 float4 tint = lerp( _Tint, _Tint2, _BlendCubemaps );
                 half3 c = DecodeHDR (env, _Tex_HDR);
                 c = c * tint.rgb * unity_ColorSpaceDouble;
                 c *= _Exposure;
                 return half4(c, tint.a);
             }
             ENDCG
         }
     }
     Fallback Off
 }