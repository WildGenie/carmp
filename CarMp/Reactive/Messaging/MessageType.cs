﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CarMP.Reactive.Messaging
{
    public enum MessageType
    {
        Trigger,
        SwitchView, 
        MediaChange,
        MediaProgress,
        MediaStateChange,
        MediaHistoryChange
    }
}
