using System.Linq;
using ubco.ovilab.HPUI.Interaction;
using UnityEditor;
using UnityEngine;

namespace ubco.ovilab.hpuiInSituComparison.study2
{
    public class LayoutTaskManger : MonoBehaviour
    {
        public GameObject buttonPrefab;
        public int buttonsPerWindow = 6;

        // FIXME: Adding these for debuging
        public int defaultOffset = 0;
        public WindowManager activeWindowManager;

        public void Setup(int numberOfWindows)
        {
            activeWindowManager.windows = Enumerable.Range(1, buttonsPerWindow)
                .Select(i =>
                {
                    GameObject windowGameObject = new GameObject($"WindowSet_{i}");
                    windowGameObject.transform.parent = transform;
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
                }).ToList();
            activeWindowManager.SetupWindows(0);
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
