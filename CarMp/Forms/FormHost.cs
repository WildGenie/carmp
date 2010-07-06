﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CarMp.Views;
using SlimDX.Direct2D;
using System.Threading;
using CarMp.ViewControls;
using System.Xml;
using CarMp.Reactive.Touch;

namespace CarMp.Forms
{
    public partial class FormHost : Form
    {
        private ManualResetEvent _viewChanging;

        private DateTime _fpsCalcDate;
        private long _fpsCalcFramesTotal;
        private long _fpsCalcFramesCurrent;

        private D2DViewControl _mouseDownViewControl;

        private List<D2DViewControl> _overlayViewControls;

        private EventToTouchBridge _mouseEventProcessor;
        private Dictionary<string, D2DView> _loadedViews;
        private D2DView _currentView;

        private D2DViewFactory _viewFactory;
        private FormDebug _debugForm;

        private Direct2D.RenderTargetWrapper _renderTarget;

        public FormHost()
        {
            // Initialize & set Touch Observable
            _mouseEventProcessor = new EventToTouchBridge(this);
            D2DViewControl.SetTouchObservables(_mouseEventProcessor.ObservablTouchActions);

            _viewChanging = new ManualResetEvent(false);
            if (SessionSettings.Debug)
            {
                OpenDebugForm();
            }   
            
            _fpsCalcFramesTotal = 0;
            _fpsCalcDate = DateTime.Now;

            this.SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint, true);

            _renderTarget = new Direct2D.RenderTargetWrapper(new WindowRenderTarget(new Factory(), new WindowRenderTargetProperties()
            {
                Handle = this.Handle,
                PixelSize = this.ClientSize
            }));

            this.ClientSizeChanged += (o, e) => { _renderTarget.Resize(this.ClientSize); };
            
            InitializeComponent();

            this.Size = SessionSettings.ScreenResolution;
            this.Location = SessionSettings.WindowLocation;

            _overlayViewControls = new List<D2DViewControl>();
            _viewFactory = new D2DViewFactory(this.Size);
            _loadedViews = new Dictionary<string, D2DView>();

            InitializeOverlayControls();

            Action renderingLoop = new Action(() => RenderingLoop());
            renderingLoop.BeginInvoke(null, null);
        }

        private void InitializeOverlayControls()
        {
            MediaControlBar controlBar = null;
            MediaInfoBar infoBar = null;
            XmlNode infoBarNode = SessionSettings.CurrentSkin.GetOverlayNodeSkin("MediaInfoBar");
            XmlNode controlBarNode = SessionSettings.CurrentSkin.GetOverlayNodeSkin("MediaControlBar");

            if (controlBarNode != null)
            {
                controlBar = new MediaControlBar();
                controlBar.ApplySkin(controlBarNode, SessionSettings.SkinPath);
                _overlayViewControls.Add(controlBar);
                controlBar.StartRender();
            }

            if (infoBarNode != null)
            {
                infoBar = new MediaInfoBar();
                infoBar.ApplySkin(infoBarNode, SessionSettings.SkinPath);
                _overlayViewControls.Add(infoBar);
                infoBar.StartRender();
            }

            GraphicalButton toggleAnimation = new GraphicalButton();
            toggleAnimation.Bounds = new RectangleF(450, 30, 64, 64);
            toggleAnimation.SetButtonUpBitmapData(@"C:\source\CarMp\trunk\Images\Skins\BMW\Box.bmp");
            int i = 0;

            toggleAnimation.Click += (sender, e) =>
                {
                    controlBar.SetAnimation(i);
                    controlBar.StartAnimation();
                    infoBar.SetAnimation(i);
                    infoBar.StartAnimation();
                    if(i > 0)
                        i = -1 ;
                    else i = 1;
                };

            toggleAnimation.StartRender();
            _overlayViewControls.Add(toggleAnimation);
        }

        public D2DView ShowView(string pViewName)
        {
            _viewChanging.Reset();
            try
            {
                if (_currentView != null)
                {
                    // View is showing, hide this view
                }

                if (_loadedViews.ContainsKey(pViewName))
                    _currentView = _loadedViews[pViewName];
                else
                {
                    // Create view
                    _currentView = _viewFactory.CreateView(pViewName);

                    _loadedViews.Add(pViewName, _currentView);
                    if (_currentView is ISkinable)
                    {
                        System.Xml.XmlNode viewSkinNode = SessionSettings.CurrentSkin.GetViewNodeSkin(pViewName);

                        if (viewSkinNode != null)
                            (_currentView as ISkinable).ApplySkin(viewSkinNode, SessionSettings.SkinPath);
                    }
                    _currentView.StartRender();
                }
            }
            finally
            {
                _viewChanging.Set();
            }

            
            return _currentView;
        }

        public void ApplySkin()
        {
            foreach (KeyValuePair<string, D2DView> kv in _loadedViews)
            {
                D2DView view = kv.Value;
                if (view is ISkinable)
                {
                    System.Xml.XmlNode viewSkinNode = SessionSettings.CurrentSkin.GetViewNodeSkin(view.Name);

                    if (viewSkinNode != null)
                        (view as ISkinable).ApplySkin(viewSkinNode, SessionSettings.SkinPath);
                }
            }
        }

        private void OpenDebugForm()
        {
            _debugForm = new FormDebug();
            
            DebugHandler.De += new DebugHandler.DebugException(ex => _debugForm.WriteException(ex));
            DebugHandler.Ds += new DebugHandler.DebugString(str => _debugForm.WriteText(str));

            _debugForm.Location =
                new Point(
                    Screen.PrimaryScreen.Bounds.Width - _debugForm.Width - 20,
                    Screen.PrimaryScreen.Bounds.Height - _debugForm.Height - 40
                    );

            _debugForm.StartPosition = FormStartPosition.Manual;
            _debugForm.Show();
        }

        // OnPaint implmentation
        //protected override void OnPaint(PaintEventArgs e)
        //{
        //    DrawDirect2D();
        //    base.OnPaint(e);
        //}

        private void RenderingLoop()
        {
            while (true)
            {
                _viewChanging.WaitOne();

                if(_currentView != null)
                    DrawDirect2D();
                
                _fpsCalcFramesTotal++;
                if ( DateTime.Now.AddSeconds(-1) > _fpsCalcDate)
                {
                    //System.Diagnostics.Debug.WriteLine("CurrentFPS: " + (_fpsCalcFramesTotal - _fpsCalcFramesCurrent).ToString());
                    _fpsCalcFramesCurrent = _fpsCalcFramesTotal;
                    _fpsCalcDate = DateTime.Now;
                }

                System.Threading.Thread.Sleep(10);
            }
        }

        private void DrawDirect2D()
        {
             if (!_renderTarget.IsOccluded)
             {
                 _renderTarget.BeginDraw();
                 _renderTarget.Transform = Matrix3x2.Identity;
                 _renderTarget.Clear(Color.Black);

                 _currentView.Render(_renderTarget);
                 
                 for (int i = _overlayViewControls.Count - 1;
                     i >= 0;
                     i--)
                 {
                     _overlayViewControls[i].Render(_renderTarget);
                 }

                 _renderTarget.EndDraw();

                 this.Invalidate();
             }
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
        }

        //protected override void OnMouseDown(MouseEventArgs e)
        //{
        //    for (int i = _overlayViewControls.Count - 1;
        //        i >= 0;
        //        i--)
        //    {
        //        _mouseDownViewControl = _overlayViewControls[i].GetViewControlContainingPoint(e.Location);
        //        if (_mouseDownViewControl != null)
        //        {
        //            _mouseDownViewControl.MouseDown(e);
        //            return;
        //        }
        //    }
        //    if (_currentView is D2DView)
        //    {
        //        _mouseDownViewControl = _currentView.GetViewControlContainingPoint(e.Location);
        //        if(_mouseDownViewControl != null)
        //            _mouseDownViewControl.MouseDown(e);
        //    }
        //}

        //protected override void OnMouseMove(MouseEventArgs e)
        //{
        //    return;
        //    EventQueue eventQueue = new EventQueue();

        //    if (_mouseDownViewControl != null)
        //        eventQueue.AddToQueue(new EventQueueEntry(new EventQueueDelegate<MouseEventArgs>(_mouseDownViewControl.MouseMove), e));

        //    for (int i = _overlayViewControls.Count - 1;
        //        i >= 0;
        //        i--)
        //    {
        //        if (_mouseDownViewControl != null &&
        //            _overlayViewControls[i] == _mouseDownViewControl)
        //            continue;

        //        if (_overlayViewControls[i].Bounds.Contains(e.Location))
        //        {
        //            eventQueue.AddToQueue(new EventQueueEntry(new EventQueueDelegate<MouseEventArgs>(_overlayViewControls[i].MouseMove), e));
        //            return;
        //        }
        //    }
        //    if (_currentView is D2DView)
        //    {
        //        D2DViewControl control = _currentView.GetViewControlContainingPoint(e.Location);
        //        if (control != null && _mouseDownViewControl != control)
        //            eventQueue.AddToQueue(new EventQueueEntry(new EventQueueDelegate<MouseEventArgs>(control.MouseMove), e));
        //    }

        //    eventQueue.ProcessQueue();
        //}

        //protected override void OnMouseUp(MouseEventArgs e)
        //{
        //    EventQueue queue = new EventQueue();

        //    if (_mouseDownViewControl != null)
        //        queue.AddToQueue(new EventQueueEntry(new EventQueueDelegate<MouseEventArgs>(_mouseDownViewControl.MouseUp), e));
            
        //    for (int i = _overlayViewControls.Count - 1;
        //        i >= 0;
        //        i--)
        //    {

        //        if (_overlayViewControls[i].Bounds.Contains(e.Location))
        //        {
        //            if (_mouseDownViewControl != null
        //                && _overlayViewControls[i] == _mouseDownViewControl)
        //            {
        //                continue;
        //            }

        //            int j = i;
        //            queue.AddToQueue(new EventQueueEntry(new EventQueueDelegate<MouseEventArgs>(_overlayViewControls[j].MouseUp), e));
        //            break;
        //        }
        //    }
        //    if (_currentView is D2DView)
        //    {
        //        D2DViewControl control = _currentView.GetViewControlContainingPoint(e.Location);
        //        if (control != null && _mouseDownViewControl != control)
        //            queue.AddToQueue(new EventQueueEntry(new EventQueueDelegate<MouseEventArgs>(control.MouseUp), e));
        //    }
        //    _mouseDownViewControl = null;

        //    queue.ProcessQueue();
        //}

        protected override void OnSizeChanged(EventArgs e)
        {
            if (_currentView is D2DView)
            {
                (_currentView as D2DView).OnSizeChanged(this, e);
            }
        }
    }
}
