// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/DepthShader"
{
	Properties
	{
		_DepthLevel("Depth Level", Range(1, 3)) = 2
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
			uniform half4 _MainTex_TexelSize;

			struct appdata
			{
				float4 pos : POSITION;	
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
				fixed4 color : COLOR;
				float d : FLOAT;

				float4 scrPos : TEXCOORD1;
			};

			v2f vert(appdata i)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(i.pos);
				float d = o.pos.z / o.pos.w;
				o.d = d;
				o.color = fixed4(d, d, d, 1.0);
				o.scrPos = ComputeScreenPos(i.pos);
				return o;
			}

			fixed4 frag(v2f i) : COLOR
			{
				float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, UNITY_PROJ_COORD(i.scrPos));

				depth = Linear01Depth(i.d);

				return fixed4(1-depth, 1-depth, 1-depth, 1.0);
				//return i.color;
			}

			ENDCG
		}
	}
}
