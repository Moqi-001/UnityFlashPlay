Shader "Unlit/VectorUnlitShader"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
		_Mask("MaskTexture", 2D) = "white" {}
        _Color("Color",Color) = (1,1,1,1)
       
        _Transformation("Transformation",Vector) = (0,0,0,0)
        _Rotation("Rotation",Vector) = (0,0,0,0)
        _Scale("Scale",Vector) = (1,1,1,1)
		_FocalPoint("FocalPoint",Vector)=(0,0,0,0)

		_ProjectionT("ProjectionT",Vector) = (0,0,0,0)
        _ProjectionR("ProjectionR",Vector) = (0,0,0,0)
        _ProjectionS("ProjectionS",Vector) = (1,1,1,0)

		_PaintTransformationT("PaintTransformationT",Vector) = (0,0,0,0)
        _PaintTransformationR("PaintTransformationR",Vector) = (0,0,0,0)
        _PaintTransformationS("PaintTransformationS",Vector) = (0,0,0,0)

        _AddTerm("AddTerm",Vector) = (0,0,0,0)
        _MulTerm("MulTerm",Vector) = (1,1,1,1)
        _Offset("Offset",Vector) = (0,0,0,0)
        _MaskChannels("MaskChannels",Vector) = (0,0,0,0)
        _IsTex("_IsTex",float) = 0
        _IsRadial("_IsRadial",float) = 0
        _IsRadial("_IsLine",float) = 0
		_IsMask("_IsMask",float) = 0
		_Ref("_Ref",int) = 2

		[Enum(UnityEngine.Rendering.BlendOp  )] _BlendOp  ("BlendOp" , Int) = 0
		[Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("SrcBlend", Int) = 1
		[Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("DstBlend", Int) = 10

		[Enum(Off, 0, On, 1)]_ZWriteMode ("ZWrite", float) = 1  //深度写入
		// Greater/GEqual/Less/LEqual/Equal/NotEqual/Always/Never/Off，默认值为LEqual 即当物体深度小于或等于缓存深度值时(越远深度越大)，该物体渲染，就是默认的先后顺序渲染。
        [Enum(UnityEngine.Rendering.CompareFunction)]_ZTestMode ("ZTest", Float) = 4 //深度测试
        [Enum(UnityEngine.Rendering.ColorWriteMask)]_ColorMask ("ColorMask", Float) = 15
	    [Enum(UnityEngine.Rendering.CullMode)]_CullMode ("Cull", float) = 2  //裁剪 
 
 
		//模板测试
		//Stencil如果开启了模板测试，GPU会首先会读取模板缓冲区的值，然后把该值和读取的参考值ref进行比较，比较方式由Comp指定，比如大于Greater就表示通过模板测试，
       //  然后由Pass Fail ZFail去指定通过和不通过模板和深度测试后对缓冲区的值进行的Operation处理。
        [Header(Stencil)]
        [Enum(UnityEngine.Rendering.CompareFunction)]_StencilComp ("Stencil Comparison", Float) = 8
        [IntRange]_StencilWriteMask ("Stencil Write Mask", Range(0,255)) = 255
        [IntRange]_StencilReadMask ("Stencil Read Mask", Range(0,255)) = 255
        [IntRange]_Stencil ("Stencil ID", Range(0,255)) = 0
        [Enum(UnityEngine.Rendering.StencilOp)]_StencilPass ("Stencil Pass", Float) = 0
        [Enum(UnityEngine.Rendering.StencilOp)]_StencilFail ("Stencil Fail", Float) = 0
        [Enum(UnityEngine.Rendering.StencilOp)]_StencilZFail ("Stencil ZFail", Float) = 0
    }
        SubShader
        {
		    //Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
            //Tags { "RenderType" = "Transparent" "Queue" = "AlphaTest"}
            //Tags { "RenderType" = "Opaque" }
            //Tags { "Queue" = "AlphaTest" "IgnoreProjector" = "True"  }
            //Tags { "QUEUE" = "AlphaTest" "IGNOREPROJECTOR" = "true" "RenderType" = "TransparentCutout" }
            LOD 100
			Tags {
			"Queue"             = "Transparent"
			"IgnoreProjector"   = "True"
			"RenderType"        = "Transparent"
			"PreviewType"       = "Plane"
			"CanUseSpriteAtlas" = "True"
		}
		Stencil
            {
                Ref [_Stencil]   //设置模板参考值
                Comp [_StencilComp] //比较方式，有8种比较方式。参数包括Greater/GEqual/Less/LEqual/Equal/NotEqual/Always/Never
                ReadMask [_StencilReadMask]  //readMask默认是255，一般不用该功能，设置隐码后 读取ref和buff值都需要与该码进行与操作。（0-255）
                WriteMask [_StencilWriteMask] //写操作进行与操作
                Pass [_StencilPass]   //这个是当stencil测试和深度测试都通过的时候，进行的stencilOperation操作方法
                Fail [_StencilFail]   //这个是在stencil测试通过的时候执行的stencilOperation方法
                ZFail [_StencilZFail] //这个是在stencil测试通过，但是深度测试没有通过的时候执行的stencilOperation方法。
			}
		    //Cull off
		    ZWrite [_ZWriteMode]
            ZTest [_ZTestMode]
            ColorMask [_ColorMask]
            Cull [_CullMode]

		    BlendOp [_BlendOp]
		    Blend [_SrcBlend] [_DstBlend]
            Pass
            {
                //Tags { "RenderType" = "Transparent" "Queue" = "AlphaTest"}
                //Tags { "QUEUE" = "AlphaTest" "IGNOREPROJECTOR" = "true" "RenderType" = "TransparentCutout" }
                //Blend SrcAlpha OneMinusSrcAlpha
				//Blend OneMinusSrcAlpha One
				//Blend One OneMinusSrcAlpha
			    //ZClip Off
			    //ZWrite Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            //#pragma multi_compile_fog
			#pragma fragmentoption ARB_precision_hint_fastest
            #pragma multi_compile_instancing //这里,第一步
            #include "UnityCG.cginc"
            uniform float4 _Color;
            uniform float4 _ProjectionT;
            uniform float4 _ProjectionR;
            uniform float4 _ProjectionS;

            uniform float4 _Transformation;
            uniform float4 _Rotation;
            uniform float4 _Scale;

			uniform float4 _PaintTransformationT;
            uniform float4 _PaintTransformationR;
            uniform float4 _PaintTransformationS;

            uniform float4 _AddTerm;
            uniform float4 _MulTerm;
            uniform float4 _Offset;
            uniform float4 _MaskChannels;
            uniform float _IsTex;
            uniform float _IsRadial;
            uniform float _IsLine;
            uniform float _IsMask;
            uniform float4 _FocalPoint;

            struct appdata
            {
                float4 vertex : POSITION;
                float4 uv : TEXCOORD0;
                float4 color : COLOR;
               
			   UNITY_VERTEX_INPUT_INSTANCE_ID //这里,第二步
            };

            struct v2f
            {
                float4 uv : TEXCOORD0;
                //UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID //这里,第二步
            };

            float4x4 Translational(float4 translational,float4 rotation,float4 scale)
            {
                return float4x4(scale.x,    rotation.x, 0.0,     translational.x,
                                rotation.y, scale.y,    0.0,     translational.y,
                                0.0,        0.0,        scale.z, translational.z,
                                0.0,        0.0,        0.0,     0.0
                    );
            }

            sampler2D _MainTex;
            float4 _MainTex_ST;
			sampler2D _Mask;
            float4 _Mask_ST;

            v2f vert(appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v); //这里第三步
                UNITY_TRANSFER_INSTANCE_ID(v, o); //第三步 
                //v.vertex.z = _Z;
				float4 tcTransform=float4(1, 1, 0.5, 0.5);
				//float4 tcTransform=float4(0, 0, 1, 1);
                v.vertex.xy = v.vertex.xy + _Offset.xy;
				v.vertex.z=1;
	
                //v.uv.xy = mul(Translational(_PaintTransformationT,_PaintTransformationR,_PaintTransformationS),v.vertex).xy;
				//v.uv.xy = mul(Translational(_ProjectionT,_ProjectionR,_Scale),v.vertex).xy;
				//v.uv.xy = (v.uv.xy + tcTransform.xy) * tcTransform.zw - _FocalPoint.xy;

                v.vertex = mul(Translational(_Transformation,_Rotation,_Scale),v.vertex);
				//v.vertex = mul(Translational(_ProjectionT,_ProjectionR,_ProjectionS), v.vertex);
				//fixed4 postion = mul(Translational(_ProjectionT,_ProjectionR,_ProjectionS), v.vertex);
				//v.uv.zw=postion.xy* float2(0.5, -0.5) + float2(0.5, 0.5);
                o.vertex = UnityObjectToClipPos(v.vertex);
				//v.uv.xy = mul(Translational(_ProjectionT,_ProjectionR,_Scale),v.vertex).xy;
				if(_IsRadial>0)
				{
				   v.uv.xy = (o.vertex.xy + tcTransform.xy) * tcTransform.zw - _FocalPoint.xy;
				}
				else
				{
				   v.uv.xy=TRANSFORM_TEX(v.uv, _MainTex);
				}
				
				v.uv.zw=o.vertex.xy* float2(0.5, -0.5) + float2(0.5, 0.5);
                o.color = v.color;
				
				o.uv=v.uv;
                 //UNITY_TRANSFER_FOG(o,o.vertex);

                return o;
             }

			float4 CxForm(float4 color)
            {
	            return clamp(color * _MulTerm + _AddTerm, 0, 1);
            }

			float4 Premultiply(float4 color)
            {
	            color.xyz *= color.w;
	            return color;
            }

			float4 RadialFill(float4 coords)
            {
	          return CxForm(tex2D(_MainTex, float2(length(coords.xy), 0.5)));
            }
			
			fixed4 LinearFill(float4 coords) 
            {
               return CxForm(tex2D(_MainTex, coords.xy));
            }

			float4 MaskPixel(float4 coords, float4 color)
            {
	            float alpha = clamp(dot(tex2D(_Mask, coords.zw), _MaskChannels), 0, 1);
	            return color * alpha;
            }

			float4 FromLinear(float4 color)
            {
	            // http://chilliant.blogspot.cz/2012/08/srgb-approximations-for-hlsl.html
	            return max(1.055 * pow(color, 0.416666667) - 0.055, 0);
             }

             fixed4 frag(v2f i) : SV_Target
             {
                 // sample the texture
                 fixed4 col = i.color;
                 if (_IsTex > 0)
                 {
				   col =tex2D(_MainTex,i.uv.xy );
				   if(_IsMask>0)
				   {
				       return MaskPixel(i.uv,Premultiply(CxForm(col)));
				   }
				   if(_IsRadial>0)
				   {
					  if(_IsLine>0)
				      {
				         //return FromLinear( LinearFill(i.uv));
				      }
				      return Premultiply(RadialFill(i.uv));
					  //return RadialFill(i.uv);
				   }
				   
                     //col =tex2D(_MainTex, float2(length(i.uv.xy), 0.5));
                   
                     //clip(col.a - 0.1f);
                    return Premultiply( CxForm(col));
                    //return CxForm(col);
                 }
				 //col=col*_Color;
                 UNITY_SETUP_INSTANCE_ID(i); //最后一步
                 //UNITY_APPLY_FOG(i.fogCoord, col);
				 //return col;
                 //return CxForm(col);
				 return Premultiply(CxForm(col));
				 //return Premultiply(CxForm(_Color));
             }
         ENDCG
     }
        }
}
