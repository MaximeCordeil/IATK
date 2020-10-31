// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "IATK/BarShader" 
{
	Properties 
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Size ("Size", Range(0, 30)) = 0.5
		_MinSize("_MinSize",Float) = 0
		_MaxSize("_MaxSize",Float) = 0
		_MinX("_MinX",Range(0, 1)) = 0
		_MaxX("_MaxX",Range(0, 1)) = 1.0
		_MinY("_MinY",Range(0, 1)) = 0
		_MaxY("_MaxY",Range(0, 1)) = 1.0
		_MinZ("_MinZ",Range(0, 1)) = 0
		_MaxZ("_MaxZ",Range(0, 1)) = 1.0		
		_MinNormX("_MinNormX",Range(0, 1)) = 0.0
		_MaxNormX("_MaxNormX",Range(0, 1)) = 1.0
		_MinNormY("_MinNormY",Range(0, 1)) = 0.0
		_MaxNormY("_MaxNormY",Range(0, 1)) = 1.0
		_MinNormZ("_MinNormZ",Range(0, 1)) = 0.0
		_MaxNormZ("_MaxNormZ",Range(0, 1)) = 1.0
		_MySrcMode("_SrcMode", Float) = 5
		_MyDstMode("_DstMode", Float) = 10

		_Tween("_Tween", Range(0, 1)) = 1
		_TweenSize("_TweenSize", Range(0, 1)) = 1
	}

	SubShader 
	{
		Pass
		{
			
			LOD 400

			Tags
			{
			"LightMode" = "ForwardBase"

			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
			}
						
			Blend[_MySrcMode][_MyDstMode]
			//Blend One One
			ColorMaterial AmbientAndDiffuse
			Lighting Off
			ZWrite On
			ZTest [unity_GUIZTestMode]
            Cull Off
			AlphaTest Greater 0
			
		
			CGPROGRAM
				#pragma target 5.0
				#pragma vertex VS_Main
				#pragma fragment FS_Main
				#pragma geometry GS_Main
				#include "UnityCG.cginc" 
				#include "UnityLightingCommon.cginc" // for _LightColor0

				// **************************************************************
				// Data structures												*
				// **************************************************************
				
		        struct VS_INPUT {
          		    float4 position : POSITION;
            		float4 color: COLOR;
					float3 normal: NORMAL;
					float4 uv_MainTex : TEXCOORD0; // index, vertex size, filtered. prev size
        		};

				struct v2g
				{
					float4 vertex : SV_POSITION;
					float4 color : COLOR;
					float3 normal : NORMAL;
					float  isBrushed : FLOAT;
				};

				struct g2f
				{
					float4 vertex : SV_POSITION;
					float4 color : COLOR;
					float2 tex0	: TEXCOORD0;
					float  isBrushed : FLOAT;
					float size : FLOAT;
				};


				// **************************************************************
				// Vars															*
				// **************************************************************

				float myXArray[3];
				int LengthArray;

				float _Size;
			float _MinSize;
			float _MaxSize;
			
			sampler2D _BrushedTexture;

			float showBrush;
			float4 brushColor;

			//Sampler2D _MainTexSampler;

			//SamplerState sampler_MainTex;

			float _DataWidth;
			float _DataHeight;

			//*******************
			// RANGE FILTERING
			//*******************

			float _MinX;
			float _MaxX;
			float _MinY;
			float _MaxY;
			float _MinZ;
			float _MaxZ;

			// ********************
			// Normalisation ranges
			// ********************

			float _MinNormX;
			float _MaxNormX;
			float _MinNormY;
			float _MaxNormY;
			float _MinNormZ;
			float _MaxNormZ;
			
			float _Tween;
			float _TweenSize;

			float4x4 _VP;
			Texture2D _SpriteTex;
			SamplerState sampler_SpriteTex;

				//*********************************
				// helper functions
				//*********************************

				float normaliseValue(float value, float i0, float i1, float j0, float j1)
				{
				float L = (j0 - j1) / (i0 - i1);
				return (j0 - (L * i0) + (L * value));
				}

				// Vertex Shader ------------------------------------------------
				v2g VS_Main(VS_INPUT v)
				{
					v2g o;

					float idx = v.uv_MainTex.x;
					float isFiltered = v.uv_MainTex.z;

					//lookup the texture to see if the vertex is brushed...
					float2 indexUV = float2((idx % _DataWidth) / _DataWidth, ((idx / _DataWidth) / _DataHeight));
					float4 brushValue = tex2Dlod(_BrushedTexture, float4(indexUV, 0.0, 0.0));

					o.isBrushed = brushValue.r;
				
					float size = lerp(v.uv_MainTex.w, v.uv_MainTex.y, _TweenSize);
					float3 pos = lerp(v.normal, v.position, _Tween);

					float4 normalisedPosition = float4(
						normaliseValue(pos.x, _MinNormX, _MaxNormX, 0, 1),
						normaliseValue(pos.y, _MinNormY, _MaxNormY, 0, 1),
						normaliseValue(pos.z, _MinNormZ, _MaxNormZ, 0, 1), 1.0);

					float4 vert = (normalisedPosition);

					o.vertex = vert;
					o.normal = float3(idx, size, isFiltered);
					o.color =  v.color;

					//precision filtering
					float epsilon = -0.00001; 

					//filtering
					if(
					 normalisedPosition.x < (_MinX + epsilon) ||
					 normalisedPosition.x > (_MaxX - epsilon) || 
					 normalisedPosition.y < (_MinY + epsilon) || 
					 normalisedPosition.y > (_MaxY - epsilon) || 
					 normalisedPosition.z < (_MinZ + epsilon) || 
					 normalisedPosition.z > (_MaxZ - epsilon) || isFiltered
					 )
					{
						o.color.w = 0;
					}


					return o;
				}


				void emitCube (float3 position, float4 color, float size, float isBrushed,  inout TriangleStream<g2f> triStream)
				{
					float xsize = size / (unity_ObjectToWorld[0].x / unity_ObjectToWorld[2].z);

					float3 NEU = float3(xsize, size,   size);
					float3 NED = float3(xsize, -size,  size);
					float3 NWU = float3(-xsize, size,  size);
					float3 NWD = float3(-xsize, -size, size);
					float3 SEU = float3(xsize, size,  -size);
					float3 SED = float3(xsize, -size, -size);
					float3 SWU = float3(-xsize, size, -size);
					float3 SWD = float3(-xsize, -size,-size);

					float4 pNEU = float4(position + NEU, 1.0f);
					float4 pNED = float4(position + NED, 1.0f);
					float4 pNWU = float4(position + NWU, 1.0f);
					float4 pNWD = float4(position + NWD, 1.0f);

					float4 pSEU = float4(position + SEU, 1.0f);
					float4 pSED = float4(position + SED, 1.0f);
					float4 pSWU = float4(position + SWU, 1.0f);
					float4 pSWD = float4(position + SWD, 1.0f);

					float3 nN = float3(0, 0, 1);
					float3 nS = float3(0, 0, -1);
					float3 nE = float3(-1, 0, 0);
					float3 nW = float3(1, 0, 0);
					float3 nU = float3(0, 1, 0);
					float3 nD = float3(1, -1, 0);
			
					pNED.y = 0;
					pNWD.y = 0;
					pSED.y = 0;
					pSWD.y = 0;
					
					//float3 nN = float3(0, 0, -1);
					//float3 nS = float3(0, 0, 1);
					//float3 nE = float3(1, 0, 0);
					//float3 nW = float3(-1, 0, 0);
					//float3 nU = float3(0, 1, 0);
					//float3 nD = float3(0, -1, 0);
					float4x4 vp = UNITY_MATRIX_MVP;

					g2f pIn;
					pIn.size = size;

					// FACE 1

					pIn.isBrushed = isBrushed;
					pIn.color = color;

					// FACE 1
					half nl;
					half3 worldNormal;

					worldNormal = UnityObjectToWorldNormal(nN);
					nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
					pIn.color = float4(_LightColor0.rgb * nl, color.a);
					pIn.color.rgb += ShadeSH9(half4(worldNormal, 1));
					pIn.color.rgb *= color.rgb;

					pIn.vertex = UnityObjectToClipPos(pNWU);
					pIn.tex0 = float2(0.0f, 1.0f);
					triStream.Append(pIn);

					pIn.vertex = UnityObjectToClipPos(pNEU);
					pIn.tex0 = float2(1.0f, 1.0f);
					triStream.Append(pIn);

					pIn.vertex = UnityObjectToClipPos(pNWD);
					pIn.tex0 = float2(0.0f, 0.0f);
					triStream.Append(pIn);

					pIn.vertex = UnityObjectToClipPos(pNED);
					pIn.tex0 = float2(1.0f, 0.0f);
					triStream.Append(pIn);

					triStream.RestartStrip();

					// FACE 2

					worldNormal = UnityObjectToWorldNormal(nW);
					nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
					pIn.color = float4(_LightColor0.rgb * nl, color.a);
					pIn.color.rgb += ShadeSH9(half4(worldNormal, 1));
					pIn.color.rgb *= color.rgb;


					pIn.vertex = UnityObjectToClipPos(pNED);
					pIn.tex0 = float2(1.0f, 0.0f);
					triStream.Append(pIn);

					pIn.vertex = UnityObjectToClipPos(pNEU);
					pIn.tex0 = float2(1.0f, 1.0f);
					triStream.Append(pIn);

					pIn.vertex = UnityObjectToClipPos(pSED);
					pIn.tex0 = float2(0.0f, 0.0f);
					triStream.Append(pIn);

					pIn.vertex = UnityObjectToClipPos(pSEU);
					pIn.tex0 = float2(0.0f, 1.0f);
					triStream.Append(pIn);

					triStream.RestartStrip();

					// FACE 3

					worldNormal = UnityObjectToWorldNormal(nU);
					nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
					pIn.color = float4(_LightColor0.rgb * nl, color.a);
					pIn.color.rgb += ShadeSH9(half4(worldNormal, 1));
					pIn.color.rgb *= color.rgb;



					pIn.vertex = UnityObjectToClipPos(pNWU);
					pIn.tex0 = float2(1.0f, 0.0f);
					triStream.Append(pIn);

					pIn.vertex = UnityObjectToClipPos(pNEU);
					pIn.tex0 = float2(1.0f, 1.0f);
					triStream.Append(pIn);

					pIn.vertex = UnityObjectToClipPos(pSWU);
					pIn.tex0 = float2(0.0f, 0.0f);
					triStream.Append(pIn);

					pIn.vertex = UnityObjectToClipPos(pSEU);
					pIn.tex0 = float2(0.0f, 1.0f);
					triStream.Append(pIn);

					triStream.RestartStrip();

					// FACE 4
					worldNormal = UnityObjectToWorldNormal(nS);
					nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
					pIn.color = float4(_LightColor0.rgb * nl, color.a);
					pIn.color.rgb += ShadeSH9(half4(worldNormal, 1));
					pIn.color.rgb *= color.rgb;


					pIn.vertex = UnityObjectToClipPos(pSWU);
					pIn.tex0 = float2(0.0f, 1.0f);
					triStream.Append(pIn);

					pIn.vertex = UnityObjectToClipPos(pSEU);
					pIn.tex0 = float2(1.0f, 1.0f);
					triStream.Append(pIn);

					pIn.vertex = UnityObjectToClipPos(pSWD);
					pIn.tex0 = float2(0.0f, 0.0f);
					triStream.Append(pIn);

					pIn.vertex = UnityObjectToClipPos(pSED);
					pIn.tex0 = float2(1.0f, 0.0f);
					triStream.Append(pIn);

					triStream.RestartStrip();

					// FACE 5
					worldNormal = UnityObjectToWorldNormal(nD);
					nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
					pIn.color = float4(_LightColor0.rgb * nl, color.a);
					pIn.color.rgb += ShadeSH9(half4(worldNormal, 1));
					pIn.color.rgb *= color.rgb;


					pIn.vertex = UnityObjectToClipPos(pNWD);
					pIn.tex0 = float2(1.0f, 0.0f);
					triStream.Append(pIn);

					pIn.vertex = UnityObjectToClipPos(pNED);
					pIn.tex0 = float2(1.0f, 1.0f);
					triStream.Append(pIn);

					pIn.vertex = UnityObjectToClipPos(pSWD);
					pIn.tex0 = float2(0.0f, 0.0f);
					triStream.Append(pIn);

					pIn.vertex = UnityObjectToClipPos(pSED);
					pIn.tex0 = float2(0.0f, 1.0f);
					triStream.Append(pIn);

					triStream.RestartStrip();

					// FACE 6
					worldNormal = UnityObjectToWorldNormal(nE);
					nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
					pIn.color = float4(_LightColor0.rgb * nl, color.a);
					pIn.color.rgb += ShadeSH9(half4(worldNormal, 1));
					pIn.color.rgb *= color.rgb;

					pIn.vertex = UnityObjectToClipPos(pNWD);
					pIn.tex0 = float2(1.0f, 0.0f);
					triStream.Append(pIn);

					pIn.vertex = UnityObjectToClipPos(pNWU);
					pIn.tex0 = float2(1.0f, 1.0f);
					triStream.Append(pIn);

					pIn.vertex = UnityObjectToClipPos(pSWD);
					pIn.tex0 = float2(0.0f, 0.0f);
					triStream.Append(pIn);

					pIn.vertex = UnityObjectToClipPos(pSWU);
					pIn.tex0 = float2(0.0f, 1.0f);
					triStream.Append(pIn);

					triStream.RestartStrip();
				}


				// Geometry Shader -----------------------------------------------------
				[maxvertexcount(48)]
				void GS_Main(point v2g p[1], inout TriangleStream<g2f> triStream)
				{
					float ensize = 1.0;// p[0].col.x;

					float sizeFactor = normaliseValue(p[0].normal.y, 0.0, 1.0, _MinSize, _MaxSize);

					float halfS = 0.025f * (_Size + (sizeFactor));
					float isBrushed = p[0].isBrushed;

					emitCube(p[0].vertex, p[0].color, halfS, isBrushed, triStream);
				}

				// Fragment Shader -----------------------------------------------
				float4 FS_Main(g2f input) : COLOR
				{
					if(input.color.w == 0)
					{
						discard;
						return float4(0.0, 0.0, 0.0, 0.0);
					}

					float dx = input.tex0.x;
					float dy = input.tex0.y;

					float xboarder = 0.01;
					float yboarder = 0.01 * input.size * input.size;

					if(dx > 1.0 - xboarder || dx < xboarder || dy < yboarder || dy > 1.0 - yboarder) 
						return float4(0.0, 0.0, 0.0, input.color.w);
			
					if (input.isBrushed > 0.0 && showBrush > 0.0)
						return brushColor;
					else
						return input.color;	
				//}
			
			}
			ENDCG
		}
	}
	
		FallBack  "Transparent"

	 
}
