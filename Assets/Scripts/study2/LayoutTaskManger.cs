using System;
using System.Collections.Generic;
using System.Linq;
using ubco.ovilab.HPUI.Interaction;
using ubco.ovilab.hpuiInSituComparison.common;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UXF;

namespace ubco.ovilab.hpuiInSituComparison.study2
{
    public class LayoutTaskManger : MonoBehaviour
    {
        [Tooltip("The button prefab used to populate a window.")]
        [SerializeField] private GameObject buttonPrefab;
        [Tooltip("The number of buttons to be populated in a window.")]
        [SerializeField] private int buttonsPerWindow = 6;
        [Tooltip("The number of total windows to genrated.")]
        [SerializeField] private int numberOfWindows = 7;
        [Tooltip("The offset on the list of frames. The windows will be shifted by this number.")]
        [SerializeField] private int defaultOffset = 0;
        [Tooltip("The list of window managers and their corresponding ids.")]
        [SerializeField] private List<WindowManager> windowManagers;
        [Tooltip("The workspace object with all the targets.")]
        [SerializeField] private GameObject workspace;
        [Tooltip("The extent of the workspace Bounds")]
        [SerializeField] private Vector3 workspaceExtents;
        [Tooltip("The peg objects.")]
        [SerializeField] private List<PegV2> pegs;

        // The min and max scale to set for the target
        [Tooltip("Min scale of targets")]
        [SerializeField] private float minScale;
        [Tooltip("Max scale of targets")]
        [SerializeField] private float maxScale;
        [Tooltip("The start zoom level to set for the workspace.")] // NOTE: This may be removed in future. See `Scale`.
        [SerializeField] private float startZoomScale = 0.5f;

        public float Scale
        {
            get {
                return (workspace.transform.localScale[0] - minScale) / (maxScale - minScale);
            }
            set {
                float scale = Math.Clamp(value, 0, 1) * (maxScale - minScale) + minScale;
                workspace.transform.localScale = Vector3.one * scale;
                // NOTE: Leaving these in here to allow scaling behaviour if needed.
                foreach (Target target in targets)
                {
                    target.SetMainAsActiveDisplayElement(true);
                }

                foreach (PegV2 peg in pegs)
                {
                    peg.Scale = scale;
                }
            }
        }

        private WindowManager activeWindowManager;
        private List<Target> targets;
        private List<InteractablesWindow> windows = new List<InteractablesWindow>();
        private List<int> activeColorLayout;
        private Dictionary<InteractableTracker, int> buttonToColorMapping;
        private List<List<int>> sequences;
        private List<List<Vector3>> sequencesLocations;
        private int currentColorIndex, currentSequenceIndex = -1, activeColorLayoutIndex = -1;
        private Target currentTarget;
        private PegV2 currentPeg;
        private Trial currentTrial;
        private UXFDataTable buttonSelectionsTable;

        private void Start()
        {
            targets = workspace.GetComponentsInChildren<Target>().ToList();
            Debug.Assert(targets.Count == pegs.Count);

            Session session = Session.instance;
            session.onBlockEnd.AddListener(OnBlockEnd);
            session.onTrialBegin.AddListener(OnTrialBegin);

            windows.Clear();
            windows.AddRange(Enumerable.Range(1, numberOfWindows)
                             .Select(i => GenerateWindow(i, buttonsPerWindow, transform, OnTap)));

            foreach (WindowManager windowManager in windowManagers)
            {
                windowManager.SetupFrames();
                windowManager.Disable(); // The active one will get enabled in due time!
            }
#if !UNITY_EDITOR
            debug = false;
#endif
        }

        #region Task configurations
        /// <summary>
        /// Sets up corresponding frames.
        /// </summary>
        public void ConfigureTaskBlock(Block block, System.Random random, InSituLayoutCompBlockData blockData, bool lastBlockCancelled)
        {
            foreach (WindowManager windowManager in windowManagers)
            {
                if (blockData.windowManager == windowManager.id)
                {
                    activeWindowManager = windowManager;
                }
                else
                {
                    windowManager.Disable();
                }
            }

            foreach(InteractableTracker interactable in GetWindowsInteractables())
            {
                interactable.GetComponentInChildren<SpriteRenderer>().sprite = ColorIndex.instance.defaultSprite;
            }

            activeWindowManager.Windows = windows;
            activeWindowManager.SetupWindows(0); // TODO: Should this random instead of 0?

            // If last block was canceled, we are redoing the same block => don't changelayout again
            if ((blockData.changeLayout && !lastBlockCancelled) || activeColorLayout == null)
            {
                int newColorLayoutIndex;
                do
                {
                    newColorLayoutIndex = random.Next(ColorIndex.instance.Count());
                } while (newColorLayoutIndex == activeColorLayoutIndex);

                activeColorLayoutIndex = newColorLayoutIndex;

                int newColorIndex;
                List<int> newColorLayout = new List<int>();

                // NOTE: Assuming there are enough colors in the color index
                // to have two completely different layouts
                for(int j = 0; j < blockData.numberOfColors; j++)
                {
                    do
                    {
                        newColorIndex = random.Next(ColorIndex.instance.Count(activeColorLayoutIndex));
                    } while (newColorLayout.Contains(newColorIndex));
                    newColorLayout.Add(newColorIndex);
                }

                activeColorLayout = newColorLayout;

                buttonToColorMapping.Clear();

                // Determine wich of the interactables in the windows are to be targets.
                List<InteractableTracker> interactables = new List<InteractableTracker>();

                // Trying to uniformly sample from all windows.
                // KLUDGE: maybe rethink the distribution of targest?
                float pColorPerWindow = blockData.numberOfColors / windows.Count;
                bool firstIteration = true;
                do
                {
                    foreach (InteractablesWindow window in windows.OrderBy(id => random.Next()))
                    {
                        if (interactables.Count == blockData.numberOfColors)
                            break;

                        float currentP = firstIteration ? pColorPerWindow : pColorPerWindow % 1;
                        do
                        {
                            if (interactables.Count == blockData.numberOfColors)
                                break;

                            if (currentP > 1 || random.NextDouble() < currentP)
                            {
                                interactables.Add(window.interactables[random.Next(window.interactables.Count)]);
                            }
                        } while (--currentP > 1);
                    }
                    firstIteration = true;
                } while (interactables.Count != blockData.numberOfColors);

                for (int j = 0; j < interactables.Count; j++)
                {
                    int colorIndex = activeColorLayout[j];
                    InteractableTracker interactable = interactables[j];
                    interactable.spriteRenderer.sprite = ColorIndex.instance.GetSprite(activeColorLayoutIndex, colorIndex);
                    buttonToColorMapping.Add(interactable, colorIndex);
                }
            }
            currentSequenceIndex = -1; // Make sure the first trial gets setup correctly

            sequences = new List<List<int>>();
            sequencesLocations = new List<List<Vector3>>();
            for (int i = 0; i < blockData.numTrials; ++i)
            {
                List<int> sequence = new List<int>();
                List<int> selectedIndices = new List<int>();
                List<int> selectedColorIndices = new List<int>();
                List<Vector3> selectedPosition = new List<Vector3>();

                int targetIndex, colorIndex;
                Vector3 position;

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
                }
                sequences.Add(sequence);
                sequencesLocations.Add(selectedPosition);
            }

            InitTargetsAndPegs();
            // TODO: Log the mapping of buttons to colors
        }

        private void OnTap(HPUITapEventArgs args)
        {
            // TODO: task specific setup
            // TODO: handle buttonSelectionsTable
            // TODO: start next trial when appropriate
        }
        #endregion

        #region UXF functions
        private void OnTrialBegin(Trial trial)
        {
            if (!activeWindowManager.isActiveAndEnabled)
            {
                activeWindowManager.Enable();
            }

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
                Scale = startZoomScale;

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

        private void OnBlockEnd(Block block)
        {
            currentTrial = null;
            foreach (PegV2 peg in pegs)
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
        #endregion

        #region helper functions
        /// <summary>
        /// Generates a window.
        /// </summary>
        public InteractablesWindow GenerateWindow(int windowIndex, int buttonsPerWindow, Transform parentTransform, UnityAction<HPUITapEventArgs> OnTap)
        {
            GameObject windowGameObject = new GameObject($"WindowSet_{windowIndex}");
            windowGameObject.transform.parent = parentTransform;
            InteractablesWindow interactablesWindow = windowGameObject.AddComponent<InteractablesWindow>();
            interactablesWindow.interactables = Enumerable.Range(1, buttonsPerWindow)
                .Select(i =>
                {
                    GameObject interactableObj = GameObject.Instantiate(buttonPrefab, windowGameObject.transform);
                    interactableObj.GetComponent<HPUIBaseInteractable>().TapEvent.AddListener(OnTap);
                    return interactableObj.AddComponent<InteractableTracker>();
                })
                .ToList();
            return interactablesWindow;
        }

        /// <summary>
        /// Get all interactables related to the task.
        /// </summary>
        public IEnumerable<IHPUIInteractable> GetInteractables()
        {
            return GetWindowsInteractables()
                .Select(i => i.interactable)
                .Union(windowManagers
                       .SelectMany(wm => wm.GetManagedInteractables()));
        }

        /// <summary>
        /// Get all active intractables.
        /// </summary>
        public IEnumerable<IHPUIInteractable> GetAvtiveInteractables()
        {
            return GetWindowsInteractables()
                .Select(i => i.interactable)
                .Union(activeWindowManager.GetManagedInteractables());
        }

        /// <summary>
        /// Get all interactables in all windows.
        /// </summary>
        private IEnumerable<InteractableTracker> GetWindowsInteractables()
        {
            return windows
                .SelectMany(w => w.interactables);
        }

        private void InitTargetsAndPegs()
        {
            List<Vector3> targetPositions;
            if (currentSequenceIndex != -1)
            {
                targetPositions = sequencesLocations[currentSequenceIndex];
            }
            else
            {
                targetPositions = sequencesLocations.First();
            }
            for (int i = 0; i < targets.Count; i++)
            {
                Target target = targets[i];
                target.Visible = true;
                target.Active = false;
                target.Position = targetPositions[i];
            }

            foreach (PegV2 peg in pegs)
            {
                peg.Active = false;
                peg.Visible = false;
            }
        }
        #endregion
    }
}
