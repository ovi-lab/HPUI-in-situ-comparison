using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using ubc.ok.ovilab.uxf.extensions;
using UXF;
using ubc.ok.ovilab.ViconUnityStream;
using ubc.ok.ovilab.HPUI.Core;
using UnityEngine;

namespace ubc.ok.ovilab.hpuiInSituComparison.study1
{
    public class InSituExperimentManager : ExperimentManager<InSituCompBlockData>
    {
        public List<Transform> buttonsRoots;
        public Color defaultColor = Color.white;
        public Color defaultHoverColor = Color.yellow;
        public Color targetButtonColor = Color.red;
        public Color defaultHighlightColor = Color.green;
        public AudioClip contactAudio;
        public AudioSource audioSource;
        public bool trackJoints = true; // Adding this for performance reasons
        public TaskManager taskManager;
        public List<string> forceTrackJoints = new List<string>(); // When trackJoints is false, bypass that for the coordinates in this list

        #region HIDDEN_VARIABLES
        private Dictionary<string, (ButtonController controller, Tracker tracker, Vector3 localScale)> buttons;
        private List<string> activeButtons;
        private ButtonController targetButton;
        private System.Random random;
        #endregion

        public override void Start()
        {
            Session.instance.settingsToLog.AddRange(new List<string>(){"colorIndex", "targetIndex", "targetLocation", "sequenceIndex", "inSequenceIndex"});
            base.Start();
        }

        #region UFX_FUNCTIONS
        protected override void OnSessionBegin(Session session)
        {
            base.OnSessionBegin(session);
            // Record the file names of the streams from vicon
            Dictionary<string, List<string>> subjectScripts = FindObjectsOfType<CustomSubjectScript>()
                .Where(subject => subject.enabled)
                .ToDictionary((subject) => subject.transform.name,
                              (subject) => subject.filePaths?.ToList());
            Session.instance.settings.SetValue("data_streams", subjectScripts);

            Tracker tracker;

            // Adding all points from the HandsManagers to the tracked objects.
            foreach (HandCoordinateManager coordinateManager in HandsManager.instance.handCoordinateManagers)
            {
                foreach (string coordinateName in coordinateManager.managedCoordinates)
                {
                    // NOTE: This condition is added to reduce the number of files that get written.
                    if (trackJoints || forceTrackJoints.Contains(coordinateName))
                    {
                        Transform coordinate = coordinateManager.GetManagedCoord(coordinateName);
                        if (coordinate != null)
                        {
                            tracker = coordinate.GetComponent<PositionRotationTracker>();
                            if (tracker == null)
                            {
                                tracker = coordinate.gameObject.AddComponent<PositionRotationTracker>();
                                tracker.objectName = coordinateName.Replace('/', '_');
                            }
                            session.trackedObjects.Add(tracker);
                        }
                    }
                }
            }

            // Adding all button locations to the tracked objects and save ButtonControllers
            buttons = new Dictionary<string, (ButtonController, Tracker, Vector3)>();
            foreach (Transform buttonsRoot in buttonsRoots)
            {
                foreach(ButtonController buttonController in buttonsRoot.GetComponentsInChildren<ButtonController>())
                {
                    Transform button = buttonController.transform.parent;
                    string buttonName = button.parent.name;
                    buttonController.name = buttonName;

                    // Configuring the colors of each buttons
                    SetButtonColor(buttonController, defaultColor);
                    buttonController.GetComponent<ButtonColorBehaviour>().highlightColor = defaultHighlightColor;

                    // Setting up the trackers
                    tracker = buttonController.GetComponent<ButtonControllerTracker>();

                    if (tracker == null)
                    {
                        tracker = buttonController.gameObject.AddComponent<ButtonControllerTracker>();
                        tracker.objectName = "btn_" + buttonName;
                    }

                    // This RecordRow will be called everytime an interaction happens
                    tracker.updateType = TrackerUpdateType.Manual;

                    // Tracking buttons
                    buttons.Add(buttonName, (buttonController, tracker, buttonController.transform.parent.localScale));

                    session.trackedObjects.Add(tracker);
                }
            }

            HideButtons();

            tracker = Camera.main.GetComponent<PositionRotationTracker>();
            if (tracker == null)
            {
                Camera.main.gameObject.AddComponent<PositionRotationTracker>();
            }
        }

        protected override void ConfigureBlock(InSituCompBlockData el, Block block)
        {
            base.ConfigureBlock(el, block);
            // TODO: take following values from the server
            // - offset for skeleton
            // - offset of thumb collider
            random = new System.Random();
            block.settings.SetValue("numTrials", el.numTrials);
            block.settings.SetValue("buttons", el.useButtons);
            block.settings.SetValue("buttonSize", el.buttonSize);
            block.settings.SetValue("changeLayout", el.changeLayout);

            // The config doesn't say which targets are shown/hidden after the calibration
            // determine this by checking if the root is active or not
            List<string> usedButtons = el.useButtons.Where(btn => buttons[btn].Item1.transform.root.gameObject.activeSelf).ToList();
            block.settings.SetValue("usedButtons", usedButtons);

            /// unique seed for participant_index + buttons size
            int seed = (int)Math.Floor(el.participant_index * el.buttonSize * 100);
            // Using seed to ensure the same sequences are genereted with a given seed
            System.Random rng = new System.Random(seed);

            IEnumerable<ButtonController> usedButtonObjects = usedButtons
                .Select(btn => buttons[btn].Item1);

            // TODO Setup trials
            // TODO Configure the the target sequence
        }

        protected override void OnBlockBegin(Block block)
        {
            base.OnBlockBegin(block);
            // TODO setup the scene
        }

        protected override void OnTrialBegin(Trial trial)
        {
            base.OnTrialBegin(trial);
            Debug.Log($"Startin trial:   Trial num: {Session.instance.CurrentTrial.number}     " +
                      $"Block num: {Session.instance.CurrentBlock.number}     ");
            // targetButton.contactAction.AddListener(OnButtonContact);
        }
        
        protected override void OnTrialEnd(Trial trial)
        {
            base.OnTrialEnd(trial);
        }

        // callback functions
        private void OnButtonContact(ButtonController buttonController)
        {
            audioSource.PlayOneShot(contactAudio);
            if (!Session.instance.hasInitialised)
            {
                Debug.Log($"{Session.instance.hasInitialised}");
                return;
            }

            buttons[buttonController.name].tracker.RecordRow();
            // targetButton.ResetStates();
            // targetButton.contactAction.RemoveListener(OnButtonContact);

            Debug.Log($"Button contact  Trial num: {Session.instance.CurrentTrial.number}     " +
                      $"Block num: {Session.instance.CurrentBlock.number}     " +
                      $"Contact button: {buttonController.transform.parent.parent.name}     ");
        }

        private void OnButtonHover(ButtonController buttonController)
        {
        }

        protected override void OnBlockEnd(Block block)
        {
            base.OnBlockEnd(block);
            HideButtons();
        }

        protected override void OnSessionEnd(Session session)
        {
            base.OnSessionEnd(session);
            HideButtons();
        }
        #endregion

        #region HPUI_Core_functions
        /// <summary>
        /// Callback to run after GetButton is called.
        /// </summary>
        private void PostGetButtonsCallback(IEnumerable<ButtonController> buttons)
        {
            
        }
        #endregion

        #region HELPER_FUNCTIONS
        private void HideButtons()
        {
            foreach ((ButtonController button, Tracker tracker, Vector3 localScale) in buttons.Values)
            {
                button.contactAction.RemoveListener(OnButtonContact);
                button.proximateAction.RemoveListener(OnButtonHover);
                button.Hide();
            }
        }

        private void SetButtonColor(ButtonController buttonController, Color color, Color? hoverColor=null)
        {
            buttonController.GetComponent<ButtonColorBehaviour>().DefaultColor = color;

            if (hoverColor == null)
            {
                buttonController.GetComponent<ButtonColorBehaviour>().hoverColor = color;  // disabling the hover color
            }
            else
            {
                buttonController.GetComponent<ButtonColorBehaviour>().hoverColor = (Color) hoverColor;
            }
        }

        #endregion
    }

    public class InSituCompBlockData: BlockData
    {
        public int numTrials;
        public List<string> useButtons;
        public float buttonSize;
        public string handedness;
        public bool changeLayout;

        public override string ToString()
        {
            return
                base.ToString() +
                $"Number of Trials: {numTrials}" +
                $"Button size: {buttonSize}" +
                $"Buttons used: {useButtons}" +
                $"Buttons used: {handedness}" +
                $"Change Layout: {changeLayout}";
        }
    }
}
