using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

namespace IATK
{
    public class ParallelCoordinatesVisualisation : AbstractVisualisation
    {

        ///// <summary>
        ///// Maps data attributes on the normal channel.
        ///// </summary>
        ///// <returns>The colours.</returns>
        ///// <param name="data">Data.</param>
        //public override void mapNormalChannel(ref Vector3[] normals, int channel, float[] data)
        //{
        //    int nbAxes = visualisationReference.parallelCoordinatesDimensions.Length;
        //    List<float> dataList = new List<float>();
        //    for (int i = 0; i < nbAxes; i++)
        //    {
        //        dataList.AddRange(data.ToList());
        //    }

        //    for (int i = 0; i < normals.Length; ++i)
        //    {
        //        Vector3 normal = normals[i];
        //        normal[channel] = dataList[i];
        //        normals[i] = normal;
        //    }
        //}

        /// <summary>
        /// Maps the colours.
        /// </summary>
        /// <returns>The colours.</returns>
        /// <param name="data">Data.</param>
        /// 
        public override Color[] mapColoursContinuous(float[] data)
        {
            int nbAxes = visualisationReference.parallelCoordinatesDimensions.Length;
            List<float> dataList = new List<float>();
            for (int i = 0; i < nbAxes; i++)
            {
                dataList.AddRange(data.ToList());
            }

            Color[] colours = new Color[dataList.Count];

            for (int i = 0; i < dataList.Count; ++i)
            {
                colours[i] = visualisationReference.dimensionColour.Evaluate(dataList[i]);
            }

            return colours;
        }

        public Color[] mapColoursPalette(float[] data, Color[] palette)
        {

            int nbAxes = visualisationReference.parallelCoordinatesDimensions.Length;
            List<float> dataList = new List<float>();
            for (int i = 0; i < nbAxes; i++)
            {
                dataList.AddRange(data.ToList());
            }

            Color[] colours = new Color[dataList.Count];

            float[] uniqueValues = data.Distinct().ToArray();

            for (int i = 0; i < dataList.Count; ++i)
            {
                int indexColor = Array.IndexOf(uniqueValues, dataList[i]);
                colours[i] = palette[indexColor];
            }

            return colours;
        }

        public override void CreateVisualisation()
        {
            if (visualisationReference.parallelCoordinatesDimensions.Length > 1 && visualisationReference.parallelCoordinatesDimensions.None(x => x == null || x.Attribute == "Undefined"))
            {
                viewList.Clear();
                destroyView();

                ViewBuilder viewParallel;

                List<float> positionsLocalX = new List<float>();
                List<float> positionsLocalY = new List<float>();
                List<float> positionsLocalZ = new List<float>();
                List<int> indices = new List<int>();

                for (int i = 0; i < visualisationReference.parallelCoordinatesDimensions.Length; i++)
                {
                    if (visualisationReference.parallelCoordinatesDimensions[i] != null && visualisationReference.parallelCoordinatesDimensions[i].Attribute != "Undefined")
                    {
                        float[] positions = visualisationReference.dataSource[visualisationReference.parallelCoordinatesDimensions[i].Attribute].Data;
                        for (int k = 0; k < positions.Length; k++)
                        {
                            positionsLocalX.Add((float)GameObject_Axes_Holders[i].transform.localPosition.x);
                            positionsLocalY.Add(positions[k] + (float)GameObject_Axes_Holders[i].transform.localPosition.y);
                            positionsLocalZ.Add((float)GameObject_Axes_Holders[i].transform.localPosition.z);
                        }
                    }
                }

                List<float> parallelCoordinatesIndices = new List<float>();
                List<float> repeatPattern = Enumerable.Range(0, visualisationReference.dataSource.DataCount).Select(x => x*1f).ToList();

                for (int i = 0; i < visualisationReference.parallelCoordinatesDimensions.Length; i++)
                {
                    parallelCoordinatesIndices.AddRange(repeatPattern);
                }

                //build indices
                for (int i = 0; i < visualisationReference.dataSource.DataCount; i++)
                {
                    for (int j = 0; j < visualisationReference.parallelCoordinatesDimensions.Length- 1; j++)
                    {
                        indices.Add(j * visualisationReference.dataSource.DataCount + i);
                        indices.Add((j + 1) * visualisationReference.dataSource.DataCount + i);
                    }
                }

                int[] lineLength = new int[visualisationReference.dataSource.DataCount];
                for (int i = 0; i < visualisationReference.dataSource.DataCount; i++)
                {
                    lineLength[i] = visualisationReference.parallelCoordinatesDimensions.Length;
                }

                viewParallel = new ViewBuilder(MeshTopology.Lines, "[IATK] Parallel Coordinates");
                viewParallel.initialiseDataView(positionsLocalX.Count);
                viewParallel.setDataDimension(positionsLocalX.ToArray(), ViewBuilder.VIEW_DIMENSION.X);
                viewParallel.setDataDimension(positionsLocalY.ToArray(), ViewBuilder.VIEW_DIMENSION.Y);
                viewParallel.setDataDimension(positionsLocalZ.ToArray(), ViewBuilder.VIEW_DIMENSION.Z);
                viewParallel.Indices = indices;
                viewParallel.LineLength = lineLength.ToList();
                //viewParallel.;

                Material mt = new Material(Shader.Find("IATK/PCPShader"));
                mt.renderQueue = 3000;
                mt.enableInstancing = true;
                View v = viewParallel.
                    createIndicesPointTopology(parallelCoordinatesIndices.ToArray()).
                    updateView().apply(gameObject, mt);
                
                //v.SetVertexIdChannel(parallelCoordinatesIndices.ToArray());

                viewList.Add(v);

                //Creation configuration management
                if (creationConfiguration == null)
                    creationConfiguration = new CreationConfiguration();

                creationConfiguration.VisualisationType = AbstractVisualisation.VisualisationTypes.PARALLEL_COORDINATES;
                creationConfiguration.parallelCoordinatesDimensions = visualisationReference.parallelCoordinatesDimensions;
                creationConfiguration.colour = visualisationReference.colour;
                creationConfiguration.ColourDimension = visualisationReference.colourDimension;
                creationConfiguration.colourKeys = visualisationReference.dimensionColour;
                creationConfiguration.Geometry = visualisationReference.geometry;
                creationConfiguration.LinkingDimension = visualisationReference.linkingDimension;
                creationConfiguration.SizeDimension = visualisationReference.sizeDimension;
                creationConfiguration.Axies = new Dictionary<CreationConfiguration.Axis, string>();

                //restore properties
                UpdateVisualisation(AbstractVisualisation.PropertyType.Colour);
                UpdateVisualisation(AbstractVisualisation.PropertyType.Size);
                UpdateVisualisation(AbstractVisualisation.PropertyType.SizeValues);

            }
        }

        public override void UpdateVisualisation(PropertyType propertyType)
        {

            if (viewList.Count != 0)
                switch (propertyType)
                {
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
                            viewList[i].SetMinSize(visualisationReference.minSize); // Data is normalised
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
                        for (int i = 0; i < GameObject_Axes_Holders.Count; ++i)
                        {
                            Axis axis = GameObject_Axes_Holders[i].GetComponent<Axis>();
                            BindMinMaxAxisValues(axis, visualisationReference.parallelCoordinatesDimensions[i]);
                        }
                        break;

                    case AbstractVisualisation.PropertyType.DimensionFiltering:
                        {
                            float[] isFiltered = new float[visualisationReference.dataSource.DataCount];
                            for (int i = 0; i < visualisationReference.dataSource.DimensionCount; i++)
                            {
                                foreach (DimensionFilter attrFilter in visualisationReference.parallelCoordinatesDimensions)
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
                        break;
                    case PropertyType.VisualisationType:

                        break;

                    case PropertyType.BlendDestinationMode:
                        float bmds = (int)(System.Enum.Parse(typeof(UnityEngine.Rendering.BlendMode), visualisationReference.blendingModeDestination));
                        for (int i = 0; i < viewList.Count; i++)
                        {
                            viewList[i].SetBlendindDestinationMode(bmds);
                        }

                        break;
                    case PropertyType.BlendSourceMode:
                        float bmd = (int)(System.Enum.Parse(typeof(UnityEngine.Rendering.BlendMode), visualisationReference.blendingModeSource));
                        for (int i = 0; i < viewList.Count; i++)
                        {
                            viewList[i].SetBlendingSourceMode(bmd);
                        }

                        break;
                    case PropertyType.DimensionChangeFiltering:
                        //foreach (var viewElement in viewList)
                        
                        break;
                    default:
                        break;
                }

            // if (visualisationReference.geometry != GeometryType.Undefined)// || visualisationType == VisualisationTypes.PARALLEL_COORDINATES)
            if (creationConfiguration != null)
                SerializeViewConfiguration(creationConfiguration);

            //Update any label on the corresponding axes
            if (propertyType == PropertyType.DimensionChange)
            {
                RebuildAxis();
            }
        }

        private void RebuildAxis()
        {
            Axis[] sceneAxes = GetComponentsInChildren<Axis>();

            for (int i = 0; i < visualisationReference.parallelCoordinatesDimensions.Length; i++)
            {
                // update
                if (visualisationReference.parallelCoordinatesDimensions[i] != null &&
                    visualisationReference.dataSource.Any(x => x.Identifier == visualisationReference.parallelCoordinatesDimensions[i].Attribute) &&
                    visualisationReference.dataSource.Any(x => x.Identifier != "Undefined"))
                {
                    if (i < GameObject_Axes_Holders.Count())
                    {
                        if (GameObject_Axes_Holders[i] != null)
                        {
                            Axis axis = GameObject_Axes_Holders[i].GetComponent<Axis>();
                            axis.Initialise(visualisationReference.dataSource, visualisationReference.parallelCoordinatesDimensions[i], visualisationReference);
                            BindMinMaxAxisValues(axis, visualisationReference.parallelCoordinatesDimensions[i]);
                            axis.AttributeName = visualisationReference.parallelCoordinatesDimensions[i].Attribute;
                        }
                    }
                    else if (sceneAxes.Any(x => x.SourceIndex == i && x.AttributeName == visualisationReference.parallelCoordinatesDimensions[i].Attribute))
                    {
                        Axis a = sceneAxes.Single(x => x.SourceIndex == i && x.AttributeName == visualisationReference.parallelCoordinatesDimensions[i].Attribute);
                        GameObject_Axes_Holders.Add(a.transform.gameObject);
                    }
                    else
                    {
                        // create only if there are no other objects in the scene that correspond to this axis
                        Vector3 pos = new Vector3(i, 0, 0);
                        var obj = CreateAxis(AbstractVisualisation.PropertyType.Y, visualisationReference.parallelCoordinatesDimensions[i], pos, Vector3.zero, i);
                        GameObject_Axes_Holders.Add(obj);
                    }
                }
            }
            // delete the remainders
            for (int i = visualisationReference.parallelCoordinatesDimensions.Length; i < GameObject_Axes_Holders.Count; i++)
            {
                GameObject toDestroy = GameObject_Axes_Holders[i];
                GameObject_Axes_Holders.Remove(toDestroy);
                DestroyImmediate(toDestroy);
            }

            //create the PCP mesh data only if there is more than 1 axis, and no attributes are undefined
            if (visualisationReference.parallelCoordinatesDimensions.Length > 1 && visualisationReference.parallelCoordinatesDimensions.None(x => x == null || x.Attribute == "Undefined"))
                CreateVisualisation();
        }

        public override void SaveConfiguration() { }

        public override void LoadConfiguration() { }

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

    }
}