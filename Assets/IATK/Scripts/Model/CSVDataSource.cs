using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using System.Linq;

namespace IATK
{

    /// <summary>
    /// CSV file data source class 
    /// </summary>
    [ExecuteInEditMode]
    public class CSVDataSource : DataSource
    {
        // DATA

        [Tooltip("Text asset containing the data")]
        public TextAsset data;

        [Tooltip("The custom metadata")]
        public DataMetadata metadata;

        private List<DimensionData> dimensionData = new List<DimensionData>();

        private Dictionary<string, Dictionary<int, string>> textualDimensionsList = new Dictionary<string, Dictionary<int, string>>();
        private Dictionary<string, Dictionary<string, int>> textualDimensionsListReverse = new Dictionary<string, Dictionary<string, int>>();

        public Dictionary<string, Dictionary<string, int>> TextualDimensionsListReverse
        {
            get { return textualDimensionsListReverse; }
            set { textualDimensionsListReverse = value; }
        }

        public Dictionary<int, List<int>> GraphEdges = new Dictionary<int, List<int>>();

        private bool isQuitting;
        private int dataCount;
        // CSV, TSV,BSV implementation
        char[] split = new char[] { ',', '\t', ';'};

        // PUBLIC

        /// <summary>
        /// Gets a value indicating whether the data is loaded.
        /// </summary>
        /// <value><c>true</c> if this instance is loaded; otherwise, <c>false</c>.</value>
        public override bool IsLoaded
        {
            get { return DimensionCount > 0; }
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
            get { return dataCount; }
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
        /// Returns the orginal value from the data dimension range
        /// </summary>
        /// <param name="normalisedValue"></param>
        /// <param name="identifier"></param>
        /// <returns></returns>
        public override object getOriginalValue(float normalisedValue, int identifier)
        {
            DimensionData.Metadata meta = this[identifier].MetaData;

            float normValue = normaliseValue(normalisedValue, 0f, 1f, meta.minValue, meta.maxValue);

            if (meta.type == DataType.String)
            {
                normValue = normaliseValue(valueClosestTo(this[identifier].Data, normalisedValue), 0f, 1f, meta.minValue, meta.maxValue);
                return textualDimensionsList[this[identifier].Identifier][(int)normValue];  // textualDimensions[(int)normValue];
            }
            else return normValue;
        }

        /// <summary>
        /// Returns the orginal value from the data dimension range
        /// </summary>
        /// <param name="normalisedValue"></param>
        /// <param name="identifier"></param>
        /// <returns></returns>
        public override object getOriginalValuePrecise(float normalisedValue, int identifier)
        {
            DimensionData.Metadata meta = this[identifier].MetaData;

            float normValue = normaliseValue(normalisedValue, 0f, 1f, meta.minValue, meta.maxValue);

            if (meta.type == DataType.String)
            {
                return textualDimensionsList[this[identifier].Identifier][(int)normValue];
            }
            else return normValue;
        }

        /// <summary>
        /// Returns the orginal value from the data dimension range
        /// </summary>
        /// <param name="normalisedValue"></param>
        /// <param name="identifier"></param>
        /// <returns></returns>
        public override object getOriginalValuePrecise(float normalisedValue, string identifier)
        {
            DimensionData.Metadata meta = this[identifier].MetaData;

            float normValue = normaliseValue(normalisedValue, 0f, 1f, meta.minValue, meta.maxValue);

            if (meta.type == DataType.String)
            {
                return textualDimensionsList[this[identifier].Identifier][(int)normValue];
            }
            else return normValue;
        }

        /// <summary>
        /// Returns the orginal value from the data dimension range
        /// </summary>
        /// <param name="normalisedValue"></param>
        /// <param name="identifier"></param>
        /// <returns></returns>
        public override object getOriginalValue(float normalisedValue, string identifier)
        {
            DimensionData.Metadata meta = this[identifier].MetaData;

            float normValue = normaliseValue(normalisedValue, 0f, 1f, meta.minValue, meta.maxValue);

            if (meta.type == DataType.String)
            {
                normValue = normaliseValue(valueClosestTo(this[identifier].Data, normalisedValue), 0f, 1f, meta.minValue, meta.maxValue);
                return textualDimensionsList[identifier][(int)normValue]; // textualDimensions[(int)normValue];
            }
            else return normValue;
        }

        /// <summary>
        /// Load the header information for the data
        /// Post: The Identifier and Metadata.type values will be available for each dimension
        /// </summary>
        public override void loadHeader()
        {
            if (data != null)
            {
                loadHeaderImpl(data.text.Split('\n'));
            }
        }

        /// <summary>
        /// Load the data
        /// </summary>
        public override void load()
        {
            if (data != null)
            {
                load(data.text, metadata);
            }
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

        /// <summary>
        /// Loads the header impl.
        /// </summary>
        /// <returns><c>true</c>, if header was loaded, <c>false</c> otherwise.</returns>
        private bool loadHeaderImpl(string[] lines)
        {
            //1: read types
            if (lines.Length > 0)
            {
                string[] identifiers = lines[0].Split(split);

                // create metadata
                DimensionData.Metadata[] metadata = new DimensionData.Metadata[identifiers.Count()];

                //clean identifiers strings
                for (int i = 0; i < identifiers.Length; i++)
                {
                    string id = identifiers[i].Replace("\r", string.Empty);
                    identifiers[i] = id;
                }
                int nbDimensions = identifiers.Length;

                if (lines.Length > 1)
                {
                    string[] typesToRead = lines[1].Split(split);

                    //type reading
                    for (int i = 0; i < typesToRead.Length; i++)
                    {
                        if (i < metadata.Length)
                        {
                            metadata[i].type = DataTypeExtension.inferFromString(cleanDataString(typesToRead[i]));
                        }
                        else
                        {
                            Debug.Log("CSVDataSource: More data in a row than dimensions in header!");
                        }
                    }
                }

                // Populate data structure
                for (int i = 0; i < nbDimensions; ++i)
                {
                    dimensionData.Add(new DimensionData(identifiers[i], i, metadata[i]));
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        public void load(string data, DataMetadata metadataPreset)
        {
            dimensionData = new List<DimensionData>();
            textualDimensionsList = new Dictionary<string, Dictionary<int, string>>();
            textualDimensionsListReverse = new Dictionary<string, Dictionary<string, int>>();


            string[] lines = data.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            if (loadHeaderImpl(lines))
            {
                float[,] dataArray = new float[lines.Length - 1, DimensionCount]; // ignore the first line of identifiers
                dataCount = dataArray.GetUpperBound(0) + 1;

                if (lines.Length > 1)
                {
                    //line reading
                    for (int i = 1; i < lines.Length; i++)
                    {
                        string[] values = lines[i].Split(split);

                        //dimension reading
                        for (int k = 0; k < values.Count(); k++)
                        {

                            string cleanedValue = cleanDataString(values[k]);

                            //1- get the corresponding type
                            if (k <= dimensionData.Count - 1) switch (dimensionData[k].MetaData.type)
                                {
                                    case DataType.Bool:
                                        {
                                            bool result = false;
                                            bool.TryParse(cleanedValue, out result);
                                            dataArray[i - 1, k] = Convert.ToSingle(result);
                                            break;
                                        }
                                    case DataType.Date:
                                        {
                                            string[] valH = cleanedValue.Split('\\');
                                            if (valH.Length == 2)
                                                dataArray[i - 1, k] = float.Parse(valH[0]) * 60f + float.Parse(valH[1]);
                                            else if (valH.Length == 3)
                                                dataArray[i - 1, k] = float.Parse(valH[0]) * 3600f + float.Parse(valH[1]) * 60f + float.Parse(valH[2]);
                                            else dataArray[i - 1, k] = 0f;
                                            break;
                                        }

                                    case DataType.Time:
                                        {
                                            string[] valH = cleanedValue.Split(':');
                                            if (valH.Length == 2)
                                                dataArray[i - 1, k] = float.Parse(valH[0]) * 60f + float.Parse(valH[1]);
                                            else if (valH.Length == 3)
                                                dataArray[i - 1, k] = float.Parse(valH[0]) * 3600f + float.Parse(valH[1]) * 60f + float.Parse(valH[2]);
                                            else dataArray[i - 1, k] = 0f;
                                            break;
                                        }

                                    case DataType.Int:
                                        {
                                            int result = 0;
                                            int.TryParse(cleanedValue, out result);
                                            dataArray[i - 1, k] = (float)result;
                                            break;
                                        }
                                    case DataType.Float:
                                        {
                                            double result = 0.0f;
                                            double.TryParse(cleanedValue, out result);
                                            dataArray[i - 1, k] = (float)result;
                                            break;
                                        }
                                    case DataType.Graph:
                                        {
                                            char[] graphSeparator = new char[] { '|' };
                                            string[] edges = cleanedValue.Split(graphSeparator);

                                            List<int> localEdges = new List<int>();

                                            //read edges
                                            for (int ed=0;ed<edges.Length;ed++)
                                            {
                                                if(edges[ed]!="")
                                                localEdges.Add(int.Parse(edges[ed]));
                                            }
                                            GraphEdges.Add(i, localEdges);

                                            break;
                                        }
                                    case DataType.String:
                                        {
                                            //check if we have a dictionnary for this dimension
                                            if (textualDimensionsList.ContainsKey(dimensionData[k].Identifier))
                                            {
                                                //if encoded
                                                //get the dictionary
                                                int valueToEncode;
                                                Dictionary<string, int> dimensionDictionaryReverse = textualDimensionsListReverse[dimensionData[k].Identifier];
                                                Dictionary<int, string> dimensionDictionary = textualDimensionsList[dimensionData[k].Identifier];

                                                if (dimensionDictionaryReverse.ContainsKey(cleanedValue))
                                                {
                                                    valueToEncode = dimensionDictionaryReverse[cleanedValue];
                                                    dataArray[i - 1, k] = valueToEncode;
                                                }
                                                else
                                                {
                                                    //increment from the last added element
                                                    int lastEncodedValue = dimensionDictionaryReverse.Values.OrderBy(x => x).Last() + 1;

                                                    dimensionDictionaryReverse.Add(cleanedValue, lastEncodedValue);
                                                    dimensionDictionary.Add(lastEncodedValue, cleanedValue);
                                                    textualDimensionsListReverse[dimensionData[k].Identifier] = dimensionDictionaryReverse;
                                                    textualDimensionsList[dimensionData[k].Identifier] = dimensionDictionary;

                                                    dataArray[i - 1, k] = lastEncodedValue;
                                                }
                                            }
                                            else //if not create one and add the first value
                                            {
                                                Dictionary<int, string> newEntry = new Dictionary<int, string>();
                                                Dictionary<string, int> newEntryReverse = new Dictionary<string, int>();

                                                newEntry.Add(0, cleanedValue);
                                                newEntryReverse.Add(cleanedValue, 0);

                                                textualDimensionsList.Add(dimensionData[k].Identifier, newEntry);
                                                textualDimensionsListReverse.Add(dimensionData[k].Identifier, newEntryReverse);
                                            }
                                            ////lookup if already encoded
                                            //if (textualDimensionsReverse.ContainsKey(cleanedValue))
                                            //{
                                            //    dataArray[i - 1, k] = textualDimensionsReverse[cleanedValue];// textualDimensions.FirstOrDefault(x => x.Value == cleanedValue).Key;
                                            //}
                                            //else
                                            //{
                                            //    //new key
                                            //    textualPointer++;
                                            //    textualDimensions.Add((int)textualPointer, cleanedValue);
                                            //    textualDimensionsReverse.Add(cleanedValue, (int)textualPointer);
                                            //    dataArray[i - 1, k] = textualPointer;
                                            //}
                                            break;
                                        }
                                    default:
                                        {
                                            dataArray[i - 1, k] = 0f;
                                            break;
                                        }
                                }// end switch

                        } // end k
                    }
                }

                // TODO: SORT MULTIPLE VALUES/CRITERIA

                // Populate data structure
                //float[] output = new float[dataCount];
                for (int i = 0; i < DimensionCount; ++i)
                {
                    dimensionData[i].setData(NormaliseCol(dataArray, metadataPreset, i), textualDimensionsList);

                }

                // Raise load event
                if (!isOnLoadNull())
                {
                    raiseOnLoad();
                }
            }
        }

        private string cleanDataString(string rawData)
        {
            return rawData.Replace("\r", string.Empty);
        }

        private float[] NormaliseCol(float[,] dataArray, DataMetadata metadataPreset, int col)
        {
            //for each dimensions (column) normalise all data
            float[] result = GetCol(dataArray, col);
            float minDimension = result.Min();
            float maxDimension = result.Max();

            if (minDimension == maxDimension)
            {
                // where there are no distinct values, need the dimension to be distinct 
                // otherwise lots of maths breaks with division by zero, etc.
                // this is the most elegant hack I could think of, but should be fixed properly in future
                minDimension -= 1.0f; 
                maxDimension += 1.0f;
            }

            DataSource.DimensionData.Metadata metadata = dimensionData[col].MetaData;

            metadata.minValue = minDimension;
            metadata.maxValue = maxDimension;
            metadata.categories = result.Distinct().Select(x => normaliseValue(x, minDimension, maxDimension, 0.0f, 1.0f)).ToArray();
            metadata.categoryCount = metadata.categories.Count();
            metadata.binCount = (int)(maxDimension - minDimension + 1);

            if (metadataPreset != null)
            {
                foreach (var binSizePreset in metadataPreset.BinSizePreset)
                {
                    if (binSizePreset.index == col)
                    {
                        metadata.binCount = binSizePreset.binCount;
                    }
                }
            }

            dimensionData[col].setMetadata(metadata);

            for (int j = 0; j < result.Length; j++)
            {
                if (minDimension < maxDimension)
                {
                    result[j] = normaliseValue(result[j], minDimension, maxDimension, 0f, 1f);
                }
                else
                {
                    // avoid NaNs or nonsensical normalization
                    result[j] = 0;
                }
            }

            return result;
        }

        /// <summary>
        /// internal function: normalises all the data input between 0 and 1
        /// </summary>
        private float[,] normaliseArray(float[,] dataArray, DataMetadata metadataPreset)
        {
            //1 make a copy of the parsed array
            float[,] normArray = new float[dataArray.GetUpperBound(0) + 1, dataArray.GetUpperBound(1) + 1];
            //for each dimensions (column) normalise all data
            for (int i = 0; i <= normArray.GetUpperBound(1); i++)
            {
                float[] rawDimension = GetCol(dataArray, i);
                float minDimension = rawDimension.Min();
                float maxDimension = rawDimension.Max();

                DataSource.DimensionData.Metadata metadata = dimensionData[i].MetaData;

                metadata.minValue = minDimension;
                metadata.maxValue = maxDimension;
                metadata.binCount = (int)(maxDimension - minDimension + 1);

                if (metadataPreset != null)
                {
                    foreach (var binSizePreset in metadataPreset.BinSizePreset)
                    {
                        if (binSizePreset.index == i)
                        {
                            metadata.binCount = binSizePreset.binCount;
                        }
                    }
                }

                dimensionData[i].setMetadata(metadata);

                float[] normalisedDimension = new float[rawDimension.Length];

                //                dimensionsRange.Add(i, new Vector2(minDimension, maxDimension));

                for (int j = 0; j < rawDimension.Length; j++)
                {
                    if (minDimension < maxDimension)
                    {
                        normalisedDimension[j] = normaliseValue(rawDimension[j], minDimension, maxDimension, 0f, 1f);
                    }
                    else
                    {
                        // avoid NaNs or nonsensical normalization
                        normalisedDimension[j] = 0;
                    }
                }

                SetCol<float>(normArray, i, normalisedDimension);
            }

            return normArray;
        }

        /// <summary>
        /// debug function that prints the 2D array
        /// </summary>
        /// <param name="data"></param>
        public void Debug2DArray(object[,] data)
        {
            for (int i = 0; i < data.GetUpperBound(0); i++)
            {
                string line = "";
                for (int j = 0; j < data.GetUpperBound(1); j++)
                {
                    line += (data[i, j]) + " ";
                }
                Debug.Log(line);
            }
        }

        /// <summary>
        /// debugs one column
        /// </summary>
        /// <param name="col"></param>
        //        public void DebugArray(int col)
        //        {
        //            float[] selection = getDimension(identifiers[col]);
        //
        //            for (int i = 0; i < selection.Length; i++)
        //                Debug.Log(selection[i]);
        //        }

        /// <summary>
        /// returns one row of 2D array
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="matrix"></param>
        /// <param name="row"></param>
        /// <returns></returns>
        public T[] GetRow<T>(T[,] matrix, int row)
        {
            var rowLength = matrix.GetLength(1);
            var rowVector = new T[rowLength];

            for (var i = 0; i < rowLength; i++)
                rowVector[i] = matrix[row, i];

            return rowVector;
        }

        /// <summary>
        /// returns one column of the 2D array
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="col"></param>
        /// <returns></returns>
        public float[] GetCol(float[,] matrix, int col)
        {
            var colLength = matrix.GetLength(0);
            var colVector = new float[colLength];

            for (var i = 0; i < colLength; i++)
            {
                colVector[i] = matrix[i, col];
            }
            return colVector;
        }

        /// <summary>
        /// sets a vector of values into a specific column
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="matrix"></param>
        /// <param name="col"></param>
        /// <param name="colVector"></param>
        public void SetCol<T>(T[,] matrix, int col, T[] colVector)
        {
            var colLength = matrix.GetLength(0);
            for (var i = 0; i < colLength; i++)
                matrix[i, col] = colVector[i];
        }


        //        public int dimensionToIndex(string dimension)
        //        {
        //            int id = -1;
        //            for (int i = 0; i < identifiers.Length; i++)
        //            {
        //
        //                if (dimension == identifiers[i])
        //                {
        //                    id = i;
        //                }
        //            }
        //            return id;
        //        }
        //
        //        public string indexToDimension(int dimensionIndex)
        //        {
        //            return identifiers.ElementAt(dimensionIndex);
        //        }

        float stringToFloat(string value)
        {
            return BitConverter.ToSingle(Encoding.UTF8.GetBytes(value), 0);
        }
        string floatToString(float value)
        {
            return BitConverter.ToString(BitConverter.GetBytes(value));
        }

        float normaliseValue(float value, float i0, float i1, float j0, float j1)
        {
            float L = (j0 - j1) / (i0 - i1);
            return (j0 - (L * i0) + (L * value));
        }

        /// <summary>
        /// returns the closest value item to target from a collection
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="target"></param>
        /// <returns></returns>
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

        public override int getNumberOfCategories(int identifier)
        {
            return this[identifier].MetaData.categoryCount;
        }

        public override int getNumberOfCategories(string identifier)
        {
            return this[identifier].MetaData.categoryCount;
        }

        //public int getNumberOfCategories(float[] column)
        //{
        //    List<float> values = new List<float>();
        //    for (int i = 0; i < column.Length; i++)
        //    {
        //        if (!values.Contains(column[i]))
        //        {
        //            values.Add(column[i]);
        //           // Debug.Log(normaliseValue(column[i], 0f, 1f, dimensionsRange[7].x, dimensionsRange[7].y));
        //        }
        //        //if (column[i] != column[i + 1])
        //        //{ Debug.Log(column[i] + "       " + column[i + 1]); categories++; }
        //    }
        //    return values.Count;
        //}
    }

}   // Namespace