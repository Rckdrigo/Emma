Shader "Custom/TransitionShader" {
	Properties {
		_Color ("Initial Color (RGB)", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_ColorTransition ("Color to texture" , Range(0,1)) = 0.5
		
      	_BumpMap ("Bumpmap", 2D) = "bump" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
		_Emission ("Emission", Range(0,1)) = 0
	}
	SubShader {
		Tags { "RenderType"="Transparent" }
		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows
		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0
		
		uniform sampler2D _MainTex;		
     	uniform sampler2D _BumpMap;
     
		uniform half _ColorTransition;
     	
		struct Input {
			float2 uv_MainTex;
        	float2 uv_BumpMap;
		};

		uniform half _Glossiness;
		uniform half _Metallic;
		uniform fixed4 _Color;
		uniform half _Emission;

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			fixed4 c_tex = tex2D (_MainTex, IN.uv_MainTex);
			fixed4 colorMerge = lerp(_Color,c_tex,_ColorTransition);
			
			o.Albedo = colorMerge.rgb;
				
				
			o.Normal = UnpackNormal (tex2D (_BumpMap, IN.uv_BumpMap));
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Emission = half3(_Emission,_Emission,_Emission);
			o.Alpha = c_tex.a;
		}
		ENDCG
	} 
	FallBack "Diffuse"
}
