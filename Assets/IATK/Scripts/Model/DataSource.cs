using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;
using System.Linq;
using System.Collections;

namespace IATK
{

    /// <summary>
    /// Data source base class. 
    /// Concrete classes will need to derive from this to provide an implementation.
    /// </summary>
    public abstract class DataSource : MonoBehaviour, InspectorChangeListener, IEnumerable<DataSource.DimensionData>
    {
        // DELEGATES

        public delegate void OnLoadEvent();

        // CLASSES

        /// <summary>
        /// Dimension data class
        /// </summary>
        public class DimensionData
        {
            /// <summary>
            /// Metadata.
            /// </summary>
            public struct Metadata 
            {
                public DataType type;
                public float minValue;
                public float maxValue;
                public int binCount;
                public float[] categories;
                public int categoryCount;
            }

            public string Identifier { get; private set; }          // Textual identifier for this dimension
            public int Index { get; private set; }                  // Integer indentifier for this dimension
            public Metadata MetaData { get; private set; }          // The MetaData for this dimension
            public float[] Data { get; private set; }               // The data array for this dimension
            public Dictionary<string, Dictionary<int, string>> StringTable { get; private set; }     // String lookup table for dimension data
            
            /// <summary>
            /// Initializes a new instance of the <see cref="IATK.DataSource+DimensionData"/> class.
            /// </summary>
            /// <param name="identifier">Identifier.</param>
            /// <param name="data">Data.</param>
            public DimensionData(string identifier, int index, Metadata metaData)
            {
                Identifier = identifier;
                Index = index;
                MetaData = metaData;
            }

            /// <summary>
            /// Sets the data.
            /// </summary>
            /// <param name="data">Data.</param>
            /// <param name="stringTable">String table.</param>
            public void setData(float[] data, Dictionary<string, Dictionary<int, string>> stringTable)
            {
                Data = data;
                StringTable = stringTable;
            }

            /// <summary>
            /// Sets the metadata.
            /// </summary>
            /// <param name="metadate">Metadate.</param>
            public void setMetadata(Metadata metadata)
            {
                MetaData = metadata;
            }
        }

        /// <summary>
        /// Event fired when data is loaded
        /// </summary>
        public event OnLoadEvent onLoad;

        /// <summary>
        /// Gets a value indicating whether the data is loaded.
        /// </summary>
        /// <value><c>true</c> if this instance is loaded; otherwise, <c>false</c>.</value>
        public abstract bool IsLoaded
        {
            get;
        }

        /// <summary>
        /// Gets the count of the dimensions to use on the indexer.
        /// </summary>
        /// <value>The count of dimensions</value>
        public abstract int DimensionCount
        {
            get;
        }

        /// <summary>
        /// Gets the data count.
        /// </summary>
        /// <value>The data count.</value>
        public abstract int DataCount
        {
            get;
        }           

        /// <summary>
        /// Gets the dimension data at the specified index.
        /// </summary>
        /// <param name="index">Index of dimension</param>
        public abstract DimensionData this[int index]
        {
            get;
        }                       
           
        /// <summary>
        /// Gets the dimension data with the specified identifier.
        /// </summary>
        /// <param name="identifier">Identifier.</param>
        public abstract DimensionData this[string identifier]
        {
            get;
        }

        /// <summary>
        /// Returns the original value of a data dimension normalised value
        /// </summary>
        /// <param name="normalisedValue"></param>
        /// <param name="identifier"></param>
        /// <returns></returns>
        public abstract object getOriginalValue(float normalisedValue, string identifier);

        /// <summary>
        /// Returns the original value of a data dimension normalised value
        /// </summary>
        /// <param name="normalisedValue"></param>
        /// <param name="identifier"></param>
        /// <returns></returns>
        public abstract object getOriginalValue(float normalisedValue, int identifier);
        
        /// <summary>
        /// gets the original from the exact normaliserdValue matching value
        /// </summary>
        /// <param name="normalisedValue"></param>
        /// <param name="identifier"></param>
        /// <returns></returns>
        public abstract object getOriginalValuePrecise(float normalisedValue, int identifier);

        /// <summary>
        /// gets the original from the exact normaliserdValue matching value
        /// </summary>
        /// <param name="normalisedValue"></param>
        /// <param name="identifier"></param>
        /// <returns></returns>
        public abstract object getOriginalValuePrecise(float normalisedValue, string identifier);


        /// <summary>
        /// Returns the number of categories for a data dimensions
        /// </summary>
        /// <param name="identifier"></param>
        /// <returns></returns>
        public abstract int getNumberOfCategories(int identifier);

        /// <summary>
        /// Returns the number of categories for a data dimensions
        /// </summary>
        /// <param name="identifier"></param>
        /// <returns></returns>
        public abstract int getNumberOfCategories(string identifier);

        /// <summary>
        /// Load the header information for the data
        /// Available here so can be called in editor
        /// Post: The Identifier and Metadata.type values will be available for each dimension
        /// </summary>
        public abstract void loadHeader();

        /// <summary>
        /// Load the data
        /// </summary>
        public abstract void load();

        /// <summary>
        /// On the inspector change.
        /// </summary>
        public void onInspectorChange()
        {
            //if (!IsLoaded)
            load();
        }

        // PROTECTED

        /// <summary>
        /// Is the onLoad event null?
        /// </summary>
        /// <returns><c>true</c>, if on load null was ised, <c>false</c> otherwise.</returns>
        protected bool isOnLoadNull()
        {
            return onLoad == null;
        }

        /// <summary>
        /// Raises the on load event.
        /// </summary>
        protected void raiseOnLoad()
        {
            onLoad();
        }

        public IEnumerator<DimensionData> GetEnumerator()
        {
            for (int i = 0; i < DimensionCount; i++)
            {
                yield return this[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            for (int i = 0; i < DimensionCount; i++)
            {
                yield return this[i];
            }
        }
    }

}   // Namespace