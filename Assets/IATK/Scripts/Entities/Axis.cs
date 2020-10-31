using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.Events;
using IATK;
using System.Linq;
using System;

public class Axis : MonoBehaviour {

    [SerializeField] public TextMeshPro label;

    [SerializeField] public GameObject axisValueLabels;
    [SerializeField] public TextMeshPro axisLabelPrefab;

    public int axisId;

    public bool isPrototype;

    //temporary hack 

    [SerializeField] Transform minFilterObject;
    [SerializeField] Transform maxFilterObject;

    [SerializeField] Transform minNormaliserObject;
    [SerializeField] Transform maxNormaliserObject;

    [SerializeField] Renderer ticksRenderer;

    [Space(10)]

    [SerializeField] UnityEvent OnEntered;
    [SerializeField] UnityEvent OnExited;

    public HashSet<Axis> ConnectedAxis = new HashSet<Axis>();

    public string AttributeName = "";

    [SerializeField] public AttributeFilter AttributeFilter;

    public float MinNormaliser;
    public float MaxNormaliser;

    public bool isDirty;

    public float Length = 1.0f;

    public int SourceIndex = -1;

    Vector2 AttributeRange;

    float ticksScaleFactor = 1.0f;

    public Visualisation visualisationReference;

    private AxisLabelDelegate labelDelegate;
    private List<TextMeshPro> axisLabels = new List<TextMeshPro>();

    private int MyDirection = 0;


    public void Init(DataSource srcData, AttributeFilter attributeFilter, Visualisation visualisation)
    {
        AttributeName = attributeFilter.Attribute;
        AttributeFilter = attributeFilter;

        int idx = Array.IndexOf(srcData.Select(m => m.Identifier).ToArray(), attributeFilter.Attribute);
        SourceIndex = idx;
        axisId = idx;
        name = "axis " + srcData[idx].Identifier;
        
        AttributeRange = new Vector2(srcData[idx].MetaData.minValue, srcData[idx].MetaData.maxValue);
        label.text = srcData[idx].Identifier;

        visualisationReference = visualisation;
        
        CalculateTicksScale(srcData[idx].MetaData.binCount);
        UpdateTicks();
        GenerateAxisLabels();
    }

    private void GenerateAxisLabels()
    {
        labelDelegate = new BasicAxisLabelDelegate(AttributeFilter, visualisationReference.dataSource, Length);

        List<GameObject> children = new List<GameObject>();
        foreach (Transform child in axisValueLabels.transform)
        {
            if (child.gameObject.activeSelf)
            {
                children.Add(child.gameObject);
            }
        }
        foreach (GameObject go in children)
        {
            DestroyImmediate(go);
        }

        axisLabels.Clear();

        for (int i = 0; i < labelDelegate.NumberOfLabels(); ++i)
        {
            var go = Instantiate(axisLabelPrefab, axisValueLabels.transform);
            go.gameObject.SetActive(true);

            go.text = labelDelegate.LabelText(i);
            SetYPos(go.transform, labelDelegate.LabelPosition(i));

            axisLabels.Add(go);
        }
    }

    private void UpdateAxisLabels()
    {
        labelDelegate = new BasicAxisLabelDelegate(AttributeFilter, visualisationReference.dataSource, Length);

        if (labelDelegate.NumberOfLabels() != axisLabels.Count)
            GenerateAxisLabels();

        for (int i = 0; i < axisLabels.Count; ++i)
        {
            var go = axisLabels[i];
            go.text = labelDelegate.LabelText(i);

            float y = labelDelegate.LabelPosition(i);
            SetYPos(go.transform, y * Length);
            go.gameObject.SetActive(y >= 0.0f && y <= 1.0f);

            go.color = new Color(1, 1, 1, labelDelegate.IsFiltered(i) ? 0.4f : 1.0f);
        }
    }
    
    // helper func
    private void SetXPos(Transform t, float value)
    {
        var p = t.localPosition;
        p.x = value;
        t.localPosition = p;
    }

    private void SetYPos(Transform t, float value)
    {
        var p = t.localPosition;
        p.y = value;
        t.localPosition = p;
    }

    // sets the direction of this axis. X=1, Y=2, Z=3
    public void SetDirection(int direction)
    {
        MyDirection = direction;
        switch (direction)
        {
            case 1:
                transform.localEulerAngles = new Vector3(0, 0, -90);
                SetXPos(axisValueLabels.transform, 1);
                foreach (TextMeshPro tmp in axisValueLabels.transform.GetComponentsInChildren<TextMeshPro>(true))
                {
                    tmp.alignment = TextAlignmentOptions.MidlineLeft;
                }
                SetXPos(label.transform, 1);
                label.alignment = TextAlignmentOptions.Top;
                break;
            case 2:
                transform.localEulerAngles = new Vector3(0, 0, 0);
                SetXPos(minNormaliserObject, -0.054f);
                SetXPos(maxNormaliserObject, -0.054f);
                minNormaliserObject.localEulerAngles = new Vector3(90, 90, 0);
                maxNormaliserObject.localEulerAngles = new Vector3(90, 90, 0);
                break;
            default:
                SetXPos(minNormaliserObject, -0.054f);
                SetXPos(maxNormaliserObject, -0.054f);
                minNormaliserObject.localEulerAngles = new Vector3(90, 90, 0);
                maxNormaliserObject.localEulerAngles = new Vector3(90, 90, 0);
                break;
        }
    }

    public void UpdateLength()
    {
        UpdateLength(Length);
    }

    public void UpdateLength(float length)
    {
        Length = length;

        transform.Find("axis_mesh").localScale = new Vector3(0.05f, Length, 0.05f);
        transform.Find("Cone").localPosition = new Vector3(0, Length, 0);

        SetMinFilter(AttributeFilter.minFilter);
        SetMaxFilter(AttributeFilter.maxFilter);

        SetMinNormalizer(AttributeFilter.minScale);
        SetMaxNormalizer(AttributeFilter.maxScale);

        UpdateAxisLabels();        
    }

    public void UpdateLabelAttribute(string attributeName)
    {
        label.text = attributeName;
        var pos = label.transform.localPosition;
        pos.y = Length * 0.5f;
        label.transform.localPosition = pos;

        UpdateAxisLabels();
    }

    void CalculateTicksScale(int binCount)
    {
        float range = AttributeRange.y - AttributeRange.x;
        if (binCount > range + 2)
        {
            ticksScaleFactor = 1.0f / (binCount / 10);
        }
        else if (range < 20)
        {
            // each tick mark represents one increment
            ticksScaleFactor = 1;
        }
        else if (range < 50)
        {
            ticksScaleFactor = 5;
        }
        else if (range < 200)
        {
            // each tick mark represents ten increment
            ticksScaleFactor = 10;            
        }
        else if (range < 600)
        {
            ticksScaleFactor = 50;           
        }
        else if (range < 3000)
        {
            ticksScaleFactor = 100;            
        }
        else
        {
            ticksScaleFactor = 500;            
        }
    }

    void UpdateTicks()
    {
        float range = Mathf.Lerp(AttributeRange.x, AttributeRange.y, MaxNormaliser + 0.5f) - Mathf.Lerp(AttributeRange.x, AttributeRange.y, MinNormaliser + 0.5f);
        float scale = range / ticksScaleFactor;
        //ticksRenderer.material.mainTextureScale = new Vector3(1, scale);
    }
    
    //
    // filters and scaling
    //

    public void SetMinFilter(float val)
    { }

    public void SetMaxFilter(float val)
    { }

    public void SetMinNormalizer(float val)
    {
        MinNormaliser = Mathf.Clamp(val, 0, 1);

        Vector3 p = minNormaliserObject.transform.localPosition;
        p.y = MinNormaliser * Length;
        minNormaliserObject.transform.localPosition = p;

        UpdateTicks();
    }

    public void SetMaxNormalizer(float val)
    {
        MaxNormaliser = Mathf.Clamp(val, 0, 1);

        Vector3 p = maxNormaliserObject.transform.localPosition;
        p.y = MaxNormaliser * Length;
        maxNormaliserObject.transform.localPosition = p;

        UpdateTicks();
    }

    #region euclidan functions

    // calculates the project of the transform tr (assumed to be the user's hand) onto the axis
    // as a float between 0...1
    public float CalculateLinearMapping(Transform tr)
    {
        Vector3 direction = MaxPosition- MinPosition;
        float length = direction.magnitude;
        direction.Normalize();

        Vector3 displacement = tr.position - MinPosition;

        return Vector3.Dot(displacement, direction) / length;
    }
    
    Vector3 _maxPos;
    public Vector3 MaxPosition
    {
        get { return _maxPos; }
    }

    Vector3 _minPos;
    public Vector3 MinPosition
    {
        get { return _minPos; }
    }

    #endregion
    

    abstract class AxisLabelDelegate
    {
        public virtual int NumberOfLabels()
        {
            return 0;
        }

        public virtual float LabelPosition(int labelIndex)
        {
            return 0;
        }

        public virtual string LabelText(int labelIndex)
        {
            return "";
        }

        public virtual bool IsFiltered(int labelIndex)
        {
            return false;
        }
    }

    public void Update()
    {
        //check if the transforms of the normalisers have been moved
        if (minNormaliserObject.hasChanged)
        {       //raise event
            visualisationReference.updateViewProperties(AbstractVisualisation.PropertyType.AttributeFiltering);
            minNormaliserObject.hasChanged = false;
        }
            if (maxNormaliserObject.hasChanged)
        {
            //raise event
            visualisationReference.updateViewProperties(AbstractVisualisation.PropertyType.DimensionChangeFiltering);
            maxNormaliserObject.hasChanged = false;
        }
    }

    class BasicAxisLabelDelegate : AxisLabelDelegate
    {
        public AttributeFilter attributeFilter;
        public DataSource dataSource;
        public float axisLength;

        public BasicAxisLabelDelegate(AttributeFilter attributeFilter, DataSource dataSource, float axisLength)
        {
            this.attributeFilter = attributeFilter;
            this.dataSource = dataSource;
            this.axisLength = axisLength;
        }
        
        bool IsDiscreet()
        {
            var type = dataSource[attributeFilter.Attribute]?.MetaData.type;
            if (type == DataType.String || type == DataType.Date)// || type == DataType.Time)
            {
                return true;
            }
            return false;
        }

        public override int NumberOfLabels()
        {
            if (IsDiscreet())
            {
                // If the axis is normalised at all, then hide the labels
                if (attributeFilter.minScale > 0.001f || attributeFilter.maxScale < 0.999f)
                {
                    return 0;
                }

                CSVDataSource csvDataSource = (CSVDataSource)dataSource;
                int numValues = csvDataSource.TextualDimensionsListReverse[attributeFilter.Attribute].Count;

                // If the number of distinct values this dimension has can comfortably fit on the axis, show them all
                if (numValues < (Mathf.CeilToInt(axisLength / 0.1f)))
                {
                    return numValues;
                }

                return 2;
            }
            else
            {
                return Mathf.CeilToInt(axisLength / 0.1f);
            }
        }

        public override float LabelPosition(int labelIndex)
        {
            return (labelIndex / (float)(NumberOfLabels() - 1));
        }

        public override string LabelText(int labelIndex)
        {
            object v = dataSource.getOriginalValue(Mathf.Lerp(attributeFilter.minScale, attributeFilter.maxScale, labelIndex / (NumberOfLabels() - 1f)), attributeFilter.Attribute);

            if (v is float)
            {
                return ((float)v).ToString("0.00");
            }
            else
            {
                return v.ToString();
            }
        }

        public override bool IsFiltered(int labelIndex)
        {
            if (IsDiscreet())
            {
                float n = labelIndex / (float)(NumberOfLabels() - 1);
                return n < attributeFilter.minFilter || n > attributeFilter.maxFilter;
            }
            else
            {
                float n = labelIndex / (float)(NumberOfLabels() - 1);
                float delta = Mathf.Lerp(attributeFilter.minScale, attributeFilter.maxScale, n);
                return delta < attributeFilter.minFilter || delta > attributeFilter.maxFilter;
            }
        }
    }

}