// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/Fast Blur"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		_Color("Tint", Color) = (1,1,1,1)

		_StencilComp("Stencil Comparison", Float) = 8
		_Stencil("Stencil ID", Float) = 0
		_StencilOp("Stencil Operation", Float) = 0
		_StencilWriteMask("Stencil Write Mask", Float) = 255
		_StencilReadMask("Stencil Read Mask", Float) = 255

		_ColorMask("Color Mask", Float) = 15
		_Quality("Quality", Range(1, 32)) = 4
		_Directions("Directions to evaluate", Range(1, 32)) = 4
		_Size("Size", Range(1.0, 128.0)) = 4.0
		_BlurAmount("Blur Amount", Range(0, 100)) = 1.0

		[Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip("Use Alpha Clip", Float) = 0
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

			Stencil
		{
			Ref[_Stencil]
			Comp[_StencilComp]
			Pass[_StencilOp]
			ReadMask[_StencilReadMask]
			WriteMask[_StencilWriteMask]
		}

			Cull Off
			Lighting Off
			ZWrite Off
			ZTest[unity_GUIZTestMode]
			Blend SrcAlpha OneMinusSrcAlpha
			ColorMask[_ColorMask]

			Pass
		{
			CGPROGRAM
	#pragma vertex vert
	#pragma fragment frag

	#include "UnityCG.cginc"
	#include "UnityUI.cginc"

	#pragma multi_compile __ UNITY_UI_ALPHACLIP

			struct appdata_t
		{
			float4 vertex   : POSITION;
			float4 color    : COLOR;
			float2 texcoord : TEXCOORD0;
		};

		struct v2f
		{
			float4 vertex   : SV_POSITION;
			fixed4 color : COLOR;
			half2 texcoord  : TEXCOORD0;
			float4 worldPosition : TEXCOORD1;
		};

		fixed4 _Color;
		fixed4 _TextureSampleAdd;

		fixed _Quality;
		fixed _Directions;
		half _Size;

		float4 _ClipRect;
		uniform float _BlurAmount;

		v2f vert(appdata_t IN)
		{
			v2f OUT;
			OUT.worldPosition = IN.vertex;
			OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);

			OUT.texcoord = IN.texcoord;

	#ifdef UNITY_HALF_TEXEL_OFFSET
			OUT.vertex.xy += (_ScreenParams.zw - 1.0) * float2(-1,1);
	#endif

			OUT.color = IN.color * _Color;
			return OUT;
		}

		sampler2D _MainTex;
		float4 _MainTex_TexelSize;

		

		fixed4 frag(v2f IN) : SV_Target
		{
			float dx = _Size * _MainTex_TexelSize.x;
			float dy = _Size * _MainTex_TexelSize.y;
			float2 uv = float2(dx * floor(IN.texcoord.x / dx), dy * floor(IN.texcoord.y / dy));

			half4 color = tex2D(_MainTex, uv) + _TextureSampleAdd;
			
			half tau = 6.28318530718;
   
			half2 radius = half2(_Size * _MainTex_TexelSize.x, _Size * _MainTex_TexelSize.y) * _BlurAmount;
    
			for( half d = 0.0; d < tau; d += (tau / _Directions))
			{
				for(half i = 0.0; i < 1.0; i += (1.0 / _Quality))
				{
					color += tex2D(_MainTex, uv + half2(cos(d), sin(d)) * radius * i) +_TextureSampleAdd;
				}
			}
    
			// Output to screen
			color /= _Quality * _Directions;

			color *= IN.color;

			color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
		
	#ifdef UNITY_UI_ALPHACLIP
			clip(color.a - 0.001);
	#endif

			return color;
		}
			ENDCG
		}
		}
}