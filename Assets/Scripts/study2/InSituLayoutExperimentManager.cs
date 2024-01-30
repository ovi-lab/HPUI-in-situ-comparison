using System.Collections.Generic;
using System.Linq;
using ubco.ovilab.uxf.extensions;
using UXF;
using ubco.ovilab.ViconUnityStream;
using UnityEngine;
using UnityEngine.UI;
using ubco.ovilab.HPUI.Interaction;
using UnityEngine.XR.Hands;
using ubco.ovilab.hpuiInSituComparison.common;

namespace ubco.ovilab.hpuiInSituComparison.study2
{
    public class InSituLayoutExperimentManager : ExperimentManager<InSituLayoutCompBlockData>
    {
        public Camera mainCamera;
        public AudioClip contactAudio;
        public AudioSource audioSource;
        public bool trackJoints = true; // Adding this for performance reasons
        public LayoutTaskManger taskManager;
        public CalibrationContainer calibrationContainer;
        public List<XRHandJointID> forceTrackJoints = new List<XRHandJointID>(); // When trackJoints is false, bypass that for the coordinates in this list
        public Button setRightButton, setLeftButton;

        #region HIDDEN_VARIABLES
        private Dictionary<IHPUIInteractable, (Tracker tracker, Vector3 localScale)> interactables;
        private System.Random random;
        private string handedness; // Set by the button in the scene
        #endregion

        public override void Start()
        {
            // TODO: Update this!
            Session.instance.settingsToLog.AddRange(new List<string>(){"colorIndex","colorGroupIndex", "targetIndex",
                        "targetLocation", "sequenceIndex", "inSequenceIndex"});
            calibrationContainer.SetupCalibrationMethods(this);
            onBlockRecieved.AddListener(OnBlockRecieved);
            setRightButton.onClick.AddListener(() => handedness = "right");
            setLeftButton.onClick.AddListener(() => handedness = "left");
            base.Start();
        }

        #region UFX_FUNCTIONS
        protected override void OnSessionBegin(Session session)
        {
            // Record the file names of the streams from vicon
            Dictionary<string, List<string>> subjectScripts = FindObjectsOfType<CustomSubjectScript>()
                .Where(subject => subject.enabled)
                .ToDictionary((subject) => subject.transform.name,
                              (subject) => subject.filePaths?.ToList());
            Session.instance.settings.SetValue("data_streams", subjectScripts);

            Tracker tracker;

            // Adding all points from the HandsManagers to the tracked objects.
            foreach (Handedness coordinateManager in new List<Handedness>(){Handedness.Left, Handedness.Right})
            {
                for(int i = XRHandJointID.BeginMarker.ToIndex(); i < XRHandJointID.EndMarker.ToIndex(); i++)
                {
                    XRHandJointID jointID = XRHandJointIDUtility.FromIndex(i);

                    // NOTE: This condition is added to reduce the number of files that get written.
                    if (trackJoints || forceTrackJoints.Contains(jointID))
                    {
                        Transform coordinate = new GameObject(jointID.ToString()).transform;
                        coordinate.parent = this.transform;
                        if (coordinate != null)
                        {
                            tracker = coordinate.GetComponent<PositionRotationTracker>();
                            if (tracker == null)
                            {
                                tracker = coordinate.gameObject.AddComponent<PositionRotationTracker>();
                                tracker.objectName = jointID.ToString();
                            }
                            session.trackedObjects.Add(tracker);
                        }
                    }
                }
            }

            interactables = new Dictionary<IHPUIInteractable, (Tracker, Vector3)>();

            foreach(IHPUIInteractable interactable in taskManager.GetInteractables())
            {
                if (!interactables.ContainsKey(interactable))
                {
                    string interactableName = $"{interactable.transform.parent.parent?.name}_{interactable.transform.parent.name}_{interactable.transform.name}";

                    // Setting up the trackers
                    tracker = interactable.transform.GetComponent<HPUIInteratableTracker>();

                    if (tracker == null)
                    {
                        tracker = interactable.transform.gameObject.AddComponent<HPUIInteratableTracker>();
                        tracker.objectName = "btn_" + interactableName;
                    }

                    InteractableTracker interactableTracker = interactable.transform.GetComponent<InteractableTracker>();
                    if (interactableTracker != null)
                    {
                        interactableTracker.Tracker = tracker as HPUIInteratableTracker;
                    }

                    // This RecordRow will be called everytime an interaction happens
                    tracker.updateType = TrackerUpdateType.Manual;

                    // Tracking interactables
                    interactables.Add(interactable, (tracker, interactable.transform.parent.localScale));

                    Session.instance.trackedObjects.Add(tracker);
                }
            }

            HideInteractables();

            tracker = mainCamera.GetComponent<PositionRotationTracker>();
            if (tracker == null)
            {
                tracker = mainCamera.gameObject.AddComponent<PositionRotationTracker>();
            }
            tracker.objectName = "mainCamera";
            session.trackedObjects.Add(tracker);
        }

        protected override void ConfigureBlock(InSituLayoutCompBlockData el, Block block, bool lastBlockCancelled)
        {
            // TODO: take following values from the server
            // - offset for skeleton
            // - offset of thumb collider
            random = new System.Random();
            block.settings.SetValue("numTrials", el.numTrials);
            block.settings.SetValue("changeLayout", el.changeLayout);

            taskManager.ConfigureTaskBlock(block, random, el, lastBlockCancelled);

            foreach(IHPUIInteractable interactable in taskManager.GetAvtiveInteractables())
            {
                if (interactable is HPUIContinuousInteractable continuousInteractable)
                {
                    continuousInteractable.GestureEvent.AddListener(OnInteractableGesture);
                }
                else
                {
                    (interactable as HPUIBaseInteractable)?.TapEvent.AddListener(OnInteractableTap);
                }
            }
        }

        protected override void OnBlockBegin(Block block)
        {
        }

        protected override void OnTrialBegin(Trial trial)
        {
            Debug.Log($"Startin trial:   Trial num: {Session.instance.CurrentTrial.number}     " +
                      $"Block num: {Session.instance.CurrentBlock.number}     ");
        }
        
        protected override void OnTrialEnd(Trial trial)
        {
        }

        protected override void OnBlockEnd(Block block)
        {
            HideInteractables();
        }

        protected override void OnSessionEnd(Session session)
        {
            HideInteractables();
        }
        #endregion

        #region HPUI functions
        // callback functions
        private void OnInteractableTap(HPUITapEventArgs args)
        {
            IHPUIInteractable interactable = args.interactableObject;
            audioSource.PlayOneShot(contactAudio);
            if (!Session.instance.hasInitialised)
            {
                Debug.Log($"{Session.instance.hasInitialised}");
                return;
            }

            Tracker tracker = interactables[interactable].tracker;
            tracker.RecordRow();
            // targetButton.ResetStates();
            // targetButton.contactAction.RemoveListener(OnButtonContact);

            Debug.Log($"Interactable contact  Trial num: {Session.instance.CurrentTrial.number}     " +
                      $"Block num: {Session.instance.CurrentBlock.number}     " +
                      $"Contact interactable: {tracker.objectName}     ");
        }

        private void OnInteractableGesture(HPUIGestureEventArgs args)
        {
            IHPUIInteractable interactable = args.interactableObject;
            audioSource.PlayOneShot(contactAudio);
            if (!Session.instance.hasInitialised)
            {
                Debug.Log($"{Session.instance.hasInitialised}");
                return;
            }
            Debug.Log($"Slider change Trial num: {Session.instance.CurrentTrial.number}     " +
                      $"Block num: {Session.instance.CurrentBlock.number}     " +
                      $"Slider: {interactables[interactable].tracker.objectName}    Value: {(args.Position - interactable.boundsMin) / (interactable.boundsMax - interactable.boundsMin)} ");
        }

        #endregion

        #region HELPER_FUNCTIONS
        private void HideInteractables()
        {
            foreach (IHPUIInteractable interactable in interactables.Keys)
            {
                if (interactable is HPUIContinuousInteractable continuousInteractable)
                {
                    continuousInteractable.GestureEvent.RemoveListener(OnInteractableGesture);
                }
                else
                {
                    (interactable as HPUIBaseInteractable)?.TapEvent.RemoveListener(OnInteractableTap);
                }
            }
        }

        private void OnBlockRecieved(InSituLayoutCompBlockData newBlockData)
        {
            newBlockData.handedness = handedness;
        }
        #endregion
    }

    public class InSituLayoutCompBlockData: HPUIBlockData
    {
        public string windowManager;
        public int numberOfColors;

        public override string ToString()
        {
            return
                base.ToString() +
                $"Number of Trials: {numTrials}    " +
                $"Buttons used: {handedness}   " +
                $"Change Layout: {changeLayout}   " +
                $"Start zoom above: {windowManager}    " +
                $"Number of colors: {numberOfColors}    ";
        }
    }
}
