Shader "IATK/LineAndDotsShader"
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
			Blend[_MySrcMode][_MyDstMode]
			Cull Off
			LOD 200
			ZWrite On
			Tags { "RenderType"="Transparent" }

			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma geometry geom
				#pragma multi_compile_fog // make fog work
				#pragma multi_compile_instancing
				#include "UnityCG.cginc"
				#include "Distort.cginc"

				struct appdata
				{
					float4 vertex : POSITION;
					float4 color : COLOR;
					float3 normal : NORMAL;
					float4 uv_MainTex : TEXCOORD0; // index, vertex size, filtered
					
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
					bool isLine : BOOL;
					
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
				v2g vert (appdata v)
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
					float3 pos = lerp(v.normal, v.vertex, Tween);
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
					if (normalisedPosition.x < MinX ||
						normalisedPosition.x > MaxX || 
						normalisedPosition.y < MinY || 
						normalisedPosition.y > MaxY || 
						normalisedPosition.z < MinZ || 
						normalisedPosition.z > MaxZ ||
						isFiltered)
					{
						o.color.w = 0;
					}

					return o;
				}

				void emitPoint(v2g _point, g2f o, inout TriangleStream<g2f> triStream)
				{
					// Access instanced variables
					float Size = UNITY_ACCESS_INSTANCED_PROP(Props, _Size);
					float MinSize = UNITY_ACCESS_INSTANCED_PROP(Props, _MinSize);
					float MaxSize = UNITY_ACCESS_INSTANCED_PROP(Props, _MaxSize);
				
					float4x4 MV = UNITY_MATRIX_MV;
					float4x4 vp = UNITY_MATRIX_VP;
					float3 up = UNITY_MATRIX_IT_MV[1].xyz;
					float3 right =  -UNITY_MATRIX_IT_MV[0].xyz;

					float sizeFactor = normaliseValue(_point.normal.y, 0.0, 1.0, MinSize, MaxSize);
					float dist = 1;
					float halfS = 0.01f * (Size + (dist * sizeFactor));
							
					float4 v[4];				

					v[0] = float4(_point.vertex + halfS * right - halfS * up, 1.0f);
					v[1] = float4(_point.vertex + halfS * right + halfS * up, 1.0f);
					v[2] = float4(_point.vertex - halfS * right - halfS * up, 1.0f);
					v[3] = float4(_point.vertex - halfS * right + halfS * up, 1.0f);
		
					o.isBrushed = _point.isBrushed;
					o.color = _point.color;
					o.isLine = false;

					o.vertex = UnityObjectToClipPos(v[0]);					
					o.tex0 = float2(1.0f, 0.0f);
					o.isBrushed = _point.isBrushed;
					UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(_point, o);
					triStream.Append(o);

					o.vertex = UnityObjectToClipPos(v[1]);
					o.tex0 = float2(1.0f, 1.0f);
					o.isBrushed = _point.isBrushed;
					UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(_point, o);
					triStream.Append(o);

					o.vertex = UnityObjectToClipPos(v[2]);
					o.isBrushed = _point.isBrushed;
					o.tex0 = float2(0.0f, 0.0f);
					UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(_point, o);
					triStream.Append(o);

					o.vertex = UnityObjectToClipPos(v[3]);
					o.isBrushed = _point.isBrushed;
					o.tex0 = float2(0.0f, 1.0f);
					UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(_point, o);
					triStream.Append(o);
					
					triStream.RestartStrip();
				}
				
				// Geometry Shader -----------------------------------------------------
				[maxvertexcount(32)]
				void geom(line v2g points[2], inout TriangleStream<g2f> triStream)
				{
					g2f o;
					
					UNITY_INITIALIZE_OUTPUT(g2f, o);
					UNITY_SETUP_INSTANCE_ID(points[0]);
					UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(points[0]);
					
					// Access instanced variables
                    float Size = UNITY_ACCESS_INSTANCED_PROP(Props, _Size);
                    float MinSize = UNITY_ACCESS_INSTANCED_PROP(Props, _MinSize);
                    float MaxSize = UNITY_ACCESS_INSTANCED_PROP(Props, _MaxSize);

					// Handle brushing line topoolgy
					if (points[0].color.w == 0) points[1].color.w = 0;
					if (points[1].color.w == 0) points[0].color.w = 0;

					// Dots geometry
					emitPoint(points[0], o, triStream);
					emitPoint(points[1], o, triStream);

					// Line geometry
					float4 p0 = UnityObjectToClipPos(points[0].vertex);
					float4 p1 = UnityObjectToClipPos(points[1].vertex);
					float w0 = p0.w;
					float w1 = p1.w;

					p0.xyz /= p0.w;
					p1.xyz /= p1.w;

					float3 line01 = p1 - p0;
					float3 dir = normalize(line01);

					// Scale to correct window aspect ratio
					float3 ratio = float3(1024, 768, 0);
					ratio = normalize(ratio);

					float3 unit_z = normalize(float3(0, 0, -1));
					float3 normal = normalize(cross(unit_z, dir) * ratio);
					float width = Size * normaliseValue(points[0].normal.y, 0.0, 1.0, MinSize, MaxSize);

					g2f v[4];

					float3 dir_offset = dir * ratio * width;
					float3 normal_scaled = normal * ratio * width;

					float3 p0_ex = p0 - dir_offset;
					float3 p1_ex = p1 + dir_offset;

					v[0].vertex = float4(p0_ex - normal_scaled, 1) * w0;
					v[0].tex0 = float2(1,0);
					v[0].color = points[0].color;
					v[0].isBrushed = points[0].isBrushed;// || points[1].isBrushed;
					v[0].isLine = true;

					v[1].vertex = float4(p0_ex + normal_scaled, 1) * w0;
					v[1].tex0 = float2(0,0);
					v[1].color = points[0].color;
					v[1].isBrushed = points[0].isBrushed;// || points[1].isBrushed;
					v[1].isLine = true;

					v[2].vertex = float4(p1_ex + normal_scaled, 1) * w1;
					v[2].tex0 = float2(1,1);
					v[2].color = points[1].color;
					v[2].isBrushed = points[0].isBrushed;// || points[1].isBrushed;
					v[2].isLine = true;

					v[3].vertex = float4(p1_ex - normal_scaled, 1) * w1;
					v[3].tex0 = float2(0,1);
					v[3].color = points[1].color;
					v[3].isBrushed = points[0].isBrushed;// || points[1].isBrushed;
					v[3].isLine = true;

					UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(points[0], v[2]);
					triStream.Append(v[2]);
					UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(points[0], v[1]);
					triStream.Append(v[1]);
					UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(points[0], v[0]);
					triStream.Append(v[0]);

					triStream.RestartStrip();

					UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(points[0], v[3]);
					triStream.Append(v[3]);
					UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(points[0], v[2]);
					triStream.Append(v[2]);
					UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(points[0], v[0]);
					triStream.Append(v[0]);

					triStream.RestartStrip();
				}

				// Fragment Shader -----------------------------------------------
				f_output frag (g2f i)
				{
					f_output o;
					
					UNITY_INITIALIZE_OUTPUT(f_output, o);
					UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
					
					// Access instanced variables
					float4 BrushColor = UNITY_ACCESS_INSTANCED_PROP(Props, _BrushColor);
					float ShowBrush = UNITY_ACCESS_INSTANCED_PROP(Props, _ShowBrush);
					
					if (i.color.w == 0)
					{
						discard;
						o.color = float4(0.0, 0.0, 0.0, 0.0);
						o.depth = 0;
						return o;
					}
					
					if (i.isLine)
					{
						fixed4 col = i.color;
						
						if (i.isBrushed && ShowBrush > 0.0)
							o.color = BrushColor;
						else
							o.color = i.color;
					}
					else
					{
						float dx = i.tex0.x - 0.5f;
						float dy = i.tex0.y - 0.5f;
						float dt = dx * dx + dy * dy;
						
						if (dt <= 0.2f)
						{
							if (i.isBrushed && ShowBrush > 0.0)
								o.color = BrushColor;
							else
								o.color = float4(i.color.x-dt*0.75,i.color.y-dt*0.75,i.color.z-dt*0.75,i.color.w);
						}
						else
						{
							discard;
							o.color = float4(0.1, 0.1, 0.1, 1.0);
							o.depth = 0;
							return o;
						}
					}
					
					o.depth = i.vertex.z;
					return o;
				}
				
			ENDCG
		}
	}
}
