// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

//*********************************
				// helper functions
				//*********************************

float normaliseValue(float value, float i0, float i1, float j0, float j1)
{
float L = (j0 - j1) / (i0 - i1);
return (j0 - (L * i0) + (L * value));
}