// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/DepthShader"
{
	Properties
	{
		_DepthLevel("Depth Level", Range(1, 3)) = 3
	}
		SubShader
	{
		Pass
		{

			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			uniform sampler2D_float _CameraDepthTexture;
			uniform fixed _DepthLevel;

			struct appdata
			{
				float4 pos : POSITION;	
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
				float d : FLOAT;
			};

			v2f vert(appdata i)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(i.pos);
				float d = o.pos.z / o.pos.w;
				o.d = o.pos.z / o.pos.w;
				return o;
			}

			float4 frag(v2f i) : COLOR
			{
				float depth = Linear01Depth(i.d);

				return float4(1.0 - depth, 1.0 - depth, 1.0 - depth, 1.0);
			}

			ENDCG
		}
	}
}
