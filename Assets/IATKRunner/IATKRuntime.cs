//RealTimeDataSource Tester
//Philipp Fleck, ICG @ TuGraz, philipp.fleck@icg.tugraz.at

using IATK;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IATKRuntime : MonoBehaviour
{
    // Start is called before the first frame update
    int n;
    float[] myValsX;
    float[] myValsY;
    float[] myValsZ;
    float[] mySizes;
    Color[] myColors;
    View view = null;
    ViewBuilder vb;
    GameObject visGo = null;
    Visualisation vis = null;
    RealTimeDataSource rtds = null;

    bool isVisReady = false;

    void Start()
    {
        //CreateCodeBasedDummy();
        StartCoroutine(DelayedInit());
    }

    IEnumerator DelayedInit()
    {
        yield return new WaitForSeconds(0.5f);
        CreateWithRTDataSource();
    }

    void CreateWithRTDataSource()
    {
        //create DataSource
        var rtdsGo = new GameObject("TestRTDS");
        rtds = rtdsGo.AddComponent<RealTimeDataSource>();

        //Add Dimension
        rtds.AddDimension("DimA", 0, 100);
        rtds.AddDimension("DimB", 0, 100);
        rtds.AddDimension("DimC", 0, 100);
        rtds.AddDimension("DimD", 0, 100);

        rtds.AddDataByStr("DimA", 75f);
        rtds.AddDataByStr("DimA", 50f);
        rtds.AddDataByStr("DimA", 25f);

        rtds.AddDataByStr("DimB", 25f);
        rtds.AddDataByStr("DimB", 20f);
        rtds.AddDataByStr("DimB", 25f);

        StartCoroutine(SimulPoints());

        //add source to graph
        CreateVisFromSource();
    }

    Visualisation CreateVisFromSource()
    {
        visGo = new GameObject("myTester");
        Debug.Log("Spawned myTester");

        vis = visGo.AddComponent<Visualisation>();
        Debug.Log("Add Visualizsation");

        if (vis != null)
        {

            if(vis.theVisualizationObject == null)
            {
                vis.CreateVisualisation(AbstractVisualisation.VisualisationTypes.SCATTERPLOT);
            }

            vis.dataSource = rtds;
            vis.xDimension = "DimA";
            vis.yDimension = "DimB";
            vis.zDimension = "DimC";
            vis.sizeDimension = "DimD";
            vis.CreateVisualisation(AbstractVisualisation.VisualisationTypes.SCATTERPLOT);


            AbstractVisualisation abstractVisualisation = vis.theVisualizationObject;
            
            // Axis
            abstractVisualisation.visualisationReference.xDimension.Attribute = "DimA";
            abstractVisualisation.UpdateVisualisation(AbstractVisualisation.PropertyType.X);
            abstractVisualisation.visualisationReference.yDimension.Attribute = "DimB";
            abstractVisualisation.UpdateVisualisation(AbstractVisualisation.PropertyType.Y);
            abstractVisualisation.visualisationReference.zDimension.Attribute = "DimC";
            abstractVisualisation.UpdateVisualisation(AbstractVisualisation.PropertyType.Z);
            abstractVisualisation.visualisationReference.sizeDimension = "DimD";
            abstractVisualisation.UpdateVisualisation(AbstractVisualisation.PropertyType.OriginDimension);

            vis.geometry = AbstractVisualisation.GeometryType.Bars;
                        
            Debug.Log("Init vis 6");
            isVisReady = true;
        }
        return vis;
    }

    IEnumerator SimulPoints()
    {
        yield return new WaitForSeconds(5f);
        //view = visGo.GetComponentInChildren<View>();
        while (true)
        {
            yield return new WaitForSeconds(0.01f);
            try
            {
                if (rtds)
                {
                    rtds.AddDataByStr("DimA", UnityEngine.Random.value * 100f);
                    rtds.AddDataByStr("DimB", UnityEngine.Random.value * 100f);
                    rtds.AddDataByStr("DimC", UnityEngine.Random.value * 100f);
                    rtds.AddDataByStr("DimD", UnityEngine.Random.value * 100f);
                    if (isVisReady && vis != null)
                    {
                        
                        Debug.Log("-- SimulPoints before vis ...");
                        //view.TweenPosition();
                        //vb.updateView();
                        vis.updateView(0);
                        Debug.Log("-- SimulPoints after vis ...");
                    }
                }
            } catch(Exception err)
            {
                Debug.LogError("SimulPoints ERROR => " + err);
            }
        }
    }

    private void SetVisulisationParameters()
    {
        /*
        if (IATKVisualisation.theVisualizationObject == null)
        {
            IATKVisualisation.CreateVisualisation(AbstractVisualisation.VisualisationTypes.SCATTERPLOT);
        }

        AbstractVisualisation abstractVisualisation = IATKVisualisation.theVisualizationObject;
        VisualisationVariant parameters = visualisationVariant;

        // Axis
        abstractVisualisation.visualisationReference.xDimension.Attribute = parameters.xAxis;
        abstractVisualisation.UpdateVisualisation(AbstractVisualisation.PropertyType.X);
        abstractVisualisation.visualisationReference.yDimension.Attribute = parameters.yAxis;
        abstractVisualisation.UpdateVisualisation(AbstractVisualisation.PropertyType.Y);
        abstractVisualisation.visualisationReference.zDimension.Attribute = parameters.zAxis;
        abstractVisualisation.UpdateVisualisation(AbstractVisualisation.PropertyType.Z);
        */

        /*
        // Connectivity
        if (parameters.geometryType == AbstractVisualisation.GeometryType.Lines ||
            parameters.geometryType == AbstractVisualisation.GeometryType.LinesAndDots)
        {
            abstractVisualisation.visualisationReference.linkingDimension = parameters.linkingDimension;
            abstractVisualisation.UpdateVisualisation(AbstractVisualisation.PropertyType.LinkingDimension);
        }

        // Set this to undefined to avoid problems
        abstractVisualisation.visualisationReference.graphDimension = VisualisationVariant.undefined;
        abstractVisualisation.UpdateVisualisation(AbstractVisualisation.PropertyType.GraphDimension);

        // Aestethics
        if (parameters.linkingDimension != VisualisationVariant.undefined) // TODO also check for GraphDimension
        {
            abstractVisualisation.visualisationReference.geometry = parameters.geometryType;
            abstractVisualisation.UpdateVisualisation(AbstractVisualisation.PropertyType.GeometryType);
        }

        // Turn off shadows
        MeshRenderer[] meshRenderers = IATKVisualisation.theVisualizationObject.GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer meshRenderer in meshRenderers)
        {
            meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }
        */
    }


    /*
    void CreateCodeBasedDummy()
    {
        n = 150;
        myValsX = new float[n];
        myValsY = new float[n];
        myValsZ = new float[n];
        mySizes = new float[n];
        myColors = new Color[n];

        for (var i = 0; i < n; i++)
        {
            //myValsX[i] = (float)(i * i) / (float)(i + 1);
            myValsX[i] = (float)i / (float)n;
            myValsY[i] = 1f / (float)(i + 1);
            myValsZ[i] = 1f / (float)(i + 1); ;
            myColors[i] = new Color((i * 2 + 1) % 255, (i * 3 * i / 2) % 255, ((i + 13) * (i + 2) * (i + 1)) % 255);
            //myColors[i] = Color.red;
            mySizes[i] = 1;
        }

        vb = new ViewBuilder(MeshTopology.Points, "TestVis").
                         initialiseDataView(n).
                         setDataDimension(myValsX, ViewBuilder.VIEW_DIMENSION.X).
                         setDataDimension(myValsY, ViewBuilder.VIEW_DIMENSION.Y).
                         setDataDimension(myValsZ, ViewBuilder.VIEW_DIMENSION.Z).
                         setSize(mySizes).
                         setColors(myColors);

        // use the IATKUtil class to get the corresponding Material mt 
        Material mt = IATKUtil.GetMaterialFromTopology(AbstractVisualisation.GeometryType.Points);
        mt.SetFloat("_MinSize", 0.01f);
        mt.SetFloat("_MaxSize", 0.05f); //0.05


        //vb.updateView();
        visGo = new GameObject("myTester");
        vis = visGo.AddComponent<Visualisation>();
        vis.CreateVisualisation(AbstractVisualisation.VisualisationTypes.SCATTERPLOT);

        var xAxisGo = new GameObject("xAxis");
        xAxisGo.transform.parent = visGo.transform;
        var xAxis = xAxisGo.AddComponent<IATK.Axis>();

        view = vb.updateView().apply(visGo, mt);
    }
    */

    // Update is called once per frame
    void Update()
    {
        /*
        for (var i = 0; i < n; i++)
        {
            myValsX[i] *= 1.0005f;
        }
        view.UpdateXPositions(myValsX);
        vb.updateView();
        */
    }
}
