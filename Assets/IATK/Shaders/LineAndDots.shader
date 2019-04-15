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
			Tags { "RenderType"="Transparent" }
			//Blend func : Blend Off : turns alpha blending off
			//#ifdef(VISUAL_ACCUMULATION)
			//Blend SrcAlpha One
			//#else
			Blend[_MySrcMode][_MyDstMode]
			//#endif
			
			//AlphaTest Greater .01
			Cull Off
			ZWrite On
			//Lighting On
		//	Zwrite On
			LOD 200

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma geometry geom
			// make fog work
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"
			#include "Distort.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
				float3 normal : NORMAL;
				float4 uv_MainTex : TEXCOORD0; // index, vertex size, filtered

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
				bool isLine : BOOL;
			};

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

			//*********************************
			// helper functions
			//*********************************
			float normaliseValue(float value, float i0, float i1, float j0, float j1)
			{
				float L = (j0 - j1) / (i0 - i1);
				return (j0 - (L * i0) + (L * value));
			}

			v2g vert (appdata v)
			{
				v2g o;
				float idx = v.uv_MainTex.x;
				float isFiltered = v.uv_MainTex.z;

				//lookup the texture to see if the vertex is brushed...
				float2 indexUV = float2((idx % _DataWidth) / _DataWidth, ((idx / _DataWidth) / _DataHeight));
				float4 brushValue = tex2Dlod(_BrushedTexture, float4(indexUV, 0.0, 0.0));


				o.isBrushed = brushValue.r;// > 0.001;

				float size = lerp(v.uv_MainTex.w, v.uv_MainTex.y, _TweenSize);
				float3 pos = lerp(v.normal, v.vertex, _Tween);


				float4 normalisedPosition = float4(
					normaliseValue(pos.x, _MinNormX, _MaxNormX, 0, 1),
					normaliseValue(pos.y, _MinNormY, _MaxNormY, 0, 1),
					normaliseValue(pos.z, _MinNormZ, _MaxNormZ, 0, 1), 1.0);

				float4 vert = (normalisedPosition);

				o.vertex = vert;
				o.normal = float3(idx,size,isFiltered);
				o.color =  v.color;

				//filtering
				if (normalisedPosition.x < _MinX ||
					normalisedPosition.x > _MaxX || 
					normalisedPosition.y < _MinY || 
					normalisedPosition.y > _MaxY || 
					normalisedPosition.z < _MinZ || 
					normalisedPosition.z > _MaxZ ||
					isFiltered				
					)
					
					{
						o.color.w = 0;
					}

				return o;
			}

			void emitPoint(v2g _point, inout TriangleStream<g2f> triStream)
			{
				float4x4 MV = UNITY_MATRIX_MV;
				float4x4 vp = UNITY_MATRIX_VP;

				float3 up = UNITY_MATRIX_IT_MV[1].xyz;
				float3 right =  -UNITY_MATRIX_IT_MV[0].xyz;

				float sizeFactor = normaliseValue(_point.normal.y, 0.0, 1.0, _MinSize, _MaxSize);
				float dist = 1;
				float halfS = 0.05f * (_Size + (dist * sizeFactor));
							
				float4 v[4];				

				v[0] = float4(_point.vertex + halfS * right - halfS * up, 1.0f);
				v[1] = float4(_point.vertex + halfS * right + halfS * up, 1.0f);
				v[2] = float4(_point.vertex - halfS * right - halfS * up, 1.0f);
				v[3] = float4(_point.vertex - halfS * right + halfS * up, 1.0f);
		
				g2f pIn;
					
				pIn.isBrushed = _point.isBrushed;
				pIn.color = _point.color;
				pIn.isLine = false;

				pIn.vertex = UnityObjectToClipPos(v[0]);					
				pIn.tex0 = float2(1.0f, 0.0f);
				pIn.isBrushed = _point.isBrushed; 
				triStream.Append(pIn);

				pIn.vertex = UnityObjectToClipPos(v[1]);
				pIn.tex0 = float2(1.0f, 1.0f);
				pIn.isBrushed = _point.isBrushed;
				triStream.Append(pIn);

				pIn.vertex = UnityObjectToClipPos(v[2]);
				pIn.isBrushed = _point.isBrushed;
				pIn.tex0 = float2(0.0f, 0.0f);
				triStream.Append(pIn);

				pIn.vertex = UnityObjectToClipPos(v[3]);
				pIn.isBrushed = _point.isBrushed;
				pIn.tex0 = float2(0.0f, 1.0f);
				triStream.Append(pIn);
				triStream.RestartStrip();


			}

			[maxvertexcount(16)]
			void geom(line v2g points[2], inout TriangleStream<g2f> triStream)
			{
				//handle brushing line topoolgy
				if (points[0].color.w == 0) points[1].color.w = 0;
				if (points[1].color.w == 0) points[0].color.w = 0;

				emitPoint(points[0], triStream);
				emitPoint(points[1], triStream);

				//line geometry
				float4 p0 = UnityObjectToClipPos(points[0].vertex);
				float4 p1 = UnityObjectToClipPos(points[1].vertex);

				float w0 = p0.w;
				float w1 = p1.w;

				p0.xyz /= p0.w;
				p1.xyz /= p1.w;

				float3 line01 = p1 - p0;
				float3 dir = normalize(line01);

				// scale to correct window aspect ratio
				float3 ratio = float3(1024, 768, 0);
				ratio = normalize(ratio);

				float3 unit_z = normalize(float3(0, 0, -1));

				float3 normal = normalize(cross(unit_z, dir) * ratio);

				float width = _Size * normaliseValue(points[0].normal.y, 0.0, 1.0, _MinSize, _MaxSize);

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

				triStream.Append(v[2]);
				triStream.Append(v[1]);
				triStream.Append(v[0]);

				triStream.RestartStrip();

				triStream.Append(v[3]);
				triStream.Append(v[2]);
				triStream.Append(v[0]);

				triStream.RestartStrip();

			

			}

			
			
			fixed4 frag (g2f i) : SV_Target
			{
				if(i.isLine)
				{
				fixed4 col = i.color;
				
				if (i.isBrushed && showBrush>0.0) col = brushColor;
				// TODO : test outline shader

				//float dx = i.tex0.x;// - 0.5f;
			    //float dy = i.tex0.y;// - 0.5f;

				//if(dx > 0.95 || dx < 0.05 /*|| dy <0.1  || dy>0.9*/ ) return float4(0.0, 1.0, 0.0, 1.0);
				if(col.w == 0) {discard; return float4(0.0,0.0,0.0,0.0);}
				return  col;
				}
				else
				{
				//FragmentOutput fo = (FragmentOutput)0;
					float dx = i.tex0.x - 0.5f;
					float dy = i.tex0.y - 0.5f;

					float dt = dx * dx + dy * dy;
					
					//if(input.color.x > 0.2 && input.color.y > 0.2 && input.color.z > 0.2)
					//{
					//			discard;
					//		return float4(0.0, 0.0, 0.0, 0.0);
					//}
					if(i.color.w == 0)
					{
						//if( dt <= 0.2f)
						//	return float4(0.1,0.1,0.1,1.0);
						//else
						//	if(dx * dx + dy * dy <= 0.25f)
						//	return float4(0.0, 0.0, 0.0, 1.0);
						//	else
						//	{
							discard;
							return float4(0.0, 0.0, 0.0, 0.0);
//							}
					}
					else
					{
					// this code outputs a cross
					// input.tex0.x>0.45 && input.tex0.x<0.55 || input.tex0.y>0.45 && input.tex0.y<0.55
					if( dt <= 0.2f)
					{
						if (i.isBrushed && showBrush>0.0) return brushColor;
						else
						return float4(i.color.x-dt*0.75,i.color.y-dt*0.75,i.color.z-dt*0.75,i.color.w);
					}// float4(input.color.x-dt*0.25,input.color.y-dt*0.25,input.color.z-dt*0.25,1.0);
					else
					//if(dx * dx + dy * dy <= 0.21f)
					//return float4(0.0, 0.0, 0.0, 1.0);
					//else
					{
					discard;	
					return float4(0.1, 0.1, 0.1, 1.0);
					}
					}
					return i.color;
					//return fo;
				}
			}
			ENDCG
		}
	}
}
