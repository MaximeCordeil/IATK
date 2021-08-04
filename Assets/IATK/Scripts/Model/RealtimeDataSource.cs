//RealTimeDataSource based on cycle and reuse
//Philipp Fleck, ICG @ TuGraz, philipp.fleck@icg.tugraz.at
//20210430, initial working version

using IATK;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IATK
{
    public class RealtimeDataSource : DataSource
    {
        private int dimensionSizeLimit = 100;
        private int dataCount;

        private List<DimensionData> dimensionData = new List<DimensionData>();
        private List<float[]> dimenstionDataArrays = new List<float[]>();
        private List<int> lastIndices = new List<int>();

        //do we need that
        
        private Dictionary<string, Dictionary<int, string>> textualDimensionsList =
            new Dictionary<string, Dictionary<int, string>>();
        private Dictionary<string, Dictionary<string, int>> textualDimensionsListReverse = 
            new Dictionary<string, Dictionary<string, int>>();

        private float[] GetDefaultArray()
        {
            var dataArray = new float[dimensionSizeLimit];
            for (int i = 0; i < dimensionSizeLimit; i++)
            {
                dataArray[i] = 0;
            }
            return dataArray;
        }

        public void AddStringDimension(string dimensionName)
        {
            if (!textualDimensionsList.ContainsKey(dimensionName))
            {
                textualDimensionsList.Add(dimensionName, new Dictionary<int, string>());
                textualDimensionsListReverse.Add(dimensionName, new Dictionary<string, int>());
                
                var metaData = new DimensionData.Metadata();
                metaData.type = DataType.String; //maybe make that adjustable
                metaData.minValue = 0;
                metaData.maxValue = dimensionSizeLimit;
                int newIndex = dimensionData.Count;

                var dataArray = GetDefaultArray();
                dimenstionDataArrays.Add(dataArray);
                lastIndices.Add(0);

                var dd = new DimensionData(dimensionName, newIndex, metaData);
                dd.setData(dataArray, textualDimensionsList);
                dimensionData.Add(dd);
                dataCount = dimensionSizeLimit;
                Debug.Log("AddDimension => " + dd.Identifier + ", " + dd.Index);
            }
        }

        public void AddDimension(string dimensionName, float minVal, float maxVal)
        {
            var dataArray = GetDefaultArray();
            dimenstionDataArrays.Add(dataArray);
            lastIndices.Add(0);

            //what are categories?
            var metaData = new DimensionData.Metadata();
            metaData.minValue = minVal;
            metaData.maxValue = maxVal;
            metaData.type = DataType.Float; //maybe make that adjustable
            int newIndex = dimensionData.Count;

            //for testing
            if (!textualDimensionsList.ContainsKey(dimensionName))
            {
                textualDimensionsList.Add(dimensionName, new Dictionary<int, string>());
                textualDimensionsListReverse.Add(dimensionName, new Dictionary<string, int>());
            }

            var dd = new DimensionData(dimensionName, newIndex, metaData);
            dd.setData(dataArray, textualDimensionsList);
            dimensionData.Add(dd);
            //dataCount += dimensionSizeLimit;
            dataCount = dimensionSizeLimit;
            Debug.Log("AddDimension => " + dd.Identifier + ", " + dd.Index);
        }

        //This important for extern bindinds which might not support
        //operator overloading or runtime reflection resolution
        public void AddDataByIdnx(int index, float val)
        {
            if (index < dimensionData.Count)
            {
                var nextIndex = GetNextIndexForDimensionAndInc(index);
                if (nextIndex >= 0)
                {
                    var dd = dimensionData[index];
                    dd.Data[nextIndex] = normaliseValue(val, dd.MetaData.minValue, dd.MetaData.maxValue, 0f, 1f);
                }
            }
        }

        private int GetNextIndexForDimensionAndInc(int index)
        {
            return 0;
            if (index < lastIndices.Count)
            {
                var ret = lastIndices[index];
                lastIndices[index] = (lastIndices[index] + 1) % dimensionSizeLimit;
                return ret;
            }
            return -1;
        }

        //This important for extern bindinds which might not support
        //operator overloading or runtime reflection resolution
        public bool AddDataByStr(string dimensionName, float val)
        {
            var dirty = false;
            try
            {
                var dd = this[dimensionName];
                if (dd != null)
                {
                    var index = dd.Index;
                    var nextIndex = GetNextIndexForDimensionAndInc(index);
                    if (nextIndex >= 0)
                    {
                        //data shift
                        for (var i = dimensionSizeLimit - 1; i >= 1; i--)
                        {
                            dd.Data[i] = dd.Data[i - 1];
                        }

                        var minV = dd.MetaData.minValue;
                        var maxV = dd.MetaData.minValue;
                        if (dd.MetaData.minValue > val)
                        {
                            minV = val;
                            dirty = true;
                        }

                        if (dd.MetaData.maxValue < val)
                        {
                            maxV = val;
                            dirty = true;
                        }

                        if (dirty)
                        {
                            var metaData = new DimensionData.Metadata();
                            metaData.minValue = minV;
                            metaData.maxValue = maxV;
                            metaData.type = DataType.Float; //maybe make that adjustable
                            dd.setMetadata(metaData);
                        }

                        if (dd.Data.Length > nextIndex && nextIndex >= 0)
                        {
                            dd.Data[nextIndex] = normaliseValue(val, dd.MetaData.minValue, dd.MetaData.maxValue, 0f, 1f);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Log("AddDataByStr ERROR => " + e);
            }
            return dirty;
        }

        
        public void AddStrDataByStr(string dimensionName, string val)
        {
            var dd = this[dimensionName];
            if (dd != null)
            {
                var N = textualDimensionsList[dimensionName].Count;

                if (!textualDimensionsList[dimensionName].ContainsKey(N))
                {
                    textualDimensionsList[dimensionName].Add(N, val);
                }
                if (!textualDimensionsListReverse.ContainsKey(val))
                {
                    textualDimensionsListReverse[dimensionName].Add(val, N);
                }
                var idx = (N) % dimensionSizeLimit;
                dd.Data[idx] = idx;
            }
        }

        public override DimensionData this[int index]
        {
            get { return dimensionData[index]; }
        }

        public override DimensionData this[string identifier]
        {
            get
            {
                foreach (DimensionData d in dimensionData)
                {
                    if (d.Identifier == identifier)
                    {
                        return d;
                    }
                }
                return null;
            }
        }

        //can always be loaded since we fill it at runtime
        public override bool IsLoaded
        {
            get { return true; }
        }

        public override int DimensionCount
        {
            get { return dimensionData.Count; }
        }

        public override int DataCount
        {
            get { return dataCount; }
        }

        public override int getNumberOfCategories(int identifier)
        {
            throw new System.NotImplementedException();
        }

        public override int getNumberOfCategories(string identifier)
        {
            throw new System.NotImplementedException();
        }

        public override object getOriginalValue(float normalisedValue, string identifier)
        {
            DimensionData.Metadata meta = this[identifier].MetaData;
            float normValue = normaliseValue(normalisedValue, 0f, 1f, meta.minValue, meta.maxValue);

            if (meta.type == DataType.String)
            {
                normValue = normaliseValue(valueClosestTo(this[identifier].Data, normalisedValue), 0f, 1f, meta.minValue, meta.maxValue);
                return textualDimensionsList[this[identifier].Identifier][(int)normValue];  // textualDimensions[(int)normValue];
            }

            return normValue;
        }

        public override object getOriginalValue(float normalisedValue, int identifier)
        {
            DimensionData.Metadata meta = this[identifier].MetaData;
            float normValue = normaliseValue(normalisedValue, 0f, 1f, meta.minValue, meta.maxValue);

            if (meta.type == DataType.String)
            {
                normValue = normaliseValue(valueClosestTo(this[identifier].Data, normalisedValue), 0f, 1f, meta.minValue, meta.maxValue);
                return textualDimensionsList[this[identifier].Identifier][(int)normValue];  // textualDimensions[(int)normValue];
            }

            return normValue;
        }

        //from CSVDAtasource
        public float valueClosestTo(float[] collection, float target)
        {
            float closest_value = collection[0];
            float subtract_result = Math.Abs(closest_value - target);
            for (int i = 1; i < collection.Length; i++)
            {
                if (Math.Abs(collection[i] - target) < subtract_result)
                {
                    subtract_result = Math.Abs(collection[i] - target);
                    closest_value = collection[i];
                }
            }
            return closest_value;
        }

        //from CSVDataSource
        float normaliseValue(float value, float i0, float i1, float j0, float j1)
        {
            float L = (j0 - j1) / (i0 - i1);
            return (j0 - (L * i0) + (L * value));
        }

        public override object getOriginalValuePrecise(float normalisedValue, int identifier)
        {
            throw new System.NotImplementedException();
        }

        public override object getOriginalValuePrecise(float normalisedValue, string identifier)
        {
            throw new System.NotImplementedException();
        }

        public override void load()
        {
            //will be filled at runtime
        }

        public override void loadHeader()
        {
            throw new System.NotImplementedException();
        }

        private void Awake()
        {
            DataSourceManager.register(this);
            if (!IsLoaded)
                load();

            //default init for realtime data
            AddStringDimension("names");
            AddStrDataByStr("names", "id");
        }



        // Start is called before the first frame update
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
