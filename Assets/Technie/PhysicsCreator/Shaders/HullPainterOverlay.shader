// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "HullPainterOverlay"
{
	Properties
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}
	SubShader
	{
		Pass
		{
			Tags
			{
				"Queue" = "Transparent-1"
				"RenderType" = "Transparent"
			}
			Blend SrcAlpha OneMinusSrcAlpha
			ZWrite On
			ZTest LEqual
			Offset -1.0, -1.0
			LOD 200

			CGPROGRAM
			#pragma vertex vert_surf
			#pragma fragment frag_surf
			#include "UnityCG.cginc"

			struct v2f_surf
			{
				float4 pos		: SV_POSITION;
				fixed4 color	: COLOR;
			};

			v2f_surf vert_surf (appdata_full v)
			{
				v2f_surf o;
				o.pos = UnityObjectToClipPos (v.vertex);
				o.color = v.color;
				return o;
			}

			float4 frag_surf (v2f_surf IN) : COLOR
			{
				return IN.color;
			}
			ENDCG
		}
	}
}
