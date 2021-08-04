//RealTimeDataSource Tester
//Philipp Fleck, ICG @ TuGraz, philipp.fleck@icg.tugraz.at

using IATK;
using System;
using System.Collections;
using UnityEngine;

namespace IATKTest
{
    public class RealtimeDataDemo : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            CreateWithRTDataSource();
        }

        void CreateWithRTDataSource()
        {
            GameObject rtdsGo = new GameObject("[IATK] New Realtime Data Source");
            RealtimeDataSource rtds = rtdsGo.AddComponent<RealtimeDataSource>();

            //Add dimensions
            rtds.AddDimension("xDim", 0, 100);
            rtds.AddDimension("yDim", 0, 100);
            rtds.AddDimension("zDim", 0, 100);
            rtds.AddDimension("sizeDim", 0, 100);

            rtds.AddDataByStr("xDim", 75f);
            rtds.AddDataByStr("xDim", 50f);
            rtds.AddDataByStr("xDim", 25f);

            rtds.AddDataByStr("yDim", 25f);
            rtds.AddDataByStr("yDim", 20f);
            rtds.AddDataByStr("yDim", 25f);

            //Add source to visualisation
            Visualisation vis = CreateVisFromSource(rtds);

            StartCoroutine(SimulateDataPoints(rtds, vis));
        }

        Visualisation CreateVisFromSource(RealtimeDataSource rtds)
        {
            GameObject visGo = new GameObject("[IATK] New Realtime Visualisation");
            Debug.Log("Spawned Realtime Visualisation");

            Visualisation vis = visGo.AddComponent<Visualisation>();
            Debug.Log("Add Visualisation");

            if (vis != null)
            {
                vis.dataSource = rtds;
                vis.xDimension = "xDim";
                vis.yDimension = "yDim";
                vis.zDimension = "zDim";
                vis.sizeDimension = "sizeDim";
                vis.CreateVisualisation(AbstractVisualisation.VisualisationTypes.SCATTERPLOT);

                AbstractVisualisation abstractVisualisation = vis.theVisualizationObject;

                // Set axis
                abstractVisualisation.visualisationReference.xDimension.Attribute = "xDim";
                abstractVisualisation.UpdateVisualisation(AbstractVisualisation.PropertyType.X);

                abstractVisualisation.visualisationReference.yDimension.Attribute = "yDim";
                abstractVisualisation.UpdateVisualisation(AbstractVisualisation.PropertyType.Y);

                abstractVisualisation.visualisationReference.zDimension.Attribute = "zDim";
                abstractVisualisation.UpdateVisualisation(AbstractVisualisation.PropertyType.Z);

                abstractVisualisation.visualisationReference.sizeDimension = "sizeDim";
                abstractVisualisation.UpdateVisualisation(AbstractVisualisation.PropertyType.OriginDimension);

                vis.geometry = AbstractVisualisation.GeometryType.Bars;

                Debug.Log("Visualisation Initialized");
            }
            return vis;
        }

        IEnumerator SimulateDataPoints(RealtimeDataSource rtds, Visualisation vis)
        {
            while (true)
            {
                try
                {
                    if (rtds != null & vis != null)
                    {
                        rtds.AddDataByStr("xDim", UnityEngine.Random.value * 100f);
                        rtds.AddDataByStr("yDim", UnityEngine.Random.value * 100f);
                        rtds.AddDataByStr("zDim", UnityEngine.Random.value * 100f);
                        rtds.AddDataByStr("sizeDim", UnityEngine.Random.value * 100f);

                        Debug.Log("SimulateDataPoints...");
                        vis.updateView(0);
                    }
                }
                catch (Exception err)
                {
                    Debug.LogError("SimulateDataPoints ERROR => " + err);
                }

                yield return new WaitForSeconds(0.01f);
            }
        }
    }
}