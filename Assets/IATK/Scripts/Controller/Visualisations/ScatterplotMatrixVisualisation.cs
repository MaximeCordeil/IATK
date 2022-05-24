using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

namespace IATK {
    public class ScatterplotMatrixVisualisation : AbstractVisualisation
    {
        public List<GameObject>[, ,] AxisHolder = new List<GameObject>[64, 64, 64];

        public float xPadding = 1.5f;
        public float yPadding = 1.5f;
        public float zPadding = 1.5f;
        
        public override void CreateVisualisation()
        {
            viewList.Clear();
            destroyView();

            //TODO: not optimal - destroying all the axes when creating the new visualisation again...
            for (int i = 0; i < 64; i++)
            {
                for (int j = 0; j < 64; j++)
                {
                    for (int k = 0; k < 64; k++)
                    {
                    //    if (i != j)//
                            if (AxisHolder[i,j,k]!=null)
                        {
                            foreach (var item in AxisHolder[i, j, k])
                            {
                                DestroyImmediate(item.gameObject);
                            }
                            AxisHolder[i, j, k] = null;
                        }
                        #region to test again
                        //if (AxisHolder[i, j, k] != null &&
                        //    (AxisHolder[i, j, k][0].GetComponent<Axis>().axisId != i ||
                        //    AxisHolder[i, j, k][1].GetComponent<Axis>().axisId != j))
                        ////AxisHolder[i, j, k][2].GetComponent<Axis>().axisId != k))
                        //{
                        //    foreach (var item in AxisHolder[i, j, k])
                        //    {
                        //        DestroyImmediate(item.gameObject);
                        //    }
                        //    AxisHolder[i, j, k] = null;
                        //}

                        //if (((k * AxisHolder.Length * AxisHolder.Length) + (j * AxisHolder.Length) + i) > viewList.Count && AxisHolder[i, j, k] != null)
                        //{
                        //    foreach (var item in AxisHolder[i, j, k])
                        //    {
                        //        DestroyImmediate(item.gameObject);
                        //    }
                        //    AxisHolder[i, j, k] = null;
                        //}
                        #endregion
                    }
                }
            }

            if (creationConfiguration == null)
                creationConfiguration = new CreationConfiguration(visualisationReference.geometry, new Dictionary<CreationConfiguration.Axis, string>());
            else
            {
                creationConfiguration.Geometry = visualisationReference.geometry;
                creationConfiguration.Axies = new Dictionary<CreationConfiguration.Axis, string>();
                creationConfiguration.VisualisationType = AbstractVisualisation.VisualisationTypes.SCATTERPLOT_MATRIX;
            }

            if (visualisationReference.zScatterplotMatrixDimensions.Length > 0)
            {
                for (int i = 0; i < visualisationReference.xScatterplotMatrixDimensions.Length; i++)
                {
                    for (int j = 0; j < visualisationReference.yScatterplotMatrixDimensions.Length; j++)
                    {
                        for (int k = 0; k < visualisationReference.zScatterplotMatrixDimensions.Length; k++)
                        {
                            if (!(i == j && j == k && i == k))
                            {
                               if (visualisationReference.xScatterplotMatrixDimensions[i] != null
                               && visualisationReference.yScatterplotMatrixDimensions[j] != null
                               && visualisationReference.zScatterplotMatrixDimensions[k] != null)
                                {
                                    Create3DVisualisation(i, j, k);

                                    Vector3 posX = new Vector3(i * xPadding, j * yPadding - 0.05f, k * zPadding);
                                    Vector3 posY = new Vector3(i * xPadding - 0.05f, j * yPadding, k * zPadding);
                                    Vector3 posZ = new Vector3(i * xPadding - 0.05f, j * yPadding - 0.05f, k * zPadding);

                                    List<GameObject> localList = new List<GameObject>
                                    {
                                        CreateAxis(PropertyType.X,
                                        visualisationReference.xScatterplotMatrixDimensions[i],
                                        posX,
                                        new Vector3(0f, 0f, -90f), i
                                        ),

                                        CreateAxis(
                                        PropertyType.Y,
                                        visualisationReference.yScatterplotMatrixDimensions[j],
                                        posY,
                                        new Vector3(0f, 0f, 0f), j),

                                        CreateAxis(
                                       PropertyType.Z,
                                       visualisationReference.zScatterplotMatrixDimensions[k],
                                       posZ,
                                       new Vector3(90f, 0f, 0f), k)
                                    };

                                    AxisHolder[i, j, k] = localList;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < visualisationReference.xScatterplotMatrixDimensions.Length; i++)
                {
                    for (int j = 0; j < visualisationReference.yScatterplotMatrixDimensions.Length; j++)
                    {
                        if (i != j)
                        {
                            if (visualisationReference.xScatterplotMatrixDimensions[i] != null
                                   && visualisationReference.yScatterplotMatrixDimensions[j] != null)
                            {
                                Create2DVisualisation(i, j);
                                
                                Vector3 posX = new Vector3(i * xPadding, j * yPadding - 0.05f, 0);
                                Vector3 posY = new Vector3(i * xPadding - 0.05f, j * yPadding, 0);

                                List<GameObject> localList = new List<GameObject>
                                {
                                    CreateAxis(PropertyType.X,
                                    visualisationReference.xScatterplotMatrixDimensions[i],
                                    posX,
                                    new Vector3(0f, 0f, -90f), i
                                    ),

                                    CreateAxis(
                                    PropertyType.Y,
                                    visualisationReference.yScatterplotMatrixDimensions[j],
                                    posY,
                                    new Vector3(0f, 0f, 0f), j)
                                };

                                AxisHolder[i, j, 0] = localList;
                            }                           
                        }
                    }
                }
            }
            
            if (viewList.Count > 0)
            {
                UpdateVisualisation(PropertyType.Colour);
                UpdateVisualisation(PropertyType.Size);
                UpdateVisualisation(PropertyType.SizeValues);                
            }
        }

        private void Create2DVisualisation(int i, int j)
        {
            ViewBuilder builder = new ViewBuilder(geometryToMeshTopology(creationConfiguration.Geometry), "Scatterplot Matrix");
            builder.initialiseDataView(visualisationReference.dataSource.DataCount);

            builder.setDataDimension(visualisationReference.dataSource[visualisationReference.xScatterplotMatrixDimensions[i].Attribute].Data, ViewBuilder.VIEW_DIMENSION.X);
            builder.setDataDimension(visualisationReference.dataSource[visualisationReference.yScatterplotMatrixDimensions[j].Attribute].Data, ViewBuilder.VIEW_DIMENSION.Y);

            View v = ApplyGeometryAndRendering(creationConfiguration, ref builder);
            //destroy the view to clean the big mesh

            if (v != null)
            {
                v.transform.localPosition = new Vector3(i * xPadding, j * yPadding, 0);
                v.transform.SetParent(transform, false);
                viewList.Add(v);
            }

           
        }

        private void Create3DVisualisation(int i, int j, int k)
        {
            ViewBuilder builder = new ViewBuilder(geometryToMeshTopology(creationConfiguration.Geometry), "Scatterplot Matrix");
            builder.initialiseDataView(visualisationReference.dataSource.DataCount);

            builder.setDataDimension(visualisationReference.dataSource[visualisationReference.xScatterplotMatrixDimensions[i].Attribute].Data, ViewBuilder.VIEW_DIMENSION.X);
            builder.setDataDimension(visualisationReference.dataSource[visualisationReference.yScatterplotMatrixDimensions[j].Attribute].Data, ViewBuilder.VIEW_DIMENSION.Y);
            builder.setDataDimension(visualisationReference.dataSource[visualisationReference.zScatterplotMatrixDimensions[k].Attribute].Data, ViewBuilder.VIEW_DIMENSION.Z);

            View v = ApplyGeometryAndRendering(creationConfiguration, ref builder);
            //destroy the view to clean the big mesh

            if (v != null)
            {
                v.transform.localPosition = new Vector3(i * xPadding, j * yPadding, k * zPadding);
                v.transform.SetParent(transform, false);
                viewList.Add(v);
            }

          
        }

        public override void LoadConfiguration()
        {
            throw new System.NotImplementedException();
        }

        public override Color[] mapColoursContinuous(float[] data)
        {
            Color[] colours = new Color[data.Length];

            for (int i = 0; i < data.Length; ++i)
            {
                colours[i] = visualisationReference.dimensionColour.Evaluate(data[i]);
            }

            return colours;
        }

        public override void SaveConfiguration()
        {
            throw new System.NotImplementedException();
        }

        public override void UpdateVisualisation(PropertyType propertyType)
        {
            if (viewList.Count == 0)
                CreateVisualisation();

            if (viewList.Count != 0)
                switch (propertyType)
                {
                    case AbstractVisualisation.PropertyType.X:
                        if (visualisationReference.xDimension.Attribute.Equals("Undefined")) viewList[0].ZeroPosition(0);
                        else viewList[0].UpdateXPositions(visualisationReference.dataSource[visualisationReference.xDimension.Attribute].Data);
                        if (creationConfiguration.Axies.ContainsKey(CreationConfiguration.Axis.X)) creationConfiguration.Axies[CreationConfiguration.Axis.X] = visualisationReference.xDimension.Attribute;
                        else creationConfiguration.Axies.Add(CreationConfiguration.Axis.X, visualisationReference.xDimension.Attribute);
                        break;
                    case AbstractVisualisation.PropertyType.Y:
                        if (visualisationReference.yDimension.Attribute.Equals("Undefined")) viewList[0].ZeroPosition(1);
                        else viewList[0].UpdateYPositions(visualisationReference.dataSource[visualisationReference.yDimension.Attribute].Data);
                        if (creationConfiguration.Axies.ContainsKey(CreationConfiguration.Axis.Y)) creationConfiguration.Axies[CreationConfiguration.Axis.Y] = visualisationReference.yDimension.Attribute;
                        else creationConfiguration.Axies.Add(CreationConfiguration.Axis.Y, visualisationReference.yDimension.Attribute);
                        break;
                    case AbstractVisualisation.PropertyType.Z:
                        if (visualisationReference.zDimension.Attribute.Equals("Undefined")) viewList[0].ZeroPosition(2);
                        else viewList[0].UpdateZPositions(visualisationReference.dataSource[visualisationReference.zDimension.Attribute].Data);
                        if (creationConfiguration.Axies.ContainsKey(CreationConfiguration.Axis.Z)) creationConfiguration.Axies[CreationConfiguration.Axis.Z] = visualisationReference.zDimension.Attribute;
                        else creationConfiguration.Axies.Add(CreationConfiguration.Axis.Z, visualisationReference.zDimension.Attribute);
                        break;
                    case AbstractVisualisation.PropertyType.Colour:
                        if (visualisationReference.colourDimension != "Undefined")
                        {
                            for (int i = 0; i < viewList.Count; i++)
                                viewList[i].SetColors(mapColoursContinuous(visualisationReference.dataSource[visualisationReference.colourDimension].Data));
                        }
                        else if (visualisationReference.colorPaletteDimension != "Undefined")
                        {
                            for (int i = 0; i < viewList.Count; i++)
                            {
                                viewList[i].SetColors(mapColoursPalette(visualisationReference.dataSource[visualisationReference.colorPaletteDimension].Data, visualisationReference.coloursPalette));
                            }
                        }
                        else
                        {
                            for (int i = 0; i < viewList.Count; i++)
                            {
                                Color[] colours = viewList[0].GetColors();
                                for (int j = 0; j < colours.Length; ++j)
                                {
                                    colours[j] = visualisationReference.colour;
                                }
                                viewList[i].SetColors(colours);
                            }

                        }

                        creationConfiguration.ColourDimension = visualisationReference.colourDimension;
                        creationConfiguration.colourKeys = visualisationReference.dimensionColour;
                        creationConfiguration.colour = visualisationReference.colour;

                        break;
                    case AbstractVisualisation.PropertyType.Size:
                        {
                            for (int i = 0; i < viewList.Count; i++)
                            {
                                if (visualisationReference.sizeDimension != "Undefined")
                                {
                                    viewList[i].SetSizeChannel(visualisationReference.dataSource[visualisationReference.sizeDimension].Data);
                                }
                                else
                                {
                                    viewList[i].SetSizeChannel(new float[visualisationReference.dataSource.DataCount]);
                                }
                            }
                            creationConfiguration.SizeDimension = visualisationReference.sizeDimension;
                            break;
                        }
                    case PropertyType.SizeValues:
                        for (int i = 0; i < viewList.Count; i++)
                        {
                            viewList[i].SetSize(visualisationReference.size);
                            viewList[i].SetMinSize(visualisationReference.minSize);        // Data is normalised
                            viewList[i].SetMaxSize(visualisationReference.maxSize);
                        }
                        creationConfiguration.Size = visualisationReference.size;
                        creationConfiguration.MinSize = visualisationReference.minSize;
                        creationConfiguration.MaxSize = visualisationReference.maxSize;

                        break;
                    case AbstractVisualisation.PropertyType.LinkingDimension:
                        creationConfiguration.LinkingDimension = visualisationReference.linkingDimension;

                        CreateVisualisation(); // needs to recreate the visualsiation because the mesh properties have changed 
                        break;

                    case AbstractVisualisation.PropertyType.GeometryType:
                        creationConfiguration.Geometry = visualisationReference.geometry;
                        CreateVisualisation(); // needs to recreate the visualsiation because the mesh properties have changed 
                        break;

                    case AbstractVisualisation.PropertyType.Scaling:

                        for (int i = 0; i < viewList.Count; i++)
                        {
                            viewList[i].SetMinNormX(visualisationReference.xDimension.minScale);
                            viewList[i].SetMaxNormX(visualisationReference.xDimension.maxScale);
                            viewList[i].SetMinNormY(visualisationReference.yDimension.minScale);
                            viewList[i].SetMaxNormY(visualisationReference.yDimension.maxScale);
                            viewList[i].SetMinNormZ(visualisationReference.zDimension.minScale);
                            viewList[i].SetMaxNormZ(visualisationReference.zDimension.maxScale);
                        }
                        break;

                    case AbstractVisualisation.PropertyType.DimensionFiltering:
                        {  
                            float[] isFiltered = new float[visualisationReference.dataSource.DataCount];
                            for (int i = 0; i < visualisationReference.dataSource.DimensionCount; i++)
                            {
                                foreach (DimensionFilter attrFilter in visualisationReference.xScatterplotMatrixDimensions)
                                {
                                    if (attrFilter.Attribute == visualisationReference.dataSource[i].Identifier)
                                    {
                                        float minFilteringValue = UtilMath.normaliseValue(attrFilter.minFilter, 0f, 1f, attrFilter.minScale, attrFilter.maxScale);
                                        float maxFilteringValue = UtilMath.normaliseValue(attrFilter.maxFilter, 0f, 1f, attrFilter.minScale, attrFilter.maxScale);

                                        for (int j = 0; j < isFiltered.Length; j++)
                                        {
                                            isFiltered[j] = (visualisationReference.dataSource[i].Data[j] < minFilteringValue || visualisationReference.dataSource[i].Data[j] > maxFilteringValue) ? 1.0f : isFiltered[j];
                                        }
                                    }
                                }
                                foreach (DimensionFilter attrFilter in visualisationReference.yScatterplotMatrixDimensions)
                                {
                                    if (attrFilter.Attribute == visualisationReference.dataSource[i].Identifier)
                                    {
                                        float minFilteringValue = UtilMath.normaliseValue(attrFilter.minFilter, 0f, 1f, attrFilter.minScale, attrFilter.maxScale);
                                        float maxFilteringValue = UtilMath.normaliseValue(attrFilter.maxFilter, 0f, 1f, attrFilter.minScale, attrFilter.maxScale);

                                        for (int j = 0; j < isFiltered.Length; j++)
                                        {
                                            isFiltered[j] = (visualisationReference.dataSource[i].Data[j] < minFilteringValue || visualisationReference.dataSource[i].Data[j] > maxFilteringValue) ? 1.0f : isFiltered[j];
                                        }
                                    }
                                }
                                foreach (DimensionFilter attrFilter in visualisationReference.zScatterplotMatrixDimensions)
                                {
                                    if (attrFilter.Attribute == visualisationReference.dataSource[i].Identifier)
                                    {
                                        float minFilteringValue = UtilMath.normaliseValue(attrFilter.minFilter, 0f, 1f, attrFilter.minScale, attrFilter.maxScale);
                                        float maxFilteringValue = UtilMath.normaliseValue(attrFilter.maxFilter, 0f, 1f, attrFilter.minScale, attrFilter.maxScale);

                                        for (int j = 0; j < isFiltered.Length; j++)
                                        {
                                            isFiltered[j] = (visualisationReference.dataSource[i].Data[j] < minFilteringValue || visualisationReference.dataSource[i].Data[j] > maxFilteringValue) ? 1.0f : isFiltered[j];
                                        }
                                    }
                                }
                            }
                            // map the filtered attribute into the normal channel of the bigmesh
                            foreach (View v in viewList)
                            {
                                v.SetFilterChannel(isFiltered);
                            }
                            // update axis details
                            for (int i = 0; i < GameObject_Axes_Holders.Count; ++i)
                            {
                                Axis axis = GameObject_Axes_Holders[i].GetComponent<Axis>();
                                BindMinMaxAxisValues(axis, visualisationReference.parallelCoordinatesDimensions[i]);
                            }
                        }
                        break;
                    case AbstractVisualisation.PropertyType.AttributeFiltering:
                        {
                            foreach (var viewElement in viewList)
                            {
                                float[] isFiltered = new float[visualisationReference.dataSource.DataCount];
                                for (int i = 0; i < visualisationReference.dataSource.DimensionCount; i++)
                                {
                                    foreach (AttributeFilter attrFilter in visualisationReference.attributeFilters)
                                    {
                                        //print(attrFilter.Attribute + "   " + dataSource[i].Identifier);
                                        if (attrFilter.Attribute == visualisationReference.dataSource[i].Identifier)
                                        {
                                            float minFilteringValue = UtilMath.normaliseValue(attrFilter.minFilter, 0f, 1f, attrFilter.minScale, attrFilter.maxScale);
                                            float maxFilteringValue = UtilMath.normaliseValue(attrFilter.maxFilter, 0f, 1f, attrFilter.minScale, attrFilter.maxScale);

                                            for (int j = 0; j < isFiltered.Length; j++)
                                            {
                                                isFiltered[j] = (visualisationReference.dataSource[i].Data[j] < minFilteringValue || visualisationReference.dataSource[i].Data[j] > maxFilteringValue) ? 1.0f : isFiltered[j];
                                            }
                                        }
                                    }
                                }
                                // map the filtered attribute into the normal channel of the bigmesh
                                foreach (View v in viewList)
                                {
                                    v.SetFilterChannel(isFiltered);
                                }
                            }
                        }
                        break;
                    case PropertyType.VisualisationType:

                        break;
                    default:
                        break;
                }

            if (visualisationReference.geometry != GeometryType.Undefined)// || visualisationType == VisualisationTypes.PARALLEL_COORDINATES)
                SerializeViewConfiguration(creationConfiguration);

            ////Update any label on the corresponding axes
            if (propertyType == AbstractVisualisation.PropertyType.DimensionChange)
                CreateVisualisation();
            
        }

        public Color[] mapColoursPalette(float[] data, Color[] palette)
        {
            Color[] colours = new Color[data.Length];

            float[] uniqueValues = data.Distinct().ToArray();

            for (int i = 0; i < data.Length; i++)
            {
                int indexColor = Array.IndexOf(uniqueValues, data[i]);
                colours[i] = palette[indexColor];
            }

            return colours;
        }


        // Use this for initialization
        void Start() {

        }

        // Update is called once per frame
        void Update() {

        }

        //private void CreateScatterplotMatrix(CreationConfiguration configuration)
        //{



        //}
    }
}