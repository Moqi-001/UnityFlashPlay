Shader "Unlit/Fill_PatternPremultiplied"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _Mask("MaskTexture", 2D) = "white" {}
        _Color("Color",Color) = (1,1,1,1)

        _Transformation("Transformation",Vector) = (0,0,0,0)
        _Rotation("Rotation",Vector) = (0,0,0,0)
        _Scale("Scale",Vector) = (1,1,1,1)
        _FocalPoint("FocalPoint",Vector) = (0,0,0,0)

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
        _IsMask("_IsMask",float) = 0

        [Enum(UnityEngine.Rendering.BlendOp)] _BlendOp("BlendOp" , Int) = 0
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("SrcBlend", Int) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("DstBlend", Int) = 10
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        Cull off
        BlendOp[_BlendOp]
        Blend[_SrcBlend][_DstBlend]
        Pass
        {
            Name "Normal"
            CGPROGRAM
            #pragma vertex Transform_ZO
            #pragma fragment TextureFill
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float4 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

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
            uniform float4 _FocalPoint;
            uniform float _IsTex;
            uniform float _IsRadial;
            uniform float _IsMask;

            float4x4 Translational(float4 translational, float4 rotation, float4 scale)
            {
                return float4x4(scale.x, rotation.x, 0.0, translational.x,
                    rotation.y, scale.y, 0.0, translational.y,
                    0.0, 0.0, scale.z, translational.z,
                    0.0, 0.0, 0.0, 0.0
                    );
            }

            v2f Transform_ZO(appdata v)
            {
                v2f o;
                v.vertex.xy = v.vertex.xy + _Offset.xy;
                //v.vertex.z = 1;
                v.vertex = mul(Translational(_Transformation, _Rotation, _Scale), v.vertex);
                o.vertex = UnityObjectToClipPos(v.vertex);

                float4 tcTransform = float4(1, 1, 0.5, 0.5);
                if (_IsRadial > 0)
                {
                    v.uv.xy = (o.vertex.xy + tcTransform.xy) * tcTransform.zw - _FocalPoint.xy;
                }
                else
                {
                    v.uv.xy = TRANSFORM_TEX(v.uv, _MainTex);
                }

                v.uv.zw = o.vertex.xy * float2(0.5, -0.5) + float2(0.5, 0.5);
                o.color = v.color;
                o.uv = v.uv;
                return o;
            }

            float4 CxForm(float4 color)
            {
                return clamp(color * _MulTerm + _AddTerm, 0, 1);
            }

            fixed4 TextureFill(v2f i) : SV_Target
            {
               return CxForm(tex2D(_MainTex, i.uv.xy));
            }
            ENDCG
        }

        Pass
        {
            Name "Linear"
            CGPROGRAM
            #pragma vertex Transform_ZO
            #pragma fragment TextureFill_L
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float4 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

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
            uniform float4 _FocalPoint;
            uniform float _IsTex;
            uniform float _IsRadial;
            uniform float _IsMask;

            float4x4 Translational(float4 translational, float4 rotation, float4 scale)
            {
                return float4x4(scale.x, rotation.x, 0.0, translational.x,
                    rotation.y, scale.y, 0.0, translational.y,
                    0.0, 0.0, scale.z, translational.z,
                    0.0, 0.0, 0.0, 0.0
                    );
            }

            v2f Transform_ZO(appdata v)
            {
                v2f o;
                v.vertex.xy = v.vertex.xy + _Offset.xy;
                //v.vertex.z = 1;
                v.vertex = mul(Translational(_Transformation, _Rotation, _Scale), v.vertex);
                o.vertex = UnityObjectToClipPos(v.vertex);

                float4 tcTransform = float4(1, 1, 0.5, 0.5);
                if (_IsRadial > 0)
                {
                    v.uv.xy = (o.vertex.xy + tcTransform.xy) * tcTransform.zw - _FocalPoint.xy;
                }
                else
                {
                    v.uv.xy = TRANSFORM_TEX(v.uv, _MainTex);
                }

                v.uv.zw = o.vertex.xy * float2(0.5, -0.5) + float2(0.5, 0.5);
                o.color = v.color;
                o.uv = v.uv;
                return o;
            }

            float4 CxForm(float4 color)
            {
                return clamp(color * _MulTerm + _AddTerm, 0, 1);
            }

            float4 FromLinear(float4 color)
            {
                return max(1.055 * pow(color, 0.416666667) - 0.055, 0);
            }

            fixed4 TextureFill(v2f i) : SV_Target
            {
               return CxForm(tex2D(_MainTex, i.uv.xy));
            }

            fixed4 TextureFill_L(v2f i) : SV_Target
            {
                return FromLinear(TextureFill(i));
            }
            ENDCG
        }

        Pass
        {
            Name "Masked"
            CGPROGRAM
            #pragma vertex Transform_ZO
            #pragma fragment Texture_M
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float4 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _Mask;
            float4 _Mask_ST;

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
            uniform float4 _FocalPoint;
            uniform float _IsTex;
            uniform float _IsRadial;
            uniform float _IsMask;

            float4x4 Translational(float4 translational, float4 rotation, float4 scale)
            {
                return float4x4(scale.x, rotation.x, 0.0, translational.x,
                    rotation.y, scale.y, 0.0, translational.y,
                    0.0, 0.0, scale.z, translational.z,
                    0.0, 0.0, 0.0, 0.0
                    );
            }

            v2f Transform_ZO(appdata v)
            {
                v2f o;
                v.vertex.xy = v.vertex.xy + _Offset.xy;
                //v.vertex.z = 1;
                v.vertex = mul(Translational(_Transformation, _Rotation, _Scale), v.vertex);
                o.vertex = UnityObjectToClipPos(v.vertex);

                float4 tcTransform = float4(1, 1, 0.5, 0.5);
                if (_IsRadial > 0)
                {
                    v.uv.xy = (o.vertex.xy + tcTransform.xy) * tcTransform.zw - _FocalPoint.xy;
                }
                else
                {
                    v.uv.xy = TRANSFORM_TEX(v.uv, _MainTex);
                }

                v.uv.zw = o.vertex.xy * float2(0.5, -0.5) + float2(0.5, 0.5);
                o.color = v.color;
                o.uv = v.uv;
                return o;
            }

            float4 CxForm(float4 color)
            {
                return clamp(color * _MulTerm + _AddTerm, 0, 1);
            }

            float4 MaskPixel(float4 coords, float4 color)
            {
                float alpha = clamp(dot(tex2D(_Mask, coords.zw), _MaskChannels), 0, 1);
                return color * alpha;
            }

            fixed4 TextureFill(v2f i) : SV_Target
            {
               return CxForm(tex2D(_MainTex, i.uv.xy));
            }

            fixed4 Texture_M(v2f i) : SV_Target
            {
                return MaskPixel(i.uv,TextureFill(i));
            }
        ENDCG
        }

        Pass
        {
            Name "MaskedLinear"
            CGPROGRAM
            #pragma vertex Transform_ZO
            #pragma fragment TextureFill_LM
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float4 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _Mask;
            float4 _Mask_ST;

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
            uniform float4 _FocalPoint;
            uniform float _IsTex;
            uniform float _IsRadial;
            uniform float _IsMask;

            float4x4 Translational(float4 translational, float4 rotation, float4 scale)
            {
                return float4x4(scale.x, rotation.x, 0.0, translational.x,
                    rotation.y, scale.y, 0.0, translational.y,
                    0.0, 0.0, scale.z, translational.z,
                    0.0, 0.0, 0.0, 0.0
                    );
            }

            v2f Transform_ZO(appdata v)
            {
                v2f o;
                v.vertex.xy = v.vertex.xy + _Offset.xy;
                //v.vertex.z = 1;
                v.vertex = mul(Translational(_Transformation, _Rotation, _Scale), v.vertex);
                o.vertex = UnityObjectToClipPos(v.vertex);

                float4 tcTransform = float4(1, 1, 0.5, 0.5);
                if (_IsRadial > 0)
                {
                    v.uv.xy = (o.vertex.xy + tcTransform.xy) * tcTransform.zw - _FocalPoint.xy;
                }
                else
                {
                    v.uv.xy = TRANSFORM_TEX(v.uv, _MainTex);
                }

                v.uv.zw = o.vertex.xy * float2(0.5, -0.5) + float2(0.5, 0.5);
                o.color = v.color;
                o.uv = v.uv;
                return o;
            }

            float4 CxForm(float4 color)
            {
                return clamp(color * _MulTerm + _AddTerm, 0, 1);
            }

            float4 FromLinear(float4 color)
            {
                return max(1.055 * pow(color, 0.416666667) - 0.055, 0);
            }

            float4 MaskPixel(float4 coords, float4 color)
            {
                float alpha = clamp(dot(tex2D(_Mask, coords.zw), _MaskChannels), 0, 1);
                return color * alpha;
            }

            fixed4 TextureFill(v2f i) : SV_Target
            {
               return CxForm(tex2D(_MainTex, i.uv.xy));
            }

            fixed4 TextureFill_LM(v2f i) : SV_Target
            {
                return MaskPixel(i.uv,FromLinear(TextureFill(i)));
            }
        ENDCG
        }
    }
}
