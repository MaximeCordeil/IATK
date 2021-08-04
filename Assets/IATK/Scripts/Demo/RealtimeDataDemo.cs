//RealTimeDataSource Tester
//Philipp Fleck, ICG @ TuGraz, philipp.fleck@icg.tugraz.at

using IATK;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IATKTest
{
    public class RealtimeDataDemo : MonoBehaviour
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
        RealtimeDataSource rtds = null;

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
            rtds = rtdsGo.AddComponent<RealtimeDataSource>();

            //Add Dimension
            rtds.AddDimension("xDim", 0, 100);
            rtds.AddDimension("yDim", 0, 100);
            rtds.AddDimension("zDim", 0, 100);
            rtds.AddDimension("size", 0, 100);

            rtds.AddDataByStr("xDim", 75f);
            rtds.AddDataByStr("xDim", 50f);
            rtds.AddDataByStr("xDim", 25f);

            rtds.AddDataByStr("yDim", 25f);
            rtds.AddDataByStr("yDim", 20f);
            rtds.AddDataByStr("yDim", 25f);

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

                if (vis.theVisualizationObject == null)
                {
                    vis.CreateVisualisation(AbstractVisualisation.VisualisationTypes.SCATTERPLOT);
                }

                vis.dataSource = rtds;
                vis.xDimension = "xDim";
                vis.yDimension = "yDim";
                vis.zDimension = "zDim";
                vis.sizeDimension = "size";
                vis.CreateVisualisation(AbstractVisualisation.VisualisationTypes.SCATTERPLOT);


                AbstractVisualisation abstractVisualisation = vis.theVisualizationObject;

                // Axis
                abstractVisualisation.visualisationReference.xDimension.Attribute = "xDim";
                abstractVisualisation.UpdateVisualisation(AbstractVisualisation.PropertyType.X);
                abstractVisualisation.visualisationReference.yDimension.Attribute = "yDim";
                abstractVisualisation.UpdateVisualisation(AbstractVisualisation.PropertyType.Y);
                abstractVisualisation.visualisationReference.zDimension.Attribute = "zDim";
                abstractVisualisation.UpdateVisualisation(AbstractVisualisation.PropertyType.Z);
                abstractVisualisation.visualisationReference.sizeDimension = "size";
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
                        rtds.AddDataByStr("xDim", UnityEngine.Random.value * 100f);
                        rtds.AddDataByStr("yDim", UnityEngine.Random.value * 100f);
                        rtds.AddDataByStr("zDim", UnityEngine.Random.value * 100f);
                        rtds.AddDataByStr("size", UnityEngine.Random.value * 100f);
                        if (isVisReady && vis != null)
                        {

                            Debug.Log("-- SimulPoints before vis ...");
                            //view.TweenPosition();
                            //vb.updateView();
                            vis.updateView(0);
                            Debug.Log("-- SimulPoints after vis ...");
                        }
                    }
                }
                catch (Exception err)
                {
                    Debug.LogError("SimulPoints ERROR => " + err);
                }
            }
        }
    }
}
