﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CarMP
{
    public class ChangeMediaListArgs : EventArgs
    {
        public int ListIndex { private set; get; }

        public ChangeMediaListArgs(int pListIndex)
        {
            ListIndex = pListIndex;
        }
    }
}
