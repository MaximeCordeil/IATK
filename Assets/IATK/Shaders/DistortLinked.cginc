// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

uniform fixed4 _ftl1;
uniform fixed4 _ftr1;
uniform fixed4 _fbl1;
uniform fixed4 _fbr1;

uniform fixed4 _btl1;
uniform fixed4 _btr1;
uniform fixed4 _bbl1;
uniform fixed4 _bbr1;

uniform fixed4 _ftl2;
uniform fixed4 _ftr2;
uniform fixed4 _fbl2;
uniform fixed4 _fbr2;

uniform fixed4 _btl2;
uniform fixed4 _btr2;
uniform fixed4 _bbl2;
uniform fixed4 _bbr2;


float4 ObjectToWorldDistort(in float3 pos)
{
	float u = (pos.x + 0.5);
	float v = 1 - (pos.y + 0.5);
	float3 o = _ftl1 * (1 - u) * (1 - v) + _ftr1 * u * (1 - v) + _fbl1 * (1 - u) * v + _fbr1 * u * v;
	return mul(UNITY_MATRIX_M, float4(o, 1.0));
}

float4 ObjectToWorldDistort3d(in float3 pos, bool isleft)
{
    float3 o;

    if (!isleft)
    {
        //pos.x += 1;

        float u = (pos.y);
        float v = 1 - (pos.x);
        float w = 1 - pos.z;
        o = _ftl1 * (1 - u) * (1 - v) * (1 - w) + _ftr1 * u * (1 - v) * (1 - w) + _fbl1 * (1 - u) * v * (1 - w) + _fbr1 * u * v * (1 - w) +
                    _btl1 * (1 - u) * (1 - v) * w + _btr1 * u * (1 - v) * w + _bbl1 * (1 - u) * v * w + _bbr1 * u * v * w;
    }
    else 
    {
        //pos.x -= 1;

        float u = (pos.y);
        float v = 1 - (pos.x);
        float w = 1 - pos.z;
        o = _ftl2 * (1 - u) * (1 - v) * (1 - w) + _ftr2 * u * (1 - v) * (1 - w) + _fbl2 * (1 - u) * v * (1 - w) + _fbr2 * u * v * (1 - w) +
                    _btl2 * (1 - u) * (1 - v) * w + _btr2 * u * (1 - v) * w + _bbl2 * (1 - u) * v * w + _bbr2 * u * v * w;
    }
	
	return mul(UNITY_MATRIX_M, float4(o, 1.0));
}

//float4 ObjectToProjectionDistort(in float3 pos)
//{
//	float u = (pos.x + 0.5);
//	float v = 1 - (pos.y + 0.5);
//	float3 o = _ftl * (1 - u) * (1 - v) + _ftr * u * (1 - v) + _fbl * (1 - u) * v + _fbr * u * v;
//	return UnityObjectToClipPos(float4(o, 1.0));
//}