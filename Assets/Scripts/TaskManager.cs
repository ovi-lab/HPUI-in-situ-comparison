using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using UXF;
using ubc.ok.ovilab.HPUI.Core;

namespace ubc.ok.ovilab.hpuiInSituComparison.study1
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
        private ButtonGroup activeButtonGroup;
        private List<Target> targets;
        private List<Peg> pegs;
        private List<List<int>> sequences;
        private int currentColorIndex, currentSequenceIndex = -1;
        private Target currentTarget;
        private Peg currentPeg;
        private Trial currentTrial;
        private UXFDataTable buttonSelectionsTable;
        private List<int> activeColorLayout;
        private Dictionary<ButtonController, int> buttonToColorMapping = new Dictionary<ButtonController, int>();
        #endregion

        #region UNITY_FUNCTIONS
        private void Start()
        {
            targets = workspace.GetComponentsInChildren<Target>().ToList();
            pegs = pegsRoot.GetComponentsInChildren<Peg>().ToList();

            Session session = Session.instance;
            session.onBlockEnd.AddListener(OnBlockEnd);
            session.onTrialBegin.AddListener(OnTrialBegin);
        }
        #endregion

        #region Setting up tasks
        /// <summary>
        /// Picks the first actve button group
        /// </summary>
        private void SetActiveButtonGroup()
        {
            activeButtonGroup = null;
            foreach (ButtonGroup bg in buttonGroups)
            {
                if (bg.IsActive())
                {
                    activeButtonGroup = bg;
                    break;
                }
            }
            InteractionManger.instance.GetButtons();
        }

        // NOTE: This is not added to the session.onBlockBeing as this depends on information
        // the experiment manager gets
        // TODO randomize target location
        public void ConfigureTaskBlock(Block block, System.Random random, InSituCompBlockData el)
        {
            SetActiveButtonGroup();
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

            InitTargetsAndPegs(el.handedness);

            if (el.changeLayout || activeColorLayout == null)
            {
                int newColorIndex;
                List<int> newColorLayout = new List<int>();

                // NOTE: Assuming there are enough colors in the color index
                // to have two completely different layouts
                for(int j = 0; j < activeButtonGroup.colorButtons.Count; j++)
                {
                    do
                    {
                        newColorIndex = random.Next(ColorIndex.instance.Count());
                    } while (newColorLayout.Contains(newColorIndex) ||
                             activeColorLayout != null && activeColorLayout.Contains(newColorIndex));
                    newColorLayout.Add(newColorIndex);
                }

                activeColorLayout = newColorLayout;

                buttonToColorMapping.Clear();
                for(int j = 0; j < activeButtonGroup.colorButtons.Count; j++)
                {
                    int colorIndex = activeColorLayout[j];
                    ButtonController btn = activeButtonGroup.colorButtons[j];
                    btn.GetComponent<SpriteRenderer>().sprite = ColorIndex.instance.GetSprite(colorIndex);
                    buttonToColorMapping.Add(btn, colorIndex);
                }
            }
            currentSequenceIndex = -1; // Make sure the first trial gets setup correctly

            sequences = new List<List<int>>();
            for (int i = 0; i < el.numTrials; ++i)
            {
                List<int> sequence = new List<int>();
                List<int> selectedIndices = new List<int>();
                List<int> selectedColorIndices = new List<int>();
                int targetIndex, colorIndex;

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

                    trial.settings.SetValue("colorIndex", colorIndex);
                    trial.settings.SetValue("targetIndex", targetIndex);
                    trial.settings.SetValue("targetLocation", targets[targetIndex].Position);
                    trial.settings.SetValue("sequenceIndex", i);
                    trial.settings.SetValue("inSequenceIndex", j);
                    trial.settings.SetValue("startZoomScale", startZoom);
                    trial.settings.SetValue("secondDisplayVisibleStartAtScale", secondDisplayStartAtScale);
                    trial.settings.SetValue("secondDisplayVisibleScaleWindow", secondDisplayVisibleScaleWindow);
                }
                sequences.Add(sequence);
            }

            // Logging the function mappings
            Dictionary<string, string> mappings = new Dictionary<string, string>()
            {
                {activeButtonGroup.acceptButton.name, "accept"},
            };

            if (activeButtonGroup.zoomUpButton != null)
            {
                mappings.Add(activeButtonGroup.zoomUpButton.name, "zoomUp");
            }
            if (activeButtonGroup.zoomDownButton != null)
            {
                mappings.Add(activeButtonGroup.zoomDownButton.name, "zoomDown");
            }
            if (activeButtonGroup.zoomSlider != null)
            {
                mappings.Add(activeButtonGroup.zoomSlider.name, "zoomSlider");
            }

            foreach (KeyValuePair<ButtonController, int> kvp in buttonToColorMapping)
            {
                mappings.Add(kvp.Key.name, kvp.Value.ToString());
            }
            block.settings.SetValue("buttonToFunctionMapping", mappings);
        }

        public List<ButtonController> GetActiveButtons()
        {
            List<ButtonController> btns = new List<ButtonController>();
            if (activeButtonGroup.zoomDownButton != null)
            {
                btns.Add(activeButtonGroup.zoomDownButton);
            }
            if (activeButtonGroup.zoomUpButton != null)
            {
                btns.Add(activeButtonGroup.zoomUpButton);
            }
            btns.Add(activeButtonGroup.acceptButton);
            btns.AddRange(activeButtonGroup.colorButtons);
            return btns;
        }

        public List<Slider> GetActiveSliders()
        {
            List<Slider> sliders = new List<Slider>();
            if (activeButtonGroup.zoomSlider != null)
            {
                sliders.Add(activeButtonGroup.zoomSlider);
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
                if(activeButtonGroup.zoomSlider != null)
                {
                    activeButtonGroup.zoomSlider.SetSliderValue(newScale);
                }

                currentSequenceIndex = sequenceIndex;
                InitTargetsAndPegs();
            }

            currentTarget = targets[targetIndex];
            currentTarget.DisplayColorIndex = currentColorIndex;
            currentTarget.Active = true;

            currentPeg = pegs[targetIndex];
            currentPeg.Active = true;
            currentPeg.Visible = true;
        }

        public void OnBlockEnd(Block block)
        {
            currentTrial = null;
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
            int colorIndex = buttonToColorMapping[btn];
            AddButtonSelectionToTable(btn.name, "color", colorIndex);
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
                audioSource.PlayOneShot(successAudio);
            }
        }
        #endregion

        #region Helper functions
        private void InitTargetsAndPegs(string handedness="None")
        {
            foreach (Target target in targets)
            {
                target.Visible = true;
                target.Active = false;
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
            currentPeg.transform.position = currentTarget.Position;
        }

        public void SetCorrectColor()
        {
            ButtonController btn = buttonToColorMapping.Where(kvp => kvp.Value == currentColorIndex).Select(kvp => kvp.Key).First();
            ButtonController.TriggerTargetButton(btn);
        }

        public void SetCorrectScale()
        {
            Scale = secondDisplayVisibleStartAtScale + secondDisplayVisibleScaleWindow / 2;
        }

        public void AcceptCorrect()
        {
            ButtonController.TriggerTargetButton(activeButtonGroup.acceptButton);
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
        }
    }
}
