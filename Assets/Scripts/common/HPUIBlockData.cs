using ubco.ovilab.uxf.extensions;

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
