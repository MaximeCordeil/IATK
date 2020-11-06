Shader "IATK/Linked-Views-Material"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_ftl1("Front Top Left Axis 1", Vector) = (-1,1,0,0)
		_ftr1("Front Top Right Axis 1", Vector) = (1,1,0,0)
		_fbl1("Front Bottom Left Axis 1", Vector) = (-1,-1,0,0)
		_fbr1("Front Bottom Right Axis 1", Vector) = (1,-1,0,0)
		_btl1("Back Top Left Axis 1", Vector) = (-1,1,-1,0)
		_btr1("Back Top Right Axis 1", Vector) = (1,1,-1,0)
		_bbl1("Back Bottom Left Axis 1", Vector) = (-1,-1,-1,0)
		_bbr1("Back Bottom Right Axis 1", Vector) = (1,-1,-1,0)

		_ftl2("Front Top Left Axis 2", Vector) = (-1,1,0,0)
		_ftr2("Front Top Right Axis 2", Vector) = (1,1,0,0)
		_fbl2("Front Bottom Left Axis 2", Vector) = (-1,-1,0,0)
		_fbr2("Front Bottom Right Axis 2", Vector) = (1,-1,0,0)
		_btl2("Back Top Left Axis 2", Vector) = (-1,1,-1,0)
		_btr2("Back Top Right Axis 2", Vector) = (1,1,-1,0)
		_bbl2("Back Bottom Left Axis 2", Vector) = (-1,-1,-1,0)
		_bbr2("Back Bottom Right Axis 2", Vector) = (1,-1,-1,0)
		_Alpha("_Alpha", Float) = 1.0
	}
	
	SubShader
	{
		Pass
		{
			Blend SrcAlpha OneMinusSrcAlpha
			LOD 200
			Zwrite On
			Tags{ "RenderType" = "Transparent" }

			CGPROGRAM
				#pragma vertex vert
				#pragma geometry geom
				#pragma fragment frag
				#pragma multi_compile_fog // make fog work
				#pragma multi_compile_instancing
				#include "UnityCG.cginc"
				#include "DistortLinked.cginc"
				#include "Helper.cginc"

				struct appdata
				{
					float4 position : POSITION;
					float2 uv : TEXCOORD0;
					float4 color: COLOR;
					float3 normal	: NORMAL;
					
					UNITY_VERTEX_INPUT_INSTANCE_ID
				};

				struct v2g
				{
					float4 vertex	:	POSITION;
					float4 color	:	COLOR;
					float2 uv		:	TEXCOORD0;
					bool filtered : BOOL;
					float isBrushed : FLOAT;
					
					UNITY_VERTEX_INPUT_INSTANCE_ID 
					UNITY_VERTEX_OUTPUT_STEREO
				};

				struct g2f
				{
					float2 uv : TEXCOORD0;
					float4 vertex : SV_POSITION;
					float4 color: COLOR;
					float isBrushed : FLOAT;
					
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
                    UNITY_DEFINE_INSTANCED_PROP(float, _MinXFilter1)
                    UNITY_DEFINE_INSTANCED_PROP(float, _MaxXFilter1)
                    UNITY_DEFINE_INSTANCED_PROP(float, _MinYFilter1)
                    UNITY_DEFINE_INSTANCED_PROP(float, _MaxYFilter1)
                    UNITY_DEFINE_INSTANCED_PROP(float, _MinZFilter1)
                    UNITY_DEFINE_INSTANCED_PROP(float, _MaxZFilter1)
                    UNITY_DEFINE_INSTANCED_PROP(float, _MinXFilter2)
                    UNITY_DEFINE_INSTANCED_PROP(float, _MaxXFilter2)
                    UNITY_DEFINE_INSTANCED_PROP(float, _MinYFilter2)
                    UNITY_DEFINE_INSTANCED_PROP(float, _MaxYFilter2)
                    UNITY_DEFINE_INSTANCED_PROP(float, _MinZFilter2)
                    UNITY_DEFINE_INSTANCED_PROP(float, _MaxZFilter2)
					
                    UNITY_DEFINE_INSTANCED_PROP(float, _MinNormX1)
                    UNITY_DEFINE_INSTANCED_PROP(float, _MaxNormX1)
                    UNITY_DEFINE_INSTANCED_PROP(float, _MinNormY1)
                    UNITY_DEFINE_INSTANCED_PROP(float, _MaxNormY1)
                    UNITY_DEFINE_INSTANCED_PROP(float, _MinNormZ1)
                    UNITY_DEFINE_INSTANCED_PROP(float, _MaxNormZ1)
                    UNITY_DEFINE_INSTANCED_PROP(float, _MinNormX2)
                    UNITY_DEFINE_INSTANCED_PROP(float, _MaxNormX2)
                    UNITY_DEFINE_INSTANCED_PROP(float, _MinNormY2)
                    UNITY_DEFINE_INSTANCED_PROP(float, _MaxNormY2)
                    UNITY_DEFINE_INSTANCED_PROP(float, _MinNormZ2)
                    UNITY_DEFINE_INSTANCED_PROP(float, _MaxNormZ2)
					
                    UNITY_DEFINE_INSTANCED_PROP(float, _ShowBrush)
                    UNITY_DEFINE_INSTANCED_PROP(float4, _BrushColor)
					
                    UNITY_DEFINE_INSTANCED_PROP(float, _Alpha)
					
                    UNITY_DEFINE_INSTANCED_PROP(float4x4, _ScaleMatrix1)
                    UNITY_DEFINE_INSTANCED_PROP(float4x4, _ScaleMatrix2)
				UNITY_INSTANCING_BUFFER_END(Props)
				
				float _DataWidth;
				float _DataHeight;
				sampler2D _BrushedTexture;
				sampler2D _MainTex;
				float4 _MainTex_ST;
				
				// **************************************************************
				// Shader Programs												*
				// **************************************************************
				
				// Vertex Shader ------------------------------------------------
				v2g vert(appdata v)
				{
					v2g o;

                    UNITY_SETUP_INSTANCE_ID(v);
                    UNITY_INITIALIZE_OUTPUT(v2g, o);
					UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
					UNITY_TRANSFER_INSTANCE_ID(v, o);
					
					// Access instanced variables
					float MinXFilter1 = UNITY_ACCESS_INSTANCED_PROP(Props, _MinXFilter1);
                    float MaxXFilter1 = UNITY_ACCESS_INSTANCED_PROP(Props, _MaxXFilter1);
                    float MinYFilter1 = UNITY_ACCESS_INSTANCED_PROP(Props, _MinYFilter1);
                    float MaxYFilter1 = UNITY_ACCESS_INSTANCED_PROP(Props, _MaxYFilter1);
                    float MinZFilter1 = UNITY_ACCESS_INSTANCED_PROP(Props, _MinZFilter1);
                    float MaxZFilter1 = UNITY_ACCESS_INSTANCED_PROP(Props, _MaxZFilter1);
					float MinXFilter2 = UNITY_ACCESS_INSTANCED_PROP(Props, _MinXFilter2);
                    float MaxXFilter2 = UNITY_ACCESS_INSTANCED_PROP(Props, _MaxXFilter2);
                    float MinYFilter2 = UNITY_ACCESS_INSTANCED_PROP(Props, _MinYFilter2);
                    float MaxYFilter2 = UNITY_ACCESS_INSTANCED_PROP(Props, _MaxYFilter2);
                    float MinZFilter2 = UNITY_ACCESS_INSTANCED_PROP(Props, _MinZFilter2);
                    float MaxZFilter2 = UNITY_ACCESS_INSTANCED_PROP(Props, _MaxZFilter2);
                    float MinNormX1 = UNITY_ACCESS_INSTANCED_PROP(Props, _MinNormX1);
                    float MaxNormX1 = UNITY_ACCESS_INSTANCED_PROP(Props, _MaxNormX1);
                    float MinNormY1 = UNITY_ACCESS_INSTANCED_PROP(Props, _MinNormY1);
                    float MaxNormY1 = UNITY_ACCESS_INSTANCED_PROP(Props, _MaxNormY1);
                    float MinNormZ1 = UNITY_ACCESS_INSTANCED_PROP(Props, _MinNormZ1);
                    float MaxNormZ1 = UNITY_ACCESS_INSTANCED_PROP(Props, _MaxNormZ1);
                    float MinNormX2 = UNITY_ACCESS_INSTANCED_PROP(Props, _MinNormX2);
                    float MaxNormX2 = UNITY_ACCESS_INSTANCED_PROP(Props, _MaxNormX2);
                    float MinNormY2 = UNITY_ACCESS_INSTANCED_PROP(Props, _MinNormY2);
                    float MaxNormY2 = UNITY_ACCESS_INSTANCED_PROP(Props, _MaxNormY2);
                    float MinNormZ2 = UNITY_ACCESS_INSTANCED_PROP(Props, _MinNormZ2);
                    float MaxNormZ2 = UNITY_ACCESS_INSTANCED_PROP(Props, _MaxNormZ2);
                    float4x4 ScaleMatrix1 = UNITY_ACCESS_INSTANCED_PROP(Props, _ScaleMatrix1);
                    float4x4 ScaleMatrix2 = UNITY_ACCESS_INSTANCED_PROP(Props, _ScaleMatrix2);
										
                    // Check if vertex is brushed by looking up the texture
					float2 indexUV = float2((v.normal.x % _DataWidth) / _DataWidth, ((v.normal.x / _DataWidth) / _DataHeight));
					float4 brushValue = tex2Dlod(_BrushedTexture, float4(indexUV, 0.0, 0.0));
					o.isBrushed = brushValue.r;

					o.uv = TRANSFORM_TEX(v.uv, _MainTex);
					o.color = v.color;

					float4 pos;
					if (v.normal.z == 0.0)
					{
								
						pos = float4(normaliseValue(v.position.x, MinNormX1, MaxNormX1, 0, 1),
									 normaliseValue(v.position.y, MinNormY1, MaxNormY1, 0, 1),
									 normaliseValue(v.position.z, MinNormZ1, MaxNormZ1, 0, 1),
									 v.position.w);

						// Check if vertex is filtered
						if (v.position.x < MinXFilter1 ||
							v.position.x > MaxXFilter1 ||
							v.position.y < MinYFilter1 ||
							v.position.y > MaxYFilter1 ||
							v.position.z < MinZFilter1 ||
							v.position.z > MaxZFilter1 ||
							pos.x < 0 ||
							pos.x > 1 ||
							pos.y < 0 ||
							pos.y > 1 ||
							pos.z < 0 ||
							pos.z > 1
							)
							o.filtered = true;	
						else
							o.filtered = false;
						
						// Scale vertex based on input matrix
						pos = mul(ScaleMatrix1, pos);
							
					}
					else if (v.normal.z == 1.0)
					{
						pos = float4(normaliseValue(v.position.x, MinNormX2,  MaxNormX2, 0, 1),
									 normaliseValue(v.position.y, MinNormY2,  MaxNormY2, 0, 1),
									 normaliseValue(v.position.z, MinNormZ2,  MaxNormZ2, 0, 1),
									 v.position.w);

						// Check if vertex is filtered
						if (v.position.x < MinXFilter2 ||
							v.position.x > MaxXFilter2 ||
							v.position.y < MinYFilter2 ||
							v.position.y > MaxYFilter2 ||
							v.position.z < MinZFilter2 ||
							v.position.z > MaxZFilter2 ||
							pos.x < 0 ||
							pos.x > 1 ||
							pos.y < 0 ||
							pos.y > 1 ||
							pos.z < 0 ||
							pos.z > 1)
							o.filtered = true;
						else
							o.filtered = false;
							
						// Scale vertex based on input matrix
						pos = mul(ScaleMatrix2, pos);
					}

					o.vertex = mul(UNITY_MATRIX_VP, ObjectToWorldDistort3d(pos, v.normal.z > 0));
					
					return o;
				}

				// Geometry Shader -----------------------------------------------------
				[maxvertexcount(2)]
				void geom(line v2g l[2], inout LineStream<g2f> lineStream)
				{					
					bool filtered = (l[0].filtered || l[1].filtered || l[0].color.w == 0 || l[1].color.w == 0);
					if (!filtered)
					{
						g2f o;
						
						UNITY_INITIALIZE_OUTPUT(g2f, o);
						UNITY_SETUP_INSTANCE_ID(l[0]);
						UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(l[0]);
					
						o.color = l[0].color;
						o.vertex = l[0].vertex;
						o.uv = l[0].uv;
						o.isBrushed = l[0].isBrushed;
						UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(l[0], o);
						lineStream.Append(o);

						o.color = l[1].color;
						o.vertex = l[1].vertex;
						o.uv = l[1].uv;
						o.isBrushed = l[1].isBrushed;
						UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(l[0], o);
						lineStream.Append(o);
					}

				}

				// Fragment Shader -----------------------------------------------
				f_output frag(g2f i)
				{
					f_output o;
					
					UNITY_INITIALIZE_OUTPUT(f_output, o);
					UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
					
					// Access instanced variables
					float4 BrushColor = UNITY_ACCESS_INSTANCED_PROP(Props, _BrushColor);
					float ShowBrush = UNITY_ACCESS_INSTANCED_PROP(Props, _ShowBrush);
					float Alpha = UNITY_ACCESS_INSTANCED_PROP(Props, _Alpha);
					
					if (i.isBrushed > 0.0 && ShowBrush > 0.0)
					{
						o.color = float4(BrushColor.x, BrushColor.y, BrushColor.z, Alpha);
					}
					else if (Alpha < 0.01)
					{
						discard;
					}
					else
					{
						o.color =  float4(i.color.x, i.color.y, i.color.z, Alpha);
					};
					
					o.depth = i.vertex.z;
					return o;
				}
				ENDCG
			}
	}
}