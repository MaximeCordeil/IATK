using System.Collections;
using System.Collections.Generic;
using Tilia.Interactions.Interactables.Interactables;
using Tilia.Interactions.Interactables.Interactables.Grab.Action;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using Tilia.Interactions.Controllables.LinearDriver;
using Tilia.Interactions.Interactables.Interactors;
using UnityEngine.Events;
using Tilia.Interactions.Interactables.Interactables.Grab.Receiver;
using UnityEditor.Events;

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

        private const string prefabInteractable = "Interactions.Interactable";
        private const string prefabInteractionsLinearJointDrive = "Interactions.LinearJointDrive";
        private const string assetSuffix = ".prefab";

        /* TODO:

        - Fix issue with overlapping Y and Z axis normalisers
        - Fix issue with sliders breaking when visualisation is scaled

        - Handle different visualisation types

        */

        void Reset()
        {
            // Runs when VRVisualisation is first added to a GameObject as a component
            MakeInteractable();
        }

#region Interactable Handeling
        private void MakeInteractable()
        {
            GameObject newInteractable = WrapInInteractablePrefab(gameObject, prefabInteractable, "[Interactable]");
            InteractableFacade facade = newInteractable.GetComponent<InteractableFacade>();

            ConfigInteractableRigidbody(newInteractable);
            ConfigInteractableGrabAction(facade);
        }
        private GameObject GetPrefab(string assetName)
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
        private void ConfigInteractableGrabAction(InteractableFacade facade)
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
#endregion

#region Visualisation Handeling
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
                    UpdateVisualisation();
                    break;
                case AbstractVisualisation.PropertyType.Y:
                    yAxisInUse = !theVisualizationObject.visualisationReference.yDimension.Attribute.Equals("Undefined");
                    UpdateVisualisation();
                    break;
                case AbstractVisualisation.PropertyType.Z:
                    zAxisInUse = !theVisualizationObject.visualisationReference.zDimension.Attribute.Equals("Undefined");
                    UpdateVisualisation();
                    break;
            }
        }
        private void UpdateVisualisation()
        {
            AddOrUpdateVisualisationBoxCollider();
            AddOrUpdateInteractableNormalisers();
        }
        private void AddOrUpdateVisualisationBoxCollider()
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
        private void AddOrUpdateInteractableNormalisers()
        {
            Axis[] axes = GetComponentsInChildren<Axis>();
            if (axes == null) return;

            foreach(Axis axis in axes)
            {
                ConvertToLinearJointDrive(axis.transform.Find("MinNormaliser")?.gameObject, axis.AxisDirection, false);
                ConvertToLinearJointDrive(axis.transform.Find("MaxNormaliser")?.gameObject, axis.AxisDirection, true);
            }
        }
        /// <summary>
        /// Wraps a given normaliser GameObject in a Tilia LinearJointDrive and configures the LinearJointDrive based on a axis direction
        /// </summary>
        /// <param name="normaliser"></param>
        /// <param name="axisDirection">X=1, Y=2, Z=3</param>
        private void ConvertToLinearJointDrive(GameObject normaliser, int axisDirection, bool isMax)
        {
            if (normaliser == null) return;

            // Sets the layer to be layer 3 ("Normaliser") so it won't collide with other normalisers
            normaliser.layer = 3;

            // Adds a box collider to the normaliser object so that it can interact with the user's 'hands'
            BoxCollider boxCollider = normaliser.GetComponent<BoxCollider>();
            if (boxCollider == null) boxCollider = normaliser.AddComponent<BoxCollider>();
            boxCollider.center = Vector3.zero;
            boxCollider.size = new Vector3(3, 3, 3);

            GameObject linerJoint = WrapInInteractablePrefab(normaliser, prefabInteractionsLinearJointDrive, "[Linear Joint]");

            // Sets the rotation of the normaliser object
            normaliser.transform.localRotation = axisDirection switch
            {
                1 => Quaternion.Euler(0, 0, 90f), // X Axis
                2 => Quaternion.Euler(0, 0, -90f), // Y Axis
                3 => Quaternion.Euler(0, 0, -90f), // Z Axis
                _ => throw new Exception("Invalid Axis Direction")
            };

            normaliser.transform.localScale = new Vector3(0.025f, 0.025f, 0.025f);
            linerJoint.transform.localScale = new Vector3(1f, 1f, 1f);

            // Sets the center position of the normaliser object to the middle of the axis
            // Values are not symmetrical due to slight differences in how each axis is positioned 
            linerJoint.transform.position = axisDirection switch
            {
                1 => new Vector3(0, -0.07f, 0f), // X Axis
                2 => new Vector3(-0.07f, 0, 0f), // Y Axis
                3 => new Vector3(-0.07f, -0.02f, 0), // Z Axis
                _ => throw new Exception("Invalid Axis Direction")
            };

            // Resets the rotation of the linerJoint to 0 because the rotation only needs to be on the normaliser object
            linerJoint.transform.localRotation = Quaternion.identity;

            LinearDriveFacade linerJointFacade = linerJoint.GetComponent<LinearDriveFacade>();

            // Connects the linerJoint to the corresponding min/max axis
            UnityAction<float> setScaleAction = (axisDirection, isMax) switch
            {
                (1, false) => SetMinScaleX,
                (1, true) => SetMaxScaleX,
                (2, false) => SetMinScaleY,
                (2, true) => SetMaxScaleY,
                (3, false) => SetMinScaleZ,
                (3, true) => SetMaxScaleZ,
                _ => throw new Exception("Invalid Axis Direction")
            };
            UnityEventTools.AddPersistentListener(linerJointFacade.ValueChanged, setScaleAction);

            // All drive axes are the same as they are rotated by a parent object to the specific axis
            linerJointFacade.DriveAxis = Tilia.Interactions.Controllables.Driver.DriveAxis.Axis.YAxis;

            linerJointFacade.MoveToTargetValue = true;
            linerJointFacade.TargetValue = isMax ? 1f : 0f;
            linerJointFacade.SetStepRangeMaximum(100f);
            linerJointFacade.DriveLimit = 0.5f;


            // Connects the linerJointFacade with the interactableFacade so the position of the normaliser object is correct 
            InteractableFacade interactableFacade = normaliser.transform.parent.parent.GetComponent<InteractableFacade>();
            UnityEventTools.AddVoidPersistentListener(interactableFacade.LastUngrabbed, linerJointFacade.SetTargetValueByStepValue);
        }

#endregion

        /// <summary>
        /// Wraps a gameObject in a given Tilia interactable prefab with a name suffix
        /// </summary>
        private GameObject WrapInInteractablePrefab(GameObject gameObject, string assetName, string suffix)
        {
            GameObject interactablePrefab = GetPrefab(assetName);
            GameObject newObject = (GameObject)PrefabUtility.InstantiatePrefab(interactablePrefab);
            newObject.name = gameObject.name + " " + suffix;

            newObject.transform.SetParent(gameObject.transform.parent);
            newObject.transform.localPosition = gameObject.transform.localPosition;
            newObject.transform.localRotation = gameObject.transform.localRotation;
            newObject.transform.localScale = gameObject.transform.localScale;

            Transform meshContainer = RecursiveFindChild(newObject.transform, "MeshContainer");
            foreach (Transform child in meshContainer)
                child.gameObject.SetActive(false);

            int siblingIndex = gameObject.transform.GetSiblingIndex(); 

            gameObject.transform.SetParent(meshContainer);
            gameObject.transform.localPosition = Vector3.zero;
            gameObject.transform.localRotation = Quaternion.identity;
            gameObject.transform.localScale = Vector3.one;

            newObject.transform.SetSiblingIndex(siblingIndex);

            return newObject;
        }
        private Transform RecursiveFindChild(Transform parent, string childName)
        {
            foreach (Transform child in parent)
            {
                if(child.name == childName) return child;
                else
                {
                    Transform found = RecursiveFindChild(child, childName);
                    if (found != null) return found;
                }
            }
            return null;
        }

#region Visualisation Property Setting
        public void SetGeometrySize(float size)
        {
            this.size = size + 0.5f;
            updateViewProperties(AbstractVisualisation.PropertyType.SizeValues);
        }
        public void SetMinScaleX(float minScale)
        {
            if(theVisualizationObject?.creationConfiguration == null) return;
            xDimension.minScale = minScale + 0.5f;
            updateViewProperties(AbstractVisualisation.PropertyType.Scaling);
        }
        public void SetMaxScaleX(float maxScale)
        {
            if(theVisualizationObject?.creationConfiguration == null) return;
            xDimension.maxScale = maxScale + 0.5f;
            updateViewProperties(AbstractVisualisation.PropertyType.Scaling);
        }
        public void SetMinScaleY(float minScale)
        {
            if(theVisualizationObject?.creationConfiguration == null) return;
            yDimension.minScale = minScale + 0.5f;
            updateViewProperties(AbstractVisualisation.PropertyType.Scaling);
        }
        public void SetMaxScaleY(float maxScale)
        {
            if(theVisualizationObject?.creationConfiguration == null) return;
            yDimension.maxScale = maxScale + 0.5f;
            updateViewProperties(AbstractVisualisation.PropertyType.Scaling);
        }
        public void SetMinScaleZ(float minScale)
        {
            if(theVisualizationObject?.creationConfiguration == null) return;
            zDimension.minScale = minScale + 0.5f;
            updateViewProperties(AbstractVisualisation.PropertyType.Scaling);
        }
        public void SetMaxScaleZ(float maxScale)
        {
            if(theVisualizationObject?.creationConfiguration == null) return;
            zDimension.maxScale = maxScale + 0.5f;
            updateViewProperties(AbstractVisualisation.PropertyType.Scaling);
        }
#endregion
    }
}