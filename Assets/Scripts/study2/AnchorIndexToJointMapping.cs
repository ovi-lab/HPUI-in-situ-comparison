using System.Collections.Generic;
using ubco.ovilab.HPUI.Tracking;
using UnityEngine;
using UnityEngine.XR.Hands;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ubco.ovilab.hpuiInSituComparison.study2
{
    public class AnchorIndexToJointMapping: MonoBehaviour
    {
        [SerializeField]
        private Handedness handedness;
        public List<JointFollowerDatum> jointDataBeingLoaded;
        private List<string> defaultJointDataNames = new List<string>()
        {
            "IndexDistal.asset",
            "IndexIntermediate.asset",
            "IndexProximal.asset",
            "MiddleDistal.asset",
            "MiddleIntermediate.asset",
            "MiddleProximal.asset",
            "RingDistal.asset",
            "RingIntermediate.asset",
            "RingProximal.asset",
            "LittleDistal.asset",
            "LittleIntermediate.asset",
            "LittleProximal.asset"
        };

        [HideInInspector] private List<JointFollower> jointFollowers;

        public List<JointFollower> JointFollowers {
            get {
                if (jointFollowers == null)
                {
                    Populate();
                }
                return jointFollowers;
            }
            private set => jointFollowers = value; }

#if UNITY_EDITOR
        protected void OnValidate()
        {
            string prefix = handedness switch
            {
                Handedness.Left => "L",
                Handedness.Right => "R",
                _ => ""
            };

            if (string.IsNullOrEmpty(prefix))
            {
                Debug.LogWarning($"Unknown handedness");
            }

            jointDataBeingLoaded.Clear();

            foreach (string assetSuffixName in defaultJointDataNames)
            {
                string assetPath = $"Packages/ubc.ok.ovilab.hpui-core/Runtime/Resources/DefaultJointData/{prefix}{assetSuffixName}";
                JointFollowerDatum followerData = (JointFollowerDatum)AssetDatabase.LoadAssetAtPath(assetPath, typeof(JointFollowerDatum));
                if (followerData == null)
                {
                    Debug.LogWarning($"{assetPath} missing! Ignoring {assetSuffixName} for {handedness}");
                }
                else
                {
                    jointDataBeingLoaded.Add(followerData);
                }
            }
        }
#endif

        protected void Populate()
        {
            jointFollowers = new List<JointFollower>();
            foreach (JointFollowerDatum datum in jointDataBeingLoaded)
            {
                GameObject obj = new GameObject(datum.name);
                obj.transform.parent = transform;
                JointFollower follower = obj.AddComponent<JointFollower>();
                follower.SetData(datum.Value);
                jointFollowers.Add(follower);
            }
        }
    }
}
