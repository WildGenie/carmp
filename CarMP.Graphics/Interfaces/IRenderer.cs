﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CarMP.Graphics.Geometry;

namespace CarMP.Graphics.Interfaces
{
    public interface IRenderer
    {
        /// <summary>
        /// Resize rendering area
        /// </summary>
        /// <param name="pSize"></param>
        void Resize(Size pSize);
        void BeginDraw();
        void EndDraw();

        void DrawRectangle(IBrush pBrush, Rectangle pRectangle, float pLineWidth);
        void DrawLine(Point pPoint1, Point pPoint2, IBrush pBrush, float pLineWidth);

        void FillRectangle(IBrush pBrush, Rectangle pRectangle);

        void DrawImage(Rectangle pRectangle, IImage pImage, float pAlpha);

        void DrawEllipse(Ellipse pEllipse, IBrush pBrush, float pLineWidth);

        void FillEllipse(Ellipse pEllipse, IBrush pBrush);

        void DrawString(Point pPoint, IStringLayout pStringLayout, IBrush pBrush);

        IBrush CreateBrush(Color pColor);

        IStringLayout CreateStringLayout();

        IImage CreateImage(byte[] pData, int pStride);
        IImage CreateImage(string pPath);
    }
}
