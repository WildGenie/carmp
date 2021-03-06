﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CarMP.Callbacks
{
    public delegate void Win32Messenger(ref Message pMessage);
    public delegate void MediaUpdate(List<MediaItem> MInfo);

    public delegate bool FileCheck(String FileName, Int32 FileSize);
    
    public delegate void ChangeMediaListHandler(object sender, ChangeMediaListArgs pEventArgs);
    public delegate void MediaChangedHandler(object sender, MediaChangedArgs pMediaArgs);
    public delegate void MediaProgressChangedHandler(object sender, MediaProgressChangedArgs pMediaArgs);
    
    public delegate void FinishHandler(object sender, FinishEventArgs pFinishEventArgs);
    public delegate void ProgressDelegate(object sender, ProgressEventArgs pEventArgs);

    public delegate void D2DInvalidateHandler(object sender, EventArgs e);
    
}
