using System.Collections.Generic;
using System.Linq;
using ubco.ovilab.uxf.extensions;
using UXF;
using ubco.ovilab.ViconUnityStream;
using UnityEngine;
using ubco.ovilab.HPUI.Interaction;
using ubco.ovilab.hpuiInSituComparison.study1;
using UnityEngine.XR.Hands;

namespace ubco.ovilab.hpuiInSituComparison.study2
{
    public class InSituLayoutExperimentManager : ExperimentManager<InSituCompBlockData>
    {
        public Camera mainCamera;
        public List<Transform> buttonsRoots;
        public Color defaultColor = Color.white;
        public Color defaultHoverColor = Color.yellow;
        public Color targetButtonColor = Color.red;
        public Color defaultHighlightColor = Color.green;
        public AudioClip contactAudio;
        public AudioSource audioSource;
        public bool trackJoints = true; // Adding this for performance reasons
        public TaskManager taskManager;
        public List<XRHandJointID> forceTrackJoints = new List<XRHandJointID>(); // When trackJoints is false, bypass that for the coordinates in this list

        #region HIDDEN_VARIABLES
        private Dictionary<string, (IHPUIInteractable interactable, Tracker tracker, Vector3 localScale)> buttons;
        private List<string> activeButtons;
        private System.Random random;
        #endregion

        public override void Start()
        {
            Session.instance.settingsToLog.AddRange(new List<string>(){"colorIndex","colorGroupIndex", "targetIndex", "targetLocation", "sequenceIndex",
                        "inSequenceIndex", "startZoomScale", "secondDisplayVisibleStartAtScale", "secondDisplayVisibleScaleWindow"});
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

            // Adding all button locations to the tracked objects and save ButtonControllers
            buttons = new Dictionary<string, (IHPUIInteractable, Tracker, Vector3)>();
            foreach (Transform buttonsRoot in buttonsRoots)
            {
                foreach(IHPUIInteractable interactable in buttonsRoot.GetComponentsInChildren<IHPUIInteractable>())
                {
                    string buttonName = interactable.transform.name;

                    // Configuring the colors of each buttons
                    // TODO: Figure out how to change colors
                    // SetButtonColor(interactable, defaultColor);
                    // interactable.GetComponent<ButtonColorBehaviour>().highlightColor = defaultHighlightColor;

                    // Setting up the trackers
                    tracker = interactable.transform.GetComponent<HPUIInteratableTracker>();

                    if (tracker == null)
                    {
                        tracker = interactable.transform.gameObject.AddComponent<ButtonControllerTracker>();
                        tracker.objectName = "btn_" + buttonName;
                    }

                    // This RecordRow will be called everytime an interaction happens
                    tracker.updateType = TrackerUpdateType.Manual;

                    // Tracking buttons
                    buttons.Add(buttonName, (interactable, tracker, interactable.transform.parent.localScale));

                    session.trackedObjects.Add(tracker);
                }
            }

            HideButtons();

            tracker = mainCamera.GetComponent<PositionRotationTracker>();
            if (tracker == null)
            {
                tracker = mainCamera.gameObject.AddComponent<PositionRotationTracker>();
            }
            tracker.objectName = "mainCamera";
            session.trackedObjects.Add(tracker);
        }

        protected override void ConfigureBlock(InSituCompBlockData el, Block block, bool lastBlockCancelled)
        {
            // TODO: take following values from the server
            // - offset for skeleton
            // - offset of thumb collider
            random = new System.Random();
            block.settings.SetValue("numTrials", el.numTrials);
            block.settings.SetValue("changeLayout", el.changeLayout);

            taskManager.ConfigureTaskBlock(block, random, el, lastBlockCancelled);

            // TODO: Get the buttons to interact with and confiure them
            // foreach (ButtonController btn in taskManager.GetActiveButtons())
            // {
            //     btn.Show();
            //     btn.ResetStates();
            //     btn.contactAction.AddListener(OnButtonContact);
            //     btn.proximateAction.AddListener(OnButtonHover);
            // }
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
            HideButtons();
        }

        protected override void OnSessionEnd(Session session)
        {
            HideButtons();
        }
        #endregion

        #region HPUI functions
        // callback functions
        private void OnButtonTap(HPUITapEventArgs args)
        {
            IHPUIInteractable interactable = args.interactableObject;
            audioSource.PlayOneShot(contactAudio);
            if (!Session.instance.hasInitialised)
            {
                Debug.Log($"{Session.instance.hasInitialised}");
                return;
            }

            buttons[interactable.transform.name].tracker.RecordRow();
            // targetButton.ResetStates();
            // targetButton.contactAction.RemoveListener(OnButtonContact);

            Debug.Log($"Button contact  Trial num: {Session.instance.CurrentTrial.number}     " +
                      $"Block num: {Session.instance.CurrentBlock.number}     " +
                      $"Contact button: {interactable.transform.name}     ");
        }

        private void OnButtonGesture(HPUIGestureEventArgs args)
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
                      $"Slider: {interactable.transform.name}    Value: {(args.Position - interactable.boundsMin) / (interactable.boundsMax - interactable.boundsMin)} ");
        }

        #endregion

        #region HELPER_FUNCTIONS
        private void HideButtons()
        {
            foreach ((IHPUIInteractable button, Tracker tracker, Vector3 localScale) in buttons.Values)
            {
                if (button is HPUIContinuousInteractable continuousInteractable)
                {
                    continuousInteractable.GestureEvent.RemoveListener(OnButtonGesture);
                }
                else
                {
                    (button as HPUIBaseInteractable)?.TapEvent.RemoveListener(OnButtonTap);
                }
                button.transform.gameObject.SetActive(false);
            }
        }

        // TODO: Do I need a set of method of button behavior?
        private void SetButtonColor(IHPUIInteractable buttonController, Color color, Color? hoverColor=null)
        {
            // buttonController.GetComponent<ButtonColorBehaviour>().DefaultColor = color;

            // if (hoverColor == null)
            // {
            //     buttonController.GetComponent<ButtonColorBehaviour>().hoverColor = color;  // disabling the hover color
            // }
            // else
            // {
            //     buttonController.GetComponent<ButtonColorBehaviour>().hoverColor = (Color) hoverColor;
            // }
        }
        #endregion
    }
}
