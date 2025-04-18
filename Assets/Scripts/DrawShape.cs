using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UnityEngine;
using XnaVG;
using Vector4 = UnityEngine.Vector4;

public class DrawShape : MonoBehaviour {
    public Material[] materials;
    public MeshRenderer meshRenderer;
    public MeshFilter meshFilter;
    public VGMatrix PaintTransformation;
    public VGMatrix Projection;
    public VGMatrix Matrice;
    public Mesh mesh;
    public UnityEngine.Vector3[] vectors;
    private Material material;
    private Texture texture;
    private void Awake()
    {
        material = new Material(meshRenderer.material);
        //for (int i = 0; i < materials.Length; i++)
        //{
        //    materials[i]= new Material(materials[i]);
        //}
        meshRenderer.material = material;
    }
    // Use this for initialization
    void Start () {

        meshRenderer.sortingOrder = transform.GetSiblingIndex();
    }

    // Update is called once per frame
    void Update () {

    }

    public void SetShow(bool isShow=true)
    {
        if(!isShow)
            gameObject.SetActive(false);
        else
            gameObject.SetActive(true);

        IsShow = isShow;
    }

    public void SetDraw(Mesh mesh, VGState State,
        Texture2D texture = null, VGCxForm cxForm = null,
        bool isRadial = false, bool isLine = false,

        UnityEngine.Vector2 FocalPoint = default(UnityEngine.Vector2),
        UnityEngine.Color color = default(UnityEngine.Color), VGPaint paint=null)
    {
        meshFilter.mesh = mesh;
        this.mesh = mesh;
        vectors = mesh.vertices;
        //if (isRadial)
        //{
        //    material = materials[0];
        //}
        //else
        //{
        //    if (isLine)
        //        material = materials[0];
        //    else
        //        material = materials[0];
        //}

        if(paint!=null)
        {
            switch (paint.Type)
            {
                case VGPaintType.Color:
                    break;
                case VGPaintType.LinearGradient:
                    break;
                case VGPaintType.RadialGradient:
                    break;
                case VGPaintType.Pattern:
                    break;
                case VGPaintType.PatternPremultiplied:
                    break;
                default:
                    break;
            }
        }

        if (texture != null)
        {
            material.SetTexture("_MainTex", texture);
            material.SetFloat("_IsTex", 1);
        }
        else
        {
            material.SetTexture("_MainTex", null);
            material.SetFloat("_IsTex", 0);
        }
        material.SetFloat("_IsRadial", isRadial ? 1 : 0);
        material.SetFloat("_IsLine", isLine ? 1 : 0);
        material.SetVector("_FocalPoint", FocalPoint);

        material.SetVector("_Color", color);

        VGMatrix transformation = State.PathToSurface.Matrix;
        VGMatrix projection = State.Projection.Matrix;
        VGMatrix paintTransformation = State.PathToFillPaint.Matrix;

        Vector4 t = new Vector4(transformation.M31, transformation.M32, transformation.M33);
        Vector4 s = new Vector4(transformation.M11, transformation.M22, transformation.M33);
        Vector4 r = new Vector4(transformation.M21, transformation.M12);
        material.SetVector("_Transformation", t);
        material.SetVector("_Scale", s);
        material.SetVector("_Rotation", r);

        material.SetVector("_AddTerm", new Vector4(cxForm.AddTerm.X, cxForm.AddTerm.Y, cxForm.AddTerm.Z, cxForm.AddTerm.W));
        material.SetVector("_MulTerm", new Vector4(cxForm.MulTerm.X, cxForm.MulTerm.Y, cxForm.MulTerm.Z, cxForm.MulTerm.W));

        //Vector4 pt = new Vector4(projection.M31, projection.M32, projection.M33);
        //Vector4 ps = new Vector4(projection.M11, projection.M22, projection.M33);
        //Vector4 pr = new Vector4(projection.M21, projection.M12);
        //material.SetVector("_ProjectionT", pt);
        //material.SetVector("_ProjectionS", ps);
        //material.SetVector("_ProjectionR", pr);

        //Vector4 ptt = new Vector4(paintTransformation.M31, paintTransformation.M32, paintTransformation.M33);
        //Vector4 pts = new Vector4(paintTransformation.M11, paintTransformation.M22, paintTransformation.M33);
        //Vector4 ptr = new Vector4(paintTransformation.M21, paintTransformation.M12);
        //material.SetVector("_PaintTransformationT", ptt);
        //material.SetVector("_PaintTransformationS", pts);
        //material.SetVector("_PaintTransformationR", ptr);
        if (DrawGL.Instance.OpenStencil)
        {
            material.SetFloat("_StencilComp", (float)State.Stencils.Cover.StencilFunction);

            material.SetFloat("_Stencil", State.Stencils.Cover.ReferenceStencil);
            material.SetFloat("_StencilPass", (float)State.Stencils.Cover.StencilPass);
            material.SetFloat("_StencilFail", (float)State.Stencils.Cover.StencilFail);
            material.SetFloat("_StencilZFail", (float)State.Stencils.Cover.StencilDepthBufferFail);
            material.SetFloat("_StencilWriteMask", (float)State.Stencils.Set.StencilMask);
            material.SetFloat("_StencilReadMask", (float)State.Stencils.Cover.StencilMask);
        }
        else
        {
            material.SetFloat("_Stencil", 0);
        }
        material.SetFloat("_CullMode", (float)(State.RasterizerState.CullMode - 1));

        SetShow(true);
    }

    public bool IsShow;

    public void SetDrawMask(Texture2D texture, Microsoft.Xna.Framework.Vector4 maskChannels)
    {
        material.SetTexture("_Mask", texture);
        material.SetFloat("_IsMask", texture==null?0:1);
        material.SetVector("_MaskChannels", maskChannels.ToVector4());
    }

    internal void SetBlendState(Microsoft.Xna.Framework.Graphics.BlendState blendState)
    {
        material.SetInt("_BlendOp", (int)blendState.AlphaBlendFunction-1);
        material.SetInt("_SrcBlend", (int)blendState.AlphaSourceBlend-1);
        material.SetInt("_DstBlend", (int)blendState.AlphaDestinationBlend-1);
    }
}
