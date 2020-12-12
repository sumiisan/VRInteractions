Shader "Custom/s_CV_Orb"
{
    Properties {
        _Color ("Color", Color) = (1,1,1,1)
        [HDR] _EmissionColor ("Emission Color", Color) = (0,0,0)
    }
    SubShader {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }

        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        LOD 200

        CGPROGRAM
        #pragma surface surf Lambert vertex:vert alpha noforwardadd
        #pragma shader_feature _EMISSION
        #pragma target 3.0

        fixed4 _Color;
        float4 _EmissionColor;
        
        struct Input {
            float4 vertexColor;
        };

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void vert (inout appdata_full v, out Input o) {
            UNITY_INITIALIZE_OUTPUT(Input,o);
            o.vertexColor = v.color; // Save the Vertex Color in the Input for the surf() method
        }

        void surf (Input IN, inout SurfaceOutput o) {
            o.Albedo = (_Color + IN.vertexColor) * 0.8;
            o.Alpha = _Color.a;
            o.Emission = _EmissionColor * IN.vertexColor;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
