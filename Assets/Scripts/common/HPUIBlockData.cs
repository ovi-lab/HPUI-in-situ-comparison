using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using ubco.ovilab.uxf.extensions;
using UXF;
using ubco.ovilab.ViconUnityStream;
using UnityEngine;

namespace ubco.ovilab.hpuiInSituComparison.common
{
    public class HPUIBlockData: BlockData
    {
        public int numTrials;
        public string handedness;
        public bool changeLayout;

        public override string ToString()
        {
            return
                base.ToString() +
                $"Number of Trials: {numTrials}    " +
                $"Buttons used: {handedness}   " +
                $"Change Layout: {changeLayout}   ";
        }
    }
}
