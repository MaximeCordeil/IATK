using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using System;

namespace IATK {
    [System.Serializable]
    public class SerializableCreationConfiguration
    {
        public string UID;

        public string VisualisationType;       // The type of the visualisation 
        //public DimensionFilter[] parallelCoordinatesDimensions;         // The Parallel Cooridnates dimensions
        //this hacky, Unity's Json does not support serialization 
        // of derived classes
        public string[] parallelCoordinatesDimensionsAttributes;
        public float[] parallelCoordinatesDimensionsMinFilter;
        public float[] parallelCoordinatesDimensionsMaxFilter;
        public float[] parallelCoordinatesDimensionsMinScale;
        public float[] parallelCoordinatesDimensionsMaxScale;

        public AbstractVisualisation.GeometryType Geometry;             // The type of geometry to create
        public CreationConfiguration.Axis[] AxiesKeys;                  // The desired axies
        public string[] AxiesValues;                                    // The desired axies

        public string ColourDimension;                                  // Colour mapped to a dimension
        public string SizeDimension;                                    // Size mapped to a dimension
        public string LinkingDimension;                                 // The linking dimension to draw links between points
        public Gradient colourKeys;                                     // The gradient colormapping
        public Color colour;                                            // The colour mapping
        public float Size;                                              // The Size factor
        public float MinSize;                                           // The Minimum Size range
        public float MaxSize;                                           // The Maximum Size range
        
        public float VisualisationWidth;                                // The width of the visualisation
        public float VisualisationHeight;                               // The height of the visualisation
        public float VisualisationDepth;                                // The depth of the visualisation

        public SerializableCreationConfiguration()
        {

        }

        public void Serialize(string serializedObjectPath, CreationConfiguration cf)
        {
            VisualisationType = cf.VisualisationType.ToString();

            //hacky, needed to serialize DimensionFilters objects
            if (cf.parallelCoordinatesDimensions != null)
            {
                parallelCoordinatesDimensionsAttributes = new string[cf.parallelCoordinatesDimensions.Length];
                parallelCoordinatesDimensionsMinFilter = new float[cf.parallelCoordinatesDimensions.Length];
                parallelCoordinatesDimensionsMaxFilter = new float[cf.parallelCoordinatesDimensions.Length];
                parallelCoordinatesDimensionsMinScale = new float[cf.parallelCoordinatesDimensions.Length];
                parallelCoordinatesDimensionsMaxScale = new float[cf.parallelCoordinatesDimensions.Length];

                for (int i = 0; i < parallelCoordinatesDimensionsAttributes.Length; i++)
                {
                    parallelCoordinatesDimensionsAttributes[i] = cf.parallelCoordinatesDimensions[i].Attribute;
                    parallelCoordinatesDimensionsMinFilter[i] = cf.parallelCoordinatesDimensions[i].minFilter;
                    parallelCoordinatesDimensionsMaxFilter[i] = cf.parallelCoordinatesDimensions[i].maxFilter;
                    parallelCoordinatesDimensionsMinScale[i] = cf.parallelCoordinatesDimensions[i].minScale;
                    parallelCoordinatesDimensionsMaxScale[i] = cf.parallelCoordinatesDimensions[i].maxScale;
                }
            }
            
            Geometry = cf.Geometry;
            AxiesKeys = cf.Axies.Keys.ToArray();
            AxiesValues = cf.Axies.Values.ToArray();

            ColourDimension = cf.ColourDimension;
            SizeDimension = cf.SizeDimension;
            LinkingDimension = cf.LinkingDimension;

            colourKeys = cf.colourKeys;
            colour = cf.colour;

            Size = cf.Size;
            MinSize = cf.MinSize;
            MaxSize = cf.MaxSize;

            VisualisationWidth = cf.VisualisationWidth;
            VisualisationHeight = cf.VisualisationHeight;
            VisualisationDepth = cf.VisualisationDepth;
            
            File.WriteAllText(serializedObjectPath, JsonUtility.ToJson(this));
        }

        public void DeSerialize(string serializedObjectPath, CreationConfiguration cf)
        {
            SerializableCreationConfiguration scc = JsonUtility.FromJson<SerializableCreationConfiguration>(File.ReadAllText(serializedObjectPath));

            cf.VisualisationType = (AbstractVisualisation.VisualisationTypes)System.Enum.Parse(typeof(AbstractVisualisation.VisualisationTypes), scc.VisualisationType);

            //rebuild the parallel coordinates dimensions filtering
            string[] attributesPCP = scc.parallelCoordinatesDimensionsAttributes;
            float[] minFiltersPCP = scc.parallelCoordinatesDimensionsMinFilter;
            float[] maxFiltersPCP = scc.parallelCoordinatesDimensionsMaxFilter;
            float[] minScalesPCP = scc.parallelCoordinatesDimensionsMinScale;
            float[] maxScalesPCP = scc.parallelCoordinatesDimensionsMaxScale;

            DimensionFilter[] parallelCoordinatesDimensions = new DimensionFilter[attributesPCP.Length];

            for (int i = 0; i < parallelCoordinatesDimensions.Length; i++)
            {
                DimensionFilter df = new DimensionFilter();
                df.Attribute = attributesPCP[i];
                df.minFilter = minFiltersPCP[i];
                df.maxFilter = maxFiltersPCP[i];
                df.minScale = minScalesPCP[i];
                df.maxScale = maxScalesPCP[i];

                parallelCoordinatesDimensions[i] = df;
            }

            cf.parallelCoordinatesDimensions = parallelCoordinatesDimensions;

            cf.Geometry = scc.Geometry;
            cf.Axies = new Dictionary<CreationConfiguration.Axis, string>();
            for (int i = 0; i < scc.AxiesKeys.Length; i++)
            {
                if (!cf.Axies.ContainsKey(scc.AxiesKeys[i]))
                    cf.Axies.Add(scc.AxiesKeys[i], scc.AxiesValues[i]);
            }
            cf.ColourDimension = scc.ColourDimension;
            cf.SizeDimension = scc.SizeDimension;
            cf.LinkingDimension = scc.LinkingDimension;
            cf.colourKeys = scc.colourKeys;
            cf.colour = scc.colour;
            cf.Size = scc.Size;
            cf.MinSize = scc.MinSize;
            cf.MaxSize = scc.MaxSize;

            cf.VisualisationWidth = scc.VisualisationWidth;
            cf.VisualisationHeight = scc.VisualisationHeight;
            cf.VisualisationDepth = scc.VisualisationDepth;
        }
    }

    public class CreationConfiguration
    {
        public enum Axis
        {
            X,
            Y,
            Z
        }

        public AbstractVisualisation.VisualisationTypes VisualisationType { get; set; }       // The type of the visualisation

        public DimensionFilter[] parallelCoordinatesDimensions;         // The Parallel Cooridnates dimensions
        public AbstractVisualisation.GeometryType Geometry { get; set; }        // The type of geometry to create
        public Dictionary<Axis, string> Axies { get; set; }             // The desired axies
        public string ColourDimension { get; set; }                     // Colour mapped to a dimension
        public string SizeDimension { get; set; }                       // Size mapped to a dimension
        public string LinkingDimension { get; set; }                    // The linking dimension to draw links between points
        public Gradient colourKeys { get; set; }                        // The colormapping
        public Color colour = Color.white;                              // The colour mapping
        public float Size = 0.3f;                                       // The Size factor
        public float MinSize = 0.01f;                                   // The Minimum Size range
        public float MaxSize = 1.0f;                                    // The Maximum Size range
        public float VisualisationWidth = 1f;                           // The width of the visualisation
        public float VisualisationHeight = 1f;                          // The height of the visualisation
        public float VisualisationDepth = 1f;                           // The depth of the visualisation  

        //avoid erasing properties
        public bool disableWriting = false;

        /// <summary>
        /// Creates an empty configuration
        /// </summary>
        public CreationConfiguration()
        { }

        /// <summary>
        /// Serializes the configuration
        /// </summary>
        /// <param name="pathObjectToSerialize"></param>
        public void Serialize(string pathObjectToSerialize)
        {
            if (!disableWriting)
            {
                SerializableCreationConfiguration scc = new SerializableCreationConfiguration();
                scc.Serialize(pathObjectToSerialize, this);
            }
        }

        /// <summary>
        /// Deserialize the configuration
        /// </summary>
        /// <param name="pathObjectToSerialize"></param>
        public void Deserialize(string pathObjectToSerialize)
        {

            SerializableCreationConfiguration scc = new SerializableCreationConfiguration();

            scc.DeSerialize(pathObjectToSerialize, this);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IATK.VisualisationCreator+CreationConfiguration"/> class.
        /// </summary>
        /// <param name="geometry">Geometry.</param>
        /// <param name="axies">Axies.</param>
        public CreationConfiguration(AbstractVisualisation.GeometryType geometry, Dictionary<Axis, string> axies)
        {
            Geometry = geometry;
            Axies = axies;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IATK.VisualisationCreator+CreationConfiguration"/> class.
        /// </summary>
        /// <param name="geometry"></param>
        /// <param name="axies"></param>
        /// <param name="colourDimension"></param>
        /// <param name="sizeDimension"></param>
        public CreationConfiguration(AbstractVisualisation.GeometryType geometry, Dictionary<Axis, string> axies, string colourDimension, string sizeDimension)
            : this(geometry, axies)
        {
            ColourDimension = colourDimension;
            SizeDimension = SizeDimension;
        }
    }
}