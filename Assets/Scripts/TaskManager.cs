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

        public bool debug = false;

        #region private_variables
        private ButtonGroup activeButtonGroup;
        private List<Target> targets;
        private List<Peg> pegs;
        private List<List<int>> sequences;
        private int currentColorIndex, currentSequenceIndex;
        private Target currentTarget;
        private Peg currentPeg;
        private List<int> activeColorLayout;
        private Dictionary<ButtonController, int> buttonToColorMapping;
        #endregion

        public float Scale
        {
            get {
                return transform.localScale[0];
            }
            set {
                float scale = Math.Clamp(value, 0, 1);
                transform.localScale = Vector3.one * scale;
                bool visibilityState = scale > secondDisplayVisibleStartAtScale && scale < secondDisplayVisibleStartAtScale + secondDisplayVisibleScaleWindow;
                foreach (Target target in targets)
                {
                    target.SetMainAsActiveDisplayElement(!visibilityState);
                }
            }
        }

        private void Start()
        {
            targets = workspace.GetComponentsInChildren<Target>().ToList();
            pegs = pegsRoot.GetComponentsInChildren<Peg>().ToList();
        }

        private void Update()
        {
            // NOTE: This is used for debugging purposes
            if (debug && workspace.hasChanged)
            {
                Scale = workspace.localScale[0];
            }
        }

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
        }

        // TODO randomize target location
        public void ConfigureTaskBlock(Block block, System.Random random, int numTrials, bool changeLayout)
        {
            SetActiveButtonGroup();
            foreach (Target target in targets)
            {
                target.Visible = true;
                target.Active = false;
            }

            foreach (Peg peg in pegs)
            {
                peg.Active = false;
                peg.Visible = false;
            }

            if (changeLayout || activeColorLayout == null)
            {
                int newColorIndex;
                bool done = false;
                List<int> newColorLayout = new List<int>();

                // NOTE: Assuming there are enough colors in the color index
                // to have two completely different layouts
                for(int j = 0; j < activeButtonGroup.colorButtons.Count; j++)
                {
                    do
                    {
                        newColorIndex = random.Next(ColorIndex.instance.Count());
                    } while (activeButtonGroup != null && activeColorLayout.Contains(newColorIndex));
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

            sequences = new List<List<int>>();
            for (int i = 0; i < numTrials; ++i)
            {
                List<int> sequence = new List<int>();
                List<int> selectedIndices = new List<int>();
                List<int> selectedColorIndices = new List<int>();
                int targetIndex, colorIndex;
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
                        colorIndex = random.Next(activeColorLayout.Count);
                    } while (selectedColorIndices.Contains(colorIndex));

                    selectedColorIndices.Add(colorIndex);


                    trial.settings.SetValue("colorIndex", colorIndex);
                    trial.settings.SetValue("targetIndex", targetIndex);
                    trial.settings.SetValue("targetLocation", targets[targetIndex].Position);
                    trial.settings.SetValue("sequenceIndex", i);
                    trial.settings.SetValue("inSequenceIndex", j);
                }
                sequences.Add(sequence);
            }
        }

        public void SetupTrial(Trial trial)
        {
            int sequenceIndex = trial.settings.GetInt("sequenceIndex");
            int inSequenceIndex = trial.settings.GetInt("inSequenceIndex");
            int targetIndex = trial.settings.GetInt("targetIndex");
            currentColorIndex = trial.settings.GetInt("colorIndex");

            if (currentSequenceIndex != sequenceIndex)
            {
                currentTarget.Active = false;
                currentTarget.Visible = false;
                currentPeg.Active = false;
                currentPeg.Visible = false;

                currentSequenceIndex = sequenceIndex;
                currentTarget = targets[targetIndex];
                currentTarget.DisplayColorIndex = currentColorIndex;
                currentTarget.Active = true;

                currentPeg = pegs[targetIndex];
                currentPeg.Active = true;
            }
        }

        [Serializable]
        class ButtonGroup
        {
            public string name;
            public ButtonController zoomUpButton, zoomDownButton, acceptButton;
            public List<ButtonController> colorButtons;

            public bool IsActive()
            {
                // FIXME somewhere else ensure all of em have the same root and check once here?
                return
                    zoomUpButton.transform.root.gameObject.activeSelf &&
                    zoomDownButton.transform.root.gameObject.activeSelf &&
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
