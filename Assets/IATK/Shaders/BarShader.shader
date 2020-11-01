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
			AlphaTest Greater 0
			Blend[_MySrcMode][_MyDstMode]
			ColorMaterial AmbientAndDiffuse
            Cull Off
			Lighting Off
			LOD 400
			ZTest [unity_GUIZTestMode]
			ZWrite On
			Tags
			{
				"LightMode" = "ForwardBase"
				"Queue" = "Transparent"
				"IgnoreProjector" = "True"
				"RenderType" = "Transparent"
			}
					
			CGPROGRAM
				#pragma target 5.0
				#pragma vertex VS_Main
				#pragma fragment FS_Main
				#pragma geometry GS_Main
				#pragma multi_compile_instancing
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
					
                    UNITY_VERTEX_INPUT_INSTANCE_ID
        		};

				struct v2g
				{
					float4 vertex : SV_POSITION;
					float4 color : COLOR;
					float3 normal : NORMAL;
					float  isBrushed : FLOAT;
					
					UNITY_VERTEX_INPUT_INSTANCE_ID 
					UNITY_VERTEX_OUTPUT_STEREO
				};

				struct g2f
				{
					float4 vertex : SV_POSITION;
					float4 color : COLOR;
					float2 tex0	: TEXCOORD0;
					float  isBrushed : FLOAT;
					float size : FLOAT;
					
                    UNITY_VERTEX_OUTPUT_STEREO
				};
				
				struct f_output
				{
					float4 color : COLOR;
					float depth : SV_Depth;
				};

				// **************************************************************
				// Variables													*
				// **************************************************************

				UNITY_INSTANCING_BUFFER_START(Props)
					UNITY_DEFINE_INSTANCED_PROP(float, _Size)
                    UNITY_DEFINE_INSTANCED_PROP(float, _MinSize)
                    UNITY_DEFINE_INSTANCED_PROP(float, _MaxSize)
				
                    UNITY_DEFINE_INSTANCED_PROP(float, _MinX)
                    UNITY_DEFINE_INSTANCED_PROP(float, _MaxX)
                    UNITY_DEFINE_INSTANCED_PROP(float, _MinY)
                    UNITY_DEFINE_INSTANCED_PROP(float, _MaxY)
                    UNITY_DEFINE_INSTANCED_PROP(float, _MinZ)
                    UNITY_DEFINE_INSTANCED_PROP(float, _MaxZ)
				
                    UNITY_DEFINE_INSTANCED_PROP(float, _MinNormX)
                    UNITY_DEFINE_INSTANCED_PROP(float, _MaxNormX)
                    UNITY_DEFINE_INSTANCED_PROP(float, _MinNormY)
                    UNITY_DEFINE_INSTANCED_PROP(float, _MaxNormY)
                    UNITY_DEFINE_INSTANCED_PROP(float, _MinNormZ)
                    UNITY_DEFINE_INSTANCED_PROP(float, _MaxNormZ)
					
                    UNITY_DEFINE_INSTANCED_PROP(float, _ShowBrush)
                    UNITY_DEFINE_INSTANCED_PROP(float4, _BrushColor)
					
                    UNITY_DEFINE_INSTANCED_PROP(float, _Tween)
                    UNITY_DEFINE_INSTANCED_PROP(float, _TweenSize)
				UNITY_INSTANCING_BUFFER_END(Props)
				
				float _DataWidth;
				float _DataHeight;
				float4x4 _VP;
				Texture2D _SpriteTex;
				SamplerState sampler_SpriteTex;
				sampler2D _BrushedTexture;

				//*********************************
				// Helper functions
				//*********************************

				float normaliseValue(float value, float i0, float i1, float j0, float j1)
				{
					float L = (j0 - j1) / (i0 - i1);
					return (j0 - (L * i0) + (L * value));
				}


				// **************************************************************
				// Shader Programs												*
				// **************************************************************
				
				// Vertex Shader ------------------------------------------------
				v2g VS_Main(VS_INPUT v)
				{
					v2g o;

                    UNITY_SETUP_INSTANCE_ID(v);
                    UNITY_INITIALIZE_OUTPUT(v2g, o);
					UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
					UNITY_TRANSFER_INSTANCE_ID(v, o);

					// Access instanced variables
					float Tween = UNITY_ACCESS_INSTANCED_PROP(Props, _Tween);
					float TweenSize = UNITY_ACCESS_INSTANCED_PROP(Props, _TweenSize);
                    float MinNormX = UNITY_ACCESS_INSTANCED_PROP(Props, _MinNormX);
                    float MaxNormX = UNITY_ACCESS_INSTANCED_PROP(Props, _MaxNormX);
                    float MinNormY = UNITY_ACCESS_INSTANCED_PROP(Props, _MinNormY);
                    float MaxNormY = UNITY_ACCESS_INSTANCED_PROP(Props, _MaxNormY);
                    float MinNormZ = UNITY_ACCESS_INSTANCED_PROP(Props, _MinNormZ);
                    float MaxNormZ = UNITY_ACCESS_INSTANCED_PROP(Props, _MaxNormZ);
					float MinX = UNITY_ACCESS_INSTANCED_PROP(Props, _MinX);
                    float MaxX = UNITY_ACCESS_INSTANCED_PROP(Props, _MaxX);
                    float MinY = UNITY_ACCESS_INSTANCED_PROP(Props, _MinY);
                    float MaxY = UNITY_ACCESS_INSTANCED_PROP(Props, _MaxY);
                    float MinZ = UNITY_ACCESS_INSTANCED_PROP(Props, _MinZ);
                    float MaxZ = UNITY_ACCESS_INSTANCED_PROP(Props, _MaxZ);

					float idx = v.uv_MainTex.x;
					float isFiltered = v.uv_MainTex.z;

                    // Check if vertex is brushed by looking up the texture
					float2 indexUV = float2((idx % _DataWidth) / _DataWidth, ((idx / _DataWidth) / _DataHeight));
					float4 brushValue = tex2Dlod(_BrushedTexture, float4(indexUV, 0.0, 0.0));
					o.isBrushed = brushValue.r;
				
                    // Lerp position and size values for animations
					float3 pos = lerp(v.normal, v.position, Tween);
					float size = lerp(v.uv_MainTex.w, v.uv_MainTex.y, TweenSize);

                    // Normalise values for min and max slider scaling
					float4 normalisedPosition = float4(
						normaliseValue(pos.x, MinNormX, MaxNormX, 0, 1),
						normaliseValue(pos.y, MinNormY, MaxNormY, 0, 1),
						normaliseValue(pos.z, MinNormZ, MaxNormZ, 0, 1),
						1.0);

					o.vertex = normalisedPosition;
					o.normal = float3(idx, size, isFiltered);
					o.color =  v.color;

                    // Filtering min and max ranges
					float epsilon = -0.00001; 
					if(normalisedPosition.x < (MinX + epsilon) ||
					   normalisedPosition.x > (MaxX - epsilon) || 
					   normalisedPosition.y < (MinY + epsilon) || 
					   normalisedPosition.y > (MaxY - epsilon) || 
					   normalisedPosition.z < (MinZ + epsilon) || 
					   normalisedPosition.z > (MaxZ - epsilon) ||
					   isFiltered)
					{
						o.color.w = 0;
					}

					return o;
				}
				
				// Geometry Shader -----------------------------------------------------
				[maxvertexcount(48)]
				void GS_Main(point v2g p[1], inout TriangleStream<g2f> triStream)
				{
					g2f o;
					
					UNITY_INITIALIZE_OUTPUT(g2f, o);
					UNITY_SETUP_INSTANCE_ID(p[0]);
					UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(p[0]);
					
					// Access instanced variables
                    float Size = UNITY_ACCESS_INSTANCED_PROP(Props, _Size);
                    float MinSize = UNITY_ACCESS_INSTANCED_PROP(Props, _MinSize);
                    float MaxSize = UNITY_ACCESS_INSTANCED_PROP(Props, _MaxSize);
					
					float sizeFactor = normaliseValue(p[0].normal.y, 0.0, 1.0, MinSize, MaxSize);
					float halfS = 0.025f * (Size + (sizeFactor));
					float isBrushed = p[0].isBrushed;
					
					o.size = halfS;
					o.isBrushed = isBrushed;
					
					// Emit bar
					float size = halfS;
					float3 position = p[0].vertex;
					float4 color = p[0].color;
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

					// FACE 1
					half nl;
					half3 worldNormal;

					worldNormal = UnityObjectToWorldNormal(nN);
					nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
					o.color = float4(_LightColor0.rgb * nl, color.a);
					o.color.rgb += ShadeSH9(half4(worldNormal, 1));
					o.color.rgb *= color.rgb;

					o.vertex = UnityObjectToClipPos(pNWU);
					o.tex0 = float2(0.0f, 1.0f);
					UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(p[0], o);
					triStream.Append(o);

					o.vertex = UnityObjectToClipPos(pNEU);
					o.tex0 = float2(1.0f, 1.0f);
					UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(p[0], o);
					triStream.Append(o);

					o.vertex = UnityObjectToClipPos(pNWD);
					o.tex0 = float2(0.0f, 0.0f);
					UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(p[0], o);
					triStream.Append(o);

					o.vertex = UnityObjectToClipPos(pNED);
					o.tex0 = float2(1.0f, 0.0f);
					UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(p[0], o);
					triStream.Append(o);

					triStream.RestartStrip();

					// FACE 2
					worldNormal = UnityObjectToWorldNormal(nW);
					nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
					o.color = float4(_LightColor0.rgb * nl, color.a);
					o.color.rgb += ShadeSH9(half4(worldNormal, 1));
					o.color.rgb *= color.rgb;
					
					o.vertex = UnityObjectToClipPos(pNED);
					o.tex0 = float2(1.0f, 0.0f);
					UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(p[0], o);
					triStream.Append(o);

					o.vertex = UnityObjectToClipPos(pNEU);
					o.tex0 = float2(1.0f, 1.0f);
					UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(p[0], o);
					triStream.Append(o);

					o.vertex = UnityObjectToClipPos(pSED);
					o.tex0 = float2(0.0f, 0.0f);
					UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(p[0], o);
					triStream.Append(o);

					o.vertex = UnityObjectToClipPos(pSEU);
					o.tex0 = float2(0.0f, 1.0f);
					UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(p[0], o);
					triStream.Append(o);

					triStream.RestartStrip();

					// FACE 3
					worldNormal = UnityObjectToWorldNormal(nU);
					nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
					o.color = float4(_LightColor0.rgb * nl, color.a);
					o.color.rgb += ShadeSH9(half4(worldNormal, 1));
					o.color.rgb *= color.rgb;

					o.vertex = UnityObjectToClipPos(pNWU);
					o.tex0 = float2(1.0f, 0.0f);
					UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(p[0], o);
					triStream.Append(o);

					o.vertex = UnityObjectToClipPos(pNEU);
					o.tex0 = float2(1.0f, 1.0f);
					UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(p[0], o);
					triStream.Append(o);

					o.vertex = UnityObjectToClipPos(pSWU);
					o.tex0 = float2(0.0f, 0.0f);
					UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(p[0], o);
					triStream.Append(o);

					o.vertex = UnityObjectToClipPos(pSEU);
					o.tex0 = float2(0.0f, 1.0f);
					UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(p[0], o);
					triStream.Append(o);

					triStream.RestartStrip();

					// FACE 4
					worldNormal = UnityObjectToWorldNormal(nS);
					nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
					o.color = float4(_LightColor0.rgb * nl, color.a);
					o.color.rgb += ShadeSH9(half4(worldNormal, 1));
					o.color.rgb *= color.rgb;
					
					o.vertex = UnityObjectToClipPos(pSWU);
					o.tex0 = float2(0.0f, 1.0f);
					UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(p[0], o);
					triStream.Append(o);

					o.vertex = UnityObjectToClipPos(pSEU);
					o.tex0 = float2(1.0f, 1.0f);
					UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(p[0], o);
					triStream.Append(o);

					o.vertex = UnityObjectToClipPos(pSWD);
					o.tex0 = float2(0.0f, 0.0f);
					UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(p[0], o);
					triStream.Append(o);

					o.vertex = UnityObjectToClipPos(pSED);
					o.tex0 = float2(1.0f, 0.0f);
					UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(p[0], o);
					triStream.Append(o);

					triStream.RestartStrip();

					// FACE 5
					worldNormal = UnityObjectToWorldNormal(nD);
					nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
					o.color = float4(_LightColor0.rgb * nl, color.a);
					o.color.rgb += ShadeSH9(half4(worldNormal, 1));
					o.color.rgb *= color.rgb;

					o.vertex = UnityObjectToClipPos(pNWD);
					o.tex0 = float2(1.0f, 0.0f);
					UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(p[0], o);
					triStream.Append(o);

					o.vertex = UnityObjectToClipPos(pNED);
					o.tex0 = float2(1.0f, 1.0f);
					UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(p[0], o);
					triStream.Append(o);

					o.vertex = UnityObjectToClipPos(pSWD);
					o.tex0 = float2(0.0f, 0.0f);
					UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(p[0], o);
					triStream.Append(o);

					o.vertex = UnityObjectToClipPos(pSED);
					o.tex0 = float2(0.0f, 1.0f);
					UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(p[0], o);
					triStream.Append(o);

					triStream.RestartStrip();

					// FACE 6
					worldNormal = UnityObjectToWorldNormal(nE);
					nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
					o.color = float4(_LightColor0.rgb * nl, color.a);
					o.color.rgb += ShadeSH9(half4(worldNormal, 1));
					o.color.rgb *= color.rgb;

					o.vertex = UnityObjectToClipPos(pNWD);
					o.tex0 = float2(1.0f, 0.0f);
					UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(p[0], o);
					triStream.Append(o);

					o.vertex = UnityObjectToClipPos(pNWU);
					o.tex0 = float2(1.0f, 1.0f);
					UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(p[0], o);
					triStream.Append(o);

					o.vertex = UnityObjectToClipPos(pSWD);
					o.tex0 = float2(0.0f, 0.0f);
					UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(p[0], o);
					triStream.Append(o);

					o.vertex = UnityObjectToClipPos(pSWU);
					o.tex0 = float2(0.0f, 1.0f);
					UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(p[0], o);
					triStream.Append(o);

					triStream.RestartStrip();
				}
				
				// Fragment Shader -----------------------------------------------
				f_output FS_Main(g2f input)
				{
					f_output o;
					
					UNITY_INITIALIZE_OUTPUT(f_output, o);
					UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
					
					// Access instanced variables
					float4 BrushColor = UNITY_ACCESS_INSTANCED_PROP(Props, _BrushColor);
					float ShowBrush = UNITY_ACCESS_INSTANCED_PROP(Props, _ShowBrush);
					
					if (input.color.w == 0)
					{
						discard;
						o.color = float4(0.0, 0.0, 0.0, 0.0);
						o.depth = 0;
						return o;
					}
					else
					{
						float dx = input.tex0.x;
						float dy = input.tex0.y;
						float xborder = 0.01;
						float yborder = 0.01 * input.size * input.size;

						if (dx > 1.0 - xborder || dx < xborder || dy < yborder || dy > 1.0 - yborder) 
							o.color = float4(0.0, 0.0, 0.0, input.color.w);
						else if (input.isBrushed > 0.0 && ShowBrush > 0.0)
							o.color = BrushColor;
						else
							o.color = input.color;
							
						o.depth = input.vertex.z;
						return o;
					}
				}
				
			ENDCG
		}
	}
	
	FallBack  "Transparent"
}
