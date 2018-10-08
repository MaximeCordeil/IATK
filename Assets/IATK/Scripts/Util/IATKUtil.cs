using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IATK
{

    public class IATKUtil
    {

        /// <summary>
        /// returns a View with the specific geometry configuration
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static Material GetMaterialFromTopology(AbstractVisualisation.GeometryType configuration)
        {
            Material mt = null;

            switch (configuration)
            {
                case AbstractVisualisation.GeometryType.Undefined:
                    return null;

                case AbstractVisualisation.GeometryType.Points:
                    mt = new Material(Shader.Find("IATK/OutlineDots"));
                    mt.mainTexture = Resources.Load("circle-outline-basic") as Texture2D;
                    mt.renderQueue = 3000;
                    return mt;
                case AbstractVisualisation.GeometryType.Lines:                   
                    mt = new Material(Shader.Find("IATK/LinesShader"));
                    mt.renderQueue = 3000;
                    return mt;
                case AbstractVisualisation.GeometryType.Quads:                   
                    mt = new Material(Shader.Find("IATK/Quads"));
                    mt.renderQueue = 3000;
                    return mt;
                case AbstractVisualisation.GeometryType.LinesAndDots:
                    mt = new Material(Shader.Find("IATK/LineAndDotsShader"));
                    mt.renderQueue = 3000;
                    return mt;
                case AbstractVisualisation.GeometryType.Cubes:
                    mt = new Material(Shader.Find("IATK/CubeShader"));
                    mt.renderQueue = 3000;
                    return mt;
                case AbstractVisualisation.GeometryType.Bars:
                    mt = new Material(Shader.Find("IATK/BarShader"));
                    mt.renderQueue = 3000;
                    return mt;
                case AbstractVisualisation.GeometryType.Spheres:
                    mt = new Material(Shader.Find("IATK/SphereShader"));
                    mt.mainTexture = Resources.Load("sphere-texture") as Texture2D;
                    mt.renderQueue = 3000;
                    return mt;
                default:
                    return null;
            }
        }

    }
}
