// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// MatCap Shader, (c) 2015 Jean Moreno

Shader "MatCap/Bumped/Plain"
{
	Properties
	{
		_Color ("Main Color", Color) = (0.5,0.5,0.5,1)
		_BumpMap ("Normal Map", 2D) = "bump" {}
		_MatCap ("MatCap (RGB)", 2D) = "white" {}
		[Toggle(MATCAP_ACCURATE)] _MatCapAccurate ("Accurate Calculation", Int) = 0
	}
	
	Subshader
	{
		Tags { "RenderType"="Opaque" }
		
		Pass
		{
			Tags { "LightMode" = "Always" }
			
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma fragmentoption ARB_precision_hint_fastest
				#pragma shader_feature MATCAP_ACCURATE
				#include "UnityCG.cginc"
				
				struct v2f
				{
					float4 pos	: SV_POSITION;
					float2 uv_bump : TEXCOORD0;
					
			#if MATCAP_ACCURATE
					fixed3 tSpace0 : TEXCOORD1;
					fixed3 tSpace1 : TEXCOORD2;
					fixed3 tSpace2 : TEXCOORD3;
			#else
					float3 c0 : TEXCOORD1;
					float3 c1 : TEXCOORD2;
			#endif
				};
				
				uniform float4 _BumpMap_ST;
				
				v2f vert (appdata_tan v)
				{
					v2f o;
					o.pos = UnityObjectToClipPos (v.vertex);
					o.uv_bump = TRANSFORM_TEX(v.texcoord,_BumpMap);
					
			#if MATCAP_ACCURATE
					//Accurate bump calculation: calculate tangent space matrix and pass it to fragment shader
					fixed3 worldNormal = UnityObjectToWorldNormal(v.normal);
					fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
					fixed3 worldBinormal = cross(worldNormal, worldTangent) * v.tangent.w;
					o.tSpace0 = fixed3(worldTangent.x, worldBinormal.x, worldNormal.x);
					o.tSpace1 = fixed3(worldTangent.y, worldBinormal.y, worldNormal.y);
					o.tSpace2 = fixed3(worldTangent.z, worldBinormal.z, worldNormal.z);
			#else
					//Faster but less accurate method (especially on non-uniform scaling)
					v.normal = normalize(v.normal);
					v.tangent = normalize(v.tangent);
					TANGENT_SPACE_ROTATION;
					o.c0 = mul(rotation, normalize(UNITY_MATRIX_IT_MV[0].xyz));
					o.c1 = mul(rotation, normalize(UNITY_MATRIX_IT_MV[1].xyz));
			#endif
					return o;
				}
				
				uniform float4 _Color;
				uniform sampler2D _MatCap;
				uniform sampler2D _BumpMap;
				
				float4 frag (v2f i) : COLOR
				{
					fixed3 normals = UnpackNormal(tex2D(_BumpMap, i.uv_bump));
					
			#if MATCAP_ACCURATE
					//Rotate normals from tangent space to world space
					float3 worldNorm;
					worldNorm.x = dot(i.tSpace0.xyz, normals);
					worldNorm.y = dot(i.tSpace1.xyz, normals);
					worldNorm.z = dot(i.tSpace2.xyz, normals);
					worldNorm = mul((float3x3)UNITY_MATRIX_V, worldNorm);
					float4 mc = tex2D(_MatCap, worldNorm.xy * 0.5 + 0.5);
			#else
					half2 capCoord = half2(dot(i.c0, normals), dot(i.c1, normals));
					float4 mc = tex2D(_MatCap, capCoord*0.5+0.5);
			#endif
					return _Color * mc * 2.0;
				}
			ENDCG
		}
	}
	
	Fallback "VertexLit"
}