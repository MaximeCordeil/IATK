using IATK;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using UnityEditor;
using System;

public class BrushingAndLinking : MonoBehaviour
{

    [SerializeField]
    public ComputeShader computeShader;
    ComputeBuffer buffer;

    ComputeBuffer cpInt;

    [SerializeField]
    public Material myRenderMaterial;

    RenderTexture brushedIndicesTexture;

    int kernelHandleBrushTexture;
    int kernelHandleBrushArrayIndices;

    [SerializeField]
    public List<Material> visualisationsMaterials;

    GameObject viewHolder;
    int texSize;

    [SerializeField]
    public Visualisation brushingVisualisation;

    [SerializeField]
    public List<Visualisation> brushedVisualisations;

    [SerializeField]
    public List<LinkingVisualisations> brushedLinkingVisualisations;

    [SerializeField]
    public bool showBrush = false;

    [SerializeField]
    public Color brushColor = Color.red;

    [SerializeField]
    public Transform input1;

    [SerializeField]
    public Transform input2;

    [SerializeField]
    [Range(0f, 1f)]
    public float radiusSphere;

    [SerializeField]
    public bool brushButtonController;

    public struct VecIndexPair
    {
        public Vector3 point;
        public int index;
    }

    public enum BrushType { SPHERE = 0, BOX = 1 };

    public BrushType BRUSH_TYPE;

    int computeTextureSize(int sizeDatast)
    {
        return NextPowerOf2((int)Mathf.Sqrt((float)sizeDatast));
    }

    public Material debugObjectTexture;

    public List<string> brushedData;

    /// <summary>
    /// Runs the shader and writes the result itn the "Result" texture
    /// </summary>
    /// <param name="texSize"></param>
    void RunShader(int texSize)
    {
        kernelHandleBrushTexture = computeShader.FindKernel("CSMain");
        kernelHandleBrushArrayIndices = computeShader.FindKernel("ComputeBrushedIndicesArray");

        computeShader.SetTexture(kernelHandleBrushTexture, "Result", brushedIndicesTexture);
        computeShader.Dispatch(kernelHandleBrushTexture, 32, 32, 1);

        setTexture(brushedIndicesTexture);
    }

    /// <summary>
    /// sets the index texture
    /// </summary>
    /// <param name="_tex"></param>
    public void setTexture(Texture _tex)
    {
        myRenderMaterial.SetTexture("_MainTex", _tex);
    }

    int[] brushIni;
    /// <summary>
    /// bind the data positions to the computer shader
    /// </summary>
    /// <param name="data"></param>
    public void setDataBuffer(Vector3[] data)
    {
        buffer = new ComputeBuffer(data.Length, 12);
        buffer.SetData(data);
        computeShader.SetBuffer(kernelHandleBrushTexture, "dataBuffer", buffer);

        cpInt = new ComputeBuffer(data.Length, 4);
        brushIni = new int[data.Length];
        for (int i = 0; i < data.Length; i++)
            brushIni[i] = -1;
        cpInt.SetData(brushIni);
        computeShader.SetBuffer(kernelHandleBrushArrayIndices, "brushedIndices", cpInt);
        computeShader.SetBuffer(kernelHandleBrushArrayIndices, "dataBuffer", buffer);


    }

    /// <summary>
    /// finds the next power of 2 for 
    /// </summary>
    /// <param name="number"></param>
    /// <returns></returns>
    private int NextPowerOf2(int number)
    {
        int pos = 0;

        while (number > 0)
        {
            pos++;
            number = number >> 1;
        }
        return (int)Mathf.Pow(2, pos);
    }

    // Use this for initialization
    void Start()
    {

        if (brushingVisualisation != null)
            //bind brushing vertices
            initializeComputeAndRenderBuffers(brushingVisualisation.theVisualizationObject.viewList[0].BigMesh.getBigMeshVertices());

        Visualisation.OnUpdateViewAction += Visualisation_OnUpdateViewAction;

    }

    private void Visualisation_OnUpdateViewAction(AbstractVisualisation.PropertyType propertyType)
    {
        if (propertyType == AbstractVisualisation.PropertyType.X || propertyType == AbstractVisualisation.PropertyType.Y || propertyType == AbstractVisualisation.PropertyType.Z)
            UpdateComputeBuffers();
    }



    public void initializeComputeAndRenderBuffers(Vector3[] data)
    {
        int datasetSize = data.Length;

        texSize = computeTextureSize(datasetSize);

        brushedIndicesTexture = new RenderTexture(texSize, texSize, 24);
        brushedIndicesTexture.enableRandomWrite = true;
        brushedIndicesTexture.filterMode = FilterMode.Point;
        brushedIndicesTexture.Create();

        setDataBuffer(data);
        setSize((float)texSize);

        RunShader(texSize);
    }

    public void UpdateComputeBuffers()
    {
        buffer.SetData(brushingVisualisation.theVisualizationObject.viewList[0].BigMesh.getBigMeshVertices());
        computeShader.SetBuffer(kernelHandleBrushTexture, "dataBuffer", buffer);
    }

    /// <summary>
    /// sets the size of the texture in the compute shader program. this is needed to adress the right uv coordinates
    /// to store the brushed information corretcly
    /// </summary>
    /// <param name="TexSize"></param>
    public void setSize(float TexSize)
    {
        computeShader.SetFloat("_size", TexSize);
    }

    float time = 0f;

    // Update is called once per frame
    void Update()
    {
        if (brushingVisualisation != null && (brushButtonController))// && brushedVisualisations.Count>0)
        {
            updateBrushTexture();

            //EXPERIMENTAL - GET details of original data
            // getDetailsOnDemand();

        }
    }

    /// <summary>
    /// reads the brushed indices
    /// </summary>
    public void readBrushTexture()
    {
        Texture2D tex = (brushingVisualisation.theVisualizationObject.viewList[0].BigMesh.SharedMaterial.GetTexture("_BrushedTexture") as Texture2D);
    }

    /// <summary>
    /// runs the compute shader kernel and updates the brushed indices
    /// </summary>
    /// Texture2D bla 
    Texture2D cachedTexture;
    public void updateBrushTexture()
    {
        //bla.ReadPixels(new Rect(),0,0).
        //set brushgin mode
        computeShader.SetInt("BrushMode", (int)(BRUSH_TYPE));

        Vector3 projectedPointer1;
        Vector3 projectedPointer2;

        switch (BRUSH_TYPE)
        {
            case BrushType.SPHERE:
                projectedPointer1 = brushingVisualisation.transform.InverseTransformPoint(input1.transform.position);
                //  Vector3 
                computeShader.SetFloat("pointer1x", projectedPointer1.x);
                computeShader.SetFloat("pointer1y", projectedPointer1.y);
                computeShader.SetFloat("pointer1z", projectedPointer1.z);
                break;
            case BrushType.BOX:
                projectedPointer1 = brushingVisualisation.transform.InverseTransformPoint(input1.transform.position);
                projectedPointer2 = brushingVisualisation.transform.InverseTransformPoint(input2.transform.position);

                //  Vector3 
                computeShader.SetFloat("pointer1x", projectedPointer1.x);
                computeShader.SetFloat("pointer1y", projectedPointer1.y);
                computeShader.SetFloat("pointer1z", projectedPointer1.z);

                computeShader.SetFloat("pointer2x", projectedPointer2.x);
                computeShader.SetFloat("pointer2y", projectedPointer2.y);
                computeShader.SetFloat("pointer2z", projectedPointer2.z);
                break;
            default:
                break;
        }

        //set the filters and normalisation values of the brushing visualisation to the computer shader
        computeShader.SetFloat("_MinNormX", brushingVisualisation.xDimension.minScale);
        computeShader.SetFloat("_MaxNormX", brushingVisualisation.xDimension.maxScale);
        computeShader.SetFloat("_MinNormY", brushingVisualisation.yDimension.minScale);
        computeShader.SetFloat("_MaxNormY", brushingVisualisation.yDimension.maxScale);
        computeShader.SetFloat("_MinNormZ", brushingVisualisation.zDimension.minScale);
        computeShader.SetFloat("_MaxNormZ", brushingVisualisation.zDimension.maxScale);

        computeShader.SetFloat("_MinX", brushingVisualisation.xDimension.minFilter);
        computeShader.SetFloat("_MaxX", brushingVisualisation.xDimension.maxFilter);
        computeShader.SetFloat("_MinY", brushingVisualisation.yDimension.minFilter);
        computeShader.SetFloat("_MaxY", brushingVisualisation.yDimension.maxFilter);
        computeShader.SetFloat("_MinZ", brushingVisualisation.zDimension.minFilter);
        computeShader.SetFloat("_MaxZ", brushingVisualisation.zDimension.maxFilter);

        computeShader.SetFloat("RadiusSphere", radiusSphere);

        //run the compute shader with all the filtering parameters
        computeShader.Dispatch(kernelHandleBrushTexture, 32, 32, 1);

        brushingVisualisation.theVisualizationObject.viewList[0].BigMesh.SharedMaterial.SetTexture("_BrushedTexture", brushedIndicesTexture);
        brushingVisualisation.theVisualizationObject.viewList[0].BigMesh.SharedMaterial.SetFloat("_DataWidth", texSize);
        brushingVisualisation.theVisualizationObject.viewList[0].BigMesh.SharedMaterial.SetFloat("_DataHeight", texSize);
        brushingVisualisation.theVisualizationObject.viewList[0].BigMesh.SharedMaterial.SetFloat("showBrush", Convert.ToSingle(showBrush));
        brushingVisualisation.theVisualizationObject.viewList[0].BigMesh.SharedMaterial.SetColor("brushColor", brushColor);

        foreach (var bv in brushedVisualisations)// visualisationsMaterials)
        {
            foreach (var v in bv.theVisualizationObject.viewList)
            {
                v.BigMesh.SharedMaterial.SetTexture("_BrushedTexture", brushedIndicesTexture);
                v.BigMesh.SharedMaterial.SetFloat("_DataWidth", texSize);
                v.BigMesh.SharedMaterial.SetFloat("_DataHeight", texSize);
                v.BigMesh.SharedMaterial.SetFloat("showBrush", Convert.ToSingle(showBrush));
                v.BigMesh.SharedMaterial.SetColor("brushColor", brushColor);
            }            
        }

        foreach (var item in brushedLinkingVisualisations)
        {
            item.View.BigMesh.SharedMaterial.SetTexture("_BrushedTexture", brushedIndicesTexture);
            item.View.BigMesh.SharedMaterial.SetFloat("_DataWidth", texSize);
            item.View.BigMesh.SharedMaterial.SetFloat("_DataHeight", texSize);
            item.View.BigMesh.SharedMaterial.SetFloat("showBrush", Convert.ToSingle(showBrush));
            item.View.BigMesh.SharedMaterial.SetColor("brushColor", brushColor);
        }

        //cachedTexture = (brushingVisualisation.theVisualizationObject.viewList[0].BigMesh.SharedMaterial.GetTexture("_BrushedTexture") as Texture2D);
        //if (cachedTexture.GetPixel(0, 0).r > 0f) print("selected!!");
        //float t = Time.time;
        //cpInt.GetData(brushIni);
        //  if (brushIni[0] > 0f) print("Selected");
        //getDetailsOnDemand();
        //debugObjectTexture.SetTexture("_MainTex", brushedIndicesTexture);
    }

    public void getDetailsOnDemand()
    {
        computeShader.Dispatch(kernelHandleBrushArrayIndices, 8, 1, 1);

        cpInt.GetData(brushIni);
        brushedData.Clear();

        for (int i = 0; i < brushIni.Length; i++)
        {
            if (brushIni[i] > 0)
            {
                float xbrushedValue = brushingVisualisation.dataSource[brushingVisualisation.xDimension.Attribute].Data[i];
                float ybrushedValue = brushingVisualisation.dataSource[brushingVisualisation.xDimension.Attribute].Data[i];
                float zbrushedValue = brushingVisualisation.dataSource[brushingVisualisation.xDimension.Attribute].Data[i];

                /*brushedData.Add*/
                print(
brushingVisualisation.dataSource.getOriginalValue(xbrushedValue, brushingVisualisation.xDimension.Attribute) +
" " +
brushingVisualisation.dataSource.getOriginalValue(ybrushedValue, brushingVisualisation.yDimension.Attribute) +
" " +
brushingVisualisation.dataSource.getOriginalValue(zbrushedValue, brushingVisualisation.zDimension.Attribute));
            }
        }
    }


    /// <summary>
    /// on destroy release the buffers on the graphic card
    /// </summary>
    void OnDestroy()
    {
        if (buffer != null)
            buffer.Release();

        if (cpInt != null)
            cpInt.Release();

        Visualisation.OnUpdateViewAction -= Visualisation_OnUpdateViewAction;
    }

    private void OnApplicationQuit()
    {
        if (buffer != null)
            buffer.Release();

        if (cpInt != null)
            cpInt.Release();

        Visualisation.OnUpdateViewAction -= Visualisation_OnUpdateViewAction;
    }

}