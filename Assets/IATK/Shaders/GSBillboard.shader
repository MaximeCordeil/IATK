Shader "IATK/OutlineDots" 
{    Properties
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
		_TweenSize("_TweenSize", Range(0, 1)) = 1
    }

    SubShader
    {
        Pass
        {
            Name "Onscreen geometry"
			Blend [_MySrcMode][_MyDstMode]
			Cull Off 
			Lighting Off 
			LOD 200
			Offset -1, -1
			ZTest LEqual
			Zwrite On
			Tags { "Queue"="Transparent" "RenderType"="Transparent" }

            CGPROGRAM
                #pragma target 5.0
                #pragma vertex vert
                #pragma geometry geom
                #pragma fragment frag
                #pragma multi_compile_instancing
                #include "UnityCG.cginc"
                #include "Distort.cginc"


				// **************************************************************
				// Data structures												*
				// **************************************************************
                
                struct app_data
                {
                    float4 position : POSITION;
                    float4 color : COLOR;
                    float3 normal : NORMAL;
                    float4 uv_MainTex : TEXCOORD0; // [index of data point, vertex size, is filtered?, previous size]

                    UNITY_VERTEX_INPUT_INSTANCE_ID
                };

                struct v2g
                {
					float4	pos : POSITION;
					float3	normal : NORMAL; // [index of data point, current size at frame, is filtered?]
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
                    float depth : SV_Depth;
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
                v2g vert(app_data v)
                {
                    v2g output;

                    UNITY_SETUP_INSTANCE_ID(v);
                    UNITY_INITIALIZE_OUTPUT(v2g, output);
					UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
					UNITY_TRANSFER_INSTANCE_ID(v, output);

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

                    // [index of data point, vertex size, is filtered?, previous size]
                    float idx = v.uv_MainTex.x;
                    float isFiltered = v.uv_MainTex.z;

                    // Check if vertex is brushed by looking up the texture
					float2 indexUV = float2((idx % _DataWidth) / _DataWidth, ((idx / _DataWidth) / _DataHeight));
					float4 brushValue = tex2Dlod(_BrushedTexture, float4(indexUV, 0.0, 0.0));
                    output.isBrushed = brushValue.r;

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
                    output.pos = normalisedPosition;
                    output.normal = float3(idx, size, isFiltered);
                    output.tex0 = float2(0, 0);
                    output.color = v.color;

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
						output.color.w = 0;
					}

                    return output;
                }

				// Geometry Shader -----------------------------------------------------
                [maxvertexcount(6)]
                void geom(point v2g input[1], inout TriangleStream<g2f> triStream)
                {
                    g2f output;

					UNITY_SETUP_INSTANCE_ID(input[0]);
					UNITY_INITIALIZE_OUTPUT(g2f, output);
					UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input[0]);

					// Access instanced variables
                    float Size = UNITY_ACCESS_INSTANCED_PROP(Props, _Size);
                    float MinSize = UNITY_ACCESS_INSTANCED_PROP(Props, _MinSize);
                    float MaxSize = UNITY_ACCESS_INSTANCED_PROP(Props, _MaxSize);

                    float3 cameraUp = UNITY_MATRIX_IT_MV[1].xyz;
                    float3 cameraRight = UNITY_MATRIX_IT_MV[0].xyz;

                    float dist = 1;
                    float sizeFactor = normaliseValue(input[0].normal.y, 0.0, 1.0, MinSize, MaxSize);
                    float halfS = 0.025f * (Size + (dist * sizeFactor));

                    // Draw a square with corners at halfS distance away from original vertex point
                    float4 v[4];
					v[0] = float4(input[0].pos + halfS * cameraRight - halfS * cameraUp, 1.0f);
					v[1] = float4(input[0].pos + halfS * cameraRight + halfS * cameraUp, 1.0f);
					v[2] = float4(input[0].pos - halfS * cameraRight - halfS * cameraUp, 1.0f);
					v[3] = float4(input[0].pos - halfS * cameraRight + halfS * cameraUp, 1.0f);

                    // Pass remaining values into fragment shader
                    output.isBrushed = input[0].isBrushed;
                    output.color = input[0].color;
                    output.normal = input[0].normal;
                    
                    output.pos = UnityObjectToClipPos(v[0]);
                    output.tex0 = float2(1.0f, 0.0f);
					UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(input[0], output);
                    triStream.Append(output);

                    output.pos = UnityObjectToClipPos(v[1]);
                    output.tex0 = float2(1.0f, 1.0f);
					UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(input[0], output);
                    triStream.Append(output);

                    output.pos = UnityObjectToClipPos(v[2]);
                    output.tex0 = float2(0.0f, 0.0f);
					UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(input[0], output);
                    triStream.Append(output);

                    output.pos = UnityObjectToClipPos(v[3]);
                    output.tex0 = float2(0.0f, 1.0f);
					UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(input[0], output);
                    triStream.Append(output);

					triStream.RestartStrip();
                }

				// Fragment Shader -----------------------------------------------
                f_output frag(g2f input)
                {
                    f_output output;

					UNITY_INITIALIZE_OUTPUT(f_output, output);
					UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
					
					// Access instanced variables
					float4 BrushColor = UNITY_ACCESS_INSTANCED_PROP(Props, _BrushColor);
					float ShowBrush = UNITY_ACCESS_INSTANCED_PROP(Props, _ShowBrush);

                    // Sample _MainTex for colour which creates the circular dot, changing colour depending if it is brushed
                    if (input.isBrushed > 0 && ShowBrush > 0.0)
                        output.color = tex2D(_MainTex, input.tex0.xy) * BrushColor;
                    else
                        output.color = tex2D(_MainTex, input.tex0.xy) * input.color;;

                    output.depth = output.color.a > 0.5 ? input.pos.z : 0;

                    return output;
                }

            ENDCG
        }
    }
}