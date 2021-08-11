//RealtimeDataReplicationDemo to test hooks in
//CreationConfiguration and Visualization
//Philipp Fleck, ICG @ TuGraz, philipp.fleck@icg.tugraz.at
//20210708, initial working version

//#define USE_MQTT

using IATK;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IATKTest
{
    public class RealtimeDataReplicationDemo : MonoBehaviour
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

            yield return new WaitForSeconds(1f);
            ConsumeReplicas();
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

        public class VisHolder
        {
            public Visualisation vis;
            public DataSource datasSource;
            public VisHolder(Visualisation vis)
            {
                this.vis = vis;
            }
        }

        Dictionary<string, VisHolder> replicas = new Dictionary<string, VisHolder>();
        public void ConsumeReplicas()
        {
            string t = "rr/vis/replication/#";
#if USE_MQTT
            Vizario.MQTTManager.Subscribe(t);
            Vizario.MQTTManager.RegisterCallbackTopicCs(
                (string topic, string payload) =>
                {
                    Debug.Log("ConsumeReplicas => topic:" + topic + ", payload:" + payload);

                    string uid = null;
                    var parts = topic.Split('/');
                    if (parts[parts.Length - 1] == "view")
                    {
                        uid = parts[parts.Length - 2];
                    }


                    if (replicas.ContainsKey(uid))
                    {
                        var vis = replicas[uid].vis;
                        if (payload.Length > 0)
                        {
                            vis.theVisualizationObject.creationConfiguration.DeserializeJson(payload);
                            SyncVis(vis);
                        }
                        vis.updateView(0);
                    }
                    else
                    {
                        if (payload.Length > 0)
                        {
                            SpawnReplicatedVis(uid, payload);
                        }
                    }

                    return topic;
                }, t);
#endif
        }

        void SpawnReplicatedVis(string uid, string payload)
        {
            visGo = new GameObject("replic-" + uid);
            var vis = visGo.AddComponent<Visualisation>();

            if (vis != null)
            {

                replicas.Add(uid, new VisHolder(vis));
                vis.dataSource = rtds; //TODO sync that

                if (vis.theVisualizationObject == null)
                {
                    vis.geometry = AbstractVisualisation.GeometryType.Bars;
                    vis.CreateVisualisation(AbstractVisualisation.VisualisationTypes.SCATTERPLOT);
                }

                vis.theVisualizationObject.creationConfiguration.DeserializeJson(payload);
                SyncVis(vis);
            }
        }

        void SyncVis(Visualisation vis)
        {
            vis.geometry = vis.theVisualizationObject.creationConfiguration.Geometry;
            vis.theVisualizationObject.visualisationReference.geometry = vis.theVisualizationObject.creationConfiguration.Geometry;
            vis.theVisualizationObject.visualisationReference.xDimension = vis.theVisualizationObject.creationConfiguration.Axies[CreationConfiguration.Axis.X];
            vis.theVisualizationObject.visualisationReference.yDimension = vis.theVisualizationObject.creationConfiguration.Axies[CreationConfiguration.Axis.Y];
            vis.theVisualizationObject.visualisationReference.zDimension = vis.theVisualizationObject.creationConfiguration.Axies[CreationConfiguration.Axis.Z];
            vis.theVisualizationObject.visualisationReference.sizeDimension = vis.theVisualizationObject.creationConfiguration.SizeDimension;
            vis.theVisualizationObject.visualisationReference.dimensionColour = vis.theVisualizationObject.creationConfiguration.colourKeys;
            vis.theVisualizationObject.visualisationReference.colourDimension = vis.theVisualizationObject.creationConfiguration.ColourDimension;
            vis.theVisualizationObject.visualisationReference.linkingDimension = vis.theVisualizationObject.creationConfiguration.LinkingDimension;


            vis.theVisualizationObject.visualisationReference.size = vis.theVisualizationObject.creationConfiguration.Size;
            vis.theVisualizationObject.visualisationReference.minSize = vis.theVisualizationObject.creationConfiguration.MinSize;
            vis.theVisualizationObject.visualisationReference.maxSize = vis.theVisualizationObject.creationConfiguration.MaxSize;

            vis.theVisualizationObject.visualisationReference.width = vis.theVisualizationObject.creationConfiguration.VisualisationWidth;
            vis.theVisualizationObject.visualisationReference.height = vis.theVisualizationObject.creationConfiguration.VisualisationHeight;
            vis.theVisualizationObject.visualisationReference.depth = vis.theVisualizationObject.creationConfiguration.VisualisationDepth;

            vis.CreateVisualisation(vis.theVisualizationObject.creationConfiguration.VisualisationType);
            vis.theVisualizationObject.UpdateVisualisation(AbstractVisualisation.PropertyType.X);
            vis.theVisualizationObject.UpdateVisualisation(AbstractVisualisation.PropertyType.Y);
            vis.theVisualizationObject.UpdateVisualisation(AbstractVisualisation.PropertyType.Z);
            vis.updateView(0);
        }

        public string Replicate(string id, string payload)
        {
            Debug.Log("ReplicationNotification => id:" + id + ", payload:" + payload);
            string topic = "rr/vis/replication/" + id + "/view";
#if USE_MQTT
            Vizario.MQTTManager.Publish(topic, payload);
#else
            string uid = id;
            if (replicas.ContainsKey(uid))
            {
                var vis = replicas[uid].vis;
                if (payload.Length > 0)
                {
                    vis.theVisualizationObject.creationConfiguration.DeserializeJson(payload);
                    SyncVis(vis);
                }
                vis.updateView(0);
            }
            else
            {
                if (payload.Length > 0)
                {
                    SpawnReplicatedVis(uid, payload);
                }
            }
#endif
            return id;
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
                vis.geometry = AbstractVisualisation.GeometryType.Bars;

                vis.theVisualizationObject.creationConfiguration.uid = vis.uid;
                vis.theVisualizationObject.creationConfiguration.ReplicationNotification = Replicate;

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

                            //Debug.Log("-- SimulPoints before vis ...");
                            //view.TweenPosition();
                            //vb.updateView();
                            vis.updateView(0);
                            //Debug.Log("-- SimulPoints after vis ...");
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
