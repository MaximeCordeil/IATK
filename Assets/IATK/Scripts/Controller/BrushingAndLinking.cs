using IATK;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System;

public class BrushingAndLinking : MonoBehaviour
{

    [SerializeField]
    public ComputeShader computeShader;

    private ComputeBuffer dataBuffer;
    private ComputeBuffer filteredIndicesBuffer;
    
    [SerializeField]
    public Material myRenderMaterial;

    RenderTexture brushedIndicesTexture;

    int kernelHandleBrushTexture;
    int kernelHandleBrushArrayIndices;
    
    int texSize;

    [SerializeField]
    public List<Visualisation> brushingVisualisations;
    [SerializeField]
    public List<LinkingVisualisations> brushedLinkingVisualisations;

    [SerializeField]
    public bool showBrush = false;

    [SerializeField]
    public Color brushColor = Color.red;

    [SerializeField]
    [Range (1f,10f)]
    public float brushSizeFactor = 1f;

    [SerializeField]
    public Transform input1;
    [SerializeField]
    public Transform input2;

    [SerializeField]
    [Range(0f, 1f)]
    public float radiusSphere;

    [SerializeField]
    public bool brushButtonController;
    
    public BrushType BRUSH_TYPE;
    public enum BrushType
    {
        SPHERE = 0,
        BOX
    };

    public SelectionType SELECTION_TYPE;
    public enum SelectionType
    {
        FREE = 0,
        ADD,
        SUBTRACT
    }


    public Material debugObjectTexture;
    public List<string> brushedData;
    private bool hasInitialised = false;
    private bool hasFreeBrushReset = false;

    private void Start()
    {
        InitialiseShaders();
    }

    private void InitialiseShaders()
    {
        kernelHandleBrushTexture = computeShader.FindKernel("CSMain");
    }

    private void InitialiseBuffersAndTextures(int dataCount)
    {
        dataBuffer = new ComputeBuffer(dataCount, 12);
        dataBuffer.SetData(new Vector3[dataCount]);
        computeShader.SetBuffer(kernelHandleBrushTexture, "dataBuffer", dataBuffer);

        filteredIndicesBuffer = new ComputeBuffer(dataCount, 4);
        filteredIndicesBuffer.SetData(new float[dataCount]);
        computeShader.SetBuffer(kernelHandleBrushTexture, "filteredIndicesBuffer", filteredIndicesBuffer);


        texSize = NextPowerOf2((int)Mathf.Sqrt(dataCount));
        brushedIndicesTexture = new RenderTexture(texSize, texSize, 24);
        brushedIndicesTexture.enableRandomWrite = true;
        brushedIndicesTexture.filterMode = FilterMode.Point;
        brushedIndicesTexture.Create();

        myRenderMaterial.SetTexture("_MainTex", brushedIndicesTexture);

        computeShader.SetFloat("_size", texSize);
        computeShader.SetTexture(kernelHandleBrushTexture, "Result", brushedIndicesTexture);

        hasInitialised = true;
    }


    public void UpdateComputeBuffers(Visualisation visualisation)
    {
        dataBuffer.SetData(visualisation.theVisualizationObject.viewList[0].BigMesh.getBigMeshVertices());
        computeShader.SetBuffer(kernelHandleBrushTexture, "dataBuffer", dataBuffer);

        filteredIndicesBuffer.SetData(visualisation.theVisualizationObject.viewList[0].GetFilterChannel());
        computeShader.SetBuffer(kernelHandleBrushTexture, "filteredIndicesBuffer", filteredIndicesBuffer);
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

    private void Update()
    {
        if (brushButtonController && brushingVisualisations.Count > 0)
        {
            if (hasInitialised)
            {
                UpdateBrushTexture();

                //EXPERIMENTAL - GET details of original data
                // getDetailsOnDemand();
            }
            else
            {
                InitialiseBuffersAndTextures(brushingVisualisations[0].dataSource.DataCount);
            }
        }
    }

    private void UpdateBrushTexture()
    {
        Vector3 projectedPointer1;
        Vector3 projectedPointer2;

        computeShader.SetInt("BrushMode", (int)BRUSH_TYPE);
        computeShader.SetInt("SelectionMode", (int)SELECTION_TYPE);

        hasFreeBrushReset = false;

        foreach (var vis in brushingVisualisations)
        {
            UpdateComputeBuffers(vis);

            switch (BRUSH_TYPE)
            {
                case BrushType.SPHERE:
                    projectedPointer1 = vis.transform.InverseTransformPoint(input1.transform.position);

                    computeShader.SetFloats("pointer1", projectedPointer1.x, projectedPointer1.y, projectedPointer1.z);

                    break;
                case BrushType.BOX:
                    projectedPointer1 = vis.transform.InverseTransformPoint(input1.transform.position);
                    projectedPointer2 = vis.transform.InverseTransformPoint(input2.transform.position);
                    
                    computeShader.SetFloats("pointer1", projectedPointer1.x, projectedPointer1.y, projectedPointer1.z);
                    computeShader.SetFloats("pointer2", projectedPointer2.x, projectedPointer2.y, projectedPointer2.z);
                    break;
                default:
                    break;
            }

            //set the filters and normalisation values of the brushing visualisation to the computer shader
            computeShader.SetFloat("_MinNormX", vis.xDimension.minScale);
            computeShader.SetFloat("_MaxNormX", vis.xDimension.maxScale);
            computeShader.SetFloat("_MinNormY", vis.yDimension.minScale);
            computeShader.SetFloat("_MaxNormY", vis.yDimension.maxScale);
            computeShader.SetFloat("_MinNormZ", vis.zDimension.minScale);
            computeShader.SetFloat("_MaxNormZ", vis.zDimension.maxScale);

            computeShader.SetFloat("_MinX", vis.xDimension.minFilter);
            computeShader.SetFloat("_MaxX", vis.xDimension.maxFilter);
            computeShader.SetFloat("_MinY", vis.yDimension.minFilter);
            computeShader.SetFloat("_MaxY", vis.yDimension.maxFilter);
            computeShader.SetFloat("_MinZ", vis.zDimension.minFilter);
            computeShader.SetFloat("_MaxZ", vis.zDimension.maxFilter);

            computeShader.SetFloat("RadiusSphere", radiusSphere);

            computeShader.SetFloat("width", vis.width);
            computeShader.SetFloat("height", vis.height);
            computeShader.SetFloat("depth", vis.depth);

            // Tell the shader whether or not the visualisation's points have already been reset by a previous brush, required to allow for multiple visualisations to be brushed
            if (SELECTION_TYPE == SelectionType.FREE)
                computeShader.SetBool("HasFreeBrushReset", hasFreeBrushReset);

            //run the compute shader with all the filtering parameters
            computeShader.Dispatch(kernelHandleBrushTexture, Mathf.CeilToInt(texSize / 32f), Mathf.CeilToInt(texSize / 32f), 1);

            vis.theVisualizationObject.viewList[0].BigMesh.SharedMaterial.SetTexture("_BrushedTexture", brushedIndicesTexture);
            vis.theVisualizationObject.viewList[0].BigMesh.SharedMaterial.SetFloat("_DataWidth", texSize);
            vis.theVisualizationObject.viewList[0].BigMesh.SharedMaterial.SetFloat("_DataHeight", texSize);
            vis.theVisualizationObject.viewList[0].BigMesh.SharedMaterial.SetFloat("showBrush", Convert.ToSingle(showBrush));
            vis.theVisualizationObject.viewList[0].BigMesh.SharedMaterial.SetColor("brushColor", brushColor);

            hasFreeBrushReset = true;
        }
    }

    
    /// <summary>
    /// on destroy release the buffers on the graphic card
    /// </summary>
    void OnDestroy()
    {
        if (dataBuffer != null)
            dataBuffer.Release();

        if (filteredIndicesBuffer != null)
            filteredIndicesBuffer.Release();
    }

    private void OnApplicationQuit()
    {
        if (dataBuffer != null)
            dataBuffer.Release();

        if (filteredIndicesBuffer != null)
            filteredIndicesBuffer.Release();
    }
}