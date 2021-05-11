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


        private float[] GetDefaultArray()
        {
            var dataArray = new float[dimensionSizeLimit];
            for (int i = 0; i < dimensionSizeLimit; i++)
            {
                dataArray[i] = 0;
            }
            return dataArray;
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
                    Debug.Log("AddDataByStr => " + dimensionName + ", " + index + ", " + val + ", " + nextIndex);

                    if (nextIndex >= 0)
                    {
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
                            Debug.Log("XXXXXXXXXXXXXXXXXXXXXXXXX => new min = " + minV);
                        }

                        if (dd.MetaData.maxValue < val)
                        {
                            maxV = val;
                            dirty = true;
                            Debug.Log("XXXXXXXXXXXXXXXXXXXXXXXXX => new max = " + maxV);
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
            }catch(Exception e)
            {
                Debug.Log("XXXXXXXXXXXXXXXXXXXXXXXXX ERROR => " + e);
            }
            return dirty;
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
            return normValue;
        }

        public override object getOriginalValue(float normalisedValue, int identifier)
        {
            DimensionData.Metadata meta = this[identifier].MetaData;
            float normValue = normaliseValue(normalisedValue, 0f, 1f, meta.minValue, meta.maxValue);
            return normValue;
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
