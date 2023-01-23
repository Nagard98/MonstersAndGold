Shader "Custom/TerrainMesh"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        //_MainTexNormal("Normal", 2D) = "bump" {}
        _MainTex2("Albedo (RGB) 2", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _Mask("SplatMask (RGBA)", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;
        //sampler2D _MainTexNormal;
        sampler2D _MainTex2;
        sampler2D _Mask;

        struct Input
        {
            float2 uv_MainTex;
            float2 uv_MainTex2;
            float2 uv_Mask;
          //  float2 uv_MainTexNormal;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            
            fixed4 color1 = tex2D(_MainTex, IN.uv_MainTex);
            fixed4 color2 = tex2D(_MainTex2, IN.uv_MainTex2);
            fixed4 mask = tex2D(_Mask, IN.uv_Mask);
            fixed4 c = color1 * mask.r + color2 * mask.g;

            //fixed3 normal1 = UnpackNormal(tex2D(_MainTexNormal, IN.uv_MainTex));
            //fixed3 normal2 = UnpackNormal(tex2D(_MainTex2, IN.uv_MainTex2));
            //fixed3 n = normal1 * mask.r + normal2 * mask.g;

            o.Albedo = c;
            //o.Normal = n;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
