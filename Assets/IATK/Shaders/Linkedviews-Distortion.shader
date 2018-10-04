Shader "Staxestk/Linked-Views-Material"
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

	}
		SubShader
	{
		Tags{ "RenderType" = "Transparent" }
			//Blend func : Blend Off : turns alpha blending off
			Blend SrcAlpha OneMinusSrcAlpha
			//Lighting On
			Zwrite On
			//ZTest NotEqual    
            //Cull Front
		LOD 200

		Pass
	{
			Tags { "RenderType"="Transparent" }

		CGPROGRAM
		#pragma vertex vert
		#pragma geometry geom
		#pragma fragment frag
				// make fog work
		#pragma multi_compile_fog

		#include "UnityCG.cginc"
		#include "DistortLinked.cginc"
		#include "Helper.cginc"

	struct appdata
	{
		float4 position : POSITION;
		float2 uv : TEXCOORD0;
		float4 color: COLOR;
		float3 normal	: NORMAL;
	};

	struct gs_in
	{
		float4 vertex	:	POSITION;
		float4 color	:	COLOR;
		float2 uv		:	TEXCOORD0;
		bool filtered	:	BOOL;
		float isBrushed	:	FLOAT;
	};

	struct v2f
	{
		float2 uv : TEXCOORD0;
		float4 vertex : SV_POSITION;
		float4 color: COLOR;
		float isBrushed	:	FLOAT;
	};

	sampler2D _MainTex;
	float4 _MainTex_ST;

					
	//*******************
	// RANGE FILTERING
	//*******************

	float _MinXFilter1;
	float _MaxXFilter1;
	float _MinYFilter1;
	float _MaxYFilter1;
	float _MinZFilter1;
	float _MaxZFilter1;

	float _MinXFilter2;
	float _MaxXFilter2;
	float _MinYFilter2;
	float _MaxYFilter2;
	float _MinZFilter2;
	float _MaxZFilter2;

	// ********************
	// Normalisation ranges
	// ********************

	float _MinNormX1;
	float _MaxNormX1;
	float _MinNormY1;
	float _MaxNormY1;
	float _MinNormZ1;
	float _MaxNormZ1;

	float _MinNormX2;
	float _MaxNormX2;
	float _MinNormY2;
	float _MaxNormY2;
	float _MinNormZ2;
	float _MaxNormZ2;

	//brush data
	sampler2D _BrushedTexture;
	float _DataWidth;
	float _DataHeight;
	float showBrush;
	float4 brushColor;

	// VERTEX SHADER
	gs_in vert(appdata v)
	{
		float4 pos;
		
		//lookup the texture to see if the vertex is brushed...
		float2 indexUV = float2((v.normal.x % _DataWidth) / _DataWidth, ((v.normal.x / _DataWidth) / _DataHeight));
		float4 brushValue = tex2Dlod(_BrushedTexture, float4(indexUV, 0.0, 0.0));
		
		gs_in o;
		o.uv = TRANSFORM_TEX(v.uv, _MainTex);
		o.color = v.color;
		//if()
		o.isBrushed = brushValue.r;

		float3 normalisedPosition;

		if(v.normal.z == 0.0)
		{


		 pos = float4(
					normaliseValue(v.position.x,_MinNormX1, _MaxNormX1, 0,1),
					normaliseValue(v.position.y,_MinNormY1, _MaxNormY1, 0,1),
					normaliseValue(v.position.z,_MinNormZ1, _MaxNormZ1, 0,1), v.position.w);

		if (v.position.x < _MinXFilter1 ||
			v.position.x > _MaxXFilter1 || 
			v.position.y < _MinYFilter1 || 
			v.position.y > _MaxYFilter1 || 
			v.position.z < _MinZFilter1 || 
			v.position.z > _MaxZFilter1 ||
			
			pos.x < 0 ||
			pos.x > 1 || 
			pos.y < 0 || 
			pos.y > 1 || 
			pos.z < 0 || 
			pos.z > 1					
			)
			{
			o.filtered = true;
			//o.color.w=0;			
			}
			else 
				o.filtered = false;
		}
		else if(v.normal.z == 1.0)
		{
		 pos = float4(
					normaliseValue(v.position.x,_MinNormX2, _MaxNormX2, 0,1),
					normaliseValue(v.position.y,_MinNormY2, _MaxNormY2, 0,1),
					normaliseValue(v.position.z,_MinNormZ2, _MaxNormZ2, 0,1), v.position.w);

		if (v.position.x < _MinXFilter2 ||
			v.position.x > _MaxXFilter2 || 
			v.position.y < _MinYFilter2 || 
			v.position.y > _MaxYFilter2 || 
			v.position.z < _MinZFilter2 || 
			v.position.z > _MaxZFilter2 ||

			pos.x < 0 ||
			pos.x > 1 || 
			pos.y < 0 || 
			pos.y > 1 || 
			pos.z < 0 || 
			pos.z > 1)
			{
			o.filtered = true;
			}
			else 
			o.filtered = false;
		}

		o.vertex = mul(UNITY_MATRIX_VP, ObjectToWorldDistort3d(pos, v.normal.z > 0));
		return o;
	}

	//GEOMETRY SHADER
	[maxvertexcount(2)]
	void geom (line gs_in l[2], inout LineStream<v2f> lineStream)
	{
		//bool filtered = false;
		bool filtered = (l[0].filtered || l[1].filtered 
			|| l[0].color.w == 0 || l[1].color.w == 0);

		if(!filtered)
		{
		v2f In;		
		In.color = l[0].color;
		In.vertex = l[0].vertex;
		In.uv = l[0].uv;
		In.isBrushed = l[0].isBrushed;

		lineStream.Append(In);

		In.color = l[1].color;
		In.vertex = l[1].vertex;
		In.uv = l[1].uv;
		In.isBrushed = l[1].isBrushed;

		lineStream.Append(In);
		}

	}

	//FRAGMENT SHADER
	fixed4 frag(v2f i) : SV_Target
	{
		if (i.isBrushed > 0.0 && showBrush > 0.0)
							return brushColor;
		else
		return i.color;	
	}
		ENDCG
	}
	}
}