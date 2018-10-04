using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace IATK
{

    public class VRMenuInteractor : MonoBehaviour
    {

        public Visualisation visualisation;
        public Slider slider;
        public Dropdown X_AxisDropDown;
        public Dropdown Y_AxisDropDown;
        public Dropdown Z_AxisDropDown;
        public Dropdown SizeAttributeDropDown;
        public Dropdown FilterAttributeDropDown;
        public Slider FilterMinSlider;
        public Slider FilterMaxSlider;


        //holds the string names of the data attributes
        private List<string> DataAttributesNames;

        // Use this for initialization
        void Start()
        {
            DataAttributesNames = GetAttributesList();
            X_AxisDropDown.AddOptions(DataAttributesNames);
            Y_AxisDropDown.AddOptions(DataAttributesNames);
            Z_AxisDropDown.AddOptions(DataAttributesNames);

            X_AxisDropDown.value = DataAttributesNames.IndexOf(visualisation.xDimension.Attribute);
            Y_AxisDropDown.value = DataAttributesNames.IndexOf(visualisation.yDimension.Attribute);
            Z_AxisDropDown.value = DataAttributesNames.IndexOf(visualisation.zDimension.Attribute);

            SizeAttributeDropDown.AddOptions(DataAttributesNames);
            FilterAttributeDropDown.AddOptions(DataAttributesNames);

            SizeAttributeDropDown.value = DataAttributesNames.IndexOf(visualisation.sizeDimension);

            if(visualisation.attributeFilters.Length>0)
            FilterAttributeDropDown.value = DataAttributesNames.IndexOf(visualisation.attributeFilters[0].Attribute);
        }

        // Update is called once per frame
        void Update()
        {

        }

        private List<string> GetAttributesList()
        {
            List<string> dimensions = new List<string>();
            dimensions.Add("Undefined");
            for (int i = 0; i < visualisation.dataSource.DimensionCount; ++i)
            {
                dimensions.Add(visualisation.dataSource[i].Identifier);
            }
            return dimensions;
        }

        public void ValidateX_AxisDropdown()
        {
            if (visualisation != null)
            {
                visualisation.xDimension = DataAttributesNames[X_AxisDropDown.value];
                visualisation.updateViewProperties(AbstractVisualisation.PropertyType.X);
            }
        }

        public void ValidateY_AxisDropdown()
        {
            if (visualisation != null)
            {
                visualisation.yDimension = DataAttributesNames[Y_AxisDropDown.value];
                visualisation.updateViewProperties(AbstractVisualisation.PropertyType.Y);

            }
        }

        public void ValidateZ_AxisDropdown()
        {
            if (visualisation != null)
            {
                visualisation.zDimension = DataAttributesNames[Z_AxisDropDown.value];
                visualisation.updateViewProperties(AbstractVisualisation.PropertyType.Z);

            }
        }

        public void ValidateSizeChangeSlider()
        {
            if (visualisation != null)
            {
                visualisation.size = slider.value;
                visualisation.updateViewProperties(AbstractVisualisation.PropertyType.SizeValues);
            }
        }

        public void ValidateAttributeSizeDropDown()
        {
            if (visualisation != null)
            {
                visualisation.sizeDimension = DataAttributesNames[SizeAttributeDropDown.value];
                visualisation.updateViewProperties(AbstractVisualisation.PropertyType.Size);
            }
        }

        public void ValidateAttributeFilteringDropDown()
        {
            if (visualisation != null)
            {
                AttributeFilter af = new AttributeFilter();
                af.Attribute = DataAttributesNames[FilterAttributeDropDown.value];
                visualisation.attributeFilters[0] = af;
                visualisation.updateViewProperties(AbstractVisualisation.PropertyType.AttributeFiltering);
            }
        }

        public void ValidateAttributeFilteringSliderMin()
        {
            if (visualisation != null && visualisation.attributeFilters.Length>0)
            {
                visualisation.attributeFilters[0].minFilter = FilterMinSlider.value;
                visualisation.updateViewProperties(AbstractVisualisation.PropertyType.AttributeFiltering);

            }
        }

        public void ValidateAttributeFilteringSliderMax()
        {
            if (visualisation != null && visualisation.attributeFilters.Length > 0)
            {
                visualisation.attributeFilters[0].maxFilter = FilterMaxSlider.value;
                visualisation.updateViewProperties(AbstractVisualisation.PropertyType.AttributeFiltering);

            }
        }
    }
}
