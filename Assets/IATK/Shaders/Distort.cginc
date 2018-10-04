// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

uniform fixed4 _ftl;
uniform fixed4 _ftr;
uniform fixed4 _fbl;
uniform fixed4 _fbr;

uniform fixed4 _btl;
uniform fixed4 _btr;
uniform fixed4 _bbl;
uniform fixed4 _bbr;


float4 ObjectToWorldDistort(in float3 pos)
{
	float u = (pos.x + 0.5);
	float v = 1 - (pos.y + 0.5);
	float3 o = _ftl * (1 - u) * (1 - v) + _ftr * u * (1 - v) + _fbl * (1 - u) * v + _fbr * u * v;
	return mul(UNITY_MATRIX_M, float4(o, 1.0));
}

float4 ObjectToWorldDistort3d(in float3 pos)
{
	float u = (pos.x + 0.5);
	float v = 1 - (pos.y + 0.5);
	float w = pos.z + 0.5;
	float3 o = _ftl * (1 - u) * (1 - v) * (1 - w) + _ftr * u * (1 - v) * (1 - w) + _fbl * (1 - u) * v * (1 - w) + _fbr * u * v * (1 - w) +
				_btl * (1 - u) * (1 - v) * w + _btr * u * (1 - v) * w + _bbl * (1 - u) * v * w + _bbr * u * v * w;
	return mul(UNITY_MATRIX_M, float4(o, 1.0));
}

float4 ObjectToProjectionDistort(in float3 pos)
{
	float u = (pos.x + 0.5);
	float v = 1 - (pos.y + 0.5);
	float3 o = _ftl * (1 - u) * (1 - v) + _ftr * u * (1 - v) + _fbl * (1 - u) * v + _fbr * u * v;
	return UnityObjectToClipPos(float4(o, 1.0));
}