//Replicator to sync and replicate visualizations
//Philipp Fleck, ICG @ TuGraz, philipp.fleck@icg.tugraz.at
//20210708, initial working version

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IATK
{
    public class Replicator : MonoBehaviour
    {
        //publish hook to add any mqtt library or similar
        public Func<string, string, string> Publish = null;
        public Func<string, string, string> PublishDatasource = null;
        public Func<string, Func<string, string, string>, string> GetStreamData = null;
        public Func<string, string> NewVisSpawnNotification = null;

        Dictionary<string, VisHolder> replicas = new Dictionary<string, VisHolder>();
        Dictionary<string, VisHolder> primes = new Dictionary<string, VisHolder>();

        // a placeholder datasource to allow vis async vis initialization
        // will be replaced once the datasource definition is received
        private RealtimeDataSource dummyDs = null;

        /// <summary>
        /// Datasource definition needed to sync data across
        /// </summary>
        [Serializable]
        public class DsDef
        {
            public string payload;
            public string dimensionName;
            public string accessKey;
            public bool isSetUp = false;
        }

        /// <summary>
        /// Datasource information used for replicated initialization
        /// </summary>
        [Serializable]
        public class DataSourceInfo
        {
            public string dataSourceName;
            public string dataSourceType;
            public List<DsDef> dataSourceDefinitions;
        }


        /// <summary>
        /// Class to keep track of visualizations
        /// </summary>
        public class VisHolder
        {
            public Visualisation vis;
            public bool allowPrimeChange = false;
            public List<DsDef> dsDefs = new List<DsDef>();
            public bool dsDirty = true;

            public VisHolder(Visualisation vis)
            {
                this.vis = vis;
            }

            public VisHolder(Visualisation vis, bool allowPrimeChange = false)
            {
                this.vis = vis;
                this.allowPrimeChange = allowPrimeChange;
            }
        }

        public void Awake()
        {
            if (dummyDs == null)
            {
                dummyDs = new GameObject("ReplDummyDS").AddComponent<RealtimeDataSource>();
            }
        }

        /// <summary>
        /// Exposed setter to set callbacks
        /// </summary>
        /// <param name="publish"></param>
        /// <param name="publishDatasource"></param>
        /// <param name="getStreamDatam"></param>
        /// <param name="newVisSpawnNotification"></param>
        public void SetCallbacks(
            Func<string, string, string> publish,
            Func<string, string, string> publishDatasource,
            Func<string, Func<string, string, string>, string> getStreamDatam,
            Func<string, string> newVisSpawnNotification)
        {
            Publish = publish;
            PublishDatasource = publishDatasource;
            GetStreamData = getStreamDatam;
            NewVisSpawnNotification = newVisSpawnNotification;
        }

        /// <summary>
        /// Adds vis for replication
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="vis"></param>
        public void AddVisToReplicate(string uid, Visualisation vis, bool allowPrimeChange)
        {
            Debug.Log("Replicator::AddVisToReplicate uid=" + uid + " / " + vis.uid);
            if (primes != null)
            {
                if (!primes.ContainsKey(uid))
                {
                    try
                    {
                        Debug.Log("Replicator::AddVisToReplicate vis=" + vis);
                        Debug.Log("Replicator::AddVisToReplicate vis.dataSource=" + vis.dataSource);

                        var vh = new VisHolder(vis, allowPrimeChange);
                        primes.Add(uid, vh);
                        vis.theVisualizationObject.creationConfiguration.SetReplicationCallback(uid, Replicate);
                        vis.theVisualizationObject.creationConfiguration.Serialize("");
                        Debug.Log("Replicator::AddVisToReplicate done ...");
                    }
                    catch (Exception err)
                    {
                        Debug.Log("Replicator::AddVisToReplicate ERROR => " + err);
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="payload"></param>
        /// <param name="dimensionName"></param>
        /// <param name="accessKey"></param>
        public void AddDataSourceDef(string uid, string payload, string dimensionName, string accessKey)
        {
            Debug.Log("Replicator::AddDataSourceDef => " + uid + ", " + dimensionName + ", " + accessKey);
            if (primes != null)
            {
                VisHolder vh;
                var dsd = new DsDef();
                dsd.payload = payload;
                dsd.accessKey = accessKey;
                dsd.dimensionName = dimensionName;
                if (primes.TryGetValue(uid, out vh))
                {
                    vh.dsDefs.Add(dsd);
                    vh.dsDirty = true;
                    Debug.Log("Replicator::AddDataSourceDef => added dsdef for " + uid);
                }

                if (vh.vis != null)
                {
                    vh.vis.theVisualizationObject.creationConfiguration.Serialize("");
                }
            }
        }

        /// <summary>
        /// Stops the replication
        /// </summary>
        /// <param name="uid"></param>
        public void StopVisReplication(string uid)
        {
            if (primes != null)
            {
                if (primes.ContainsKey(uid))
                {
                    if (primes[uid].vis != null)
                    {
                        primes[uid].vis.theVisualizationObject.creationConfiguration.ReplicationNotification = null;
                    }
                    primes.Remove(uid);
                }
            }
        }

        /// <summary>
        /// This function is passed to vis.createConfiguration to report the serialized config
        /// </summary>
        /// <param name="id"></param>
        /// <param name="payload"></param>
        /// <returns></returns>
        public string Replicate(string id, string payload)
        {
            if (payload.Length > 0 && primes.ContainsKey(id) && primes[id].vis != null)
            {
                Publish?.Invoke(id, payload);

                if (PublishDatasource != null)
                {
                    VisHolder vbh;
                    if (primes.TryGetValue(id, out vbh))
                    {
                        if (vbh != null && vbh.dsDirty && vbh.vis != null && vbh.vis.dataSource != null)
                        {
                            vbh.dsDirty = false;
                            var ds = vbh.vis.dataSource;
                            Debug.Log("Replicator::DS Rp vbh.vis.name=" + vbh.vis.name);
                            if (ds != null)
                            {
                                var dsName = ds.name;
                                var dsType = ds.GetType();
                                Debug.Log("Replicator::DS Rp dsName=" + dsName);
                                Debug.Log("Replicator::DS Rp dsType=" + dsType);

                                var dsInfo = new DataSourceInfo();
                                dsInfo.dataSourceName = dsName;
                                dsInfo.dataSourceType = dsType.ToString();
                                dsInfo.dataSourceDefinitions = vbh.dsDefs;

                                var dsInfoJson = JsonUtility.ToJson(dsInfo);
                                PublishDatasource?.Invoke(id, dsInfoJson);
                            }
                            else
                            {
                                Debug.Log("Replicator::DS NULL ds=" + ds);
                            }
                        }
                    }
                }
            }
            return id;
        }

        /// <summary>
        /// This function should be called when received a new payload for a view
        /// </summary>
        /// <param name="id"></param>
        /// <param name="payload"></param>
        /// <returns></returns>
        public void UpdateReplicas(string uid, string payload)
        {
            Debug.Log("Replicator::UpdateRepliques uid=" + uid + ", #payload=" + payload.Length);
            if (uid.Length > 0)
            {
                if (replicas.ContainsKey(uid))
                {
                    var vis = replicas[uid].vis;
                    if (payload.Length > 0 && vis != null)
                    {
                        vis.theVisualizationObject.creationConfiguration.DeserializeJson(payload);
                        SyncVis(vis);
                    }
                    if (vis != null)
                    {
                        vis.updateView(0);
                    }
                }
                else
                {
                    if (payload.Length > 0)
                    {
                        SpawnReplicatedVis(uid, payload);
                    }

                    if (primes.ContainsKey(uid))
                    {
                        //received changes from replique 
                        var vh = primes[uid];
                        if (vh.allowPrimeChange)
                        {
                            if (payload.Length > 0)
                            {
                                vh.vis.theVisualizationObject.creationConfiguration.DeserializeJson(payload);
                                SyncVis(vh.vis);
                            }
                            vh.vis.updateView(0);
                        }
                    }
                    else
                    {
                        //if (payload.Length > 0)
                        //{
                        //    SpawnReplicatedVis(uid, payload);
                        //}
                    }
                }
            }
        }

        /// <summary>
        /// This function should be called when received a new datasource for an existing view
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="payload"></param>
        public void UpdateDatasource(string uid, string payload)
        {
            Debug.Log("Replicator::UpdateDatasource uid= " + uid + ", payload=" + payload);
            if (uid.Length > 0)
            {
                if (replicas.ContainsKey(uid))
                {
                    var vis = replicas[uid].vis;
                    if (payload.Length > 0)
                    {
                        //TODO apply ds hee to vis
                        var dsInfo = JsonUtility.FromJson<DataSourceInfo>(payload);
                        Debug.Log("Replicator::UpdateDatasource dsInfo => " + dsInfo);

                        if (dsInfo != null)
                        {
                            var dsName = dsInfo.dataSourceName;
                            var dsTypeStr = dsInfo.dataSourceType;
                            Type dsType = Type.GetType(dsTypeStr);

                            Debug.Log("Replicator::UpdateDatasource type " + dsTypeStr + " vs " + dsType);

                            if (dsType != null)
                            {
                                Component ds = null;
                                var dsGo = GameObject.Find("replique-" + dsName); //TODO check if it already exists
                                if (dsGo == null)
                                {
                                    dsGo = new GameObject("replique-" + dsName);
                                    ds = dsGo.AddComponent(dsType);
                                }
                                ds = dsGo.GetComponent(dsType);

                                Debug.Log("Replicator::UpdateDatasource created ds ...");

                                if (dsType.Equals(typeof(RealtimeDataSource)))
                                {
                                    vis.dataSource = ds as RealtimeDataSource;
                                    Debug.Log("Replicator::UpdateDatasource set ds to vis ...");

                                    Debug.Log("Replicator::UpdateDatasource found #defs = " + dsInfo.dataSourceDefinitions.Count);
                                    if (dsInfo.dataSourceDefinitions.Count > 0)
                                    {
                                        foreach (var dsdef in dsInfo.dataSourceDefinitions)
                                        {
                                            if (dsdef.isSetUp)
                                            {
                                                continue;
                                            }
                                            var dKey = dsdef.accessKey;
                                            var dDimName = dsdef.dimensionName;
                                            var dPayload = dsdef.payload;

                                            Debug.Log("Replicator::UpdateDatasource adding defs => " + dKey + ", " + dDimName);

                                            var rtds = ds as RealtimeDataSource;
                                            vis.dataSource = rtds;

                                            //rtds.AddDefaultIdDimension();
                                            rtds.AddDimension("id", 0, 100);
                                            for (var i = 0; i < 100; i++)
                                            {
                                                rtds.SetData("id", i);
                                            }
                                            rtds.AddDimension(dDimName, 0, 100); //was dKey
                                            vis.updateView(0);

                                            Debug.Log("Replicator::UpdateDatasource before calling  GetStreamData");

                                            if (GetStreamData != null)
                                            {
                                                GetStreamData(dPayload, (string topic, string content) =>
                                                {
                                                    try
                                                    {
                                                        Debug.Log("Replicator::UpdateDatasource OnNewData => " + content);
                                                        //TODO add here json parser of your choice
#if true
                                                        var parsed = new Dictionary<string, string>(); //this is only a placeholder for a json parser
#else
                                                        object parsed = Json.JSON.Parse(content);
#endif
                                                        var val = parsed[dKey];

                                                        Debug.Log("Replicator::UpdateDatasource OnNewData parsed[" + dKey + "]=" + val);

                                                        if (val != null && val.ToString().Length > 0)
                                                        {
                                                            double fVal;
                                                            bool isNum = double.TryParse(val.ToString(), out fVal);
                                                            if (isNum)
                                                            {
                                                                if (rtds != null)
                                                                {
                                                                    //rtds.AddDataByStr(dDimName, (float)fVal);
                                                                    rtds.SetData(dDimName, (float)fVal);
                                                                }
                                                                else
                                                                {
                                                                    Debug.Log("Replicator::UpdateDatasource rtds is NULL!");
                                                                }
                                                            }
                                                            else
                                                            {
                                                                //TODO handle string types here
                                                            }

                                                            if (vis != null)
                                                            {
                                                                vis.updateView(0);
                                                            }
                                                            else
                                                            {
                                                                Debug.Log("Replicator::UpdateDatasource vis is NULL!");
                                                            }
                                                        }
                                                    }
                                                    catch (Exception err)
                                                    {
                                                        Debug.Log("Replicator::UpdateDatasource ERROR => " + err);
                                                    }
                                                    return "";
                                                });
                                            }
                                            dsdef.isSetUp = true;
                                        }
                                    }
                                }

                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Spawns a replique of a received configuration
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="payload"></param>
        private void SpawnReplicatedVis(string uid, string payload)
        {
            try
            {
                Debug.Log("Replicator::SpawnReplicatedVis uid=" + uid);
                var goName = "replique-" + uid;
                var go = new GameObject(goName);
                var vis = go.AddComponent<Visualisation>();

                if (vis != null)
                {
                    replicas.Add(uid, new VisHolder(vis));
                    //vis.dataSource = rtds; //TODO sync that

                    if (vis.theVisualizationObject == null)
                    {
                        vis.geometry = AbstractVisualisation.GeometryType.Bars;
                        vis.CreateVisualisation(AbstractVisualisation.VisualisationTypes.SCATTERPLOT);
                    }

                    vis.theVisualizationObject.creationConfiguration.DeserializeJson(payload);


                    if (vis.dataSource == null)
                    {
                        vis.dataSource = dummyDs;
                    }

                    SyncVis(vis);

                    if (NewVisSpawnNotification != null)
                    {
                        NewVisSpawnNotification?.Invoke(goName);
                    }
                }
            }
            catch (Exception err)
            {
                Debug.Log("Replicator::SpawnReplicatedVis ERROR => " + err);
            }
        }

        /// <summary>
        /// Performs a vis update
        /// </summary>
        /// <param name="vis"></param>
        private void SyncVis(Visualisation vis)
        {
            try
            {
                vis.geometry = vis.theVisualizationObject.creationConfiguration.Geometry;
                vis.theVisualizationObject.visualisationReference.geometry = vis.theVisualizationObject.creationConfiguration.Geometry;

                if (vis.theVisualizationObject.creationConfiguration.Axies.ContainsKey(CreationConfiguration.Axis.X))
                {
                    vis.theVisualizationObject.visualisationReference.xDimension = vis.theVisualizationObject.creationConfiguration.Axies[CreationConfiguration.Axis.X];
                }

                if (vis.theVisualizationObject.creationConfiguration.Axies.ContainsKey(CreationConfiguration.Axis.Y))
                {
                    vis.theVisualizationObject.visualisationReference.yDimension = vis.theVisualizationObject.creationConfiguration.Axies[CreationConfiguration.Axis.Y];
                }

                if (vis.theVisualizationObject.creationConfiguration.Axies.ContainsKey(CreationConfiguration.Axis.Z))
                {
                    vis.theVisualizationObject.visualisationReference.zDimension = vis.theVisualizationObject.creationConfiguration.Axies[CreationConfiguration.Axis.Z];
                }

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

                if (vis != null)
                {
                    vis.updateView(0);
                }
            }
            catch (Exception err)
            {
                Debug.Log("SyncVis ERROR => " + err);
            }
        }
    }
}
