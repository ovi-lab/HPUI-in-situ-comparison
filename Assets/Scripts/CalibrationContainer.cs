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
        public Button getPositionsButton, executeCalibrationButton, doneCalibrationButton;
        public float relativeSeperateFactor;
        public CalibrateButton2 deformableDisplayCalibration;
        public List<CalibrationSettings> settings;
        public List<ComputeDistancePair> pairs;
        #endregion

        #region PRIVATE_VARIABLES
        private bool recordedCalibration = false;
        private Vector3 headPosition, headForward, activeFingertipPos, rightFingertipPos, leftFingertipPos;
        private string currentCalibrationName = null;
        private float computedSeperation;
        private Dictionary<string, object> calibrationParameters;
        private bool getDelayedGetPosition = false;
        #endregion

        void Start()
        {
            foreach (CalibrationSettings setting in settings)
            {
                experimentManager.AddCalibrationMethod(setting.name, (blockData) => InitiateCalibration(setting.name, blockData.handedness));
            }
            getPositionsButton.onClick.AddListener(DelayedGetPositions);
            doneCalibrationButton.onClick.AddListener(OnCalibrationCompleteButton);
            executeCalibrationButton.onClick.AddListener(SetCalibration);
        }

        void Update()
        {
            if (getDelayedGetPosition)
            {
                getPositionsButton.interactable = true;
                GetPositions();
                getDelayedGetPosition = false;
            }
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

        void InitiateCalibration(String name, string handedness)
        {
            DeactivateAll();

            if (handedness == "left")
            {
                SetHandedness(Handedness.Left);
            }
            else if (handedness == "right")
            {
                SetHandedness(Handedness.Right);
            }
            else
            {
                throw new ArgumentException($"{handedness} not valid, need to be `right` or `left`");
            }

            getPositionsButton.interactable = true;
            executeCalibrationButton.interactable = recordedCalibration;
            doneCalibrationButton.interactable = false;
            currentCalibrationName = name;
        }

        void SetCalibration()
        {
            if (settings.Select(x => x.name).Contains(currentCalibrationName))
            {
                DeactivateAll();
                SetHandedness(handedness); // Making sure active finger tip is set correctly
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
                calibrationParameters.Add("activeFingertip", activeFingertipPos);
                calibrationParameters.Add("rightFingertip", rightFingertipPos);
                calibrationParameters.Add("leftFingertip", leftFingertipPos);

                foreach(CalibrationParamters s in setting.configurePosition)
                {
                    ProcessCalibration(s);
                }

                doneCalibrationButton.interactable = true;
            }
            else
            {
                Debug.LogError($"Unknown calibration {name}");
            }
        }

        void OnCalibrationCompleteButton()
        {
            getPositionsButton.interactable = false;
            executeCalibrationButton.interactable = false;
            doneCalibrationButton.interactable = false;
            experimentManager.CalibrationComplete(calibrationParameters);
            calibrationParameters = null;
        }

        void GetPositionsDominantRight()
        {
            SetHandedness(Handedness.Right);
            GetPositions();
        }

        void GetPositionsDominantLeft()
        {
            SetHandedness(Handedness.Left);
            GetPositions();
        }

        void SetHandedness(Handedness handedness)
        {
            if (handedness == Handedness.Right)
            {
                this.handedness = Handedness.Left;
                activeFingertipPos = leftFingertipPos;
            }
            else if (handedness == Handedness.Left)
            {
                this.handedness = Handedness.Right;
                activeFingertipPos = rightFingertipPos;
            }
        }

        void DelayedGetPositions()
        {
            getPositionsButton.interactable = false;
            StartCoroutine(DelayedCallGetPositions());
        }

	IEnumerator DelayedCallGetPositions()
	{
	    yield return new WaitForSeconds(3);
            getDelayedGetPosition = true;
	}

        void GetPositions()
        {
            headPosition = cameraBase.transform.position;
            headForward = cameraBase.transform.forward;
            rightFingertipPos = rightFingertip.position;
            leftFingertipPos = leftFingertip.position;

            computedSeperation = 0;
            int computedSeperationCount = 0;
            foreach(ComputeDistancePair pair in pairs)
            {
                // NOTE: Even if not active, use the location, we are using both hands anyhow,
                // hence this shouldn't be an issue
                computedSeperation += (pair.p1.position - pair.p2.position).magnitude;
                computedSeperationCount += 1;
            }

            computedSeperation /= computedSeperationCount;

            GetAnchorPositions(Handedness.Left, "L", 1);
            GetAnchorPositions(Handedness.Right, "R", 0);

            recordedCalibration = true;
            executeCalibrationButton.interactable = true;

            deformableDisplayCalibration.OnClick();
        }

        void GetAnchorPositions(Handedness handedness, string prefix, int handIndex)
        {
            Transform anchor = null;

            if (handedness == Handedness.Right)
            {
                anchor = rightAboveHandAnchor;
            } else if (handedness == Handedness.Left){
                anchor = leftAboveHandAnchor;
            }

            Transform palmBase = HandsManager.instance.handCoordinateManagers[handIndex].palmBase.transform;
            Transform indexMiddle = HandsManager.instance.handCoordinateManagers[handIndex].GetProxyTrasnform(prefix + "2D2");

            Vector3 anchorUp = Vector3.Cross(palmBase.up, -palmBase.forward);
            if (handedness == Handedness.Right)
            {
                anchorUp = -anchorUp;
            }
            anchor.rotation = Quaternion.LookRotation(-palmBase.forward.normalized - palmBase.up.normalized * 0.3f, anchorUp);
            anchor.position = indexMiddle.position + palmBase.up.normalized * computedSeperation * 0.7f
                + anchor.up.normalized * computedSeperation * relativeSeperateFactor
                + anchor.forward.normalized * computedSeperation * 0.6f;
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
                fixedLayout.SetParameters(s.buttonSeperation, s.buttonScale, position, rotation);
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
            Vector3 shoulderOrHipPoint = headPlane.ClosestPointOnPlane(activeFingertipPos); // aka shoulderOrHip point
            Vector3 armVector = activeFingertipPos - shoulderOrHipPoint;
            Vector3 position = activeFingertipPos + armVector * s.forwardOffset +
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
            public List<CalibrationParamters> configurePosition;
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
