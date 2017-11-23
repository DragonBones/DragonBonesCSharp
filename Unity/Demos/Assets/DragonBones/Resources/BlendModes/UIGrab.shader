Shader "DragonBones/BlendModes/UIGrab"
{
	Properties
	{
		[PerRendererData] 
		_MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
		
		_StencilComp ("Stencil Comparison", Float) = 8
		_Stencil ("Stencil ID", Float) = 0
		_StencilOp ("Stencil Operation", Float) = 0
		_StencilWriteMask ("Stencil Write Mask", Float) = 255
		_StencilReadMask ("Stencil Read Mask", Float) = 255
		_ColorMask ("Color Mask", Float) = 15
	}

	SubShader
	{
		Tags
		{ 
			"Queue" = "Transparent" 
			"IgnoreProjector" = "True" 
			"RenderType" = "Transparent" 
			"PreviewType" = "Plane"		
		}
		
		Stencil
		{
			Ref [_Stencil]
			Comp [_StencilComp]
			Pass [_StencilOp] 
			ReadMask [_StencilReadMask]
			WriteMask [_StencilWriteMask]
		}

		Cull Off
		Lighting Off
		ZWrite Off
		ZTest [unity_GUIZTestMode]
		Fog { Mode Off }
		Blend SrcAlpha OneMinusSrcAlpha
		ColorMask [_ColorMask]

		GrabPass { }

		Pass
		{
			CGPROGRAM
			
			#include "UnityCG.cginc"

			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag
			
			sampler2D _MainTex;
			sampler2D _GrabTexture;
			fixed4 _Color;
			
			struct input
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct output
			{
				float4 vertex : SV_POSITION;
				fixed4 color : COLOR;
				half2 texcoord : TEXCOORD0;
				float4 screenPos : TEXCOORD1;
			};
			
            output vert(input vi)
			{
                output vo;
                vo.vertex = UnityObjectToClipPos(vi.vertex);
                vo.screenPos = vo.vertex;
                vo.texcoord = vi.texcoord;

                //在平台上定义的，在映射texels到像素时需要偏移调整(例如Direct3D 9)。
				#ifdef UNITY_HALF_TEXEL_OFFSET
                vo.vertex.xy += (_ScreenParams.zw - 1.0) * float2(-1.0, 1.0);
				#endif
                vo.color = vi.color * _Color;
							
				return vo;
			}

			fixed4 frag(output vo) : SV_Target
			{
				half4 color = tex2D(_MainTex, vo.texcoord) * vo.color;
                //如果x的任何分量小于零，则丢弃当前像素。
				clip(color.a - .01);
				
                //pos的范围是【-1,1】+1为【0,2】，乘以0.5变成uv的范围【0,1】
				float2 grabTexcoord = vo.screenPos.xy / vo.screenPos.w;
				grabTexcoord.x = (grabTexcoord.x + 1.0) * .5;
				grabTexcoord.y = (grabTexcoord.y + 1.0) * .5; 

                //解决平台差异 D3D原点在顶部，openGL在底部
				#if UNITY_UV_STARTS_AT_TOP
				grabTexcoord.y = 1.0 - grabTexcoord.y;
				#endif
				
                //抓取的当前屏幕颜色
				fixed4 grabColor = tex2D(_GrabTexture, grabTexcoord); 
				
                //Add Mode TODO others blendMode
                fixed4 result = grabColor + color;
                result.a = color.a;
                return result;
			}
			
			ENDCG
		}
	}

	Fallback "UI/Default"
}
