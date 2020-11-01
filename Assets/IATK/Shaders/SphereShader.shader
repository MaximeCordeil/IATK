
Shader "IATK/SphereShader"
{
	Properties
	{
		_MainTex("Base (RGB)", 2D) = "White" {}
		_Size("Size", Range(0, 1)) = 0.5
		_MinSize("Min Size", Range(0, 1)) = 0.01
		_MaxSize("Max Size", Range(0, 1)) = 1.0
		_BrushSize("BrushSize",Float) = 0.05
		_MinX("_MinX",Range(0, 1)) = 0
		_MaxX("_MaxX",Range(0, 1)) = 1.0
		_MinY("_MinY",Range(0, 1)) = 0
		_MaxY("_MaxY",Range(0, 1)) = 1.0
		_MinZ("_MinZ",Range(0, 1)) = 0
		_MaxZ("_MaxZ",Range(0, 1)) = 1.0
		_data_size("data_size",Float) = 0
		_tl("Top Left", Vector) = (-1,1,0,0)
		_tr("Top Right", Vector) = (1,1,0,0)
		_bl("Bottom Left", Vector) = (-1,-1,0,0)
		_br("Bottom Right", Vector) = (1,-1,0,0)
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
			Name "Onscreen geometry"
			Blend[_MySrcMode][_MyDstMode]
			Cull Off
			LOD 200
			Zwrite On
			ZTest LEqual
			Tags{ "LightMode" = "ForwardBase" "Queue" = "Transparent" "RenderType" = "Transparent" }
			
			CGPROGRAM
				#pragma target 5.0
				#pragma vertex VS_Main
				#pragma fragment FS_Main
				#pragma geometry GS_Main
				#pragma multi_compile_instancing
				#include "UnityCG.cginc" 
				#include "Distort.cginc"
				#include "UnityLightingCommon.cginc"

				// **************************************************************
				// Data structures												*
				// **************************************************************

				struct app_data
				{
					float4 position : POSITION;
					float4 color: COLOR;
					float3 normal: NORMAL;
					float4 uv_MainTex : TEXCOORD0; // index, vertex size, filtered
					
                    UNITY_VERTEX_INPUT_INSTANCE_ID
				};

				struct v2g
				{
					float4	pos : POSITION;
					float3	normal : NORMAL;
					float2  tex0 : TEXCOORD0;
					float4  color : COLOR;
					float	isBrushed : FLOAT;
					
					UNITY_VERTEX_INPUT_INSTANCE_ID 
					UNITY_VERTEX_OUTPUT_STEREO
				};

				struct g2f
				{
					float4	pos : POSITION;
					float2  tex0 : TEXCOORD0;
					float4  color : COLOR;
					float	isBrushed : FLOAT;
					float3	normal : NORMAL;
					
                    UNITY_VERTEX_OUTPUT_STEREO
				};

				struct f_output
				{
					float4 color : COLOR;
				};

				// **************************************************************
				// Variables													*
				// **************************************************************
               
                UNITY_INSTANCING_BUFFER_START(Props)
                    UNITY_DEFINE_INSTANCED_PROP(float, _Size)
                    UNITY_DEFINE_INSTANCED_PROP(float, _MinSize)
                    UNITY_DEFINE_INSTANCED_PROP(float, _MaxSize)

                    UNITY_DEFINE_INSTANCED_PROP(float, _BrushSize)

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

                sampler2D _MainTex;
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
				v2g VS_Main(app_data v)
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
                        normaliseValue(pos.x, MinNormX, MaxNormX, 0,1),
                        normaliseValue(pos.y, MinNormY, MaxNormY, 0,1),
                        normaliseValue(pos.z, MinNormZ, MaxNormZ, 0,1),
                        v.position.w);

                    // Set rest of v2g values
                    o.pos = normalisedPosition;
                    o.normal = float3(idx, size, isFiltered);
                    o.tex0 = float2(0, 0);
                    o.color = v.color;

                    // Filtering min and max ranges
                    float epsilon = -0.00001;
					if (normalisedPosition.x < (MinX + epsilon) ||
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
				[maxvertexcount(6)]
				void GS_Main(point v2g p[1], inout TriangleStream<g2f> triStream)
				{
                    g2f o;

					UNITY_SETUP_INSTANCE_ID(p[0]);
					UNITY_INITIALIZE_OUTPUT(g2f, o);
					UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(p[0]);

					// Access instanced variables
                    float Size = UNITY_ACCESS_INSTANCED_PROP(Props, _Size);
                    float MinSize = UNITY_ACCESS_INSTANCED_PROP(Props, _MinSize);
                    float MaxSize = UNITY_ACCESS_INSTANCED_PROP(Props, _MaxSize);
					
					float4x4 MV = UNITY_MATRIX_MV;
					float4x4 vp = UNITY_MATRIX_VP;
					float3 up = UNITY_MATRIX_IT_MV[1].xyz;
					float3 right = -UNITY_MATRIX_IT_MV[0].xyz;
					
					float dist = 1;
					float sizeFactor = normaliseValue(p[0].normal.y, 0.0, 1.0, MinSize, MaxSize);
					float halfS = 0.025f * (Size + (dist * sizeFactor));

					float4 v[4];
					v[0] = float4(p[0].pos + halfS * right - halfS * up, 1.0f);
					v[1] = float4(p[0].pos + halfS * right + halfS * up, 1.0f);
					v[2] = float4(p[0].pos - halfS * right - halfS * up, 1.0f);
					v[3] = float4(p[0].pos - halfS * right + halfS * up, 1.0f);

					o.isBrushed = p[0].isBrushed;
					o.color = p[0].color;
					o.normal = p[0].normal;

					o.pos = UnityObjectToClipPos(v[0]);
					o.tex0 = float2(1.0f, 0.0f);
					o.normal = p[0].normal;
					UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(p[0], o);
					triStream.Append(o);

					o.pos = UnityObjectToClipPos(v[1]);
					o.tex0 = float2(1.0f, 1.0f);
					o.normal = p[0].normal;
					UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(p[0], o);
					triStream.Append(o);

					o.pos = UnityObjectToClipPos(v[2]);
					o.tex0 = float2(0.0f, 0.0f);
					o.normal = p[0].normal;
					UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(p[0], o);
					triStream.Append(o);

					o.pos = UnityObjectToClipPos(v[3]);
					o.tex0 = float2(0.0f, 1.0f);
					o.normal = p[0].normal;
					UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(p[0], o);
					triStream.Append(o);
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

					half3 n = tex2D(_MainTex, input.tex0);
					n.x = (n.x - 0.5) / 0.5;
					n.y = -(n.y - 0.5) / 0.5;
					n.z = -(n.z - 0.5) / 0.5;

					n = mul(((float3x3)-UNITY_MATRIX_V), n);
					n = mul((float3x3)unity_ObjectToWorld, n);

					half3 worldNormal = n;
					half nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));

					o.color = _LightColor0;
					o.color.a = input.color.a;
					o.color.rgb *= nl;
					o.color.rgb += ShadeSH9(half4(worldNormal, 1));
					o.color.rgb *= input.color;

					half2 d = input.tex0 - float2(0.5, 0.5);
					if (length(d) > 0.5 || input.color.w == 0)
					{
						discard;
					}
					
					return o;
				}

			ENDCG
		}
	}
}