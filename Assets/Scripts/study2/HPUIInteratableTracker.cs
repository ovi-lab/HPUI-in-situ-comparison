using System.Collections.Generic;
using UXF;
using System.Linq;
using ubco.ovilab.HPUI.Tracking;
using ubco.ovilab.HPUI.Interaction;
using UnityEngine;

namespace ubco.ovilab.hpuiInSituComparison.study2
{
    /// <summary>
    /// Extending the PositionRotationTracker to also include data from an HPUIInteratable.
    /// </summary>
    public class HPUIInteratableTracker : PositionRotationTracker
    {
        public override string MeasurementDescriptor => "HPUIInteratable";
        public override IEnumerable<string> CustomHeader => base.CustomHeader.Union(new string[] {
                "parentName", "interactableName",
                "contactPoint_x", "contactPoint_y",
                "boundsMax_x", "boundsMax_y",
                "boundsMin_x", "boundsMin_y",
            }).ToArray();

        public string ParentName { get => parentName; set => parentName = value; }

        JointFollower jointFollower;
        IHPUIInteractable interactable;
        string parentName, interactableName;

        void Start()
        {
            jointFollower = GetComponent<JointFollower>();
            interactable = GetComponent<IHPUIInteractable>();
            interactableName = interactable.transform.name;
        }

        /// <summary>
        /// Returnns the data collected from the contactZone object
        /// </summary>
        /// <returns></returns>
        protected override UXFDataRow GetCurrentValues()
        {
            UXFDataRow data = base.GetCurrentValues();

            IHPUIInteractor interactor = interactable.interactorsSelecting.Select(i => i as IHPUIInteractor).FirstOrDefault(i => i != null);
            Vector2 contactPoint;
            if (interactor != null)
            {
                contactPoint = interactable.ComputeInteractorPostion(interactor);
            }
            else
            {
                contactPoint = Vector2.zero;
            }

            Vector2 boundsMax = interactable.boundsMax;
            Vector2 boundsMin = interactable.boundsMin;

            UXFDataRow newData =  new UXFDataRow()
            {
                ("parentName", parentName),
                ("interactableName", interactableName),
                ("contactPoint_x", contactPoint.x),
                ("contactPoint_y", contactPoint.y),
                ("boundsMax_x", boundsMax.x),
                ("boundsMax_y", boundsMax.y),
                ("boundsMin_x", boundsMin.x),
                ("boundsMin_y", boundsMin.y)
            };
            data.AddRange(newData);
            return data;
        }
    }
}
