Shader "Vertex Color with Approx Normals" {
    SubShader {
        Tags { "RenderType"="Opaque" }
        CGPROGRAM
        #pragma surface surf Lambert vertex:vert noforwardadd

        struct Input {
            float2 uv_MainTex;
            float3 viewDir;
            fixed4 color : COLOR;
        };

        void vert (inout appdata_full v, out Input o) {
            UNITY_INITIALIZE_OUTPUT(Input, o);
        }

        void surf (Input IN, inout SurfaceOutput o) {
            o.Albedo = IN.color.rgb;
            fixed3 worldNormal = UnityObjectToWorldNormal(IN.color.rgb);
            fixed3 worldViewDir = normalize(UnityWorldSpaceViewDir(IN.color.rgb));
            fixed dpdx = ddx(worldViewDir.z);
            fixed dpdy = ddy(worldViewDir.z);
            fixed3 surfGrad = fixed3(-dpdx, -dpdy, 1);
            fixed3 normal = cross(surfGrad, float3(0, 0, 1));
            o.Normal = normalize(worldNormal + normal);
        }
        ENDCG
    }
    FallBack "Diffuse"
}
