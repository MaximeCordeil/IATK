// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

// MatCap Shader, (c) 2015 Jean Moreno

Shader "MatCap/Vertex/Textured Lit"
{
	Properties
	{
		_Color ("Color", Color) = (0.5,0.5,0.5,1)
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_MatCap ("MatCap (RGB)", 2D) = "white" {}
	}
	
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		
		#pragma surface surf Lambert vertex:vert 
		
		sampler2D _MainTex;
		sampler2D _MatCap;
		float4 _Color;
		
		struct Input
		{
			half2 uv_MainTex : TEXCOORD0;
			float2 matcapUV;
		};
		
		void vert (inout appdata_full v, out Input o)
		{
			UNITY_INITIALIZE_OUTPUT(Input,o);
			
			float3 worldNorm = normalize(unity_WorldToObject[0].xyz * v.normal.x + unity_WorldToObject[1].xyz * v.normal.y + unity_WorldToObject[2].xyz * v.normal.z);
			worldNorm = mul((float3x3)UNITY_MATRIX_V, worldNorm);
			o.matcapUV = worldNorm.xy * 0.5 + 0.5;
		}
		
		void surf (Input IN, inout SurfaceOutput o)
		{
			half4 c = tex2D(_MainTex, IN.uv_MainTex);
			half4 mc = tex2D(_MatCap, IN.matcapUV);
			o.Albedo = c.rgb * mc.rgb * _Color.rgb * 2.0;
		}
		ENDCG
	}
	
	Fallback "VertexLit"
}
