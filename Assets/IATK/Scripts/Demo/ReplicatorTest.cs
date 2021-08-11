//ReplicatorTest
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
    public class ReplicatorTest : MonoBehaviour
    {
        Replicator repl = null;
        GameObject visGo = null;
        Visualisation vis = null;
        RealtimeDataSource rtds = null;

        bool isVisReady = false;

        void Start()
        {
            StartCoroutine(DelayedInit());
        }

        IEnumerator DelayedInit()
        {
            yield return new WaitForSeconds(2f);

#if UNITY_EDITOR
            yield return new WaitForSeconds(0.5f);
            CreateWithRTDataSource();
#endif

            yield return new WaitForSeconds(3f);
            repl = FindObjectOfType<IATK.Replicator>();

            if (repl == null)
            {
                Debug.Log("ReplicatorTest repl not found, adding new");
                repl = gameObject.AddComponent<IATK.Replicator>();
            }
            Debug.Log("ReplicatorTest repl => " + repl + ", " + repl.gameObject);

            if (repl != null)
            {
                ListenForReplicationUpdates();
                repl.Publish = PublishImple;
                repl.PublishDatasource = PublishDatasourceImple;
                repl.GetStreamData = GetStreamData;
            }
            Debug.Log("ReplicatorTest repl setup done ");

#if UNITY_EDITOR
            yield return new WaitForSeconds(6f);
            StartReplication();

            Debug.Log("ReplicatorTest replication started ...");
#endif
        }

        public void StartReplication()
        {
            if (vis == null)
            {
                return;
            }

            if (vis.uid.Length > 0 && repl != null)
            {
                repl.AddVisToReplicate(vis.uid, vis, false);
                repl.AddDataSourceDef(vis.uid, "telemetry/inffeld16/2nd/id2068/littleserver/RJ45/#", "", "bandwidth_Mbps");
                repl.AddDataSourceDef(vis.uid, "telemetry/inffeld16/2nd/id2068/bigserver/RJ45/#", "", "inbound_Mbps");
            }
        }

        public string PublishImple(string id, string payload)
        {
            string topic = "rr/vis/replication/" + id + "/view";
#if USE_MQTT
            Vizario.MQTTManager.Publish(topic, payload);
#endif
            return topic;
        }

        public string PublishDatasourceImple(string id, string payload)
        {
            string topic = "rr/vis/replication/" + id + "/ds";
#if USE_MQTT
            Vizario.MQTTManager.Publish(topic, payload);
#endif
            return topic;
        }

        public string GetStreamData(string uri, Func<string, string, string> OnNewData)
        {
 #if USE_MQTT
            Vizario.MQTTManager.Subscribe(uri);
            Vizario.MQTTManager.RegisterCallbackTopicCs(OnNewData, uri);
#endif
            return "";
        }

        public void ListenForReplicationUpdates()
        {
            string t = "rr/vis/replication/#";
#if USE_MQTT
            Vizario.MQTTManager.Subscribe(t);
            Vizario.MQTTManager.RegisterCallbackTopicCs(
                (string topic, string payload) =>
                {
                    Debug.Log("ListenForReplicationUpdates => topic:" + topic + ", payload:" + payload);

                    string uid = null;
                    var parts = topic.Split('/');
                    var plType = parts[parts.Length - 1];

                    if (parts.Length > 1)
                    {
                        uid = parts[parts.Length - 2];
                    }

                    Debug.Log("ListenForReplicationUpdates plType=" + plType + ", uid=" + uid);

                    if (plType == "view" && uid != null && uid.Length > 0)
                    {
                        //process payload
                        Debug.Log("ListenForReplicationUpdates updating repliques ...");
                        repl.UpdateReplicas(uid, payload);
                    }

                    if (plType == "ds" && uid != null && uid.Length > 0)
                    {
                        Debug.Log("ListenForReplicationUpdates updating repliques ...");
                        repl.UpdateDatasource(uid, payload);
                    }

                    return topic;
                }, t);
#endif
        }

        void CreateWithRTDataSource()
        {
            //create DataSource
            var rtdsGo = new GameObject("TestRTDS");
            rtds = rtdsGo.AddComponent<RealtimeDataSource>();

            if (false)
            {
                //Add Dimension
                rtds.AddDimension("DimA", 0, 100);
                rtds.AddDimension("DimB", 0, 100);
                rtds.AddDimension("DimC", 0, 100);
                rtds.AddDimension("DimD", 0, 100);

                rtds.SetData("DimA", 75f);
                rtds.SetData("DimA", 50f);
                rtds.SetData("DimA", 25f);

                rtds.SetData("DimB", 25f);
                rtds.SetData("DimB", 20f);
                rtds.SetData("DimB", 25f);

                StartCoroutine(SimulPoints());
            }
            else
            {
                rtds.AddDimension("id", 0, 100);
                for (var i = 0; i < 100; i++)
                {
                    rtds.SetData("id", i);
                }
                //rtds.AddDefaultIdDimension();
                if (true)
                {
                    var t = "telemetry/inffeld16/2nd/id2068/littleserver/RJ45/#";
                    var key = "bandwidth_Mbps";
                    rtds.AddDimension(key, 0, 100);

                    #if USE_MQTT
                    Vizario.MQTTManager.Subscribe(t);
                    Vizario.MQTTManager.RegisterCallbackTopicCs((string topic, string payload) =>
                    {
                        //Debug.Log("ReplicatorTest OnNewData topic=" + topic + ", payload=" + payload);
                        if (payload.Length > 0)
                        {
                            try
                            {
                                var parsed = Json.JSON.Parse(payload); //this might need change
                                var val = parsed[key];

                                if (val != null && val.ToString().Length > 0)
                                {
                                    float fVal = (float)double.Parse(val.ToString());
                                    if (rtds != null)
                                    {
                                        Debug.Log("ReplicatorTest key=" + key + ", fVal=" + fVal);
                                        rtds.SetData(key, fVal);
                                    }
                                    else
                                    {
                                        Debug.Log("ReplicatorTest OnNewData is NULL!");
                                    }

                                    if (isVisReady && vis != null)
                                    {
                                        vis.updateView(0);
                                    }
                                    else
                                    {
                                        Debug.Log("ReplicatorTest OnNewData isVisReady=" + isVisReady + ", vis=" + vis);
                                    }
                                }
                            }
                            catch (Exception err)
                            {
                                Debug.Log("ReplicatorTest OnNewData ERROR => " + err);
                            }
                        }
                        return "";
                    }, t);
#endif
                }

                if (true)
                {
                    var t = "telemetry/inffeld16/2nd/id2068/bigserver/RJ45/#";
                    var key = "inbound_Mbps";
                    rtds.AddDimension(key, 0, 100);

#if USE_MQTT
                    Vizario.MQTTManager.Subscribe(t);
                    Vizario.MQTTManager.RegisterCallbackTopicCs((string topic, string payload) =>
                    {
                        //Debug.Log("ReplicatorTest OnNewData topic=" + topic + ", payload=" + payload);
                        if (payload.Length > 0)
                        {
                            try
                            {
                                var parsed = Json.JSON.Parse(payload); //this might need change
                                var val = parsed[key];

                                if (val != null && val.ToString().Length > 0)
                                {
                                    float fVal = (float)double.Parse(val.ToString());
                                    if (rtds != null)
                                    {
                                        rtds.SetData(key, fVal);
                                    }
                                    else
                                    {
                                        Debug.Log("ReplicatorTest OnNewData is NULL!");
                                    }

                                    if (isVisReady && vis != null)
                                    {
                                        vis.updateView(0);
                                    }
                                    else
                                    {
                                        Debug.Log("ReplicatorTest OnNewData isVisReady=" + isVisReady + ", vis=" + vis);
                                    }
                                }
                            }
                            catch (Exception err)
                            {
                                Debug.Log("ReplicatorTest OnNewData ERROR => " + err);
                            }
                        }
                        return "";
                    }, t);
#endif
                }
            }
            //add source to graph
            CreateVisFromSource();
        }

        Visualisation CreateVisFromSource()
        {
            visGo = new GameObject("myTester");
            Debug.Log("Spawned myTester");

            vis = visGo.AddComponent<Visualisation>();
            Debug.Log("Add Visualizsation");
            try
            {

                if (vis != null)
                {
                    Debug.Log("CreateVisFromSource 1");
                    if (vis.theVisualizationObject == null)
                    {
                        vis.CreateVisualisation(AbstractVisualisation.VisualisationTypes.SCATTERPLOT);
                    }

                    vis.dataSource = rtds;
                    vis.CreateVisualisation(AbstractVisualisation.VisualisationTypes.SCATTERPLOT);
                    vis.geometry = AbstractVisualisation.GeometryType.Bars;

                    AbstractVisualisation abstractVisualisation = vis.theVisualizationObject;

                    Debug.Log("CreateVisFromSource 2");

                    // Axis
                    if (false)
                    {
                        abstractVisualisation.visualisationReference.xDimension.Attribute = "DimA";
                        abstractVisualisation.UpdateVisualisation(AbstractVisualisation.PropertyType.X);
                        abstractVisualisation.visualisationReference.yDimension.Attribute = "DimB";
                        abstractVisualisation.UpdateVisualisation(AbstractVisualisation.PropertyType.Y);
                        abstractVisualisation.visualisationReference.zDimension.Attribute = "DimC";
                        abstractVisualisation.UpdateVisualisation(AbstractVisualisation.PropertyType.Z);
                        abstractVisualisation.visualisationReference.sizeDimension = "DimD";
                        abstractVisualisation.UpdateVisualisation(AbstractVisualisation.PropertyType.Size);
                    }
                    else
                    {
                        Debug.Log("CreateVisFromSource 2a");
                        abstractVisualisation.visualisationReference.xDimension.Attribute = "id";
                        abstractVisualisation.UpdateVisualisation(AbstractVisualisation.PropertyType.X);
                        Debug.Log("CreateVisFromSource 2b");
                        abstractVisualisation.visualisationReference.yDimension.Attribute = "bandwidth_Mbps";
                        abstractVisualisation.UpdateVisualisation(AbstractVisualisation.PropertyType.Y);
                        Debug.Log("CreateVisFromSource 2c");
                        abstractVisualisation.visualisationReference.zDimension.Attribute = "Undefined";
                        abstractVisualisation.UpdateVisualisation(AbstractVisualisation.PropertyType.Z);
                        Debug.Log("CreateVisFromSource 2d");
                        abstractVisualisation.visualisationReference.sizeDimension = "Undefined";
                        abstractVisualisation.UpdateVisualisation(AbstractVisualisation.PropertyType.Size);
                    }

                    Debug.Log("CreateVisFromSource 3");

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

                    Debug.Log("CreateVisFromSource 4");

                    gradient.SetKeys(colorKey, alphaKey);
                    abstractVisualisation.visualisationReference.dimensionColour = gradient;
                    abstractVisualisation.visualisationReference.colourDimension = "bandwidth_Mbps";
                    abstractVisualisation.UpdateVisualisation(AbstractVisualisation.PropertyType.Colour);

                    Debug.Log("CreateVisFromSource 5");

                    isVisReady = true;
                }
            }
            catch (Exception err)
            {
                Debug.Log("CreateVisFromSource ERROR => " + err);
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
                            vis.updateView(0);
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
