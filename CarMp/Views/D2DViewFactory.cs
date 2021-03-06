﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CarMP.Graphics.Geometry;

namespace CarMP.Views
{
    public class D2DViewFactory
    {
        public const string MEDIA = "Media";
        public const string HOME = "Home";
        public const string NAVIGATION = "Navigation";
        public const string OPTIONS = "Options";
        public const string OVERLAY = "Overlay";

        private Size _windowSize;

        public D2DViewFactory(Size pWindowSize)
        {
            _windowSize = pWindowSize;
        }

        public D2DView CreateView(string pViewName)
        {
            D2DView newView = null;

            switch (pViewName)
            {
                case MEDIA:
                    newView = new MediaView(_windowSize);
                    break;
                case HOME:
                    newView = new HomeView(_windowSize);
                    break;
                case NAVIGATION:
                    break;
                case OPTIONS:
                    newView = new OptionsView(_windowSize);
                    break;

                case OVERLAY:
                    newView = new OverlayView(_windowSize);
                    break;

                default:
                    throw new Exception("Attempt to create an invalid form: " + pViewName);
            }

            return newView;
        }
    }
}
