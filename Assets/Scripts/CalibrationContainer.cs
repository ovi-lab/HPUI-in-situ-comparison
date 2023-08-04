using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using ubc.ok.ovilab.HPUI.Core;
using ubc.ok.ovilab.ViconUnityStream;
using ubc.ok.ovilab.uxf.extensions;

namespace ubc.ok.ovilab.hpuiInSituComparison.study1
{
    public class CalibrationContainer : MonoBehaviour
    {
        #region PUBLIC_VARIABLES
        public InSituExperimentManager experimentManager;
        public Handedness handedness = Handedness.Right;
        public Transform rightFingertip, leftFingertip, cameraBase;
        public Transform rightAboveHandAnchor, leftAboveHandAnchor;
        public Button dominantRightButton, dominantLeftButton, doneCalibrationButton, executeCalibrationButton;
        public float relativeSeperateFactor;
        public CustomHandScript rightScript, leftScript;
        public List<CalibrationSettings> settings;
        public List<ComputeDistancePair> rightComputeSeperatePairs;
        public List<ComputeDistancePair> leftComputeSeperatePairs;
        #endregion

        #region PRIVATE_VARIABLES
        private bool recordedCalibration = false;
        private Vector3 headPosition, headForward, fingertip;
        private string currentCalibrationName = null;
        private float computedSeperation;
        private Dictionary<string, object> calibrationParameters;
        #endregion

        void Start()
        {
            foreach (CalibrationSettings setting in settings)
            {
                experimentManager.AddCalibrationMethod(setting.name, () => ExecuteCalibration(setting.name));
            }
            dominantRightButton.onClick.AddListener(GetPositionsDominantRight);
            dominantLeftButton.onClick.AddListener(GetPositionsDominantLeft);
            doneCalibrationButton.onClick.AddListener(OnCalibrationCompleteButton);
            executeCalibrationButton.onClick.AddListener(SetCalibration);
        }

        #region COMMON_FUNCTIONS
        void DeactivateAll()
        {
            foreach (CalibrationSettings setting in settings)
            {
                foreach (GameObject obj in setting.commonObjects)
                {
                    obj.SetActive(false);
                }
                foreach (GameObject obj in setting.rightHandedObjects)
                {
                    obj.SetActive(false);
                }
                foreach (GameObject obj in setting.leftHandedObjects)
                {
                    obj.SetActive(false);
                }
            }
        }

        void ExecuteCalibration(String name)
        {
            DeactivateAll();
            // In case the computed locations need to be recomputed
            rightComputeSeperatePairs[0].p1.root.gameObject.SetActive(true);
            leftComputeSeperatePairs[0].p1.root.gameObject.SetActive(true);
            dominantRightButton.interactable = true;
            dominantLeftButton.interactable = true;
            executeCalibrationButton.interactable = recordedCalibration;
            doneCalibrationButton.interactable = false;
            currentCalibrationName = name;
        }

        void SetCalibration()
        {
            if (settings.Select(x => x.name).Contains(currentCalibrationName))
            {
                DeactivateAll();
                CalibrationSettings setting = settings.First(x => x.name == currentCalibrationName);
                foreach (GameObject obj in setting.commonObjects)
                {
                    obj.SetActive(true);
                } 
                foreach (GameObject obj in setting.rightHandedObjects)
                {
                    obj.SetActive(handedness == Handedness.Right);
                } 
                foreach (GameObject obj in setting.leftHandedObjects)
                {
                    obj.SetActive(handedness == Handedness.Left);
                }

                calibrationParameters = new Dictionary<string, object>();
                calibrationParameters.Add("name", currentCalibrationName);
                calibrationParameters.Add("handedness", handedness == Handedness.Right ? "right" : "left");
                calibrationParameters.Add("headPosition", headPosition);
                calibrationParameters.Add("headForward", headForward);
                calibrationParameters.Add("fingertip", fingertip);

                foreach(CalibrationParamters s in setting.configurePosition)
                {
                    ProcessCalibration(s);
                }
                ProcessCalibration(setting.displayElementLocation);

                doneCalibrationButton.interactable = true;
            }
            else
            {
                Debug.LogError($"Unknown calibration {name}");
            }
        }

        void OnCalibrationCompleteButton()
        {
            dominantRightButton.interactable = false;
            dominantLeftButton.interactable = false;
            executeCalibrationButton.interactable = false;
            doneCalibrationButton.interactable = false;
            experimentManager.CalibrationComplete(calibrationParameters);
            calibrationParameters = null;
        }

        void GetPositionsDominantRight()
        {
            GetPositions(Handedness.Right, "R", 0);
        }

        void GetPositionsDominantLeft()
        {
            GetPositions(Handedness.Left, "L", 1);
        }

        void GetPositions(Handedness handedness, string prefix, int handIndex)
        {
            Transform anchor1 = null, anchor2 = null;
            this.handedness = handedness;
            headPosition = cameraBase.transform.position;
            headForward = cameraBase.transform.forward;
            List<ComputeDistancePair> pairs = null;
            if (handedness == Handedness.Right)
            {
                fingertip = rightFingertip.position;
                anchor1 = rightAboveHandAnchor;
                anchor2 = leftAboveHandAnchor;
                pairs = rightComputeSeperatePairs;
            } else if (handedness == Handedness.Left){
                fingertip = leftFingertip.position;
                anchor1 = leftAboveHandAnchor;
                anchor2 = rightAboveHandAnchor;
                pairs = leftComputeSeperatePairs;
            }
            recordedCalibration = true;
            executeCalibrationButton.interactable = true;

            computedSeperation = 0;
            foreach(ComputeDistancePair pair in pairs)
            {
                computedSeperation += (pair.p1.position - pair.p2.position).magnitude;
            }

            computedSeperation /= pairs.Count;

            Transform palmBase = HandsManager.instance.handCoordinateManagers[handIndex].palmBase.transform;
            Transform indexMiddle = HandsManager.instance.handCoordinateManagers[handIndex].GetProxyTrasnform(prefix + "2D2");

            Vector3 anchorUp = Vector3.Cross(palmBase.up, -palmBase.forward);
            if (handedness == Handedness.Right)
            {
                anchorUp = -anchorUp;
            }
            anchor1.rotation = Quaternion.LookRotation(-palmBase.forward.normalized - palmBase.up.normalized * 0.3f, anchorUp);
            anchor1.position = indexMiddle.position + palmBase.up.normalized * computedSeperation * 0.7f
                + anchor1.up.normalized * computedSeperation * relativeSeperateFactor
                + anchor1.forward.normalized * computedSeperation * 0.6f;
            Vector3 pos = anchor1.localPosition;
            pos.x = -pos.x;
            anchor2.localPosition = pos;
            anchor2.localRotation = anchor1.localRotation;
            anchor2.rotation = Quaternion.LookRotation(anchor2.forward, -anchor2.up);
        }

        public void ProcessCalibration(CalibrationParamters s) 
        {
            TransformLinker linker;
            AlwaysFaceCamera alwaysFaceCamera;

            if (s.useComputedButtonSeperation)
            {
                s.buttonSeperation = computedSeperation * s.buttonScale;
            }

            switch(s.option)
            {
                case CalibrationOptions.InFrontOfFace:
                    linker = s.t.GetComponent<TransformLinker>();
                    alwaysFaceCamera = s.t.GetComponent<AlwaysFaceCamera>();
                    if (linker)
                    {
                        linker.enabled = false;
                    }
                    if (alwaysFaceCamera)
                    {
                        alwaysFaceCamera.enabled = false;
                    }
                    SetInFrontOfFace(s);
                    break;
                case CalibrationOptions.InFrontOfShoulderOrHip:
                    linker = s.t.GetComponent<TransformLinker>();
                    alwaysFaceCamera = s.t.GetComponent<AlwaysFaceCamera>();
                    if (linker)
                    {
                        linker.enabled = false;
                    }
                    if (alwaysFaceCamera)
                    {
                        alwaysFaceCamera.enabled = false;
                    }
                    SetInFrontOfShoulderOrHip(s);
                    break;
                case CalibrationOptions.AboveDominantHand:
                    SetSettingsTransformPosition(s, Vector3.zero, Quaternion.identity);
                    linker = s.t.GetComponent<TransformLinker>();
                    alwaysFaceCamera = s.t.GetComponent<AlwaysFaceCamera>();
                    if (handedness == Handedness.Right)
                    {
                        linker.parent = rightAboveHandAnchor;
                    }
                    else
                    {
                        linker.parent = leftAboveHandAnchor;
                    }
                    linker.enabled = true;
                    if (alwaysFaceCamera != null)
                    {
                        alwaysFaceCamera.enabled = true;
                    }
                    break;
                case CalibrationOptions.AboveNonDominantHand:
                    SetSettingsTransformPosition(s, Vector3.zero, Quaternion.identity);
                    linker = s.t.GetComponent<TransformLinker>();
                    alwaysFaceCamera = s.t.GetComponent<AlwaysFaceCamera>();
                    if (handedness == Handedness.Right)
                    {
                        linker.parent = leftAboveHandAnchor;
                    }
                    else
                    {
                        linker.parent = rightAboveHandAnchor;
                    }
                    linker.enabled = true;
                    if (alwaysFaceCamera != null)
                    {
                        alwaysFaceCamera.enabled = true;
                    }
                    break;
            }

            string _prefix = s.t.name;
            calibrationParameters.Add(_prefix + "s.buttonScale", s.buttonScale);
            calibrationParameters.Add(_prefix + "s.buttonSeperation", s.buttonSeperation);
            calibrationParameters.Add(_prefix + "s.forwardOffset", s.forwardOffset);
            calibrationParameters.Add(_prefix + "s.horizontalOffset", s.horizontalOffset);
            calibrationParameters.Add(_prefix + "s.option", s.option);
            calibrationParameters.Add(_prefix + "s.t.name", s.t.name);
            calibrationParameters.Add(_prefix + "s.tiltAngle", s.tiltAngle);
            calibrationParameters.Add(_prefix + "s.useComputedButtonSeperation", s.useComputedButtonSeperation);
            calibrationParameters.Add(_prefix + "s.verticalOffset", s.verticalOffset);
        }

        private void SetSettingsTransformPosition(CalibrationParamters s, Vector3 position, Quaternion rotation)
        {
            FixedButtonLayout fixedLayout = s.t.GetComponent<FixedButtonLayout>();
            if (fixedLayout != null)
            {
                fixedLayout.relativeSeperateFactor = relativeSeperateFactor;
                if (s.relativeFixedButtonLayout == null)
                {
                    fixedLayout.SetParameters(s.buttonSeperation, s.buttonScale, position, rotation);
                }
                else
                {
                    fixedLayout.SetParameters(s.buttonSeperation, s.buttonScale, s.relativeFixedButtonLayout);
                }
            }
            else
            {
                s.t.position = position;
                s.t.rotation = rotation;
            }
        }
        #endregion

        #region SPECIFIC_FUNCTIONS
        public void SetInFrontOfFace(CalibrationParamters s)
        {
            Vector3 position = headPosition + headForward.normalized * s.forwardOffset + Vector3.up * s.verticalOffset;
            Quaternion rotation = Quaternion.LookRotation(-headForward, Vector3.up);
            SetSettingsTransformPosition(s, position, rotation);
        }

        public void SetInFrontOfShoulderOrHip(CalibrationParamters s)
        {
            float horizontalOffset = handedness == Handedness.Right ? s.horizontalOffset : -s.horizontalOffset;
            // Point on coronal plane (plne making the lataral ventral/dorsal axis)
            Plane headPlane = new Plane(headForward, headPosition);
            Vector3 shoulderOrHipPoint = headPlane.ClosestPointOnPlane(fingertip); // aka shoulderOrHip point
            Vector3 armVector = fingertip - shoulderOrHipPoint;
            Vector3 position = fingertip + armVector * s.forwardOffset +
                Vector3.up.normalized * armVector.magnitude * s.verticalOffset +
                Vector3.Cross(headForward, Vector3.up).normalized * horizontalOffset;
            float tiltAngleRadian = Mathf.Deg2Rad * s.tiltAngle;
            Vector3 tiltedForward = (-headForward.normalized) * Mathf.Cos(tiltAngleRadian) + Vector3.up * Mathf.Sin(tiltAngleRadian);
            Vector3 tiltedUp = Vector3.Cross(Vector3.Cross(tiltedForward, Vector3.up), tiltedForward);
            Quaternion rotation = Quaternion.LookRotation(tiltedForward, tiltedUp);
            SetSettingsTransformPosition(s, position, rotation);
        }
        #endregion

        [Serializable]
        public struct CalibrationSettings
        {
            public string name;
            [Tooltip("Objects that will be enabled when this setting is enabled.")]
            public List<GameObject> commonObjects;
            public List<GameObject> rightHandedObjects;
            public List<GameObject> leftHandedObjects;
            public CalibrationParamters displayElementLocation;
            public List<CalibrationParamters> configurePosition;
            public bool switchToDefaultPose;
        }

        [Serializable]
        public class CalibrationParamters
        {
            public Transform t;
            public CalibrationOptions option;
            public float forwardOffset;
            public float verticalOffset;
            public float horizontalOffset;
            public float tiltAngle;
            public float buttonScale = 1;
            public float buttonSeperation;
            public bool useComputedButtonSeperation;
            public FixedButtonLayout relativeFixedButtonLayout;
        }

        public enum CalibrationOptions {
            InFrontOfFace,
            InFrontOfShoulderOrHip,
            AboveDominantHand,
            AboveNonDominantHand
        }

        [Serializable]
        public class ComputeDistancePair
        {
            public Transform p1;
            public Transform p2;
        }
    }
}
