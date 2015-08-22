Shader "Custom/MyShader" {
	Properties {
		_MainTex("Base Texture",2D) = "white"{}		
		_SecondTex ("Second Texture)", 2D) = "white" {}
		
		_Intensity("Lerp intensity",Range(0,1)) = 0.0
		_Color ("General color", Color) = (1,1,1,1)
		
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		fixed4 _InitialColor;
		float _Intensity;
		sampler2D _MainTex,_SecondTex;

		struct Input {
			float2 uv_MainTex;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;

		void surf (Input IN, inout SurfaceOutputStandard o) {
			fixed4 base = tex2D (_MainTex, IN.uv_MainTex);
			fixed4 second = tex2D (_SecondTex, IN.uv_MainTex);
			
			fixed4 c = (base + second * _Intensity)* _Color;
			
			o.Albedo = c.rgb;
			
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
		}
		ENDCG
	} 
	FallBack "Standard"
}
