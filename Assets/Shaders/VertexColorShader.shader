Shader "Custom/VertexColorShader" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Lambert vertex:vert

        struct Input {
            float4 color : COLOR;
        };

        sampler2D _MainTex;
        
        void vert(inout appdata_full v) {
        }

        void surf(Input IN, inout SurfaceOutput o) {
            o.Albedo = IN.color.rgb;
            o.Alpha = IN.color.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
