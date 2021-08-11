using System.Collections;
using System.Collections.Generic;
using Tilia.Interactions.Interactables.Interactables;
using Tilia.Interactions.Interactables.Interactables.Grab.Receiver;
using Tilia.Interactions.Interactables.Interactables.Grab.Action;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;

namespace IATK
{
    /// <summary>
    /// VR Visualisation class to act as a view controller - reads the model to create the view
    /// </summary>
    [ExecuteInEditMode]
    public class VRVisualisation : Visualisation
    {
        private bool xAxisInUse = false; 
        private bool yAxisInUse = false; 
        private bool zAxisInUse = false; 

        private const string assetName = "Interactions.Interactable";
        private const string assetSuffix = ".prefab";

        // Runs when the VRVisualisation Component is first added to a GameObject
        void Reset()
        {
            ConvertToInteractable(gameObject);

            // Alter head parent to:
                // ✔ Set heading back to: [IATK] New VR Visualisation
                // ✔ Alter Rigidbody:
                    // ✔ Disable gravity
                    // ✔ Enable kinematic
                // ✔ Set grab actions
            
            // Alter visualisation to:
                // ✔ Add Box Collider
                    // ✔ Add support for both 2D and 3D visualisations
                // Add a Linear Drive to each Normaliser dragger
                    // Add support for both 2D and 3D visualisations
                // Connect all Linear Drives to the appropriate Normaliser values
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

            ConfigInteractableRigidbody(newInteractable);
            ConfigInteractableGrabbable(facade);
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
        private void ConfigInteractableRigidbody(GameObject interactable)
        {
            Rigidbody rigidbody = interactable.GetComponent<Rigidbody>();
            rigidbody.useGravity = false;
            rigidbody.isKinematic = true;
        }
        private void ConfigInteractableGrabbable(InteractableFacade facade)
        {
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

        private void ConfigVisualisationBoxCollider()
        {
            int numberOfAxisInUse = CountTrue(xAxisInUse, yAxisInUse, zAxisInUse);

            if(numberOfAxisInUse == 0)
            {
                // If no axis are set then remove the box collider
                DestroyImmediate(gameObject.GetComponent<BoxCollider>());
                return;
            }

            Vector3 center = new Vector3(0, 0, 0);
            Vector3 size = new Vector3(0.1f, 0.1f, 0.1f);
            
            if (xAxisInUse)
            {
                center.x = 0.5f;
                size.x = 1f;

                // Needs an additional offset if only one axis is used
                if (numberOfAxisInUse == 1) center.y = 0.03f;
            }
            if (yAxisInUse)
            {
                center.y = 0.5f;
                size.y = 1f;

                // Needs an additional offset if only one axis is used
                if (numberOfAxisInUse == 1) center.x = 0.03f;
            }
            if (zAxisInUse)
            {
                center.z = 0.5f;
                size.z = 1f;

                // Needs an additional offset if only one axis is used
                if (numberOfAxisInUse == 1) center.x = 0.03f;
            }

            BoxCollider boxCollider = gameObject.GetComponent<BoxCollider>();
            if (boxCollider == null) boxCollider = gameObject.AddComponent<BoxCollider>();
            boxCollider.center = center;
            boxCollider.size = size;
        }
        private static int CountTrue(params bool[] args)
        {
            return args.Count(t => t);
        }

        public override void updateViewProperties(AbstractVisualisation.PropertyType propertyType)
        {
            // Debug.Log("updateViewProperties: " + propertyType);

            base.updateViewProperties(propertyType);

            switch (propertyType)
            {
                case AbstractVisualisation.PropertyType.VisualisationType:
                    // TODO: Handle visualisation types
                    // When visualisation type changes, we need to:
                        // Alter Box Collider
                        // Find any new Normaliser draggers and add a Linear Drive to each
                        // Connect all Linear Drives to the appropriate Normaliser values
                    break;
                case AbstractVisualisation.PropertyType.X:
                    xAxisInUse = !theVisualizationObject.visualisationReference.xDimension.Attribute.Equals("Undefined");
                    ConfigVisualisationBoxCollider();
                    break;
                case AbstractVisualisation.PropertyType.Y:
                    yAxisInUse = !theVisualizationObject.visualisationReference.yDimension.Attribute.Equals("Undefined");
                    ConfigVisualisationBoxCollider();
                    break;
                case AbstractVisualisation.PropertyType.Z:
                    zAxisInUse = !theVisualizationObject.visualisationReference.zDimension.Attribute.Equals("Undefined");
                    ConfigVisualisationBoxCollider();
                    break;
            }
        }
    }
}