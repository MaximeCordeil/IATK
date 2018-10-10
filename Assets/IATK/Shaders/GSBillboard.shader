// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "IATK/OutlineDots" 
{
	Properties 
	{
		_MainTex("Base (RGB)", 2D) = "White" {}
		_BrushedTexture("Base (RGB)", 2D) = "White" {}
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
	}
    
	SubShader 
	{
		Pass
		{
			Name "Onscreen geometry"
			Tags { "Queue"="Transparent" "RenderType"="Transparent" }
			Zwrite On
			ZTest LEqual
			Blend [_MySrcMode][_MyDstMode]
			Cull Off 
			Lighting Off 
			Offset -1, -1 // This line is added to default Unlit/Transparent shader
			LOD 200

			CGPROGRAM
				#pragma target 5.0
				#pragma vertex VS_Main
				#pragma fragment FS_Main
				#pragma geometry GS_Main
				#include "UnityCG.cginc" 
				#include "Distort.cginc"

				// **************************************************************
				// Data structures												*
				// **************************************************************
			
		        struct VS_INPUT {
          		    float4 position : POSITION;
            		float4 color: COLOR;
					float3 normal: NORMAL;
					float4 uv_MainTex : TEXCOORD0; // index, vertex size, filtered, prev size
        		};
				
				struct GS_INPUT
				{
					float4	pos : POSITION;
					float3	normal : NORMAL;
					float2  tex0 : TEXCOORD0;
					float4  color : COLOR;
					float	isBrushed : FLOAT;
				};

				struct FS_INPUT
				{
					float4	pos : POSITION;
					float2  tex0 : TEXCOORD0;
					float4  color : COLOR;
					float	isBrushed : FLOAT;
					float3	normal : NORMAL;
				};

				struct FS_OUTPUT
				{
					float4 color : COLOR;
					float depth : SV_Depth;
				};

				// **************************************************************
				// Vars															*
				// **************************************************************

				float _Size;
				float _MinSize;
				float _MaxSize;

				float _BrushSize;
				
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

				sampler2D _BrushedTexture;
				sampler2D _MainTex;

				float _DataWidth;
				float _DataHeight;
				float showBrush;
				float4 brushColor;
				
				float _Tween;
				//*********************************
				// helper functions
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
				GS_INPUT VS_Main(VS_INPUT v)
				{
					GS_INPUT output = (GS_INPUT)0;
					
					float idx = v.uv_MainTex.x;
					float size = v.uv_MainTex.y;
					float isFiltered = v.uv_MainTex.z;

					//lookup the texture to see if the vertex is brushed...
					float2 indexUV = float2((idx % _DataWidth) / _DataWidth, ((idx / _DataWidth) / _DataHeight));
					float4 brushValue = tex2Dlod(_BrushedTexture, float4(indexUV, 0.0, 0.0));

					output.isBrushed = brushValue.r;

					float3 pos = lerp(v.normal, v.position, _Tween);

					float4 normalisedPosition = float4(
					normaliseValue(pos.x,_MinNormX, _MaxNormX, 0,1),
					normaliseValue(pos.y,_MinNormY, _MaxNormY, 0,1),
					normaliseValue(pos.z,_MinNormZ, _MaxNormZ, 0,1), v.position.w);

					output.pos = normalisedPosition;
					output.normal = float3(idx, size, isFiltered);
					output.tex0 = float2(0, 0);
					output.color = v.color;
				
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
						output.color.w = 0;
					}

					return output;
				}



				// Geometry Shader -----------------------------------------------------
				[maxvertexcount(6)]
				void GS_Main(point GS_INPUT p[1], inout TriangleStream<FS_INPUT> triStream)
				{
					float4x4 MV = UNITY_MATRIX_MV;
					float4x4 vp = UNITY_MATRIX_VP;

					float3 up = UNITY_MATRIX_IT_MV[1].xyz;
					float3 right =  -UNITY_MATRIX_IT_MV[0].xyz;

					float dist = 1;
					float sizeFactor = normaliseValue(p[0].normal.y, 0.0, 1.0, _MinSize, _MaxSize);
										
					float halfS = 0.025f * (_Size + (dist * sizeFactor));
							
					float4 v[4];				

					v[0] = float4(p[0].pos + halfS * right - halfS * up, 1.0f);
					v[1] = float4(p[0].pos + halfS * right + halfS * up, 1.0f);
					v[2] = float4(p[0].pos - halfS * right - halfS * up, 1.0f);
					v[3] = float4(p[0].pos - halfS * right + halfS * up, 1.0f);
		
					FS_INPUT pIn;
					
					pIn.isBrushed = p[0].isBrushed;
					pIn.color = p[0].color;
					pIn.normal = p[0].normal;

					pIn.pos = UnityObjectToClipPos(v[0]);					
					pIn.tex0 = float2(1.0f, 0.0f);
					pIn.normal = p[0].normal;
					triStream.Append(pIn);

					pIn.pos = UnityObjectToClipPos(v[1]);
					pIn.tex0 = float2(1.0f, 1.0f);
					pIn.normal = p[0].normal;
					triStream.Append(pIn);

					pIn.pos = UnityObjectToClipPos(v[2]);
					pIn.tex0 = float2(0.0f, 0.0f);
					pIn.normal = p[0].normal;
					triStream.Append(pIn);

					pIn.pos = UnityObjectToClipPos(v[3]);
					pIn.tex0 = float2(0.0f, 1.0f);
					pIn.normal = p[0].normal;
					triStream.Append(pIn);
					
				}

				// Fragment Shader -----------------------------------------------
				FS_OUTPUT FS_Main(FS_INPUT input)
				{
					FS_OUTPUT o;
					if(input.isBrushed >0 && showBrush > 0.0)
					o.color = tex2D(_MainTex, input.tex0.xy) *brushColor;
					
					else
					o.color = tex2D(_MainTex, input.tex0.xy) *input.color;

					o.depth = o.color.a > 0.5 ? input.pos.z : 0;
					return o;
				}

				float4 FS_Main2(FS_INPUT input) : SV_Target0
				{
					float dx = input.tex0.x - 0.5f;
					float dy = input.tex0.y - 0.5f;
					float dt = dx * dx + dy * dy;

					if (input.color.w == 0)
					{
						discard;
						return float4(0.0, 0.0, 0.0, 0.0);					
					}
					else
					{
						if (dt <= 0.16f)
						{
							if (input.isBrushed > 0.0 && showBrush > 0.0)
								return brushColor;
							else
								return float4(input.color.x - dt*0.25,
											  input.color.y - dt*0.25,
											  input.color.z - dt*0.25,
											  input.color.w);
						}
						else
						{
							if (dt <= 0.21f)
							{
								return float4(0.0, 0.0, 0.0, 1.0);
							}
							else
							{
								discard;
								return float4(0.1, 0.1, 0.1, 1.0);
							}
						}
					}
				}

				// Fragment Shader -----------------------------------------------
				float4 FS_Main3(FS_INPUT input) : SV_Target0
				{
					float dx = input.tex0.x - 0.5f;
					float dy = input.tex0.y - 0.5f;

					float dt = dx * dx + dy * dy;
					
					if(input.color.w == 0)
					{
						discard;
						return float4(0.0, 0.0, 0.0, 0.0);
					}
					else
					{
						if( dt <= 0.0f)
						{
							if(input.isBrushed==1.0)
							return float4(1.0,0.0,0.0,1.0);
							else
							return float4(input.color.x-dt*0.75,input.color.y-dt*0.75,input.color.z-dt*0.75,input.color.w);
						}
						else
						{
							discard;	
							return float4(0.1, 0.1, 0.1, 1.0);
						}
					}
					return input.color;
				}

			ENDCG
		} // end pass
	} 
}