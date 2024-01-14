using UnityEngine;
using ubco.ovilab.HPUI.Legacy;
using ubco.ovilab.hpuiInSituComparison.common;

namespace ubco.ovilab.hpuiInSituComparison.common
{
    public interface IPeg
    {
        public Transform trackingObject { get; set; }
        public int DisplayColorIndex { get; set; }
        public int DisplayColorGroupIndex { get; set; }
        public bool Visible { get; set; }
        public bool Active { get; set; }
        public float Scale { get; set; }
    }
}
