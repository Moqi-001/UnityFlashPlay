using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VectorGraphics;
using XnaFlash.Swf.Structures;
using static XnaFlash.Swf.Structures.FillStyle;
using XnaVG;
using XnaFlash.Swf.Structures.Gradients;
using XnaFlash.Swf.Paths;
using XnaVG.Rendering.Tesselation;
//using Microsoft.Xna.Framework;
//using SwfLib;
//using SwfLib.Tags.ShapeTags;
//using SwfLib.Shapes.Records;
//using SwfLib.Shapes.FillStyles;

namespace Unity.Flash
{
	public class ShapeParser
	{
		int m_X;
		int m_Y;
		int m_ID;
		int m_Style0;
		int m_Style1;
		Rect m_Bounds;
        //List<Style> m_AllStyles;
        List<Style> m_AllStyles;
        Dictionary<int, Style> m_Styles;
        public List<Mesh> mesh;
        public Mesh oneMesh;

        public ShapeParser()
        {
			Init();

		}

        //Style fillStyle0 => m_Styles[m_Style0];
        //Style fillStyle0;
        //Style fillStyle1;
        Style fillStyle0
        {
            get
            {
                if(m_Styles.Count<=m_Style0)
                {

                }
                return m_Styles[m_Style0];
            }
        }
        Style fillStyle1
        {
            get
            {
                return m_Styles[m_Style1];
            }
        }

        public void Init()
		{
			//m_ID = tag.ShapeID;
			//m_Bounds = new Rect(X,Y,X ,Y);
			m_X = 0;
			m_Y = 0;
			m_Style0 = 0;
			m_Style1 = 0;
            m_Styles = new Dictionary<int, Style>();
            //FillStyle fillStyle = new FillStyle();
            //m_Styles.Add(0, new Style(DefaultFill(fillStyle), fillStyle));
            m_Styles.Add(0, null);
            m_AllStyles = new List<Style>();
            mesh = new List<Mesh>();
            ParseFillStyles(ShapeInfo.fillStyleArrays.Styles);
            //ParseShapeRecords(ShapeRecords);
            //Tessellate();
        }



		public static IFill DefaultFill(FillStyle fillStyle)
		{
            switch (fillStyle.FillType)
            {
                case FillStyleType.Solid:
                    return new SolidFill()
                    {
                       Color = fillStyle.Color.ToColor(),
                       Mode = FillMode.NonZero,
                       Opacity = fillStyle.Color.A / 255f
                    };

                case FillStyleType.Linear:
                    return new GradientFill()
                    {
                        Type = GradientFillType.Linear,
                        Stops = new GradientStop[2]
                                            {
                            new GradientStop() { Color =fillStyle.Gradient.GradientStops[0].Color.ToColor(), StopPercentage =fillStyle.Gradient.GradientStops[0].Ratio/255f},
                            new GradientStop() { Color =fillStyle.Gradient.GradientStops[1].Color.ToColor(), StopPercentage =fillStyle.Gradient.GradientStops[1].Ratio/255f},

                                            },
                        Mode = FillMode.NonZero,
                        Opacity = 1,
                        Addressing = AddressMode.Clamp

            };

                case FillStyleType.Radial:
                    
                case FillStyleType.Focal:
                    GradientFill gradientFill = new GradientFill();
                    
                    gradientFill.Type = GradientFillType.Radial;
                    gradientFill.Stops = new GradientStop[fillStyle.Gradient.GradientStops.Length];
                    int index = 0;
                    foreach (var item in fillStyle.Gradient.GradientStops)
                    {
                        gradientFill.Stops[index] =
                            new GradientStop()
                            {
                                Color = fillStyle.Gradient.GradientStops[index].Color.ToColor(),
                                StopPercentage = fillStyle.Gradient.GradientStops[index].Ratio / 255f
                            };
                        index++;
                    }

                    gradientFill.Mode = FillMode.NonZero;
                    gradientFill.Opacity = 1;
                    switch (fillStyle.Gradient.PadMode)
                    {
                        case GradientInfo.Padding.Pad:
                            gradientFill.Addressing = AddressMode.Clamp;
                            break;
                        case GradientInfo.Padding.Reflect:
                            gradientFill.Addressing = AddressMode.Mirror;
                            break;
                        case GradientInfo.Padding.Repeat:
                            gradientFill.Addressing = AddressMode.Wrap;
                            break;
                    }

                    return gradientFill;

                case FillStyleType.RepeatingBitmap:
        
                case FillStyleType.ClippedBitmap:
        
                case FillStyleType.RepeatingNonsmoothedBitmap:
       
                case FillStyleType.ClippedNonsmoothedBitmap:

					return new TextureFill()
					{
						Mode = FillMode.NonZero,
					
					};
				default:
					return new SolidFill()
					{
						Color = Color.white,
						Mode = FillMode.NonZero,
						Opacity = 1
					};
            }
           
		}

		// Parse Styles RGBA

		public void ParseFillStyles(IList<FillStyle> fillStyles)
		{
			foreach (var fillStyle in fillStyles)
			{
                m_Styles.Add(m_Styles.Count, new Style(DefaultFill(fillStyle), fillStyle));
			}
			
		}

		public void AddParseFillStyles(FillStyle fillStyle)
		{
		   m_Styles.Add(m_Styles.Count, Parse(fillStyle));
		}

		Style Parse(FillStyle fillStyle)
		{
			var fill = new SolidFill();
            fill.Color = fillStyle.Color.ToColor();
			fill.Mode = FillMode.NonZero;
			fill.Opacity = fill.Color.a;
			return new Style(fill, fillStyle);
		}


		//void Parse(StraightEdgeShapeRecord tag)
		public void Parse(int DeltaX,int DeltaY)
		{
			var begin = new Vector2(m_X, m_Y);
			var end = begin + new Vector2(DeltaX, DeltaY);

			fillStyle0?.Add0(begin, begin, end, end);
			fillStyle1?.Add1(begin, begin, end, end);

			m_X += DeltaX;
			m_Y += DeltaY;
		}

		//void Parse(CurvedEdgeShapeRecord tag)
		public void Parse(int ControlDeltaX,int ControlDeltaY,int AnchorDeltaX,int AnchorDeltaY)
		{
			var begin = new Vector2(m_X, m_Y);
			var control = begin + new Vector2(ControlDeltaX, ControlDeltaY);
			var end = control + new Vector2(AnchorDeltaX, AnchorDeltaY);

            //fillStyle0?.Add0(begin, begin, end, end);
            //fillStyle1?.Add1(begin, begin, end, end);
            fillStyle0?.Add0(begin, control, end, end);
            fillStyle1?.Add1(begin, control, end, end);

            m_X += ControlDeltaX + AnchorDeltaX;
			m_Y += ControlDeltaY + AnchorDeltaY;
		}

		public void ParseClose()
		{
			fillStyle0?.Close();
			fillStyle1?.Close();
            m_AllStyles.AddRange(m_Styles.Values);
            m_Styles.Clear();
		}

        // Parse Shape Records RGBA

        //void ParseShapeRecords(IList<IShapeRecordRGBA> records)
        //{
        //	foreach (var record in records)
        //		if (record is StyleChangeShapeRecordRGBA)
        //			Parse(record as StyleChangeShapeRecordRGBA);
        //		else if (record is StraightEdgeShapeRecord)
        //			Parse(record as StraightEdgeShapeRecord);
        //		else if (record is CurvedEdgeShapeRecord)
        //			Parse(record as CurvedEdgeShapeRecord);
        //		else
        //			Parse(record as EndShapeRecord);
        //}

        //void Parse(StyleChangeShapeRecordRGBA tag)

        public void Parse(bool NewLineStyle, bool FillStyle0HasValue, bool FillStyle1HasValue, int MoveDeltaX, int MoveDeltaY, bool StateMoveTo, bool StateNewStyles, FillStyle FillStyle0Value, FillStyle FillStyle1Value, ShapeState State)
        {
            if (StateMoveTo || FillStyle0HasValue)
                fillStyle0?.Close();

            if (StateMoveTo || FillStyle1HasValue)
                fillStyle1?.Close();

            //if (FillStyle0HasValue || FillStyle1HasValue||StateMoveTo)
            //{
            //    fillStyle0?.Close();
            //    fillStyle1?.Close();
            //}

            if (StateMoveTo)
            {
                m_X = MoveDeltaX;
                m_Y = MoveDeltaY;

            }


            if (StateNewStyles)
                UpdateFillStyles(State.GetFillStyles());

            if (FillStyle0HasValue)
            {
                if (FillStyle0Value != null)
                {
                    m_Style0 = (int)FillStyle0Value.Index;
                }
                else m_Style0 = 0;
            }


            if (StateMoveTo || FillStyle0HasValue)
                fillStyle0?.New();

            if (FillStyle1HasValue)
            {
                if (FillStyle1Value != null)
                {
                    m_Style1 = (int)FillStyle1Value.Index;
                }
                else m_Style1 = 0;
            }


            if (StateMoveTo || FillStyle1HasValue)
                fillStyle1?.New();
        }


        // Style Override


        void UpdateFillStyles(FillStyle[] Styles=null)
		{
            m_AllStyles.AddRange(m_Styles.Values);
            //Dictionary<int, Style> styles = new Dictionary<int, Style>();
            //styles.Add(0, null);
            //foreach (var item in m_Styles)
            //{
            //    if (item.Value != null)
            //        styles.Add(styles.Count, new Style(DefaultFill(item.Value .FillStyle), item.Value.FillStyle));
            //}
            //m_Styles = styles;
            m_Styles.Clear();
            m_Styles.Add(0, null);
            ParseFillStyles(Styles);
		}

		// Tesselation
		static int num;
        public void Tessellate(Dictionary<FillStyle, PathBuilder> Fills)
        {
            List<Unity.VectorGraphics.Shape> shapes = new List<Unity.VectorGraphics.Shape>();
            List<FillStyle> fillStyles = new List<FillStyle>(Fills.Keys);
            foreach (var fill in Fills)
            {
                PathBuilder s = fill.Value;
                s.Flush();
                Unity.VectorGraphics.Shape shape = new Unity.VectorGraphics.Shape();
                List<BezierContour> Contours = new List<BezierContour>();
                List<BezierPathSegment> Segments = new List<BezierPathSegment>();
                //foreach (var item in s.Edges)
                //{
                //    Segments.Add(new BezierPathSegment()
                //    {
                //        P0 = new Vector2(item.From.X, item.From.Y),
                //        P1 = new Vector2(item.From.X+item.Ctl.X,item.From.Y+item.Ctl.Y),
                //        P2 = new Vector2(item.To.X, item.To.Y),
                //    });
                //}
                int i=0;
                Vector2 pos=Vector2.zero;
                Vector2 posLast = Vector2.zero;
                Vector2 posControls = Vector2.zero;
                foreach (var item in s.Segments)
                {
                    pos = new Vector2(item.Target.X, item.Target.Y);
                    if (item.Controls != null)
                    {
                        posControls = new Vector2(item.Controls[0].X, item.Controls[0].Y);
                    }
                    else
                    {
                        posControls = posLast;
                    }
                    if (pos != Vector2.zero && posLast != Vector2.zero)
                    {
                       
                        if (item.Controls != null)
                        {
                            BezierPathSegment bezierPath = new BezierPathSegment();
                            bezierPath.P0 = posLast;
                            bezierPath.P1 = posControls;
                            bezierPath.P2 = pos;
                            Segments.Add(bezierPath);
                        }
                        else
                        {
                            //BezierPathSegment[] bezierPathSegments = VectorUtils.BezierSegmentToPath( VectorUtils.MakeLine(posLast, pos));
                            //Segments.AddRange(bezierPathSegments);
                            BezierSegment bezierSegment= VectorUtils.MakeLine(posLast, pos);
                            BezierPathSegment bezierPath = new BezierPathSegment();
                            bezierPath.P0 = bezierSegment.P0;
                            bezierPath.P1 = bezierSegment.P1;
                            bezierPath.P2 = bezierSegment.P2;
                            Segments.Add(bezierPath);
                        }
                    }
                    i++;
                    posLast = pos;
                }
                var closed = Segments[0].P0 == Segments[Segments.Count - 1].P2;
                if (!closed)
                    Segments.Add(new BezierPathSegment() {
                        P0 = Segments[Segments.Count - 1].P2,
                        P1 = Segments[Segments.Count - 1].P1,
                        P2 = Segments[Segments.Count - 1].P0,
                    });


                Contours.Add(new BezierContour() { Segments = Segments.ToArray(), Closed = closed });


                shape.Contours = Contours.ToArray();
                shape.Fill = DefaultFill(fill.Key);
                shape.FillTransform = Matrix2D.identity;
                //shape.PathProps = default(PathProperties);
                Stroke Stroke = new Stroke();
                Stroke.Color = new Color(240.0f / 255.0f, 248.0f / 255.0f, 255.0f / 255.0f);
                shape.PathProps = new PathProperties() { Stroke = Stroke, Head = PathEnding.Round, Tail = PathEnding.Square, Corners = PathCorner.Beveled };
                shape.IsConvex = false;
                shapes.Add(shape);
            }
           
            // Create Shape List
            this.shapes = shapes;
            FillStyles = fillStyles;
            Paint = new List<VGPaint>();
            // Create Scene Node

        }
        public void Tessellate()
		{
			// Create Shape List
			shapes = new List<Unity.VectorGraphics.Shape>(m_AllStyles.Count);
            FillStyles= new List<FillStyle>(m_AllStyles.Count-1);
            Paint = new List<VGPaint>();
            for (int c = 0; c <  m_AllStyles.Count; c++)
            {
                if (m_AllStyles[c] != null)
                {
                    if (m_AllStyles[c].GetCan())
                    {
                        shapes.Add(m_AllStyles[c].ToShape());
                        FillStyles.Add(m_AllStyles[c].FillStyle);
                    }
                }
            }
			// Create Scene Node
			
		}
		public List<Unity.VectorGraphics.Shape> shapes;
        public IList<FillStyle> FillStyles;
        public List<VGPaint> Paint;

    }
}