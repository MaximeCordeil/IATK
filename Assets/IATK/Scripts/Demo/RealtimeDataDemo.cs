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
            rtds.AddDimension("DimA", 0, 100);
            rtds.AddDimension("DimB", 0, 100);
            rtds.AddDimension("DimC", 0, 100);
            rtds.AddDimension("DimD", 0, 100);

            /*
            rtds.AddStrDataByStr("names", "DimA");
            rtds.AddStrDataByStr("names", "DimB");
            rtds.AddStrDataByStr("names", "DimC");
            rtds.AddStrDataByStr("names", "DimD");
            */

            rtds.SetData("DimA", 75f);
            rtds.SetData("DimA", 50f);
            rtds.SetData("DimA", 25f);

            rtds.SetData("DimB", 25f);
            rtds.SetData("DimB", 20f);
            rtds.SetData("DimB", 25f);

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
                //vis.xDimension = "DimA";
                //vis.yDimension = "DimB";
                //vis.zDimension = "DimC";
                //vis.sizeDimension = "DimD";
                vis.CreateVisualisation(AbstractVisualisation.VisualisationTypes.SCATTERPLOT);


                AbstractVisualisation abstractVisualisation = vis.theVisualizationObject;

                // Axis
                abstractVisualisation.visualisationReference.xDimension.Attribute = "DimA"; ;
                abstractVisualisation.UpdateVisualisation(AbstractVisualisation.PropertyType.X);
                abstractVisualisation.visualisationReference.yDimension.Attribute = "DimB";
                abstractVisualisation.UpdateVisualisation(AbstractVisualisation.PropertyType.Y);
                abstractVisualisation.visualisationReference.zDimension.Attribute = "DimC";
                abstractVisualisation.UpdateVisualisation(AbstractVisualisation.PropertyType.Z);
                abstractVisualisation.visualisationReference.sizeDimension = "DimD";
                abstractVisualisation.UpdateVisualisation(AbstractVisualisation.PropertyType.Size);

                var gradient = new Gradient();

                // Populate the color keys at the relative time 0 and 1 (0 and 100%)
                var colorKey = new GradientColorKey[2];
                colorKey[0].color = Color.red;
                colorKey[0].time = 0.0f;
                colorKey[1].color = Color.blue;
                colorKey[1].time = 1.0f;

                // Populate the alpha  keys at relative time 0 and 1  (0 and 100%)
                var alphaKey = new GradientAlphaKey[2];
                alphaKey[0].alpha = 1.0f;
                alphaKey[0].time = 0.0f;
                alphaKey[1].alpha = 0.0f;
                alphaKey[1].time = 1.0f;

                gradient.SetKeys(colorKey, alphaKey);
                abstractVisualisation.visualisationReference.dimensionColour = gradient;
                abstractVisualisation.visualisationReference.colourDimension = "DimD";
                abstractVisualisation.UpdateVisualisation(AbstractVisualisation.PropertyType.Colour);

                //abstractVisualisation.visualisationReference.colorPaletteDimension = "DimD";
                //abstractVisualisation.UpdateVisualisation(AbstractVisualisation.PropertyType.Colour);

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
                        rtds.SetData("DimA", UnityEngine.Random.value * 100f);
                        rtds.SetData("DimB", UnityEngine.Random.value * 100f);
                        rtds.SetData("DimC", UnityEngine.Random.value * 100f);
                        rtds.SetData("DimD", UnityEngine.Random.value * 100f);
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
