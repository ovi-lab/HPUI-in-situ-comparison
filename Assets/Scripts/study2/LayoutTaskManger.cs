using System.Linq;
using ubco.ovilab.HPUI.Interaction;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace ubco.ovilab.hpuiInSituComparison.study2
{
    public class LayoutTaskManger : MonoBehaviour
    {
        public GameObject buttonPrefab;
        public int buttonsPerWindow = 6;
        public int defaultOffset = 0;
        public WindowManager activeWindowManager;

        /// <summary>
        /// Sets up corresponding frames.
        /// </summary>
        public void Setup(int numberOfWindows)
        {
            activeWindowManager.Windows = Enumerable.Range(1, numberOfWindows)
                .Select(i => GenerateWindow(i, buttonsPerWindow, transform, OnTap)).ToList();
            activeWindowManager.SetupWindows(0);
        }

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


        // TODO: For debugging
        public void Start()
        {
            activeWindowManager.SetupFrames();
            Setup(7);
        }

        public void OnTap(HPUITapEventArgs args)
        {
        }
    }
}
