// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Hidden/Additive_ClipByUI"
{
	Properties
	{
		_TintColor("Tint Color", Color) = (0.5,0.5,0.5,0.5)
		_MainTex("Particle Texture", 2D) = "white" {}
		_PanelSizeX("_PanelSizeX", Range(0.000000,300.000000)) = 0.000000
		_PanelSizeY("_PanelSizeY", Range(0.000000,300.000000)) = 0.000000
		_PanelCenterAndSharpness("_PanelCenterAndSharpness", Vector) = (0,0,0,0)
	}

	Category
	{
		Tags
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
		}

		LOD 100
		Cull Off
		Lighting Off
		ZWrite Off
		Fog{ Mode Off }
		ColorMask RGB
		AlphaTest Greater .01
		Blend SrcAlpha One

		SubShader
		{
			Pass
			{
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile_particles

				#include "UnityCG.cginc"

				sampler2D _MainTex;
				fixed4 _TintColor;
				float4 _PanelCenterAndSharpness;
				float _PanelSizeX;
				float _PanelSizeY;



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
					float2 posInPanel : TEXCOORD1;
					//float2 worldPos : TEXCOORD2;
				};

				float4 _MainTex_ST;

				v2f vert(appdata v)
				{
					v2f o;

					//o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
					//o.color = v.color;
					//o.uv = v.uv;
					//o.worldPos = v.vertex.xy * _ClipRange0.zw + _ClipRange0.xy;


					o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
					o.color = v.color;
					o.uv = TRANSFORM_TEX(v.uv, _MainTex);

					float2 clipSpace = o.vertex.xy / o.vertex.w;

					// Normalize clip space  

					o.posInPanel = (clipSpace.xy + 1) * 0.5;

					// Adjust for panel offset  
					o.posInPanel.x -= _PanelCenterAndSharpness.x;
					o.posInPanel.y -= _PanelCenterAndSharpness.y;

					// Adjust for panel size  
					o.posInPanel.x *= (2 / _PanelSizeX);
					o.posInPanel.y *= (2 / _PanelSizeY);

					return o;
				}

				half4 frag(v2f IN) : COLOR
				{
					float2 k = float2(_PanelCenterAndSharpness.z, _PanelCenterAndSharpness.w) *0.5;
					// Softness factor  
					float2 factor = (float2(1.0, 1.0) - abs(IN.posInPanel)) * _PanelCenterAndSharpness.zw *0.5;
					//  (float2(1, 1) - abs(vx’, vy’)) * (pw’ / sx, ph’ / sy) 
					//                                    pw’ = 0.5 * pw, ph’ = 0.5 * ph
					// Sample the texture  
					half4 col = 2.0f * tex2D(_MainTex, IN.uv) * _TintColor * IN.color;
					col.a *= clamp(min(factor.x, factor.y), 0.0, 1.0);
					return col;
				}
				ENDCG
			}
		}
	}
}