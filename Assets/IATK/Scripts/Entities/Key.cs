using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
namespace IATK
{
    public class Key : MonoBehaviour
    {

        public LineRenderer gradientColorLineRenderer;
        public TextMeshPro Legend;

        private string legend="";


        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void UpdateProperties(AbstractVisualisation.PropertyType propertyType, Visualisation v)
        {
            legend = "";

            if (v.colourDimension != "Undefined")
            {
                gradientColorLineRenderer.gameObject.SetActive(true);
                legend += "Colour: " + v.colourDimension + "\n";
                SetGradientColor(v.dimensionColour);
            }

            if (v.sizeDimension != "Undefined")
                legend += "Size: " + v.sizeDimension + "\n";
            if (v.linkingDimension != "Undefined")
                legend += "Linking Attribute: " + v.linkingDimension + "\n";

            if (v.colorPaletteDimension != "Undefined")
            {
                gradientColorLineRenderer.gameObject.SetActive(false);
                legend += "Colour: " + v.colorPaletteDimension + "\n";
                float[] uniqueValues = v.dataSource[v.colorPaletteDimension].MetaData.categories;
                string[] stringValues = new string[uniqueValues.Length];

                for (int i = 0; i < uniqueValues.Length; i++)
                    stringValues[i] = v.dataSource.getOriginalValue(uniqueValues[i], v.colorPaletteDimension).ToString();

                for (int i = 0; i < v.coloursPalette.Length; i++)
                {
                    legend += "<color=#" + ColorUtility.ToHtmlStringRGB(v.coloursPalette[i]) + "> *** </color>" + stringValues[i] + "\n";
                }
            }
            Legend.text = legend;
        }

        /// <summary>
        /// Binds a Gradient color object to the line renderer
        /// </summary>
        /// <param name="gradient"></param>
        void SetGradientColor(Gradient gradient)
        {
            Vector3[] vertices = new Vector3[gradient.colorKeys.Length];
            GradientColorKey[] colorKeys = gradient.colorKeys;
            for (int i = 0; i < vertices.Length; i++)
                vertices[i] = new Vector3(colorKeys[i].time, 0f, 0f);

            gradientColorLineRenderer.positionCount = vertices.Length;
            gradientColorLineRenderer.SetPositions(vertices);
            gradientColorLineRenderer.colorGradient = gradient;
        }

    }
}