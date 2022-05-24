using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using System;

namespace IATK
{
    public abstract class AbstractVisualisation : MonoBehaviour
    {
        // ENUMS

        public enum GeometryType
        {
            Undefined,
            Points,
            Lines,
            Quads,
            LinesAndDots,
            Cubes,
            Bars,
            Spheres
        }

        public enum PropertyType
        {
            None,
            X,
            Y,
            Z,
            Colour,
            Size,
            GeometryType,
            LinkingDimension,
            OriginDimension,
            DestinationDimension,
            GraphDimension,
            DimensionFiltering,
            Scaling,
            BlendSourceMode,
            BlendDestinationMode,
            AttributeFiltering,
            DimensionChange,
            VisualisationType,
            SizeValues,
            DimensionChangeFiltering,
            VisualisationWidth,
            VisualisationHeight,
            VisualisationLength
        }

        public enum NormalChannel
        {
            VertexId,
            Size,
            Filter
        }

        public enum VisualisationTypes {
            SCATTERPLOT, 
            SCATTERPLOT_MATRIX, 
            PARALLEL_COORDINATES, 
            GRAPH_LAYOUT};

        // PUBLIC HIDDEN

        [SerializeField]
        [HideInInspector]
        public Visualisation visualisationReference;

        [HideInInspector]
        // The list of views in the visualisation
        public List<View> viewList = new List<View>();

        //List of GameObjects holding references to Axis elements
        [HideInInspector]
        public List<GameObject> GameObject_Axes_Holders = new List<GameObject>();

        [HideInInspector]
        public GameObject X_AXIS, Y_AXIS, Z_AXIS;

        [HideInInspector]
        public CreationConfiguration creationConfiguration;

        [HideInInspector]
        public string serializedObjectPath = "SerializedFields"; 

        // ******************************************************
        // ABSTRACT METHODS THAT VISUALISATIONS HAVE TO IMPLEMENT
        // ******************************************************

        public abstract void CreateVisualisation();

        public abstract void UpdateVisualisation(PropertyType propertyType);

        public abstract void SaveConfiguration();

        public abstract void LoadConfiguration();

        public abstract Color[] mapColoursContinuous(float[] data);

        // ******************************************************
        // COMMON METHODS THAT VISUALISATIONS USE
        // ******************************************************

        /// <summary>
        /// Creates an axis Gameobject with metadata
        /// </summary>
        /// <param name="propertyType"></param>
        /// <param name="dimensionFilter"></param>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        protected GameObject CreateAxis(AbstractVisualisation.PropertyType propertyType, DimensionFilter dimensionFilter, Vector3 position, Vector3 rotation, int index)
        {
            GameObject AxisHolder;
            
            AxisHolder = (GameObject)Instantiate(Resources.Load("Axis"));

            AxisHolder.transform.parent = transform;
            AxisHolder.name = propertyType.ToString();
            AxisHolder.transform.eulerAngles= (rotation);
            AxisHolder.transform.localPosition = position;

            Axis axis = AxisHolder.GetComponent<Axis>();
            axis.Initialise(visualisationReference.dataSource, dimensionFilter, visualisationReference);
            axis.SetDirection((int)propertyType);
            BindMinMaxAxisValues(axis, dimensionFilter);


            return AxisHolder;
        }

        /// <summary>
        /// Binds metadata to an axis component
        /// </summary>
        /// <param name="axis"></param>
        /// <param name="dim"></param>
        protected void BindMinMaxAxisValues(Axis axis, DimensionFilter dim)
        {
            object minvalue = visualisationReference.dataSource.getOriginalValue(dim.minFilter, dim.Attribute);
            object maxvalue = visualisationReference.dataSource.getOriginalValue(dim.maxFilter, dim.Attribute);

            object minScaledvalue = visualisationReference.dataSource.getOriginalValue(dim.minScale, dim.Attribute);
            object maxScaledvalue = visualisationReference.dataSource.getOriginalValue(dim.maxScale, dim.Attribute);

            axis.AttributeFilter = dim;
            
            axis.UpdateAxisAttribute(dim.Attribute);
            axis.SetMinNormalizer(dim.minScale);
            axis.SetMaxNormalizer(dim.maxScale);
        }

        /// <summary>
        /// Serialize view configuraiton to disk
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public void SerializeViewConfiguration(CreationConfiguration creationConfiguration)
        {
            string path = ConfigurationFileName();
            if (!Directory.Exists(Path.GetDirectoryName(path)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
            }
            creationConfiguration.Serialize(ConfigurationFileName());
        }

        private string ConfigurationFileName()
        {
            string PathName = Application.streamingAssetsPath + Path.DirectorySeparatorChar + serializedObjectPath;
            return PathName + Path.DirectorySeparatorChar + visualisationReference.uid + ".json";
        }

        /// <summary>
        /// Destroy immediately all the views
        /// </summary>
        public void destroyView()
        {
            string backupname = name;
            List<GameObject> children = new List<GameObject>();

            for (int i = 0; i < transform.childCount; i++)
            {
                if (transform.GetChild(i).GetComponent<View>() != null)
                    children.Add(transform.GetChild(i).gameObject);
            }

            foreach (var item in children)
            {
                DestroyImmediate(item);
            }
            name = backupname;
        }

        /// <summary>
        /// returns a View with the specific geometry configuration
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="builder"></param>
        /// <returns></returns>
        protected View ApplyGeometryAndRendering(CreationConfiguration configuration, ref ViewBuilder builder)
        {
            Material mt = null;

            switch (configuration.Geometry)
            {
                case AbstractVisualisation.GeometryType.Undefined:
                    return null;

                case AbstractVisualisation.GeometryType.Points:
                    builder.createIndicesPointTopology();
                    mt = new Material(Shader.Find("IATK/OutlineDots"));
                    mt.mainTexture = Resources.Load("circle-outline-basic") as Texture2D;
                    mt.renderQueue = 3000;
                    mt.enableInstancing = true;
                    return builder.updateView().
                       apply(gameObject, mt);

                case AbstractVisualisation.GeometryType.Lines:
                    if (visualisationReference.graphDimension != "Undefined")
                    {
                        CSVDataSource csvds = (CSVDataSource)(visualisationReference.dataSource);
                        builder.createIndicesGraphTopology(csvds.GraphEdges);
                        mt = new Material(Shader.Find("IATK/LinesShader"));
                        mt.renderQueue = 3000;
                        mt.enableInstancing = true;
                        return builder.updateView().
                        apply(gameObject, mt);
                    }
                    else
                    if (visualisationReference.linkingDimension != "Undefined")
                    {
                        builder.createIndicesConnectedLineTopology(visualisationReference.dataSource[visualisationReference.linkingDimension].Data);
                        mt = new Material(Shader.Find("IATK/LinesShader"));
                        mt.renderQueue = 3000;
                        mt.enableInstancing = true;
                        return builder.updateView().
                        apply(gameObject, mt);
                    }
                    else
                    {
                        throw new UnityException("'Linkinfield' or 'GraphDimension' is undefined. Please select a linking field or a graph dimension");
                    }

                case AbstractVisualisation.GeometryType.Quads:
                    builder.createIndicesPointTopology();
                    mt = new Material(Shader.Find("IATK/Quads"));
                    mt.renderQueue = 3000;
                    mt.enableInstancing = true;
                    return builder.updateView().
                       apply(gameObject, mt);

                case AbstractVisualisation.GeometryType.LinesAndDots:
                    if (visualisationReference.graphDimension != "Undefined")
                    {
                        CSVDataSource csvds = (CSVDataSource)(visualisationReference.dataSource);
                        builder.createIndicesGraphTopology(csvds.GraphEdges);
                        mt = new Material(Shader.Find("IATK/LineAndDotsShader"));
                        mt.renderQueue = 3000;
                        mt.enableInstancing = true;
                        return builder.updateView().
                        apply(gameObject, mt);
                    }
                    if (visualisationReference.linkingDimension != "Undefined")
                    {
                        builder.createIndicesConnectedLineTopology(visualisationReference.dataSource[visualisationReference.linkingDimension].Data);
                        mt = new Material(Shader.Find("IATK/LineAndDotsShader"));
                        mt.renderQueue = 3000;
                        mt.enableInstancing = true;
                        return builder.updateView().
                        apply(gameObject, mt);
                    }
                    else
                    {
                        throw new UnityException("'Linkinfield' or 'GraphDimension' is undefined. Please select a linking field or a graph dimension");
                    }


                case AbstractVisualisation.GeometryType.Cubes:
                    builder.createIndicesPointTopology(); // createIndicesLinkedTopology(dataSource[linkingDimension].Data);
                    mt = new Material(Shader.Find("IATK/CubeShader"));
                    mt.renderQueue = 3000;
                    mt.enableInstancing = true;
                    return builder.updateView().
                    apply(gameObject, mt);

                case AbstractVisualisation.GeometryType.Bars:
                    builder.createIndicesPointTopology(); // createIndicesLinkedTopology(dataSource[linkingDimension].Data);
                    mt = new Material(Shader.Find("IATK/BarShader"));
                    mt.renderQueue = 3000;
                    mt.enableInstancing = true;
                    return builder.updateView().
                    apply(gameObject, mt);

                case AbstractVisualisation.GeometryType.Spheres:
                    builder.createIndicesPointTopology(); // createIndicesLinkedTopology(dataSource[linkingDimension].Data);
                    mt = new Material(Shader.Find("IATK/SphereShader"));
                    mt.mainTexture = Resources.Load("sphere-texture") as Texture2D;
                    mt.renderQueue = 3000;
                    mt.enableInstancing = true;
                    return builder.updateView().
                    apply(gameObject, mt);

                default:
                    return null;
            }
        }
        
        /// <summary>
        /// Geometries to mesh topology.
        /// </summary>
        /// <returns>The to mesh topology.</returns>
        /// <param name="geometry">Geometry.</param>
        protected MeshTopology geometryToMeshTopology(AbstractVisualisation.GeometryType geometry)
        {
            switch (geometry)
            {
                case AbstractVisualisation.GeometryType.Points:
                    return MeshTopology.Points;
                case AbstractVisualisation.GeometryType.Lines:
                    return MeshTopology.Lines;
                case AbstractVisualisation.GeometryType.LinesAndDots:
                    return MeshTopology.Lines;
                case AbstractVisualisation.GeometryType.Bars:
                    return MeshTopology.Points;
                case AbstractVisualisation.GeometryType.Cubes:
                    return MeshTopology.Points;
                case AbstractVisualisation.GeometryType.Spheres:
                    return MeshTopology.Points;

                default:
                    return MeshTopology.Points;
            }
        }
        
        //void OnDestroy()
        //{
        //    destroyView();
        //    //foreach (Transform item in transform)
        //    //{
        //    //    DestroyImmediate(item.gameObject);
        //    //}
        //}

        //void OnApplicationQuit()
        //{
        //    if (creationConfiguration != null)
        //        SerializeViewConfiguration(creationConfiguration);
        //}

    }
}