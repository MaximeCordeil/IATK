//RealTimeDataSource based on cycle and reuse
//Philipp Fleck, ICG @ TuGraz, philipp.fleck@icg.tugraz.at
//20210430, initial working version

using System;
using System.Collections.Generic;
using UnityEngine;

namespace IATK
{
    public class RealtimeDataSource : DataSource
    {
        /* 
        The max amount of data that can be displayed at a single time,
        will loop back around (replacing older data) when it goes over this limit
        */ 
        private int dimensionSizeLimit = 100;
        private List<int> dimensionPointers = new List<int>();

        private bool isQuitting;

        private List<DimensionData> dimensionData = new List<DimensionData>();
        
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
        /// <param name="numberOfCategories">The data type of categories (unique values) in the data</param>
        /// <param name="type">The data type of the dimension</param>
        /// <returns>True if successfully added, false otherwise</returns>
        public bool AddDimension(string dimensionName, float numberOfCategories, DataType type = DataType.String)
        {
            return AddDimension(dimensionName, 0, numberOfCategories - 1f, type);
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
            dimensionPointers.Add(0);

            //Debug.Log("RTDS AddDimension => " + dd.Identifier + ", " + dd.Index);
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
            return SetData(this[index].Identifier, val);
        }


        /// <summary>
        /// This is important otherwise the overloading can NOT be resolved from outside (eg JS)
        /// </summary>
        /// <param name="dimensionName"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public bool SetDataStrVal(string dimensionName, float val)
        {
            //Debug.Log("RTDS SetDataStrVal => " + dimensionName + ", " + val);
            return SetData(dimensionName, val);
        }

        /// <summary>
        /// This is important otherwise the overloading can NOT be resolved from outside (eg JS)
        /// </summary>
        /// <param name="dimensionName"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public bool SetDataStrStr(string dimensionName, string val)
        {
            return SetData(dimensionName, val);
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

                //this is needed since we do not know the data
                //auto scale start
                bool dirty = false;
                var minV = dd.MetaData.minValue;
                var maxV = dd.MetaData.minValue;
                if (dd.MetaData.minValue > val)
                {
                    minV = (float)Math.Floor(val);
                    dirty = true;
                }

                if (dd.MetaData.maxValue < val)
                {
                    maxV = (float)Math.Ceiling(val);
                    dirty = true;
                }

                if (dirty)
                {
                    //Debug.Log("SetData updating min max => " + minV + ", " + maxV);
                    var metaData = new DimensionData.Metadata();
                    metaData.minValue = minV;
                    metaData.maxValue = maxV;
                    metaData.type = DataType.Float; //maybe make that adjustable
                    dd.setMetadata(metaData);
                }
                //autoscale stop

                //this is going to cut off values and not doind auto normalization for unknown data
                if (dd != null && dd.MetaData.minValue <= val && dd.MetaData.maxValue >= val && dd.Data.Length > 0)
                {
                    SetDimensionData(dd, normaliseValue(val, dd.MetaData.minValue, dd.MetaData.maxValue, 0f, 1f));
                    return true;
                }
            }
            catch (Exception e)
            {
                Debug.Log("SetData ERROR => " + e);
            }

            return false;
        }

        /// <summary>
        /// Sets a data value by index
        /// </summary>
        /// <param name="index">Index of dimension</param>
        /// <param name="val">Value to set the dimension</param>
        /// <returns>True if successfully set, false otherwise</returns>
        public bool SetData(int index, string val)
        {
            return SetData(this[index].Identifier, val);
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
                if (!textualDimensionsListReverse[dimensionName].ContainsKey(val))
                {
                    int numberOfDimensions = textualDimensionsList[dimensionName].Count;
                    textualDimensionsList[dimensionName].Add(numberOfDimensions, val);
                    textualDimensionsListReverse[dimensionName].Add(val, numberOfDimensions);
                }

                float idx = (float)textualDimensionsListReverse[dimensionName][val];
                return SetData(dimensionName, idx);
            }
            catch (Exception e)
            {
                Debug.Log("SetData ERROR => " + e);
            }

            return false;
        }

        /// <summary>
        /// Sets a data value to a dimension
        /// </summary>
        /// <param name="dd">The data dimension to put the data in</param>
        /// <param name="val">The data that is to be set</param>
        private void SetDimensionData(DimensionData dd, float val)
        {
            // Each dimension has its own pointer that loops around between 0 and dimensionSizeLimit
            int ptr = dimensionPointers[dd.Index];
            dd.Data[ptr] = val;
            ptr++;
            if(ptr >= dimensionSizeLimit) ptr = 0;
            dimensionPointers[dd.Index] = ptr;
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

        /// <summary>
        /// Will always return true since the data is filled at runtime.
        /// </summary>
        /// <value></value>
        public override bool IsLoaded
        {
            get { return true; }
        }

        /// <summary>
        /// Gets the count of the dimensions to use on the indexer.
        /// </summary>
        /// <value>The count of dimensions</value>
        public override int DimensionCount
        {
            get { return dimensionData.Count; }
        }

        /// <summary>
        /// Gets the data count.
        /// </summary>
        /// <value>The data count.</value>
        public override int DataCount
        {
            get { return dimensionSizeLimit; }
        }

        public override int getNumberOfCategories(int identifier)
        {
            return textualDimensionsList[this[identifier].Identifier].Count;
        }

        public override int getNumberOfCategories(string identifier)
        {
            return textualDimensionsList[this[identifier].Identifier].Count;
        }

        /// <summary>
        /// Returns the orginal value from the data dimension range. Used to dispaly axis labels.
        /// </summary>
        /// <param name="normalisedValue"></param>
        /// <param name="identifier"></param>
        /// <returns>An object depending on the datatype of the value (e.g. Float, String...)</returns>
        public override object getOriginalValue(float normalisedValue, string identifier)
        {
            DimensionData.Metadata meta = this[identifier].MetaData;
            float normValue = normaliseValue(normalisedValue, 0f, 1f, meta.minValue, meta.maxValue);

            if (meta.type == DataType.String)
            {
                return textualDimensionsList[identifier][(int)normValue];
            }

            return normValue;
        }

        /// <summary>
        /// Returns the orginal value from the data dimension range. Used to display "Dimension Filter" values in the Unity editor.
        /// </summary>
        /// <param name="normalisedValue"></param>
        /// <param name="identifier"></param>
        /// <returns>An object depending on the datatype of the value (e.g. Float, String...)</returns>
        public override object getOriginalValue(float normalisedValue, int identifier)
        {
            DimensionData.Metadata meta = this[identifier].MetaData;
            float normValue = normaliseValue(normalisedValue, 0f, 1f, meta.minValue, meta.maxValue);

            if (meta.type == DataType.String)
            {
                return textualDimensionsList[this[identifier].Identifier][(int)normValue];
            }

            return normValue;
        }

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

        /// <summary>
        /// Does nothing. Do not need to load data at launch as the data will be loaded in at runtime.
        /// </summary>
        public override void load() { }

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
