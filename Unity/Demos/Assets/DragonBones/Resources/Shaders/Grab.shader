
Shader "DragonBones/BlendModes/Grab"
{
	Properties
	{
		[PerRendererData] 
		_MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
	}

	SubShader
	{
		Tags
		{ 
			"Queue" = "Transparent" 
			"IgnoreProjector" = "True" 
			"RenderType" = "Transparent" 
			"PreviewType" = "Plane"
			"CanUseSpriteAtlas" = "True"
		}

		Cull Off
		Lighting Off
		ZWrite Off
		Fog { Mode Off }
		Blend SrcAlpha OneMinusSrcAlpha
		
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
                vo.color = vi.color * _Color;
							
				return vo;
			}
			
			fixed4 frag(output vo) : SV_Target
			{                
				fixed4 color = tex2D(_MainTex, vo.texcoord) * vo.color;
				
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
	
	Fallback "Sprites/Default"
}
