using System.Collections.Generic;
using UXF;
using System.Linq;
using ubco.ovilab.HPUI.Legacy;
using UnityEngine;

namespace ubco.ovilab.hpuiInSituComparison.study1
{
    /// <summary>
    /// Extending the PositionRotationTracker to also include data from the button controller.
    /// </summary>
    public class ButtonControllerTracker : PositionRotationTracker
    {
        public override string MeasurementDescriptor => "buttonController";
        public override IEnumerable<string> CustomHeader => base.CustomHeader.Union(new string[] {
                "fingerName", "buttonName",
                "zonePos_x", "zonePos_y", "zonePos_z",
                "scale_x", "scale_y", "scale_z",
                "forward_x", "forward_y", "forward_z",
                "colliderPos_x", "colliderPos_y", "colliderPos_z",
                "colliderScale_x", "colliderScale_y", "colliderScale_z",
                "colliderSurfacePoint_x", "colliderSurfacePoint_y", "colliderSurfacePoint_z",
                "contactPoint_x", "contactPoint_y", "contactPoint_z",
                "contactPlanePoint_x", "contactPlanePoint_y", "contactPlanePoint_z",
                "worldToLocalMatrix00", "worldToLocalMatrix01", "worldToLocalMatrix02", "worldToLocalMatrix03",
                "worldToLocalMatrix10", "worldToLocalMatrix11", "worldToLocalMatrix12", "worldToLocalMatrix13",
                "worldToLocalMatrix20", "worldToLocalMatrix21", "worldToLocalMatrix22", "worldToLocalMatrix23",
                "worldToLocalMatrix30", "worldToLocalMatrix31", "worldToLocalMatrix32", "worldToLocalMatrix33",
                "parentWorldToLocalMatrix00", "parentWorldToLocalMatrix01", "parentWorldToLocalMatrix02", "parentWorldToLocalMatrix03",
                "parentWorldToLocalMatrix10", "parentWorldToLocalMatrix11", "parentWorldToLocalMatrix12", "parentWorldToLocalMatrix13",
                "parentWorldToLocalMatrix20", "parentWorldToLocalMatrix21", "parentWorldToLocalMatrix22", "parentWorldToLocalMatrix23",
                "parentWorldToLocalMatrix30", "parentWorldToLocalMatrix31", "parentWorldToLocalMatrix32", "parentWorldToLocalMatrix33",
                "globalContainer_Pos_x", "globalContainer_Pos_y", "globalContainer_Pos_z",
                "globalContainer_Rot_x", "globalContainer_Rot_y", "globalContainer_Rot_z",
                "globalContainer_Scale_x", "globalContainer_Scale_y", "globalContainer_Scale_z",
                "globalContainerWorldToLocalMatrix00", "globalContainerWorldToLocalMatrix01", "globalContainerWorldToLocalMatrix02", "globalContainerWorldToLocalMatrix03",
                "globalContainerWorldToLocalMatrix10", "globalContainerWorldToLocalMatrix11", "globalContainerWorldToLocalMatrix12", "globalContainerWorldToLocalMatrix13",
                "globalContainerWorldToLocalMatrix20", "globalContainerWorldToLocalMatrix21", "globalContainerWorldToLocalMatrix22", "globalContainerWorldToLocalMatrix23",
                "globalContainerWorldToLocalMatrix30", "globalContainerWorldToLocalMatrix31", "globalContainerWorldToLocalMatrix32", "globalContainerWorldToLocalMatrix33"
            }).ToArray();

        TransformLinker parentTransformLinker;
        ButtonController controller;
        ButtonZone contactZone;
        Transform globalContainer;
        string parentName, buttonName;

        void Start()
        {
            parentTransformLinker = transform.parent.parent.GetComponent<TransformLinker>();
            controller = GetComponent<ButtonController>();
            contactZone = controller.contactZone;
            if (parentTransformLinker != null)
            {
                globalContainer = HandsManager.instance.handCoordinateManagers[parentTransformLinker.handIndex].palmBase.transform;
                parentName = parentTransformLinker.parentName;
                buttonName = parentTransformLinker.transform.name;
            }
            else
            {
                globalContainer = transform.parent.parent.parent;
                parentName = globalContainer.name;
                buttonName = transform.parent.parent.name;
            }
        }

        /// <summary>
        /// Returnns the data collected from the contactZone object
        /// </summary>
        /// <returns></returns>
        protected override UXFDataRow GetCurrentValues()
        {
            UXFDataRow data = base.GetCurrentValues();

            // get position and rotation
            Vector3 p = globalContainer.position;
            Vector3 r = globalContainer.eulerAngles;
            Vector3 s = globalContainer.lossyScale;
            Matrix4x4 m = globalContainer.worldToLocalMatrix;
            
            UXFDataRow newData =  new UXFDataRow()
            {
                ("fingerName", parentName),
                ("buttonName", buttonName),
                ("zonePos_x", contactZone.selfScale.x),
                ("zonePos_y", contactZone.selfScale.y),
                ("zonePos_z", contactZone.selfScale.z),
                ("scale_x", contactZone.selfScale.x),
                ("scale_y", contactZone.selfScale.y),
                ("scale_z", contactZone.selfScale.z),
                ("forward_x", contactZone.selfForward.x),
                ("forward_y", contactZone.selfForward.y),
                ("forward_z", contactZone.selfForward.z),
                ("colliderPos_x", contactZone.colliderPosition.x),
                ("colliderPos_y", contactZone.colliderPosition.y),
                ("colliderPos_z", contactZone.colliderPosition.z),
                ("colliderScale_x", contactZone.colliderScale.x),
                ("colliderScale_y", contactZone.colliderScale.y),
                ("colliderScale_z", contactZone.colliderScale.z),
                ("colliderSurfacePoint_x", contactZone.colliderSurfacePoint.x),
                ("colliderSurfacePoint_y", contactZone.colliderSurfacePoint.y),
                ("colliderSurfacePoint_z", contactZone.colliderSurfacePoint.z),
                ("contactPoint_x", contactZone.contactPoint.x),
                ("contactPoint_y", contactZone.contactPoint.y),
                ("contactPoint_z", contactZone.contactPoint.z),
                ("contactPlanePoint_x", contactZone.contactPlanePoint.x),
                ("contactPlanePoint_y", contactZone.contactPlanePoint.y),
                ("contactPlanePoint_z", contactZone.contactPlanePoint.z),
                ("worldToLocalMatrix00", contactZone.worldToLocalMatrix[0, 0]),
                ("worldToLocalMatrix01", contactZone.worldToLocalMatrix[0, 1]),
                ("worldToLocalMatrix02", contactZone.worldToLocalMatrix[0, 2]),
                ("worldToLocalMatrix03", contactZone.worldToLocalMatrix[0, 3]),
                ("worldToLocalMatrix10", contactZone.worldToLocalMatrix[1, 0]),
                ("worldToLocalMatrix11", contactZone.worldToLocalMatrix[1, 1]),
                ("worldToLocalMatrix12", contactZone.worldToLocalMatrix[1, 2]),
                ("worldToLocalMatrix13", contactZone.worldToLocalMatrix[1, 3]),
                ("worldToLocalMatrix20", contactZone.worldToLocalMatrix[2, 0]),
                ("worldToLocalMatrix21", contactZone.worldToLocalMatrix[2, 1]),
                ("worldToLocalMatrix22", contactZone.worldToLocalMatrix[2, 2]),
                ("worldToLocalMatrix23", contactZone.worldToLocalMatrix[2, 3]),
                ("worldToLocalMatrix30", contactZone.worldToLocalMatrix[3, 0]),
                ("worldToLocalMatrix31", contactZone.worldToLocalMatrix[3, 1]),
                ("worldToLocalMatrix32", contactZone.worldToLocalMatrix[3, 2]),
                ("worldToLocalMatrix33", contactZone.worldToLocalMatrix[3, 3]),
                ("parentWorldToLocalMatrix00", contactZone.parentWorldToLocalMatrix[0, 0]),
                ("parentWorldToLocalMatrix01", contactZone.parentWorldToLocalMatrix[0, 1]),
                ("parentWorldToLocalMatrix02", contactZone.parentWorldToLocalMatrix[0, 2]),
                ("parentWorldToLocalMatrix03", contactZone.parentWorldToLocalMatrix[0, 3]),
                ("parentWorldToLocalMatrix10", contactZone.parentWorldToLocalMatrix[1, 0]),
                ("parentWorldToLocalMatrix11", contactZone.parentWorldToLocalMatrix[1, 1]),
                ("parentWorldToLocalMatrix12", contactZone.parentWorldToLocalMatrix[1, 2]),
                ("parentWorldToLocalMatrix13", contactZone.parentWorldToLocalMatrix[1, 3]),
                ("parentWorldToLocalMatrix20", contactZone.parentWorldToLocalMatrix[2, 0]),
                ("parentWorldToLocalMatrix21", contactZone.parentWorldToLocalMatrix[2, 1]),
                ("parentWorldToLocalMatrix22", contactZone.parentWorldToLocalMatrix[2, 2]),
                ("parentWorldToLocalMatrix23", contactZone.parentWorldToLocalMatrix[2, 3]),
                ("parentWorldToLocalMatrix30", contactZone.parentWorldToLocalMatrix[3, 0]),
                ("parentWorldToLocalMatrix31", contactZone.parentWorldToLocalMatrix[3, 1]),
                ("parentWorldToLocalMatrix32", contactZone.parentWorldToLocalMatrix[3, 2]),
                ("parentWorldToLocalMatrix33", contactZone.parentWorldToLocalMatrix[3, 3]),
                ("globalContainer_Pos_x", p.x),
                ("globalContainer_Pos_y", p.y),
                ("globalContainer_Pos_z", p.z),
                ("globalContainer_Rot_x", r.x),
                ("globalContainer_Rot_y", r.y),
                ("globalContainer_Rot_z", r.z),
                ("globalContainer_Scale_x", s.x),
                ("globalContainer_Scale_y", s.y),
                ("globalContainer_Scale_z", s.z),
                ("globalContainerWorldToLocalMatrix00", m[0, 0]),
                ("globalContainerWorldToLocalMatrix01", m[0, 1]),
                ("globalContainerWorldToLocalMatrix02", m[0, 2]),
                ("globalContainerWorldToLocalMatrix03", m[0, 3]),
                ("globalContainerWorldToLocalMatrix10", m[1, 0]),
                ("globalContainerWorldToLocalMatrix11", m[1, 1]),
                ("globalContainerWorldToLocalMatrix12", m[1, 2]),
                ("globalContainerWorldToLocalMatrix13", m[1, 3]),
                ("globalContainerWorldToLocalMatrix20", m[2, 0]),
                ("globalContainerWorldToLocalMatrix21", m[2, 1]),
                ("globalContainerWorldToLocalMatrix22", m[2, 2]),
                ("globalContainerWorldToLocalMatrix23", m[2, 3]),
                ("globalContainerWorldToLocalMatrix30", m[3, 0]),
                ("globalContainerWorldToLocalMatrix31", m[3, 1]),
                ("globalContainerWorldToLocalMatrix32", m[3, 2]),
                ("globalContainerWorldToLocalMatrix33", m[3, 3])
            };
            data.AddRange(newData);
            return data;
        }
    }
}
