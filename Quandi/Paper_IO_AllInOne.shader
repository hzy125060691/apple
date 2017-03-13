// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Tank/Paper_IO_AllInOne"
{
	Properties
	{
		_PointsInfo("PointsInfo", 2D) = "white" {}
		_GroupTex1("Group 1", 2D) = "white" {}
		_GroupTex2("Group 2", 2D) = "white" {}
		_GroupTex3("Group 3", 2D) = "white" {}
		_GroupTex4("Group 4", 2D) = "white" {}
		_GroupTail1("Group Tail 1", 2D) = "white" {}
		_GroupTail2("Group Tail 2", 2D) = "white" {}
		_GroupTail3("Group Tail 3", 2D) = "white" {}
		_GroupTail4("Group Tail 4", 2D) = "white" {}
	}

	Category
	{
		Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha
		Lighting Off

		SubShader
		{
			Pass
			{
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag

				#include "UnityCG.cginc"

				sampler2D _PointsInfo;
				sampler2D _GroupTex1;
				sampler2D _GroupTex2;
				sampler2D _GroupTex3;
				sampler2D _GroupTex4;
				sampler2D _GroupTail1;
				sampler2D _GroupTail2;
				sampler2D _GroupTail3;
				sampler2D _GroupTail4;
				int _XMax;
				int _YMax;
				int _XCount;
				int _YCount;

				struct appdata
				{
					float4 vertex : POSITION;
					fixed4 color : COLOR;
					float2 uv : TEXCOORD0;
				};

				struct v2f
				{
					float4 vertex : SV_POSITION;
					fixed4 color : COLOR;
					float2 uv : TEXCOORD0;
				};

				float4 _PointsInfo_ST;


				v2f vert(appdata v)
				{
					v2f o;
					o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
					o.uv = TRANSFORM_TEX(v.uv, _PointsInfo);
					o.color = v.color;
					return o; 
				}

				fixed4 frag(v2f IN) : COLOR
				{
					float xx = IN.uv.x * (_XCount);
					float yy = IN.uv.y * (_YCount);
					fixed4 c = tex2D(_PointsInfo, float2(((floor(xx))+ 0.5 )/ _XMax, ((floor(yy)) + 0.5) / _YMax));
					fixed4 r = 0;
					if (c.a == 1)
					{
						
						
						int s = (int)(c.r * 256);

						if (c.g == 1)
						{
							if (s == 1)
							{
								r = tex2D(_GroupTail1, float2(xx - floor(xx), yy - floor(yy)));
							}
							else if (s == 2)
							{
								r = tex2D(_GroupTail2, float2(xx - floor(xx), yy - floor(yy)));
							}
							else if (s == 3)
							{
								r = tex2D(_GroupTail3, float2(xx - floor(xx), yy - floor(yy)));
							}
							else if (s == 4)
							{
								r = tex2D(_GroupTail4, float2(xx - floor(xx), yy - floor(yy)));
							}
							/*else if (s == 5)
							{
								r = tex2D(_GroupTail5, float2(xx - floor(xx), yy - floor(yy)));
							}
							else if (s == 6)
							{
								r = tex2D(_GroupTail6, float2(xx - floor(xx), yy - floor(yy)));
							}
							else if (s == 7)
							{
								r = tex2D(_GroupTail7, float2(xx - floor(xx), yy - floor(yy)));
							}
							else if (s == 8)
							{
								r = tex2D(_GroupTail8, float2(xx - floor(xx), yy - floor(yy)));
							}*/
							else
							{
								r = tex2D(_GroupTail4, float2(xx - floor(xx), yy - floor(yy)));
							}
						}
						else
						{
							if (s == 1)
							{
								r = tex2D(_GroupTex1, float2(xx - floor(xx), yy - floor(yy)));
							}
							else if (s == 2)
							{
								r = tex2D(_GroupTex2, float2(xx - floor(xx), yy - floor(yy)));
							}
							else if (s == 3)
							{
								r = tex2D(_GroupTex3, float2(xx - floor(xx), yy - floor(yy)));
							}
							else if (s == 4)
							{
								r = tex2D(_GroupTex4, float2(xx - floor(xx), yy - floor(yy)));
							}
							/*else if (s == 5)
							{
								r = tex2D(_GroupTex5, float2(xx - floor(xx), yy - floor(yy)));
							}
							else if (s == 6)
							{
								r = tex2D(_GroupTex6, float2(xx - floor(xx), yy - floor(yy)));
							}
							else if (s == 7)
							{
								r = tex2D(_GroupTex7, float2(xx - floor(xx), yy - floor(yy)));
							}
							else if (s == 8)
							{
								r = tex2D(_GroupTex8, float2(xx - floor(xx), yy - floor(yy)));
							}*/
							else
							{
								r = tex2D(_GroupTex4, float2(xx - floor(xx), yy - floor(yy)));
							}
						}
					}
					return r*IN.color;
				}
				ENDCG
			}
		}
	}
}