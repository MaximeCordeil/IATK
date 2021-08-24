using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Tilia.Interactions.Controllables.LinearDriver;
using Tilia.Interactions.Interactables.Interactables;
using UnityAction = UnityEngine.Events.UnityAction<Tilia.Interactions.Interactables.Interactors.InteractorFacade>;

namespace IATK
{
    /// <summary>
    /// VR Visualisation class to act as a view controller - reads the model to create the view
    /// </summary>
    [ExecuteInEditMode]
    public class VRVisualisation : Visualisation
    {
        private GameObject linearJointDrivePrefab;
        private bool xAxisInUse = false; 
        private bool yAxisInUse = false; 
        private bool zAxisInUse = false; 

        /* TODO:
        - ✔ Move UnityEditor code to editor files
        - Handle scaling of visualisation for normaliser handels
        - Handle different visualisation types
        */
        
        public void initialize(GameObject go) { linearJointDrivePrefab = go; }
        void Start() { CreateInteractableNormalisers(); }

#region Visualisation Handling
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
                case AbstractVisualisation.PropertyType.Scaling:
                    // TODO: Handle scaling of visualisation for normaliser handels
                    break;
            }
        }
        private void UpdateVisualisation() { AddOrUpdateVisualisationBoxCollider(); }
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
        private static int CountTrue(params bool[] args) { return args.Count(t => t); }
        private void CreateInteractableNormalisers()
        {
            Axis[] axes = GetComponentsInChildren<Axis>();
            if (axes == null) return;

            foreach(Axis axis in axes)
            {
                ConfigureHandle(axis, "MinAxisHandle", false);
                ConfigureHandle(axis, "MaxAxisHandle", true);
            }
        }
        private void ConfigureHandle(Axis axis, string handleName, bool isMax)
        {
            int axisDirection = axis.AxisDirection;
            GameObject handle = axis.transform.Find(handleName)?.gameObject;

            if(handle != null)
            {
                // Runs once the first time the scene is played after the visualisation was created
                ConvertToLinearJointDrive(handle, axisDirection, isMax);
            }
            else
            {
                // Runs every time the scene is played after the first time
                ConfigureHandle(axis.transform.Find(handleName + " [Linear Joint]"), axisDirection, isMax);
            }
        }
        /// <summary>
        /// Wraps a given normaliser GameObject in a Tilia 'LinearJointDrive' and configures the Linear Joint based on a axis direction
        /// </summary>
        /// <param name="handle">The GameObject to wrap</param>
        /// <param name="axisDirection">X=1, Y=2, Z=3</param>
        /// <param name="isMax">True if the normaliser is the maximum filter</param>
        private void ConvertToLinearJointDrive(GameObject handle, int axisDirection, bool isMax)
        {
            handle.SetActive(true);

            GameObject newLinerJointPrefab = Instantiate(linearJointDrivePrefab);
            GameObject linerJoint = WrapObject(handle, newLinerJointPrefab, "[Linear Joint]");
            LinearDriveFacade linerJointFacade = linerJoint.GetComponent<LinearDriveFacade>();

            ConfigNormaliserSlider(handle, axisDirection);
            ConfigLinerJoint(linerJoint, axisDirection);
            ConfigLinerJointFacade(linerJointFacade, axisDirection, isMax);

            // Connects the linerJointFacade with the interactableFacade so the position of the normaliser object is correct 
            InteractableFacade interactableFacade = handle.transform.parent.parent.GetComponent<InteractableFacade>();

            UnityAction action = (_) => linerJointFacade.SetTargetValueByStepValue();
            interactableFacade.LastUngrabbed.AddListener(action);
        }
        private void ConfigureHandle(Transform handle, int axisDirection, bool isMax)
        {
            Transform interactable = RecursiveFindChild(handle, "Interactions.Interactable");

            // Connects the linerJointFacade with the interactableFacade so the position of the normaliser object is correct 
            InteractableFacade interactableFacade = interactable.GetComponent<InteractableFacade>();

            LinearDriveFacade linerJointFacade = handle.GetComponent<LinearDriveFacade>();
            UnityAction action = (_) => linerJointFacade.SetTargetValueByStepValue();
            interactableFacade.LastUngrabbed.AddListener(action);
            ConfigLinerJointFacade(linerJointFacade, axisDirection, isMax);
        }
        private void ConfigNormaliserSlider(GameObject handle, int axisDirection)
        {
            // Sets the layer to be layer 3 ("Normaliser") so it won't collide with other normalisers
            handle.layer = 3;

            // Adds a box collider to the normaliser object so that it can interact with the user's 'hands'
            BoxCollider boxCollider = handle.GetComponent<BoxCollider>();
            if (boxCollider == null) boxCollider = handle.AddComponent<BoxCollider>();
            boxCollider.center = Vector3.zero;
            boxCollider.size = new Vector3(3, 3, 3);

            handle.transform.localScale = new Vector3(0.025f, 0.025f, 0.025f);
        }
        private void ConfigLinerJoint(GameObject linerJoint, int axisDirection)
        {
            linerJoint.transform.localScale = new Vector3(1f, 1f, 1f);

            // Sets the center position of the normaliser object to the middle of the axis
            // Values are not symmetrical due to slight differences in how each axis is positioned 
            linerJoint.transform.position = axisDirection switch
            {
                1 => new Vector3(0.5f, -0.07f, 0f), // X Axis
                2 => new Vector3(-0.07f, 0.5f, 0f), // Y Axis
                3 => new Vector3(-0.07f, -0.02f, 0.5f), // Z Axis
                _ => throw new Exception("Invalid Axis Direction")
            };

            // Resets the rotation of the linerJoint to 0 because the rotation only needs to be on the normaliser object
            linerJoint.transform.localRotation = Quaternion.identity;
        }
        private LinearDriveFacade ConfigLinerJointFacade(LinearDriveFacade linerJointFacade, int axisDirection, bool isMax)
        {
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
            linerJointFacade.ValueChanged.AddListener(setScaleAction);

            // All drive axes are the same as they are rotated by a parent object to the specific axis
            linerJointFacade.DriveAxis = Tilia.Interactions.Controllables.Driver.DriveAxis.Axis.YAxis;

            linerJointFacade.MoveToTargetValue = true;
            linerJointFacade.TargetValue = isMax ? 1f : 0f;
            linerJointFacade.SetStepRangeMaximum(100f);

            return linerJointFacade;
        }
#endregion

#region Helper Functions
        /// <summary>
        /// Wraps the current game object in another. Used to convert visualisations into interactables.
        /// </summary>
        /// <param name="toWrapIn">The GameObject to wrap the current object in</param>
        /// <param name="suffix">The suffix name of the wrapping object</param>
        /// <returns>The wrapped GameObject</returns>
        public GameObject WrapIn(GameObject toWrapIn, string suffix)
        {
            return WrapObject(gameObject, toWrapIn, suffix);
        }

        /// <summary>
        /// Wraps a GameObject in another GameObject with a name suffix. Used to convert visualisations into interactables.
        /// </summary>
        private static GameObject WrapObject(GameObject toBeWrapped, GameObject toWrapIn, string suffix)
        {
            toWrapIn.name = toBeWrapped.name + " " + suffix;

            toWrapIn.transform.SetParent(toBeWrapped.transform.parent);
            toWrapIn.transform.localPosition = toBeWrapped.transform.localPosition;
            toWrapIn.transform.localRotation = toBeWrapped.transform.localRotation;
            toWrapIn.transform.localScale = toBeWrapped.transform.localScale;

            Transform meshContainer = RecursiveFindChild(toWrapIn.transform, "MeshContainer");
            foreach (Transform child in meshContainer)
                child.gameObject.SetActive(false);

            int siblingIndex = toBeWrapped.transform.GetSiblingIndex(); 

            toBeWrapped.transform.SetParent(meshContainer);
            toBeWrapped.transform.localPosition = Vector3.zero;
            toBeWrapped.transform.localRotation = Quaternion.identity;
            toBeWrapped.transform.localScale = Vector3.one;

            toWrapIn.transform.SetSiblingIndex(siblingIndex);

            return toWrapIn;
        }
        private static Transform RecursiveFindChild(Transform parent, string childName)
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
#endregion

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