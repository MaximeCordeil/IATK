using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Events;
using IATK;
using System.Linq;
using System;

namespace IATK
{
    [ExecuteInEditMode]
    public class Axis : MonoBehaviour
    {
        #region Public/Inspector variables

        [Header("Child GameObject references")]
        [SerializeField]
        [Tooltip("The tip (cone) of the axis.")]
        private Transform axisTip;
        [SerializeField]
        [Tooltip("The rod (cylinder) of the axis.")]
        private Transform axisRod;
        [SerializeField]
        [Tooltip("The main attribute label of this axis.")]
        private TextMeshPro attributeLabel;
        [SerializeField]
        [Tooltip("The GameObject which holds all of the axis value labels.")]
        private GameObject axisTickLabelHolder;
        [SerializeField]
        [Tooltip("The base axis tick label to duplicate and use.")]
        private GameObject axisTickLabelPrefab;
        [SerializeField]
        [Tooltip("The minimum normaliser handle.")]
        private Transform minNormaliserObject;
        [SerializeField]
        [Tooltip("The maximum normaliser handle.")]
        private Transform maxNormaliserObject;
        [SerializeField]
        [Tooltip("The minimum filter handle.")]
        private Transform minFilterObject;
        [SerializeField]
        [Tooltip("The maximum normaliser handle.")]
        private Transform maxFilterObject;
        [Header("Axis Visual Properties")]
        [SerializeField]
        [Tooltip("The maximum amount of spacing that each axis tick label should have between each other.")]
        private float AxisTickSpacing = 0.075f;
        
        [HideInInspector]
        public string AttributeName = "";
        [HideInInspector]
        public AttributeFilter AttributeFilter;
        [HideInInspector]
        public float Length = 1.0f;
        [HideInInspector]
        public float MinNormaliser;
        [HideInInspector]
        public float MaxNormaliser;
        [HideInInspector]
        public HashSet<Axis> ConnectedAxis = new HashSet<Axis>();
        [HideInInspector]
        public int SourceIndex = -1;
        [HideInInspector]
        public int AxisDirection = 0;
        
        #endregion

        #region Private variables

        private Visualisation visualisationReference;
        private DataSource dataSource;

        #endregion

        /// <summary>
        /// Initialises the axis.
        /// </summary>
        /// <param name="srcData"></param>
        /// <param name="attributeFilter"></param>
        /// <param name="visualisation"></param>
        public void Initialise(DataSource srcData, AttributeFilter attributeFilter, Visualisation visualisation)
        {
            AttributeFilter = attributeFilter;
            dataSource = srcData;
            
            int idx = Array.IndexOf(srcData.Select(m => m.Identifier).ToArray(), attributeFilter.Attribute);
            SourceIndex = idx;
            name = "Axis " + srcData[idx].Identifier;
            
            attributeLabel.text = srcData[idx].Identifier;
            visualisationReference = visualisation;
            axisTickLabelPrefab.SetActive(false);
            
            UpdateAxisAttribute(attributeFilter.Attribute);
        }

        /// <summary>
        /// Updates the attribute (i.e., dimension) that this axis represents.
        /// </summary>
        /// <param name="newAttribute"></param>
        public void UpdateAxisAttribute(string newAttribute)
        {
            AttributeName = newAttribute;
            attributeLabel.text = AttributeName;
            SetYLocalPosition(attributeLabel.transform, Length * 0.5f);
            
            UpdateAxisTickLabels();
        }

        /// <summary>
        /// Sets the direction (dimension) that this axis represents
        /// </summary>
        /// <param name="direction">X=1, Y=2, Z=3</param>
        public void SetDirection(int direction)
        {
            AxisDirection = direction;
            switch (direction)
            {
                case 1:
                    // Fix the alignment of the axis tick labels
                    foreach (Transform child in axisTickLabelHolder.GetComponentsInChildren<Transform>(true))
                    {
                        if (child.gameObject.name.Contains("Text"))
                        {
                            TextMeshPro labelText = child.GetComponent<TextMeshPro>();
                            labelText.alignment = TextAlignmentOptions.MidlineLeft;
                            labelText.GetComponent<RectTransform>().pivot = new Vector2(0, 0.5f);
                        }
                        else if (child.gameObject.name.Contains("Tick"))
                        {
                            SetXLocalPosition(child, -child.localPosition.x);
                        }
                    }
                    
                    
                    transform.localEulerAngles = new Vector3(0, 0, -90);
                    SetXLocalPosition(axisTickLabelHolder.transform, 0);
                    attributeLabel.alignment = TextAlignmentOptions.Top;  
                    attributeLabel.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
                    UpdateLength(visualisationReference.width);
                    break;
                    
                case 2:
                    transform.localEulerAngles = new Vector3(0, 0, 0);
                    SetXLocalPosition(minNormaliserObject, -minNormaliserObject.transform.localPosition.x);
                    SetXLocalPosition(maxNormaliserObject, -maxNormaliserObject.transform.localPosition.x);
                    minNormaliserObject.localEulerAngles = new Vector3(90, 90, 0);
                    maxNormaliserObject.localEulerAngles = new Vector3(90, 90, 0);
                    UpdateLength(visualisationReference.height);
                    break;
                    
                case 3:
                    transform.localEulerAngles = new Vector3(90, 0, 0);
                    SetXLocalPosition(minNormaliserObject, -minNormaliserObject.transform.localPosition.x);
                    SetXLocalPosition(maxNormaliserObject, -maxNormaliserObject.transform.localPosition.x);
                    minNormaliserObject.localEulerAngles = new Vector3(90, 90, 0);
                    maxNormaliserObject.localEulerAngles = new Vector3(90, 90, 0);
                    UpdateLength(visualisationReference.depth);
                    break;
            }
        }
              
        /// <summary>
        /// Updates the length of this axis.
        /// </summary>
        /// <param name="length">The new length of this axis, in metres</param>
        public void UpdateLength(float length)
        {
            Length = length;
            
            axisRod.localScale = new Vector3(axisRod.localScale.x, Length, axisRod.localScale.z);
            axisTip.localPosition = new Vector3(axisTip.localPosition.x, Length, axisTip.localPosition.z);
            axisTip.localEulerAngles = new Vector3(length >= 0 ? 0 : 180, -45, 0);

            SetMinFilter(AttributeFilter.minFilter);
            SetMaxFilter(AttributeFilter.maxFilter);

            SetMinNormalizer(AttributeFilter.minScale);
            SetMaxNormalizer(AttributeFilter.maxScale);

            UpdateAxisAttribute(AttributeName);        
        }
        
        /// <summary>
        /// Updates all of the tick labels on this axis.
        /// </summary>
        public void UpdateAxisTickLabels()
        {
            List<GameObject> axisTickLabels = GetAxisTickLabels();
            int currentNumberOfLabels = axisTickLabels.Count;
            int targetNumberOfLabels = CalculateNumAxisTickLabels();
            
            if (currentNumberOfLabels != targetNumberOfLabels)
            {
                DestroyAxisTickLabels();
                
                // Create new labels
                for (int i = 0; i < targetNumberOfLabels; i++)
                {
                    Instantiate(axisTickLabelPrefab, axisTickLabelHolder.transform);
                }
            }
            
            // Update label positions and text
            axisTickLabels = GetAxisTickLabels();
            for (int i = 0; i < targetNumberOfLabels; i++)
            {
                GameObject label = axisTickLabels[i];
                label.SetActive(true);
                float y = GetAxisTickLabelPosition(i, targetNumberOfLabels);
                SetYLocalPosition(label.transform, y * Length);
                
                TextMeshPro labelText = label.GetComponentInChildren<TextMeshPro>();
                labelText.gameObject.SetActive(y >= 0.0f && y <= 1.0f);
                labelText.text = GetAxisTickLabelText(i, targetNumberOfLabels);
                labelText.color = new Color(1, 1, 1, GetAxisTickLabelFiltered(i, targetNumberOfLabels) ? 0.4f : 1.0f);
            }
        }
        
        /// <summary>
        /// Destroys all of the axis tick labels on this axis, excluding the referenced base label.
        /// </summary>
        public void DestroyAxisTickLabels()
        {
            foreach (GameObject label in GetAxisTickLabels())
            {
                #if !UNITY_EDITOR
                Destroy(label);
                #else
                DestroyImmediate(label);
                #endif
            }
        }
        
        public void SetMinFilter(float val)
        {
            UpdateAxisTickLabels();
        }

        public void SetMaxFilter(float val)
        {
            UpdateAxisTickLabels();
        }

        public void SetMinNormalizer(float val)
        {
            MinNormaliser = Mathf.Clamp(val, 0, 1);

            Vector3 p = minNormaliserObject.transform.localPosition;
            p.y = MinNormaliser * Length;
            minNormaliserObject.transform.localPosition = p;
            
            UpdateAxisTickLabels();
        }

        public void SetMaxNormalizer(float val)
        {
            MaxNormaliser = Mathf.Clamp(val, 0, 1);

            Vector3 p = maxNormaliserObject.transform.localPosition;
            p.y = MaxNormaliser * Length;
            maxNormaliserObject.transform.localPosition = p;

            UpdateAxisTickLabels();
        }
        
        #region Private helper functions
        
        private List<GameObject> GetAxisTickLabels()
        {
            List<GameObject> labels = new List<GameObject>();
            foreach (Transform t in axisTickLabelHolder.transform)
            {
                if (t.gameObject.name.Contains("Clone"))
                {
                    labels.Add(t.gameObject);
                }
            }
            return labels;
        }
        
        private int CalculateNumAxisTickLabels()
        {
            if (IsAttributeDiscrete())
            {
                // If this axis dimension has been rescaled at all, don't show any ticks
                if (AttributeFilter.minScale > 0.001f || AttributeFilter.maxScale < 0.999f)
                    return 0;
                
                // If this discrete dimension has less unique values than the maximum number of ticks allowed due to spacing,
                // give an axis tick label for each unique value
                int numValues = ((CSVDataSource)dataSource).TextualDimensionsListReverse[AttributeName].Count;
                int maxTicks = Mathf.CeilToInt(Length / AxisTickSpacing);
                if (numValues < maxTicks)
                    return numValues;
                // Otherwise just use 2 labels
                else
                {
                    return 2;
                }
            }
            else
            {
                return Mathf.CeilToInt(Length / AxisTickSpacing);
            }
        }
        
        private bool IsAttributeDiscrete()
        {
            var type = dataSource[AttributeFilter.Attribute].MetaData.type;
            
            return (type == DataType.String || type == DataType.Date);
        }
        
        private float GetAxisTickLabelPosition(int labelIndex, int numLabels)
        {
            if (numLabels == 1)
                return 0;
                
            return (labelIndex / (float) (numLabels - 1));
        }
        
        private string GetAxisTickLabelText(int labelIndex, int numLabels)
        {
            object v = dataSource.getOriginalValue(Mathf.Lerp(AttributeFilter.minScale, AttributeFilter.maxScale, labelIndex / (numLabels - 1f)), AttributeFilter.Attribute);

            if (v is float && v.ToString().Length > 4)
            {
                return ((float)v).ToString("#,##0.0");
            }
            else
            {
                return v.ToString();
            }
        }
        
        private bool GetAxisTickLabelFiltered(int labelIndex, int numLabels)
        {
            float n = labelIndex / (float)(numLabels - 1);
            float delta = Mathf.Lerp(AttributeFilter.minScale, AttributeFilter.maxScale, n);
            return delta < AttributeFilter.minFilter || delta > AttributeFilter.maxFilter;        
        }
        
            
        private void SetXLocalPosition(Transform t, float value)
        {
            var p = t.localPosition;
            p.x = value;
            t.localPosition = p;
        }

        private void SetYLocalPosition(Transform t, float value)
        {
            var p = t.localPosition;
            p.y = value;
            t.localPosition = p;
        }

        #endregion // Private helper functions
    }
}