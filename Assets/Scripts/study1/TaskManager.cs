using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using UXF;
using ubco.ovilab.HPUI.Legacy;

namespace ubco.ovilab.hpuiInSituComparison.study1
{
    public class TaskManager : MonoBehaviour
    {
        // The min and max scale to set for the target
        public float minScale, maxScale;
        // At which scale values to show the display and how big is the window
        // Values in 0-1;
        [Range(0,1)]
        public float secondDisplayVisibleScaleWindow;
        [Range(0,1)]
        public float secondDisplayVisibleStartAtScale;

        public Transform workspace;
        [Tooltip("The extent of the workspace Bounds")]
        public Vector3 workspaceExtents;
        public GameObject pegsRoot;
        [SerializeField]
        List<ButtonGroup> buttonGroups;

        public AudioClip failAudio;
        public AudioClip successAudio;
        public AudioSource audioSource;

        public Transform leftIndex, rightIndex;

        public bool debug = false;

        public float Scale
        {
            get {
                return (workspace.transform.localScale[0] - minScale) / (maxScale - minScale);
            }
            set {
                float scale = Math.Clamp(value, 0, 1) * (maxScale - minScale) + minScale;
                workspace.transform.localScale = Vector3.one * scale;
                bool visibilityState = value > secondDisplayVisibleStartAtScale && value < secondDisplayVisibleStartAtScale + secondDisplayVisibleScaleWindow;
                foreach (Target target in targets)
                {
                    target.SetMainAsActiveDisplayElement(!visibilityState);
                }

                foreach (Peg peg in pegs)
                {
                    peg.Scale = scale;
                }
            }
        }

        #region private_variables
        private List<ButtonGroup> activeButtonGroups = new List<ButtonGroup>();
        private List<Target> targets;
        private List<Peg> pegs;
        private List<List<int>> sequences;
        private List<List<Vector3>> sequencesLocations;
        private int currentColorIndex, currentSequenceIndex = -1, activeColorLayoutIndex = -1;
        private Target currentTarget;
        private Peg currentPeg;
        private Trial currentTrial;
        private UXFDataTable buttonSelectionsTable;
        private List<int> activeColorLayout;
        private Dictionary<ButtonController, (int index, string groupName)> buttonToColorMapping = new Dictionary<ButtonController, (int, string)>();
        #endregion

        #region UNITY_FUNCTIONS
        private void Start()
        {
            targets = workspace.GetComponentsInChildren<Target>().ToList();
            pegs = pegsRoot.GetComponentsInChildren<Peg>().ToList();

            Session session = Session.instance;
            session.onBlockEnd.AddListener(OnBlockEnd);
            session.onTrialBegin.AddListener(OnTrialBegin);
#if !UNITY_EDITOR
            debug = false;
#endif
        }
        #endregion

        #region Setting up tasks
        /// <summary>
        /// Picks all active button groups
        /// </summary>
        private void SetActiveButtonGroup()
        {
            activeButtonGroups.Clear();
            foreach (ButtonGroup bg in buttonGroups)
            {
                if (bg.IsActive())
                {
                    activeButtonGroups.Add(bg);
                }
            }
            InteractionManger.instance.GetButtons();
        }

        // NOTE: This is not added to the session.onBlockBeing as this depends on information
        // the experiment manager gets
        public void ConfigureTaskBlock(Block block, System.Random random, InSituCompBlockData el, bool lastBlockCancelled)
        {
            SetActiveButtonGroup();

            foreach(ButtonGroup activeButtonGroup in buttonGroups)
            {
                if (activeButtonGroup.BindCallbacks)
                {
                    activeButtonGroup.zoomDownButton?.contactAction.AddListener(ZoomDownButtonContact);
                    activeButtonGroup.zoomUpButton?.contactAction.AddListener(ZoomUpButtonContact);
                    if (activeButtonGroup.zoomSlider != null)
                    {
                        activeButtonGroup.zoomSlider.OnSliderEventChange += ZoomSliderChange;
                    }
                    activeButtonGroup.acceptButton.contactAction.AddListener(AcceptButtonContact);
                    foreach (ButtonController btn in activeButtonGroup.colorButtons)
                    {
                        btn.contactAction.AddListener(ColorButtonContact);
                    }
                }
            }

            // If last block was canceled, we are redoing the same block => don't changelayout again
            if ((el.changeLayout && !lastBlockCancelled) || activeColorLayout == null)
            {
                int newColorLayoutIndex;
                do
                {
                    newColorLayoutIndex = random.Next(ColorIndex.instance.Count());
                } while (newColorLayoutIndex == activeColorLayoutIndex);

                activeColorLayoutIndex = newColorLayoutIndex;

                int newColorIndex;
                List<int> newColorLayout = new List<int>();

                // Make sure the number of color buttons are equal in all groups
                if (activeButtonGroups.Select(x => x.colorButtons.Count).Distinct().Count() != 1)
                {
                    throw new Exception($"Active button" + string.Join(",",activeButtonGroups.Select(x=>x.name)) +  "groups have different counts");
                }

                // NOTE: Assuming there are enough colors in the color index
                // to have two completely different layouts
                for(int j = 0; j < activeButtonGroups[0].colorButtons.Count; j++)
                {
                    do
                    {
                        newColorIndex = random.Next(ColorIndex.instance.Count(activeColorLayoutIndex));
                    } while (newColorLayout.Contains(newColorIndex));
                    newColorLayout.Add(newColorIndex);
                }

                activeColorLayout = newColorLayout;

                buttonToColorMapping.Clear();
                foreach(ButtonGroup activeButtonGroup in activeButtonGroups)
                {
                    for (int j = 0; j < activeButtonGroup.colorButtons.Count; j++)
                    {
                        int colorIndex = activeColorLayout[j];
                        ButtonController btn = activeButtonGroup.colorButtons[j];
                        btn.GetComponent<SpriteRenderer>().sprite = ColorIndex.instance.GetSprite(activeColorLayoutIndex, colorIndex);
                        buttonToColorMapping.Add(btn, (colorIndex, activeButtonGroup.name));
                    }
                }
            }
            currentSequenceIndex = -1; // Make sure the first trial gets setup correctly

            sequences = new List<List<int>>();
            sequencesLocations = new List<List<Vector3>>();
            for (int i = 0; i < el.numTrials; ++i)
            {
                List<int> sequence = new List<int>();
                List<int> selectedIndices = new List<int>();
                List<int> selectedColorIndices = new List<int>();
                List<Vector3> selectedPosition = new List<Vector3>();

                int targetIndex, colorIndex;
                Vector3 position;

                float secondDisplayStartAtScale, startZoom;
                if (el.startZoomAbove)
                {
                    secondDisplayStartAtScale = (float)Math.Clamp(random.NextDouble(), secondDisplayVisibleScaleWindow, 0.5f);
                    startZoom = (float)Math.Clamp(random.NextDouble(), secondDisplayStartAtScale, 1);
                }
                else
                {
                    secondDisplayStartAtScale = (float)Math.Clamp(random.NextDouble(), 0.5f, 1 - secondDisplayVisibleScaleWindow);
                    startZoom = (float)Math.Clamp(random.NextDouble(), 0, secondDisplayStartAtScale - 0.1f);
                }

                for (int j = 0; j < targets.Count; j++)
                {
                    Trial trial = block.CreateTrial();

                    do
                    {
                        targetIndex = random.Next(targets.Count);
                    } while (selectedIndices.Contains(targetIndex));

                    selectedIndices.Add(targetIndex);

                    sequence.Add(targetIndex);

                    // Avoid using the same color in consequetive trials
                    do
                    {
                        colorIndex = activeColorLayout[random.Next(activeColorLayout.Count)];
                    } while (selectedColorIndices.Contains(colorIndex));

                    selectedColorIndices.Add(colorIndex);

                    int x = 0;
                    do
                    {
                        position = new Vector3(UnityEngine.Random.value * workspaceExtents.x * 2 - workspaceExtents.x,
                                               0,
                                               UnityEngine.Random.value * workspaceExtents.y * 2 - workspaceExtents.y);
                    } while (selectedPosition.Any(pos => (pos - position).magnitude < 0.08));

                    selectedPosition.Add(position);

                    trial.settings.SetValue("colorIndex", colorIndex);
                    trial.settings.SetValue("colorGroupIndex", activeColorLayoutIndex);
                    trial.settings.SetValue("targetIndex", targetIndex);
                    trial.settings.SetValue("targetLocation", position);
                    trial.settings.SetValue("sequenceIndex", i);
                    trial.settings.SetValue("inSequenceIndex", j);
                    trial.settings.SetValue("startZoomScale", startZoom);
                    trial.settings.SetValue("secondDisplayVisibleStartAtScale", secondDisplayStartAtScale);
                    trial.settings.SetValue("secondDisplayVisibleScaleWindow", secondDisplayVisibleScaleWindow);
                }
                sequences.Add(sequence);
                sequencesLocations.Add(selectedPosition);
            }

            InitTargetsAndPegs(el.handedness);

            // Logging the function mappings
            Dictionary<string, string> mappings = new Dictionary<string, string>();

            foreach (ButtonGroup activeButtonGroup in activeButtonGroups)
            {
                string _name = activeButtonGroup.name;
                mappings.Add($"{_name}::" + activeButtonGroup.acceptButton.name, "accept");

                if (activeButtonGroup.zoomUpButton != null)
                {
                    mappings.Add($"{_name}::" + activeButtonGroup.zoomUpButton.name, "zoomUp");
                }
                if (activeButtonGroup.zoomDownButton != null)
                {
                    mappings.Add($"{_name}::" + activeButtonGroup.zoomDownButton.name, "zoomDown");
                }
                if (activeButtonGroup.zoomSlider != null)
                {
                    mappings.Add($"{_name}::" + activeButtonGroup.zoomSlider.name, "zoomSlider");
                }
            }

            foreach (KeyValuePair<ButtonController, (int, string)> kvp in buttonToColorMapping)
            {
                mappings.Add($"{kvp.Value.Item2}::" + kvp.Key.name, kvp.Value.Item1.ToString());
            }
            block.settings.SetValue("buttonToFunctionMapping", mappings);
        }

        public List<ButtonController> GetActiveButtons()
        {
            return activeButtonGroups.Where(g => g.BindCallbacks).SelectMany(g => g.GetActiveButtons()).ToList();
        }

        public List<Slider> GetActiveSliders()
        {
            List<Slider> sliders = new List<Slider>();
            foreach(ButtonGroup activeButtonGroup in activeButtonGroups)
            {
                if (activeButtonGroup.zoomSlider != null)
                {
                    sliders.Add(activeButtonGroup.zoomSlider);
                }
            }
            return sliders;
        }
        #endregion

        #region UXF functions
        public void OnTrialBegin(Trial trial)
        {
            buttonSelectionsTable = new UXFDataTable("time","buttonName", "function", "value");
            currentTrial = trial;

            int sequenceIndex = trial.settings.GetInt("sequenceIndex");
            int inSequenceIndex = trial.settings.GetInt("inSequenceIndex");
            int targetIndex = trial.settings.GetInt("targetIndex");
            currentColorIndex = trial.settings.GetInt("colorIndex");

            if (currentTarget != null)
            {
                currentTarget.Active = false;
                currentTarget.Visible = false;
            }
            if (currentPeg != null)
            {
                currentPeg.Active = false;
                currentPeg.Visible = false;
            }

            if (currentSequenceIndex != sequenceIndex)
            {
                secondDisplayVisibleStartAtScale = trial.settings.GetFloat("secondDisplayVisibleStartAtScale");
                float newScale = trial.settings.GetFloat("startZoomScale");
                Scale = newScale;

                foreach (ButtonGroup activeButtonGroup in activeButtonGroups)
                {
                    if (activeButtonGroup.zoomSlider != null)
                    {
                        activeButtonGroup.zoomSlider.SetSliderValue(newScale);
                    }
                }

                currentSequenceIndex = sequenceIndex;
                InitTargetsAndPegs();
            }

            currentTarget = targets[targetIndex];
            currentTarget.DisplayColorGroupIndex = activeColorLayoutIndex;
            currentTarget.DisplayColorIndex = currentColorIndex;
            currentTarget.Active = true;

            currentPeg = pegs[targetIndex];
            currentPeg.Active = true;
            currentPeg.Visible = true;
        }

        public void OnBlockEnd(Block block)
        {
            currentTrial = null;
            foreach(ButtonGroup activeButtonGroup in activeButtonGroups)
            {
                if (activeButtonGroup.BindCallbacks)
                {
                    if (activeButtonGroup.zoomDownButton != null)
                    {
                        activeButtonGroup.zoomDownButton.contactAction.RemoveListener(ZoomDownButtonContact);
                    }
                    if (activeButtonGroup.zoomUpButton != null)
                    {
                        activeButtonGroup.zoomUpButton.contactAction.RemoveListener(ZoomUpButtonContact);
                    }
                    activeButtonGroup.acceptButton.contactAction.RemoveListener(AcceptButtonContact);
                    foreach (ButtonController btn in activeButtonGroup.colorButtons)
                    {
                        btn.contactAction.RemoveListener(ColorButtonContact);
                    }

                    if (activeButtonGroup.zoomSlider != null)
                    {
                        activeButtonGroup.zoomSlider.OnSliderEventChange -= ZoomSliderChange;
                    }
                }
            }

            foreach (Peg peg in pegs)
            {
                peg.Active = false;
                peg.Visible = false;
            }

            foreach (Target target in targets)
            {
                target.Active = false;
                target.Visible = false;
            }
        }

        public void AddButtonSelectionToTable(string btn, string function, float val)
        {
            UXFDataRow row = new UXFDataRow(){
                ("time", Time.time),
                ("buttonName", btn),
                ("function", function),
                ("value", val)
            };
            buttonSelectionsTable.AddCompleteRow(row);
        }
        #endregion

        #region Button callbacks
        private void ColorButtonContact(ButtonController btn)
        {
            int colorIndex = buttonToColorMapping[btn].index;
            AddButtonSelectionToTable(btn.name, "color", colorIndex);
            currentPeg.DisplayColorGroupIndex = activeColorLayoutIndex;
            currentPeg.DisplayColorIndex = colorIndex;
        }

        private void ZoomUpButtonContact(ButtonController btn)
        {
            Scale += 0.01f;
            AddButtonSelectionToTable(btn.name, "zoomUp", Scale);
        }

        private void ZoomDownButtonContact(ButtonController btn)
        {
            Scale -= 0.01f;
            AddButtonSelectionToTable(btn.name, "zoomDown", Scale);
        }

        private void ZoomSliderChange(float val, Slider slider)
        {
            // val is between 0-1
            Scale = val;
            AddButtonSelectionToTable(slider.name, "zoomSlider", Scale);
        }

        private void AcceptButtonContact(ButtonController btn)
        {
            if (currentPeg.DisplayColorIndex == currentColorIndex && currentTarget.IsSelected)
            {
                AddButtonSelectionToTable(btn.name, "accept", 1);
                currentTrial.SaveDataTable(buttonSelectionsTable, "buttonSelections");
                audioSource.PlayOneShot(successAudio);
                try
                {
                    Session.instance.EndCurrentTrial();
                    Session.instance.BeginNextTrial();
                }
                catch (NoSuchTrialException)
                {
                    Debug.Log($"Session ended. (probably?)");
                }
            }
            else
            {
                AddButtonSelectionToTable(btn.name, "accept", 0);
                audioSource.PlayOneShot(failAudio);
            }
        }
        #endregion

        #region Helper functions
        private void InitTargetsAndPegs(string handedness="None")
        {
            List<Vector3> targetPositions;
            if (currentSequenceIndex != -1)
            {
                targetPositions = sequencesLocations[currentSequenceIndex];
            }
            else
            {
                targetPositions = sequencesLocations.Last();
            }
            for (int i = 0; i < targets.Count; i++)
            {
                Target target = targets[i];
                target.Visible = true;
                target.Active = false;
                target.Position = targetPositions[i];
            }

            foreach (Peg peg in pegs)
            {
                peg.Active = false;
                peg.Visible = false;
                if (handedness == "left" && !debug)
                {
                    peg.trackingObject = rightIndex;
                }
                else if (handedness == "right" && !debug)
                {
                    peg.trackingObject = leftIndex;
                }
            }
        }
        #endregion

#if UNITY_EDITOR
        public void PegInTheHole()
        {
            currentPeg.transform.position = currentTarget.transform.position;
        }

        public void SetCorrectColor()
        {
            ButtonController btn = buttonToColorMapping.Where(kvp => kvp.Value.index == currentColorIndex).Select(kvp => kvp.Key).First();
            ButtonController.TriggerTargetButton(btn);
        }

        public void SetCorrectScale()
        {
            Scale = secondDisplayVisibleStartAtScale + secondDisplayVisibleScaleWindow / 2;
        }

        public void AcceptCorrect()
        {
            ButtonController.TriggerTargetButton(activeButtonGroups[0].acceptButton);
        }

        public void CompleteStep()
        {
            SetCorrectColor();
            SetCorrectScale();
            PegInTheHole();
            AcceptCorrect();
        }
#endif

        [Serializable]
        class ButtonGroup
        {
            public string name;
            public Slider zoomSlider;
            public ButtonController zoomUpButton, zoomDownButton, acceptButton;
            public List<ButtonController> colorButtons;
            public bool BindCallbacks = true;

            public bool IsActive()
            {
                return
                    (zoomUpButton != null &&
                     zoomUpButton.transform.root.gameObject.activeSelf &&
                     zoomDownButton != null &&
                     zoomDownButton.transform.root.gameObject.activeSelf ||
                     zoomSlider != null &&
                     zoomSlider.gameObject.activeSelf) &&
                    acceptButton.transform.root.gameObject.activeSelf &&
                    colorButtons.Aggregate(
                        true,
                        (active, current) => active && current.transform.root.gameObject.activeSelf);
            }

            public bool ButtonInGroup(ButtonController btn)
            {
                return
                    zoomUpButton == btn ||
                    zoomDownButton == btn ||
                    acceptButton == btn ||
                    colorButtons.Aggregate(
                        true,
                        (active, current) => active || current == btn);
            }

            public List<ButtonController> GetActiveButtons()
            {
                List<ButtonController> btns = new List<ButtonController>();
                if (zoomDownButton != null)
                {
                    btns.Add(zoomDownButton);
                }
                if (zoomUpButton != null)
                {
                    btns.Add(zoomUpButton);
                }
                btns.Add(acceptButton);
                btns.AddRange(colorButtons);
                return btns;
            }
        }
    }
}
