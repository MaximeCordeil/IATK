using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace IATK
{

    /// <summary>
    /// Visualisation editor. Custom editor for inspector in Visualisation component.
    /// </summary>
    [CustomEditor(typeof(Visualisation))]
    [CanEditMultipleObjects]
    public class AbstractVisualisationEditor : Editor
    {
        private SerializedProperty uidProperty;
        private SerializedProperty dataSourceProperty;
        private SerializedProperty geometryProperty;
        private SerializedProperty colourProperty;
        private SerializedProperty sizeProperty;

        private SerializedProperty xDimensionProperty;
        private SerializedProperty yDimensionProperty;
        private SerializedProperty zDimensionProperty;

        private SerializedProperty colourDimensionProperty;
        private SerializedProperty colourPaletteProperty;
        private SerializedProperty colourGradientProperty;

        private SerializedProperty blendingModeSourceProperty;
        private SerializedProperty blendingModeDestinationProperty;

        private SerializedProperty sizeDimensionProperty;
        private SerializedProperty minSizeProperty;
        private SerializedProperty maxSizeProperty;

        private SerializedProperty linkingDimensionProperty;
        private SerializedProperty originDimensionProperty;
        private SerializedProperty destinationDimensionProperty;
        private SerializedProperty graphDimensionProperty;

        private SerializedProperty visualisationTypeProperty;

        private SerializedProperty xScatterplotMatrixDimensionsProperty;
        private SerializedProperty yScatterplotMatrixDimensionsProperty;
        private SerializedProperty zScatterplotMatrixDimensionsProperty;

        private SerializedProperty parallelCoordinatesDimensionsProperty;

        private SerializedProperty attributeFiltersProperty;
        private SerializedProperty theVisualizationObjectProperty;

        private SerializedProperty colorPaletteDimensionProperty;

        private SerializedProperty widthProperty;
        private SerializedProperty heightProperty;
        private SerializedProperty depthProperty;


        private DataSource dataSource;

        private string undefinedString = "Undefined";

        protected List<string> dimensions = new List<string>();
        private List<string> visualisations = new List<string>();

        void DrawMinMaxSlider(Rect rect, SerializedProperty minFilterProp, SerializedProperty maxFilterProp, string attributeid, DataSource dataSource)
        {
            bool isUndefined = dataSource == null || attributeid == "Undefined";
            int idx = Array.IndexOf(dataSource.Select(m => m.Identifier).ToArray(), attributeid);

            // get the normalized value
            float minValue = !isUndefined ? dataSource[attributeid].MetaData.minValue : 0.0f;
            float maxValue = !isUndefined ? dataSource[attributeid].MetaData.maxValue : 1.0f;

            // calculate the real value
            float min = UtilMath.normaliseValue(minFilterProp.floatValue, 0, 1, minValue, maxValue);
            float max = UtilMath.normaliseValue(maxFilterProp.floatValue, 0, 1, minValue, maxValue);

            // get the string representation
            string minLogical = isUndefined ? "" : dataSource.getOriginalValue(minFilterProp.floatValue, idx).ToString();
            string maxLogical = isUndefined ? "" : dataSource.getOriginalValue(maxFilterProp.floatValue, idx).ToString();

            EditorGUI.TextField(new Rect(rect.x, rect.y, 75, rect.height), minLogical);
            EditorGUI.MinMaxSlider(new Rect(rect.x + 75, rect.y, rect.width - 150, rect.height), GUIContent.none, ref min, ref max, minValue, maxValue);
            EditorGUI.TextField(new Rect(rect.x + rect.width - 78, rect.y, 75, rect.height), maxLogical);

            minFilterProp.floatValue = UtilMath.normaliseValue(min, minValue, maxValue, 0, 1);
            maxFilterProp.floatValue = UtilMath.normaliseValue(max, minValue, maxValue, 0, 1);
        }

        AbstractVisualisation.PropertyType DrawAttributeFilterEditor(SerializedProperty element, Rect rect)
        {
            AbstractVisualisation.PropertyType dirtyFlag = AbstractVisualisation.PropertyType.None;

            Rect contentRect = rect;
            contentRect.height = EditorGUIUtility.singleLineHeight;

            Rect copyrect = rect;
            copyrect.height -= copyrect.height / 10f;

            EditorGUI.BeginChangeCheck();
            var attributeProp = element.FindPropertyRelative("Attribute");
            int attributeIndex = dimensions.IndexOf(attributeProp.stringValue);

            if (attributeIndex >= 0)
            {
                attributeIndex = EditorGUI.Popup(contentRect, attributeIndex, dimensions.ToArray());
                attributeProp.stringValue = dimensions[attributeIndex];
                if (EditorGUI.EndChangeCheck())
                {
                    dirtyFlag = AbstractVisualisation.PropertyType.DimensionChange;
                }
            }

            EditorGUI.BeginDisabledGroup(attributeIndex < 1);

            contentRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            EditorGUI.BeginChangeCheck();
            var minFilterProp = element.FindPropertyRelative("minFilter");
            var maxFilterProp = element.FindPropertyRelative("maxFilter");
            DrawMinMaxSlider(contentRect, minFilterProp, maxFilterProp, attributeProp.stringValue, dataSource);
            if (EditorGUI.EndChangeCheck())
            {
                dirtyFlag = AbstractVisualisation.PropertyType.DimensionFiltering;
            }

            contentRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            EditorGUI.BeginChangeCheck();

            var minScaleProp = element.FindPropertyRelative("minScale");
            var maxScaleProp = element.FindPropertyRelative("maxScale");
            DrawMinMaxSlider(contentRect, minScaleProp, maxScaleProp, attributeProp.stringValue, dataSource);

            if (EditorGUI.EndChangeCheck())
            {
                dirtyFlag = AbstractVisualisation.PropertyType.Scaling;
            }

            EditorGUI.EndDisabledGroup();

            return dirtyFlag;
        }

        private UnityEditorInternal.ReorderableList dimensionsListParallelCoordinates;

        private UnityEditorInternal.ReorderableList scatterplotMatrixListX;
        private UnityEditorInternal.ReorderableList scatterplotMatrixListY;
        private UnityEditorInternal.ReorderableList scatterplotMatrixListZ;

        void OnEnable()
        {
            uidProperty = serializedObject.FindProperty("uid");
            dataSourceProperty = serializedObject.FindProperty("dataSource");
            geometryProperty = serializedObject.FindProperty("geometry");
            colourProperty = serializedObject.FindProperty("colour");
            sizeProperty = serializedObject.FindProperty("size");
            xDimensionProperty = serializedObject.FindProperty("xDimension");
            yDimensionProperty = serializedObject.FindProperty("yDimension");
            zDimensionProperty = serializedObject.FindProperty("zDimension");
            colourDimensionProperty = serializedObject.FindProperty("colourDimension");
            colourPaletteProperty = serializedObject.FindProperty("coloursPalette");
            colourGradientProperty = serializedObject.FindProperty("dimensionColour");
            blendingModeSourceProperty = serializedObject.FindProperty("blendingModeSource");
            blendingModeDestinationProperty = serializedObject.FindProperty("blendingModeDestination");
            sizeDimensionProperty = serializedObject.FindProperty("sizeDimension");
            minSizeProperty = serializedObject.FindProperty("minSize");
            maxSizeProperty = serializedObject.FindProperty("maxSize");
            linkingDimensionProperty = serializedObject.FindProperty("linkingDimension");
            originDimensionProperty = serializedObject.FindProperty("originDimension");
            destinationDimensionProperty = serializedObject.FindProperty("destinationDimension");
            graphDimensionProperty = serializedObject.FindProperty("graphDimension");

            visualisationTypeProperty = serializedObject.FindProperty("visualisationType");

            attributeFiltersProperty = serializedObject.FindProperty("attributeFilters");
            colorPaletteDimensionProperty = serializedObject.FindProperty("colorPaletteDimension");

            widthProperty = serializedObject.FindProperty("width");
            heightProperty = serializedObject.FindProperty("height");
            depthProperty = serializedObject.FindProperty("depth");

            xScatterplotMatrixDimensionsProperty = serializedObject.FindProperty("xScatterplotMatrixDimensions");
            yScatterplotMatrixDimensionsProperty = serializedObject.FindProperty("yScatterplotMatrixDimensions");
            zScatterplotMatrixDimensionsProperty = serializedObject.FindProperty("zScatterplotMatrixDimensions");

            parallelCoordinatesDimensionsProperty = serializedObject.FindProperty("parallelCoordinatesDimensions");

            scatterplotMatrixListX = new UnityEditorInternal.ReorderableList(
                serializedObject,
                xScatterplotMatrixDimensionsProperty,
                true, true, true, true);

            scatterplotMatrixListY = new UnityEditorInternal.ReorderableList(
                serializedObject,
                yScatterplotMatrixDimensionsProperty,
                true, true, true, true);

            scatterplotMatrixListZ = new UnityEditorInternal.ReorderableList(
                serializedObject,
                zScatterplotMatrixDimensionsProperty,
                true, true, true, true);

            dimensionsListParallelCoordinates = new UnityEditorInternal.ReorderableList(
                serializedObject,
                parallelCoordinatesDimensionsProperty,
                true, true, true, true
            );

            scatterplotMatrixListX.onAddCallback = (UnityEditorInternal.ReorderableList list) =>
            {
                Visualisation targetVisualisation = (Visualisation)serializedObject.targetObject;
                Array.Resize(ref targetVisualisation.xScatterplotMatrixDimensions, targetVisualisation.xScatterplotMatrixDimensions.Length + 1);
                EditorUtility.SetDirty(serializedObject.targetObject);
            };

            scatterplotMatrixListY.onAddCallback = (UnityEditorInternal.ReorderableList list) =>
            {
                Visualisation targetVisualisation = (Visualisation)serializedObject.targetObject;
                Array.Resize(ref targetVisualisation.yScatterplotMatrixDimensions, targetVisualisation.yScatterplotMatrixDimensions.Length + 1);
                EditorUtility.SetDirty(serializedObject.targetObject);
            };

            scatterplotMatrixListZ.onAddCallback = (UnityEditorInternal.ReorderableList list) =>
            {
                Visualisation targetVisualisation = (Visualisation)serializedObject.targetObject;
                Array.Resize(ref targetVisualisation.zScatterplotMatrixDimensions, targetVisualisation.zScatterplotMatrixDimensions.Length + 1);
                EditorUtility.SetDirty(serializedObject.targetObject);
            };

            dimensionsListParallelCoordinates.onAddCallback = (UnityEditorInternal.ReorderableList list) =>
            {
                Visualisation targetVisualisation = (Visualisation)serializedObject.targetObject;
                Array.Resize(ref targetVisualisation.parallelCoordinatesDimensions, targetVisualisation.parallelCoordinatesDimensions.Length + 1);
                EditorUtility.SetDirty(serializedObject.targetObject);
            };

            dimensionsListParallelCoordinates.drawElementCallback = drawDimensionFilterElementParalleCoordinates;
            scatterplotMatrixListX.drawElementCallback = drawDimensionFilterElementscatterplotMatrixListX;
            scatterplotMatrixListY.drawElementCallback = drawDimensionFilterElementscatterplotMatrixListY;
            scatterplotMatrixListZ.drawElementCallback = drawDimensionFilterElementscatterplotMatrixListZ;

            dimensionsListParallelCoordinates.elementHeight = EditorGUIUtility.singleLineHeight * 4f;
            scatterplotMatrixListX.elementHeight = EditorGUIUtility.singleLineHeight * 4f;
            scatterplotMatrixListY.elementHeight = EditorGUIUtility.singleLineHeight * 4f;
            scatterplotMatrixListZ.elementHeight = EditorGUIUtility.singleLineHeight * 4f;

            dimensionsListParallelCoordinates.drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(rect, "Parallel Coordinate Plot Dimensions");
            };

            scatterplotMatrixListX.drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(rect, "X Scatterplot Matrix dimensions");
            };
            scatterplotMatrixListY.drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(rect, "Y Scatterplot Matrix dimensions");
            };
            scatterplotMatrixListZ.drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(rect, "Z Scatterplot Matrix dimensions");
            };

            if (dataSourceProperty != null)
            {
                dataSource = (DataSource)dataSourceProperty.objectReferenceValue;
                if (dataSource != null)
                {
                    if (dataSource.IsLoaded)
                    {
                        loadData();
                    }
                    else
                    {
                        dataSource.onLoad += OnDataSourceLoad;
                        if (!dataSource.IsLoaded)
                            dataSource.load();
                    }
                }
            }
        }

        void drawDimensionFilterElements(UnityEditor.SerializedProperty element, UnityEngine.Rect rect, int index, bool isActive, bool isFocused)
        {
            var labelRect = new Rect(rect.x, rect.y, 50, rect.height);
            var contentRect = new Rect(rect.x + labelRect.width, rect.y, rect.width - labelRect.width, rect.height);

            var dirtyFlag = DrawAttributeFilterEditor(element, contentRect);
            if (dirtyFlag != AbstractVisualisation.PropertyType.None)
            {
                _globalDirtyFlags = dirtyFlag;
            }
        }

        void drawDimensionFilterElementParalleCoordinates(Rect rect, int index, bool isActive, bool isFocused)
        {
            drawDimensionFilterElements(dimensionsListParallelCoordinates.serializedProperty.GetArrayElementAtIndex(index),
                rect,
                index,
                isActive,
                isFocused);
        }

        void drawDimensionFilterElementscatterplotMatrixListX(Rect rect, int index, bool isActive, bool isFocused)
        {
            drawDimensionFilterElements(scatterplotMatrixListX.serializedProperty.GetArrayElementAtIndex(index),
                rect,
                index,
                isActive,
                isFocused);
        }

        void drawDimensionFilterElementscatterplotMatrixListY(Rect rect, int index, bool isActive, bool isFocused)
        {
            drawDimensionFilterElements(scatterplotMatrixListY.serializedProperty.GetArrayElementAtIndex(index),
                rect,
                index,
                isActive,
                isFocused);
        }

        void drawDimensionFilterElementscatterplotMatrixListZ(Rect rect, int index, bool isActive, bool isFocused)
        {
            drawDimensionFilterElements(scatterplotMatrixListZ.serializedProperty.GetArrayElementAtIndex(index),
                rect,
                index,
                isActive,
                isFocused);
        }

        private void OnDataSourceLoad()
        {
            loadData();
            Repaint();
        }

        AbstractVisualisation.PropertyType _globalDirtyFlags = AbstractVisualisation.PropertyType.None;

        /// <summary>
        /// Draw the inspector and update Visualisation when a property changes
        /// </summary>
        public override void OnInspectorGUI()
        {
            AbstractVisualisation.PropertyType? dirtyFlags = null;
            Visualisation targetVisualisation = (Visualisation)serializedObject.targetObject;

            serializedObject.Update();

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(uidProperty);
            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(dataSourceProperty);

            if (EditorGUI.EndChangeCheck())
            {
                loadData();

                targetVisualisation.xScatterplotMatrixDimensions = dimensions.Select(x => new DimensionFilter { Attribute = x }).ToArray();
                targetVisualisation.yScatterplotMatrixDimensions = dimensions.Select(x => new DimensionFilter { Attribute = x }).ToArray();
                targetVisualisation.zScatterplotMatrixDimensions = dimensions.Select(x => new DimensionFilter { Attribute = x }).ToArray();
                targetVisualisation.parallelCoordinatesDimensions = dimensions.Select(x => new DimensionFilter { Attribute = x }).ToArray();
            }

            if (dataSourceProperty.objectReferenceValue != null)
            {
                //Check if changing the visualisation type
                EditorGUI.BeginChangeCheck();

                EditorGUILayout.PropertyField(visualisationTypeProperty);

                if (EditorGUI.EndChangeCheck())
                {
                    dirtyFlags = AbstractVisualisation.PropertyType.VisualisationType;
                }

                //int visType = visualisationTypeProperty.intValue;
                //EnumPopup("Visualisation Type", Enum.GetNames(typeof(AbstractViualisation.VisualisationTypes)), ref visType);
                //visualisationTypeProperty.intValue = visType;

                EditorGUI.indentLevel++;

                //                switch ((AbstractViualisation.VisualisationTypes)visualisationTypeProperty.intValue)
                switch (targetVisualisation.visualisationType)
                {
                    case AbstractVisualisation.VisualisationTypes.SCATTERPLOT:
                        ShowSimpleVisualisationMenu(ref dirtyFlags);
                        break;
                    case AbstractVisualisation.VisualisationTypes.SCATTERPLOT_MATRIX:
                        ShowScatterplotMatrixMenu(ref dirtyFlags);
                        break;
                    case AbstractVisualisation.VisualisationTypes.PARALLEL_COORDINATES:
                        ShowParallelCoordinatesMenu(ref dirtyFlags);
                        break;
                    case AbstractVisualisation.VisualisationTypes.GRAPH_LAYOUT:
                        break;
                    default:
                        break;
                }
                EditorGUI.indentLevel--;
                EditorGUILayout.Space();

            }

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.LabelField("Aesthetics", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(geometryProperty);

            if (EditorGUI.EndChangeCheck())
            {
                dirtyFlags = AbstractVisualisation.PropertyType.GeometryType;
            }

            if (EnumPopup("Colour dimension", dimensions.ToArray(), colourDimensionProperty))
            {
                dirtyFlags = AbstractVisualisation.PropertyType.Colour;
                colorPaletteDimensionProperty.stringValue = "Undefined";
                colourPaletteProperty.ClearArray();
                colourPaletteProperty.arraySize = 0;
            }

            if (EnumPopup("Bind Colour palette", dimensions.ToArray(), colorPaletteDimensionProperty))
            {
                if (colorPaletteDimensionProperty.stringValue != "Undefined")
                {
                    int nbPaletteCategories = dataSource.getNumberOfCategories(colorPaletteDimensionProperty.stringValue);
                    //float[] uniqueValues = dataSource[colorPaletteDimensionProperty.stringValue].MetaData.categories;
                    
                    colourPaletteProperty.ClearArray();
                    colourPaletteProperty.arraySize = nbPaletteCategories;
                    colourDimensionProperty.stringValue = "Undefined";
                }
            }

            if (colorPaletteDimensionProperty.stringValue != "Undefined" && colorPaletteDimensionProperty.stringValue != "")
            {
                EditorGUI.BeginChangeCheck();

                float[] paletteValues = dataSource[colorPaletteDimensionProperty.stringValue].MetaData.categories;
                float[] ordererdPalette = paletteValues.OrderBy(x => x).ToArray();
                List<int> sortedId = new List<int>();

                //not optimal...
                // adding an inderiction pointer indices to reorder values on the GUI -- not sorted in database
                for (int idOri = 0; idOri < ordererdPalette.Length; idOri++)
                {
                    //what's the id of the non sorted?
                    float sortEl = ordererdPalette[idOri];
                    int _sortedId = paletteValues.ToList().IndexOf(sortEl);
                    sortedId.Add(_sortedId);
                }

                int nbPaletteCategories = paletteValues.Length;

                EditorGUI.indentLevel += 1;
                for (int i = 0; i < nbPaletteCategories; i++)
                {
                    EditorGUILayout.PropertyField(
                        colourPaletteProperty.GetArrayElementAtIndex(sortedId[i]),
                        new GUIContent(dataSource.getOriginalValue(paletteValues[sortedId[i]], colorPaletteDimensionProperty.stringValue).ToString())
                    );
                }
                EditorGUI.indentLevel -= 1;

                if (EditorGUI.EndChangeCheck())
                {
                    dirtyFlags = AbstractVisualisation.PropertyType.Colour;
                }
            }

            if (EnumPopup("Blending Mode Source", Enum.GetNames(typeof(UnityEngine.Rendering.BlendMode)), blendingModeSourceProperty))
            {
                dirtyFlags = AbstractVisualisation.PropertyType.BlendSourceMode;
            }

            if (EnumPopup("Blending Mode Destination", Enum.GetNames(typeof(UnityEngine.Rendering.BlendMode)), blendingModeDestinationProperty))
            {

                dirtyFlags = AbstractVisualisation.PropertyType.BlendDestinationMode;
            }

            if (colourDimensionProperty.stringValue != "" && colourDimensionProperty.stringValue != "Undefined")
            {
                EditorGUI.BeginChangeCheck();

                EditorGUILayout.PropertyField(colourGradientProperty);

                if (EditorGUI.EndChangeCheck())
                {
                    dirtyFlags = AbstractVisualisation.PropertyType.Colour;
                }
            }
            else
            {
                EditorGUI.BeginChangeCheck();

                EditorGUILayout.PropertyField(colourProperty);

                if (EditorGUI.EndChangeCheck())
                {
                    dirtyFlags = AbstractVisualisation.PropertyType.Colour;
                }
            }

            
            if (EnumPopup("Size dimension", dimensions.ToArray(), sizeDimensionProperty))
            {
                dirtyFlags = AbstractVisualisation.PropertyType.Size;
            }

            // Size
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(sizeProperty);
            EditorGUILayout.PropertyField(minSizeProperty);
            EditorGUILayout.PropertyField(maxSizeProperty);

            if (EditorGUI.EndChangeCheck())
            {
                dirtyFlags = AbstractVisualisation.PropertyType.SizeValues;
            }

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.LabelField("Connectivity", EditorStyles.boldLabel);
            
            if (EnumPopup("Linking dimension", dimensions.ToArray(), linkingDimensionProperty))
            {
                dirtyFlags = AbstractVisualisation.PropertyType.LinkingDimension;
            }

            if (EnumPopup("Origin dimension", dimensions.ToArray(), originDimensionProperty))
            {
                dirtyFlags = AbstractVisualisation.PropertyType.OriginDimension;
            }

            if (EnumPopup("Destination dimension", dimensions.ToArray(), destinationDimensionProperty))
            {
                dirtyFlags = AbstractVisualisation.PropertyType.DestinationDimension;
            }

            if (EnumPopup("Graph dimension", dimensions.ToArray(), graphDimensionProperty))
            {
                dirtyFlags = AbstractVisualisation.PropertyType.GraphDimension;
            }

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(attributeFiltersProperty, true);
            if (EditorGUI.EndChangeCheck())
            {
                dirtyFlags = AbstractVisualisation.PropertyType.AttributeFiltering;
            }
           

            // Visualisation dimensions
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(widthProperty);
            EditorGUILayout.PropertyField(heightProperty);
            EditorGUILayout.PropertyField(depthProperty);

            if (EditorGUI.EndChangeCheck())
            {
                dirtyFlags = AbstractVisualisation.PropertyType.Scaling;
            }

            // Update the options for this visualisation
            serializedObject.ApplyModifiedProperties();

            if (dirtyFlags == AbstractVisualisation.PropertyType.VisualisationType)
                targetVisualisation.CreateVisualisation((AbstractVisualisation.VisualisationTypes)visualisationTypeProperty.intValue);
            else
                if (dirtyFlags != null)
                {
                    targetVisualisation.updateViewProperties(dirtyFlags.Value);
                }
        }

        void ShowScatterplotMatrixMenu(ref AbstractVisualisation.PropertyType? dirtyFlags)
        {
            EditorGUI.BeginChangeCheck();

            scatterplotMatrixListX.DoLayoutList();
            if (EditorGUI.EndChangeCheck())
            {
                dirtyFlags = _globalDirtyFlags;
            }
            EditorGUI.BeginChangeCheck();

            scatterplotMatrixListY.DoLayoutList();
            if (EditorGUI.EndChangeCheck())
            {
                dirtyFlags = _globalDirtyFlags;
            }

            EditorGUI.BeginChangeCheck();

            scatterplotMatrixListZ.DoLayoutList();

            if (EditorGUI.EndChangeCheck())
            {
                dirtyFlags = _globalDirtyFlags;
            }
        }

        void ShowParallelCoordinatesMenu(ref AbstractVisualisation.PropertyType? dirtyFlags)
        {
            EditorGUI.BeginChangeCheck();
            dimensionsListParallelCoordinates.DoLayoutList();
            if (EditorGUI.EndChangeCheck())
            {
                dirtyFlags = _globalDirtyFlags;
            }
        }

        /// <summary>
        /// shows the menu for simple visualisations
        /// </summary>
        void ShowSimpleVisualisationMenu(ref AbstractVisualisation.PropertyType? dirtyFlags)
        {
            var rect = EditorGUILayout.GetControlRect(true, 50);

            var copyrect = rect;
            copyrect.height -= rect.height;

            EditorGUILayout.PrefixLabel(new GUIContent("X_AXIS"));

            var dirty = DrawAttributeFilterEditor(xDimensionProperty, rect);
            if (dirty == AbstractVisualisation.PropertyType.DimensionChange)
            {
                dirtyFlags = AbstractVisualisation.PropertyType.X;
            }
            else if (dirty != AbstractVisualisation.PropertyType.None)
            {
                dirtyFlags = dirty;
            }

            rect = EditorGUILayout.GetControlRect(true, 50);
            dirty = DrawAttributeFilterEditor(yDimensionProperty, rect);

            EditorGUILayout.PrefixLabel(new GUIContent("Y_AXIS"));


            if (dirty == AbstractVisualisation.PropertyType.DimensionChange)
            {
                dirtyFlags = AbstractVisualisation.PropertyType.Y;
            }
            else if (dirty != AbstractVisualisation.PropertyType.None)
            {
                dirtyFlags = dirty;
            }

            rect = EditorGUILayout.GetControlRect(true, 50);
            dirty = DrawAttributeFilterEditor(zDimensionProperty, rect);

            EditorGUILayout.PrefixLabel(new GUIContent("Z_AXIS"));


            if (dirty == AbstractVisualisation.PropertyType.DimensionChange)
            {
                dirtyFlags = AbstractVisualisation.PropertyType.Z;
            }
            else if (dirty != AbstractVisualisation.PropertyType.None)
            {
                dirtyFlags = dirty;
            }
        }

        /// <summary>
        /// Enums the popup.
        /// </summary>
        /// <param name="label">Label.</param>
        /// <param name="enumArray">Enum array.</param>
        /// <param name="selected">Selected.</param>
        private bool EnumPopup(string label, string[] enumArray, SerializedProperty selected)
        {
            string oldSelected = selected.stringValue;

            if (enumArray.Length > 0)
            {
                selected.stringValue = enumArray[EditorGUILayout.Popup(label, enumIndexOf(enumArray, selected.stringValue), enumArray)];
            }

            return selected.stringValue != oldSelected;
        }

        /// <summary>
        /// Enums the popup.
        /// </summary>
        /// <param name="label">Label.</param>
        /// <param name="enumArray">Enum array.</param>
        /// <param name="selected">Selected.</param>
        private bool EnumPopup(string label, string[] enumArray, DimensionFilter selected)
        {
            string oldSelected = selected.Attribute;

            if (enumArray.Length > 0)
            {
                selected.Attribute = enumArray[EditorGUILayout.Popup(label, enumIndexOf(enumArray, selected.Attribute), enumArray)];
            }

            return selected.Attribute != oldSelected;
        }

        /// <summary>
        /// Enums the popup.
        /// </summary>
        /// <returns><c>true</c>, if popup was enumed, <c>false</c> otherwise.</returns>
        /// <param name="label">Label.</param>
        /// <param name="enumArray">Enum array.</param>
        /// <param name="selected">Selected.</param>
        private bool EnumPopup(string label, string[] enumArray, ref string selected)
        {
            string oldSelected = selected;

            if (enumArray.Length > 0)
            {
                selected = enumArray[EditorGUILayout.Popup(label, enumIndexOf(enumArray, selected), enumArray)];
            }

            return selected != oldSelected;
        }

        /// <summary>
        /// Enums the popup.
        /// </summary>
        private bool EnumPopup(string label, string[] enumArray, ref int selected)
        {
            int oldSelected = selected;

            if (enumArray.Length > 0)
            {
                selected = EditorGUILayout.Popup(label, selected, enumArray);
            }

            return selected != oldSelected;
        }

        /// <summary>
        /// Find the enum index of an array
        /// </summary>
        /// <returns>The index of.</returns>
        /// <param name="stringArray">String array.</param>
        /// <param name="toFind">To find.</param>
        private int enumIndexOf(string[] stringArray, string toFind)
        {
            int index = stringArray.ToList().IndexOf(toFind);

            return (index >= 0) ? index : 0;        // Return the "Undefined"
        }


        /// <summary>
        /// Extracts the name of the AbstractViuslisation. Format "Name 1" where 1 in this case is the version number
        /// </summary>
        /// <returns>The visualisation name.</returns>
        /// <param name="visualisationEnumString">Visualisation enum string.</param>
        private string extractVisualisationName(string visualisationEnumString)
        {
            int spaceIndex = visualisationEnumString.LastIndexOf(" ");

            return (spaceIndex >= 0) ? visualisationEnumString.Substring(0, visualisationEnumString.LastIndexOf(" ")) : visualisationEnumString;
        }

        /// <summary>
        /// Extracts the version of the AbstractViuslisation.
        /// </summary>
        /// <returns>The visualisation name.</returns>
        /// <param name="visualisationEnumString">Visualisation enum string.</param>
        private int extractVisualisationVersion(string visualisationEnumString)
        {
            int spaceIndex = visualisationEnumString.LastIndexOf(" ");

            return (spaceIndex >= 0) ? int.Parse(visualisationEnumString.Substring(visualisationEnumString.LastIndexOf(" ") + 1)) : 0;
        }

        /// <summary>
        /// Loads the data.
        /// </summary>
        private void loadData()
        {
            if (dataSourceProperty != null)
            {
                dataSource = (DataSource)dataSourceProperty.objectReferenceValue;
                if (dataSource != null)
                {
                    dimensions.Clear();
                    dimensions.Add(undefinedString);
                    for (int i = 0; i < dataSource.DimensionCount; ++i)
                    {
                        dimensions.Add(dataSource[i].Identifier);
                    }

                }
            }
        }

        /// <summary>
        /// Constructs the visualisation enum.
        /// </summary>
        /// <returns>The visualisation enum.</returns>
        /// <param name="name">Name.</param>
        /// <param name="version">Version.</param>
        private string constructVisualisationEnum(string name, int version)
        {
            return (name != undefinedString) ? name + " " + version.ToString() : name;
        }

        [MenuItem("GameObject/IATK/CSV Data Source", false, 10)]
        static void CreateCSVDataSourcePrefab()
        {
            GameObject obj = new GameObject("[IATK] New Data Source");
            obj.AddComponent<CSVDataSource>();
            Selection.activeGameObject = obj;
        }

        [MenuItem("GameObject/IATK/Visualisation", false, 10)]
        static void CreateVisualisationPrefab()
        {
            GameObject obj = new GameObject("[IATK] New Visualisation");
            obj.AddComponent<Visualisation>();
            Selection.activeGameObject = obj;
        }

        [MenuItem("GameObject/IATK/LinkedVisualisation", false, 10)]
        static void CreateLinkedVisualisationPrefab()
        {
            GameObject obj = new GameObject("[IATK] New Linked Visualisation");
            obj.AddComponent<LinkingVisualisations>();
            Selection.activeGameObject = obj;
        }

        [MenuItem("GameObject/IATK/BrushingAndLinking", false, 10)]
        static void CreateBrushingAndLinkingPrefab()
        {
            GameObject obj = (GameObject)Instantiate(Resources.Load("BrushingAndLinking"));// new GameObject("[IATK] New Brushing And Linking");           
            Selection.activeGameObject = obj;
        }
    }

    [CustomPropertyDrawer(typeof(DimensionFilter))]
    public class DimensionFilterDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.serializedObject.isEditingMultipleObjects) return 0f;
            return base.GetPropertyHeight(property, label) + 32f;
        }

        // Draw the property inside the given rect
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Using BeginProperty / EndProperty on the parent property means that
            // prefab override logic works on the entire property.
            EditorGUI.BeginProperty(position, label, property);

            // Draw label
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            var dimensionRect = new Rect(position.x, position.y, position.width, 16f);

            string[] options = new string[] { "test1", "test2", "test3" };
            EditorGUI.Popup(dimensionRect, 0, options);

            dimensionRect.y += 16;
            position.y += 16;
            float min = property.FindPropertyRelative("minFilter").floatValue;
            float max = property.FindPropertyRelative("maxFilter").floatValue;
            EditorGUI.MinMaxSlider(dimensionRect, GUIContent.none, ref min, ref max, 0, 1);
            property.FindPropertyRelative("minFilter").floatValue = min;
            property.FindPropertyRelative("maxFilter").floatValue = max;

            dimensionRect.y += 16;
            min = property.FindPropertyRelative("minScale").floatValue;
            max = property.FindPropertyRelative("maxScale").floatValue;
            EditorGUI.MinMaxSlider(dimensionRect, GUIContent.none, ref min, ref max, 0, 1);
            property.FindPropertyRelative("minScale").floatValue = min;
            property.FindPropertyRelative("maxScale").floatValue = max;

            EditorGUI.EndProperty();

        }
    }

}   // Namespace
