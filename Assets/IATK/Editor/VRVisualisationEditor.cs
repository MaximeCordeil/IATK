using UnityEngine;
using UnityEditor;
using Tilia.Interactions.Interactables.Interactables;
using Tilia.Interactions.Interactables.Interactables.Grab.Action;

namespace IATK
{
    [CustomEditor(typeof(VRVisualisation))]
    [CanEditMultipleObjects]
    public class VRVisualisationEditor : AbstractVisualisationEditor
    {
        private const string prefabInteractable = "Interactions.Interactable";
        private const string prefabInteractionsLinearJointDrive = "Interactions.LinearJointDrive";
        private const string assetSuffix = ".prefab";
        private const string interactableSuffix = "[Interactable]";

        [MenuItem("GameObject/IATK/VR Visualisation", false, 10)]
        static void CreateVRVisualisationPrefab()
        {
            GameObject obj = new GameObject("[IATK] New VR Visualisation");
            VRVisualisation vrVisComponent = obj.AddComponent<VRVisualisation>();
            Selection.activeGameObject = obj;
            MakeInteractable(vrVisComponent);
        }

        private static void MakeInteractable(VRVisualisation vrVisComponent)
        {
            GameObject linearJointDrivePrefab = GetPrefab(prefabInteractionsLinearJointDrive);
            vrVisComponent.linearJointDrivePrefab = linearJointDrivePrefab;

            GameObject interactablePrefab = GetPrefab(prefabInteractable);
            GameObject newInteractable = vrVisComponent.WrapIn(Instantiate(interactablePrefab), interactableSuffix);

            ConfigInteractableRigidbody(newInteractable);
            ConfigInteractableGrabAction(newInteractable);
        }
        private static GameObject GetPrefab(string assetName)
        {
            GameObject interactablePrefab = null;
            foreach (string assetGUID in AssetDatabase.FindAssets(assetName))
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(assetGUID);
                if (assetPath.Contains(assetName + assetSuffix))
                {
                    interactablePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                }
            }
            return interactablePrefab;
        }
        private static void ConfigInteractableRigidbody(GameObject interactable)
        {
            Rigidbody rigidbody = interactable.GetComponent<Rigidbody>();
            rigidbody.useGravity = false;
            rigidbody.isKinematic = true;
        }
        private static void ConfigInteractableGrabAction(GameObject interactable)
        {
            InteractableFacade facade = interactable.GetComponent<InteractableFacade>();

            // Set "Primary Action" to "Follow"
            int primaryActionIndex = 1; // Follow
            GameObject primaryActionPrefab = (GameObject)PrefabUtility.InstantiatePrefab(facade.Configuration.GrabConfiguration.ActionTypes.NonSubscribableElements[primaryActionIndex], facade.Configuration.GrabConfiguration.ActionTypes.transform);
            GrabInteractableAction primaryAction = primaryActionPrefab.GetComponent<GrabInteractableAction>();
            facade.Configuration.GrabConfiguration.PrimaryAction = primaryAction;

            // Set "Grab Offset" to "Precision Point"
            GrabInteractableFollowAction followAction = (GrabInteractableFollowAction)primaryAction;
            SerializedObject actionObject = new SerializedObject(followAction);
            SerializedProperty foundProperty = actionObject.FindProperty("grabOffset");
            foundProperty.intValue = 2; // Precision Point
            foundProperty.serializedObject.ApplyModifiedProperties();

            // Set "Secondary Action" to "Scale"
            // int secondaryActionIndex = 4; // Scale disabled until scaling bug is fixed in Tilia: shorturl.at/ayFP3
            int secondaryActionIndex = 2; // Swap
            GameObject secondaryActionPrefab = (GameObject)PrefabUtility.InstantiatePrefab(facade.Configuration.GrabConfiguration.ActionTypes.NonSubscribableElements[secondaryActionIndex], facade.Configuration.GrabConfiguration.ActionTypes.transform);
            GrabInteractableAction secondaryAction = secondaryActionPrefab.GetComponent<GrabInteractableAction>();
            facade.Configuration.GrabConfiguration.SecondaryAction = secondaryAction;
        }
    }

}   // Namespace