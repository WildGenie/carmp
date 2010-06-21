﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;

using SlimDX;
using SlimDX.Direct2D;
using SlimDX.Windows;


namespace CarMp.ViewControls
{
    public class DragableListTextItem : DragableListItem
    {
        private SlimDX.DirectWrite.TextLayout StringLayout = null;
        private SlimDX.Direct2D.LinearGradientBrush LinearGradient = null;

        private static SlimDX.DirectWrite.TextFormat StringDrawFormat = null;
        public DragableListTextItem()
        {

            if (StringDrawFormat == null)
                StringDrawFormat = new SlimDX.DirectWrite.TextFormat(
                    Direct2D.StringFactory,
                    "Arial",
                    SlimDX.DirectWrite.FontWeight.Normal,
                    SlimDX.DirectWrite.FontStyle.Normal,
                    SlimDX.DirectWrite.FontStretch.Normal,
                    20F,
                    "en-us") {  TextAlignment = SlimDX.DirectWrite.TextAlignment.Leading,
                                WordWrapping = SlimDX.DirectWrite.WordWrapping.NoWrap };

        }

        /// <summary>
        /// String shown to the user
        /// </summary>
        public string DisplayString { get; set; }


        internal override void DrawItem(Direct2D.RenderTargetWrapper pRenderer, RectangleF pRectangle)
        {
            if(StringLayout == null)
            {
                StringLayout = new SlimDX.DirectWrite.TextLayout(Direct2D.StringFactory, DisplayString, StringDrawFormat, pRectangle.Width, pRectangle.Height);
            }

            if (LinearGradient == null)
            {
                LinearGradient = new SlimDX.Direct2D.LinearGradientBrush(pRenderer.Renderer,
                    new GradientStopCollection(pRenderer.Renderer, new GradientStop[] {
                    new GradientStop
                        {
                            Color = Color.Gray,
                            Position = 0
                        }
                        ,
                    new GradientStop
                        {
                            Color = Color.White,
                            Position = 1
                        }
                    }),
                        new LinearGradientBrushProperties()
                        {
                            StartPoint = new PointF(0, pRectangle.Top),
                            EndPoint = new PointF(0, pRectangle.Bottom)
                        }
                    );
            }

            LinearGradient.EndPoint = new PointF(0, pRectangle.Bottom);
            LinearGradient.StartPoint = new PointF(0, pRectangle.Top);

            pRenderer.DrawTextLayout(new PointF(pRectangle.Location.X + 4,pRectangle.Location.Y), StringLayout, LinearGradient);
            
            // Call base which will draw the selection is selected
            base.DrawItem(pRenderer, pRectangle);
        }
    }
}