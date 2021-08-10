using System.Collections;
using System.Collections.Generic;
using Tilia.Interactions.Interactables.Interactables;
using Tilia.Interactions.Interactables.Interactables.Grab.Receiver;
using Tilia.Interactions.Interactables.Interactables.Grab.Action;
using UnityEngine;
using UnityEditor;
using System;

namespace IATK
{
    /// <summary>
    /// VR Visualisation class to act as a view controller - reads the model to create the view
    /// </summary>
    [ExecuteInEditMode]
    public class VRVisualisation : Visualisation
    {
        private const string assetName = "Interactions.Interactable";
        private const string assetSuffix = ".prefab";

        // Runs when the VRVisualisation Component is first added to a GameObject
        void Reset() {
            ConvertToInteractable(gameObject);

            // Alter head parent to:
                // ✔ Set heading back to: [IATK] New VR Visualisation
                // ✔ Alter Rigidbody:
                    // ✔ Disable gravity
                    // ✔ Enable kinematic
                // ✔ Set grab actions
            
            // Alter visualisation to:
                // Add Box Collider
                // Add a Linear Drive to each Normaliser dragger
                // Connect all Linear Drives to the appropriate Normaliser values
                // Remember to consider 2D and 3D visualisations
        }

        /// <summary>
        /// Wraps the gameObject in a Tilia interactable prefab
        /// </summary>
        /// <returns>Tilia interactable, that contains the VR Visualisation</returns>
        private void ConvertToInteractable(GameObject gameObject)
        {
            GameObject interactablePrefab = GetInteractablePrefab();

            int siblingIndex = gameObject.transform.GetSiblingIndex();
            GameObject newInteractable = (GameObject)PrefabUtility.InstantiatePrefab(interactablePrefab);
            newInteractable.name = gameObject.name + " Interactable";
            InteractableFacade facade = newInteractable.GetComponent<InteractableFacade>();

            newInteractable.transform.SetParent(gameObject.transform.parent);
            newInteractable.transform.localPosition = gameObject.transform.localPosition;
            newInteractable.transform.localRotation = gameObject.transform.localRotation;
            newInteractable.transform.localScale = gameObject.transform.localScale;

            foreach (MeshRenderer defaultMeshes in facade.Configuration.MeshContainer.GetComponentsInChildren<MeshRenderer>())
            {
                defaultMeshes.gameObject.SetActive(false);
            }

            gameObject.transform.SetParent(facade.Configuration.MeshContainer.transform);
            gameObject.transform.localPosition = Vector3.zero;
            gameObject.transform.localRotation = Quaternion.identity;
            gameObject.transform.localScale = Vector3.one;

            newInteractable.transform.SetSiblingIndex(siblingIndex);

            ConfigRigidbody(newInteractable);
            ConfigGrabbable(facade);
        }
        private GameObject GetInteractablePrefab()
        {
            GameObject interactablePrefab = null;
            foreach (string assetGUID in AssetDatabase.FindAssets(assetName))
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(assetGUID);
                if (assetPath.Contains(assetName + assetSuffix))
                {
                    // TODO: See if I can make this more efficient
                    interactablePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                }
            }
            return interactablePrefab;
        }
        private void ConfigRigidbody(GameObject interactable)
        {
            Rigidbody rigidbody = interactable.GetComponent<Rigidbody>();
            rigidbody.useGravity = false;
            rigidbody.isKinematic = true;
        }
        private void ConfigGrabbable(InteractableFacade facade)
        {
            facade.GrabType = GrabInteractableReceiver.ActiveType.Toggle;

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
            int secondaryActionIndex = 4; // Scale
            GameObject secondaryActionPrefab = (GameObject)PrefabUtility.InstantiatePrefab(facade.Configuration.GrabConfiguration.ActionTypes.NonSubscribableElements[secondaryActionIndex], facade.Configuration.GrabConfiguration.ActionTypes.transform);
            GrabInteractableAction secondaryAction = secondaryActionPrefab.GetComponent<GrabInteractableAction>();
            facade.Configuration.GrabConfiguration.SecondaryAction = secondaryAction;
        }
    }
}