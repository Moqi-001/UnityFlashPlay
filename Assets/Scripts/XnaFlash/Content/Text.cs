using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Unity.VectorGraphics;
using XnaFlash.Movie;
using XnaFlash.Swf;
using XnaFlash.Swf.Tags;
using XnaVG;
using XnaVG.Loaders;
using XnaVG.Paints;
using static XnaFlash.Swf.Paths.Shape;

namespace XnaFlash.Content
{
    public class Text : ICharacter, Movie.IDrawable
    {
        public ushort ID { get; private set; }
        public VGMatrix Matrix { get; private set; }
        public VGPreparedPath[] TextParts { get; private set; }
        List<Unity.VectorGraphics.Shape> shapes;
        public Rectangle? Bounds { get; private set; }
        public CharacterType Type { get { return CharacterType.Text; } }
        private List<SubShape> _subShapes = new List<SubShape>();

        public Text(DefineTextTag tag, ISystemServices services, FlashDocument document)
        {
            ID = tag.CharacterID;
            Matrix = tag.Matrix;
            Bounds = tag.Bounds;

            var path = new VGPath();
            var parts = new List<VGPreparedPath>();
            var scale = Vector2.One;
            shapes = new List<Unity.VectorGraphics.Shape>();

            var leftTop = new Vector2(tag.Bounds.Left, tag.Bounds.Top);
            Font font = null;
            ushort? lastFont = null;
            VGColor? lastColor = null;

            foreach (var rec in tag.TextRecords)
            {
                if ((rec.HasFont && lastFont != rec.FontId) || (rec.HasColor && lastColor != rec.Color))
                {
                    if (!path.IsEmpty && lastFont.HasValue && lastColor.HasValue)
                    {
                        var pp = services.VectorDevice.PreparePath(path, VGPaintMode.Fill);
                        pp.Tag = services.VectorDevice.CreateColorPaint(lastColor.Value);
                        parts.Add(pp);
                    }

                    path = new VGPath();
                }

                if (rec.HasColor) lastColor = rec.Color;
                if (rec.HasFont)
                {
                    font = document[rec.FontId] as Font;
                    scale = new Vector2(rec.FontSize, rec.FontSize);
                    if (font != null) lastFont = rec.FontId;
                }

                if (font == null || !lastColor.HasValue || rec.Glyphs.Length == 0)
                    continue;

                var offset = new Vector2(rec.HasXOffset ? rec.XOffset : 0, rec.HasYOffset ? rec.YOffset : 0);
                var refPt = Vector2.Zero;
                if (rec.Glyphs[0].GlyphIndex < font.GlyphFont.Length)
                    refPt = font.GlyphFont[rec.Glyphs[0].GlyphIndex].ReferencePoint * scale;
                var xoff = Vector2.Zero;

                foreach (var g in rec.Glyphs)
                {
                    if (g.GlyphIndex >= font.GlyphFont.Length) continue;

                    var fg = font.GlyphFont[g.GlyphIndex];
                    if (fg.GlyphPath == null) continue;

                    var rpt = fg.ReferencePoint.X * scale.X;

                    var p = fg.GlyphPath.Clone();
                    p.Scale(scale);
                    Vector2 NewOffset = offset + xoff;

                    p.Offset(offset + xoff);
                    path.Append(p);
                    if (tag.Version>6)
                    {
                        scale = DefineFontTag.EMSquareInv * rec.FontSize / 20f;
                        //scale = DefineFontTag.EMSquareInv * rec.FontSize;
                    }
                    else
                        scale = DefineFontTag.EMSquareInv * rec.FontSize;
                    if (fg.SubShape!=null)
                    {
                        _subShapes.Add(fg.SubShape);
                        for (int i = 0; i < fg.SubShape.shapeParser.shapes.Count; i++)
                        {
                            //Matrix2D matrix2D = fg.SubShape.shapeParser.shapes[i].FillTransform;
                            Matrix2D matrix2D = Matrix2D.identity;
                            matrix2D.m00 = scale.X;
                            matrix2D.m11 = scale.Y;
                            matrix2D.m02 = NewOffset.X;
                            matrix2D.m12 = NewOffset.Y;
                            Unity.VectorGraphics.Shape shape = new Unity.VectorGraphics.Shape();
                            Unity.VectorGraphics.Shape shape1 = fg.SubShape.shapeParser.shapes[i];
                            shape.Fill = shape1.Fill;
                            ((SolidFill)shape.Fill).Color = rec.Color.ToColor();
                            shape.Contours = shape1.Contours;
                            shape.FillTransform = matrix2D;
                            shape.PathProps = shape1.PathProps;
                            shape.IsConvex = shape1.IsConvex;
                            shapes.Add(shape);
                            //for (int j = 0; j < fg.SubShape.shapeParser.shapes[i].Contours.Length; j++)
                            //{
                            //    for (int s = 0; s < fg.SubShape.shapeParser.shapes[i].Contours[j].Segments.Length; s++)
                            //    {
                            //        BezierPathSegment segments = fg.SubShape.shapeParser.shapes[i].Contours[j].Segments[s];
                            //        segments.P0 = segments.P0.ScaleAndOffset(scale, NewOffset);
                            //        segments.P1 = segments.P1.ScaleAndOffset(scale, NewOffset);
                            //        segments.P2 = segments.P2.ScaleAndOffset(scale, NewOffset);

                            //        fg.SubShape.shapeParser.shapes[i].Contours[j].Segments[s] = segments;

                            //        //if (rec.HasColor)
                            //        {
                            //            ((SolidFill)fg.SubShape.shapeParser.shapes[i].Fill).Color = rec.Color.ToColor();
                            //        }

                            //    }
                            //}
                        }
                    }
                    xoff.X += g.GlyphAdvance;

                }
            }
            if(DrawGL.ins.isNewMeshMake)
            {

            }
            else
            {
                if (!path.IsEmpty && lastFont.HasValue && lastColor.HasValue)
                {
                    var pp = services.VectorDevice.PreparePath(path, VGPaintMode.Fill);
                    pp.Tag = services.VectorDevice.CreateColorPaint(lastColor.Value);
                    parts.Add(pp);
                }
            }
            

            TextParts = parts.ToArray();

            
        }
        public List<UnityEngine.Mesh> meshs=new List<UnityEngine.Mesh>();

        public Movie.IDrawable MakeInstance(Movie.DisplayObject container, RootMovieClip root) { return this; }
        public void Draw(IVGRenderContext<Movie.DisplayState> target) 
        {
            target.State.PathToSurface.PushCombineLeft(Matrix);
            foreach (var part in TextParts)
            {
                target.State.SetFillPaint(part.Tag as VGPaint);
                target.DrawPath(part, VGPaintMode.Fill);
            }
            var state = target.State;

                if (DrawGL.ins.isNewMeshMake)
                {

                    //DrawGL.ins.SetMatrices(state.PathToSurface.Matrix, state.Projection.Matrix, state.PathToFillPaint.Matrix);
                    //for (int index = 0; index < shape.shapeParser.shapes.Count; index++)
                    for (int index = 0; index < shapes.Count; index++)
                    {
                        Texture2D texture = null;
                        bool isSolidFill = false;
                        bool isRadial = false;
                        if (shapes[index].Fill is SolidFill)
                        {
                            isSolidFill = true;
                            //break;
                        }
                        else
                        {
                            

                        }
                        if (meshs.Count <= index)
                        {
                            meshs.Add(DrawGL.ins.SetMesh(shapes[index], texture));
                        }
                        var cxForm = state.ColorTransformationEnabled ? state.ColorTransformation.CxForm : VGCxForm.Identity;
                        DrawGL.ins.SetDrawShape(meshs[index], state.PathToSurface.Matrix, state.Projection.Matrix, 
                            state.PathToTextPaint.Matrix, texture, cxForm, isRadial,UnityEngine. Vector2.zero, ((SolidFill)shapes[index].Fill).Color);
                        DrawGL.ins.SetBlendState(XnaVG.Rendering.States.BlendStates.BlendStatesIns.GetBlendState(state.BlendMode, state.ColorChannels));
                    }
                }
                else
                {
                   
                }

            
            target.State.PathToSurface.Pop();

        }
        StageObject Parent;
        public void SetParent(StageObject parent) { Parent = parent; }
        public void OnNextFrame() { }

        public void Dispose()
        {
            if (TextParts != null)
            {
                foreach (var part in TextParts)
                {
                    (part.Tag as IDisposable).Dispose();
                    part.Dispose();
                }
                TextParts = null;
            }
        }
    }
}
