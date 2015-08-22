Shader "MyShaders/HalfLambert" {
	Properties{
		_MainTex("Main Texture",2D) = "white"{}
	}
	
	SubShader {
		Tags{"RenderType" = "Opaque"}
		
			CGPROGRAM
			#pragma surface surf HalfLambert
		
			sampler2D _MainTex;
			    	
       		struct Input {
          		float2 uv_MainTex;
  			};   
  			
	       	inline float4 LightingHalfLambert(SurfaceOutput s, fixed3 lightDir, fixed atten){
				float difLight = max(0,dot(s.Normal , lightDir));
				float hLamber = difLight / 2 + 0.5;
				float4 col;
				
				col.rgb = s.Albedo * _LightColor0.rgb * (hLamber * atten * 2);
				col.a = s.Alpha;
				
				return col;
			}
			
	       	void surf (Input IN, inout SurfaceOutput o){
	       		half4 c = tex2D(_MainTex,IN.uv_MainTex); 
	       		
	       		o.Albedo = c.rgb;
	       		o.Alpha = c.a;
	       	}
			ENDCG
		
	} 
}
