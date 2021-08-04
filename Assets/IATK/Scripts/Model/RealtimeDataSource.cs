//RealTimeDataSource based on cycle and reuse
//Philipp Fleck, ICG @ TuGraz, philipp.fleck@icg.tugraz.at
//20210430, initial working version

using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace IATK
{
    public class RealtimeDataSource : DataSource
    {
        /* 
        The max amount of data that can be displayed at a single time,
        will loop back around (replacing older data) when it goes over this limit
        */ 
        private int dimensionSizeLimit = 100;
        private bool isQuitting;
        private int dataCount;

        private List<DimensionData> dimensionData = new List<DimensionData>();
        private List<int> lastIndices = new List<int>();
        
        private Dictionary<string, Dictionary<int, string>> textualDimensionsList = new Dictionary<string, Dictionary<int, string>>();
        private Dictionary<string, Dictionary<string, int>> textualDimensionsListReverse = new Dictionary<string, Dictionary<string, int>>();

        private float[] GetDefaultArray()
        {
            var dataArray = new float[dimensionSizeLimit];
            for (int i = 0; i < dimensionSizeLimit; i++)
            {
                dataArray[i] = 0;
            }
            return dataArray;
        }

        /// <summary>
        /// Creates a dimension that can later have data set to it
        /// </summary>
        /// <param name="dimensionName">Sets the dimension name, used to identify the dimension (Must be unique).</param>
        /// <param name="type">The data type of the dimension</param>
        /// <returns>True if successfully added, false otherwise</returns>
        public bool AddDimension(string dimensionName, DataType type = DataType.String)
        {
            return AddDimension(dimensionName, 0, dimensionSizeLimit, type);
        }

        /// <summary>
        /// Creates a dimension that can later have data set to it
        /// </summary>
        /// <param name="dimensionName">Sets the dimension name, used to identify the dimension (Must be unique).</param>
        /// <param name="minVal">The minimum value the dimension can hold</param>
        /// <param name="maxVal">The maximum value the dimension can hold</param>
        /// <param name="type">The data type of the dimension</param>
        /// <returns>True if successfully added, false otherwise</returns>
        public bool AddDimension(string dimensionName, float minVal, float maxVal, DataType type = DataType.Float)
        {
            // Don't add the dimension if it already exists
            if (textualDimensionsList.ContainsKey(dimensionName)) return false;

            lastIndices.Add(0);

            var metaData = new DimensionData.Metadata();
            metaData.minValue = minVal;
            metaData.maxValue = maxVal;
            metaData.type = type;

            int index = dimensionData.Count;

            textualDimensionsList.Add(dimensionName, new Dictionary<int, string>());
            textualDimensionsListReverse.Add(dimensionName, new Dictionary<string, int>());

            var dd = new DimensionData(dimensionName, index, metaData);
            dd.setData(GetDefaultArray(), textualDimensionsList);
            dimensionData.Add(dd);

            dataCount = dimensionSizeLimit;
            
            Debug.Log("AddDimension => " + dd.Identifier + ", " + dd.Index);

            return true;
        }

        /// <summary>
        /// Sets a data value by index
        /// </summary>
        /// <param name="index">Index of dimension</param>
        /// <param name="val">Value to set the dimension</param>
        /// <returns>True if successfully set, false otherwise</returns>
        public bool SetData(int index, float val)
        {
            if (index < dimensionData.Count)
            {
                var dd = this[index];

                if(dd.MetaData.type != DataType.Float) return false;

                // TODO: Find a better solution than shifting the data,
                //       perhapse have a var for the current item being replaced that loops around
                //data shift
                for (var i = dimensionSizeLimit - 1; i >= 1; i--)
                {
                    dd.Data[i] = dd.Data[i - 1];
                }

                if (dd.MetaData.minValue <= val && dd.MetaData.maxValue >= val
                    && dd.Data.Length > 0 && dd.MetaData.type == DataType.Float)
                {
                    dd.Data[0] = normaliseValue(val, dd.MetaData.minValue, dd.MetaData.maxValue, 0f, 1f);

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Sets a data value by dimension name (identifier)
        /// </summary>
        /// <param name="index">Name of dimension</param>
        /// <param name="val">Value to set the dimension</param>
        /// <returns>True if successfully set, false otherwise</returns>
        public bool SetData(string dimensionName, float val)
        {
            try
            {
                var dd = this[dimensionName];
                if (dd != null && dd.MetaData.type == DataType.Float)
                {
                    // TODO: Find a better solution than shifting the data,
                    //       perhapse have a var for the current item being replaced that loops around
                    //data shift
                    for (var i = dimensionSizeLimit - 1; i >= 1; i--)
                    {
                        dd.Data[i] = dd.Data[i - 1];
                    }

                    // Question: This section is used to alter the min an max values if they do not fit,
                    //  this kinda goes against the point of having a min and max value?
                    // Question: Can we remove it?

                    // var minV = dd.MetaData.minValue;
                    // var maxV = dd.MetaData.minValue;
                    // if (dd.MetaData.minValue > val)
                    // {
                    //     minV = val;
                    //     dirty = true;
                    // }

                    // if (dd.MetaData.maxValue < val)
                    // {
                    //     maxV = val;
                    //     dirty = true;
                    // }

                    // if (dirty)
                    // {
                    //     var metaData = new DimensionData.Metadata();
                    //     metaData.minValue = minV;
                    //     metaData.maxValue = maxV;
                    //     metaData.type = DataType.Float; //maybe make that adjustable
                    //     dd.setMetadata(metaData);
                    // }

                    if (dd.MetaData.minValue <= val && dd.MetaData.maxValue >= val && dd.Data.Length > 0)
                    {
                        dd.Data[0] = normaliseValue(val, dd.MetaData.minValue, dd.MetaData.maxValue, 0f, 1f);

                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Log("SetData ERROR => " + e);
            }

            return false;
        }

        /// <summary>
        /// Sets a data value by dimension name (identifier)
        /// </summary>
        /// <param name="index">Name of dimension</param>
        /// <param name="val">Value to set the dimension</param>
        /// <returns>True if successfully set, false otherwise</returns>
        public bool SetData(string dimensionName, string val)
        {
            try
            {
                var dd = this[dimensionName];
                if (dd == null || dd.MetaData.type != DataType.String) return false;

                // TODO: Find a better solution than shifting the data,
                //       perhapse have a var for the current item being replaced that loops around
                //data shift
                for (var i = dimensionSizeLimit - 1; i >= 1; i--)
                {
                    dd.Data[i] = dd.Data[i - 1];
                }

                if (!textualDimensionsListReverse[dimensionName].ContainsKey(val))
                {
                    int N = textualDimensionsList[dimensionName].Count;
                    textualDimensionsList[dimensionName].Add(N, val);
                    textualDimensionsListReverse[dimensionName].Add(val, N);
                }

                int idx = textualDimensionsListReverse[dimensionName][val];
                dd.Data[idx] = idx;

                // Debug.Log("-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-");
                // Debug.Log("idx: " + idx);
                // Debug.Log("dimensionName: " + dimensionName);

                return true;
            }
            catch (Exception e)
            {
                Debug.Log("SetData ERROR => " + e);
            }

            return false;
        }

        /// <summary>
        /// Gets the dimension data at the specified index.
        /// </summary>
        /// <param name="index">Index of dimension</param>
        public override DimensionData this[int index]
        {
            get { return dimensionData[index]; }
        }

        /// <summary>
        /// Gets the dimension data with the specified identifier.
        /// </summary>
        /// <param name="identifier">Identifier.</param>
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

        // Can always be loaded since we fill it at runtime
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
            return textualDimensionsList[this[identifier].Identifier].Count;
        }

        public override int getNumberOfCategories(string identifier)
        {
            return textualDimensionsList[this[identifier].Identifier].Count;
        }

        public override object getOriginalValue(float normalisedValue, string identifier)
        {
            DimensionData.Metadata meta = this[identifier].MetaData;
            float normValue = normaliseValue(normalisedValue, 0f, 1f, meta.minValue, meta.maxValue);

            if (meta.type == DataType.String)
            {
                var keys = textualDimensionsList[identifier].Select(x => x.Key);
                normValue = normaliseValue(normalisedValue, 0f, 1f, keys.Min(), keys.Max());
                string value = textualDimensionsList[identifier][(int)normValue];

                return value;
            }

            return normValue;
        }

        public override object getOriginalValue(float normalisedValue, int identifier)
        {
            DimensionData.Metadata meta = this[identifier].MetaData;
            float normValue = normaliseValue(normalisedValue, 0f, 1f, meta.minValue, meta.maxValue);

            if (meta.type == DataType.String)
            {
                var keys = textualDimensionsList[this[identifier].Identifier].Select(x => x.Key);
                normValue = normaliseValue(normalisedValue, 0f, 1f, keys.Min(), keys.Max());
                string value = textualDimensionsList[this[identifier].Identifier][(int)normValue];

                return value;
            }

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

        /// <summary>
        /// Awake this instance.
        /// </summary>
        void Awake()
        {
            DataSourceManager.register(this);

            if (!IsLoaded)
                load();

        }

        /// <summary>
        /// Raises the destroy event.
        /// </summary>
        void OnDestroy()
        {
            if (!isQuitting)
            {
                DataSourceManager.unregister(this);
            }
        }

        /// <summary>
        /// Raises the application quit event.
        /// </summary>
        void OnApplicationQuit()
        {
            isQuitting = true;
        }
    }
}
