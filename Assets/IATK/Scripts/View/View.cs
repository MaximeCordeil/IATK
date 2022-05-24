using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace IATK
{

    /// <summary>
    /// View class
    /// </summary>
    [ExecuteInEditMode]
    public class View : MonoBehaviour 
    {
        // EVENTS  
        public delegate void OnViewChangeDelegate(AbstractVisualisation.PropertyType propertyType);

        //bigmesh object
        BigMesh bigMesh;
        
        public BigMesh BigMesh
        {
            get
            {
                return bigMesh;
            }

            set
            {
                bigMesh = value;
            }
        }

        void Start()
        {

        }


        /// <summary>
        /// hides or show the view
        /// </summary>
        /// <param name="show"></param>
        public void Show(bool show)
        {
            bigMesh.gameObject.SetActive(show);
        }

        /// <summary>
        /// Maps a data array to the size of the glyphs
        /// </summary>
        /// <param name="vertexIdData"></param>
        public void SetVertexIdChannel(float[] vertexIdData)
        {
            bigMesh.MapUVChannel(0, (int)AbstractVisualisation.NormalChannel.VertexId, vertexIdData);
        }

        /// <summary>
        /// Maps a data array to the size of the glyphs
        /// </summary>
        /// <param name="sizeData"></param>
        public void SetSizeChannel(float[] sizeData)
        {
            bigMesh.MapUVChannel(0, (int)AbstractVisualisation.NormalChannel.Size, sizeData);
        }

        /// <summary>
        /// Sets the filter chanel
        /// </summary>
        /// <param name="filteredData"></param>
        public void SetFilterChannel(float[] filteredData)
        {
            bigMesh.MapUVChannel(0, (int)AbstractVisualisation.NormalChannel.Filter, filteredData);
        }

        /// <summary>
        /// Gets the filtered data
        /// </summary>
        /// <returns></returns>
        public float[] GetFilterChannel()
        {
            return bigMesh.GetUVs(0).Select(v => v.z).ToArray();
        }

        /// <summary>
        /// Sets all the channels at once
        /// </summary>
        /// <param name="channels"></param>
        public void SetAllChannels(Vector3[] channels)
        {
            bigMesh.updateBigMeshNormals(channels);
        }

        /// <summary>
        /// Updates colors of the view
        /// </summary>
        /// <param name="colors"></param>
        public void SetColors(Color[] colors)
        {
            bigMesh.updateBigMeshColours(colors);
        }

        /// <summary>
        /// Gets colors of the view
        /// </summary>
        /// <returns></returns>
        public Color[] GetColors()
        {
            return bigMesh.getColors();
        }
      
        /// <summary>
        /// Sets the size of the glyph
        /// </summary>
        /// <param name="size"></param>
        public void SetSize(float size)
        {
            bigMesh.SharedMaterial.SetFloat("_Size", size);
        }

        /// <summary>
        /// Sets the min size of the glyph
        /// </summary>
        /// <param name="minsize"></param>
        public void SetMinSize(float minsize)
        {
            bigMesh.SharedMaterial.SetFloat("_MinSize", minsize);
        }

        /// <summary>
        /// Sets the max size of the glyph
        /// </summary>
        /// <param name="maxsize"></param>
        public void SetMaxSize(float maxsize)
        {
            bigMesh.SharedMaterial.SetFloat("_MaxSize", maxsize);
        }

        /// <summary>
        /// Sets the min normalised x value
        /// </summary>
        /// <param name="minnormx"></param>
        public void SetMinNormX(float minnormx)
        {
            bigMesh.SharedMaterial.SetFloat("_MinNormX", minnormx);
        }

        /// <summary>
        /// Sets the min normalised x value
        /// </summary>
        /// <param name="maxnormx"></param>
        public void SetMaxNormX(float maxnormx)
        {
            bigMesh.SharedMaterial.SetFloat("_MaxNormX", maxnormx);
        }

        /// <summary>
        /// Sets the min normalised y value
        /// </summary>
        /// <param name="minnormy"></param>
        public void SetMinNormY(float minnormy)
        {
            bigMesh.SharedMaterial.SetFloat("_MinNormY", minnormy);
        }

        /// <summary>
        /// Sets the min normalised y value
        /// </summary>
        /// <param name="maxnormy"></param>
        public void SetMaxNormY(float maxnormy)
        {
            bigMesh.SharedMaterial.SetFloat("_MaxNormY", maxnormy);
        }

        /// <summary>
        /// Sets the min normalised z value
        /// </summary>
        /// <param name="minnormz"></param>
        public void SetMinNormZ(float minnormz)
        {
            bigMesh.SharedMaterial.SetFloat("_MinNormZ", minnormz);
        }

        /// <summary>
        /// Sets the min normalised z value
        /// </summary>
        /// <param name="maxnormz"></param>
        public void SetMaxNormZ(float maxnormz)
        {
            bigMesh.SharedMaterial.SetFloat("_MaxNormZ", maxnormz);
        }
        
        /// <summary>
        /// Sets the min x value
        /// </summary>
        /// <param name="minx"></param>
        public void SetMinX(float minx)
        {
            bigMesh.SharedMaterial.SetFloat("_MinX", minx);
        }

        /// <summary>
        /// Sets the min alised x value
        /// </summary>
        /// <param name="maxx"></param>
        public void SetMaxX(float maxx)
        {
            bigMesh.SharedMaterial.SetFloat("_MaxX", maxx);
        }

        /// <summary>
        /// Sets the min alised y value
        /// </summary>
        /// <param name="miny"></param>
        public void SetMinY(float miny)
        {
            bigMesh.SharedMaterial.SetFloat("_MinY", miny);
        }

        /// <summary>
        /// Sets the min alised y value
        /// </summary>
        /// <param name="maxy"></param>
        public void SetMaxY(float maxy)
        {
            bigMesh.SharedMaterial.SetFloat("_MaxY", maxy);
        }

        /// <summary>
        /// Sets the min alised z value
        /// </summary>
        /// <param name="minz"></param>
        public void SetMinZ(float minz)
        {
            bigMesh.SharedMaterial.SetFloat("_MinZ", minz);
        }

        /// <summary>
        /// Sets the min alised z value
        /// </summary>
        /// <param name="maxz"></param>
        public void SetMaxZ(float maxz)
        {
            bigMesh.SharedMaterial.SetFloat("_MaxZ", maxz);
        }

        public void SetBlendindDestinationMode(float bds)
        {
            bigMesh.SharedMaterial.SetFloat("_MyDstMode", bds);
        }

        public void SetBlendingSourceMode(float bmd)
        {
            bigMesh.SharedMaterial.SetFloat("_MySrcMode", bmd);
        }
        
        /// <summary>
        /// Zeroes the values of the visualisation spatial dimensions.
        /// 0 -> X
        /// 1 -> Y
        /// 2 -> Z
        /// </summary>
        /// <param name="spatialDimension"></param>
        public void ZeroPosition(int spatialDimension)
        {
            bigMesh.zeroPosition(spatialDimension);
        }

        /// <summary>
        /// Updates vertices positions on X
        /// </summary>
        /// <param name="dataX"></param>
        public void UpdateXPositions(float[] dataX)
        {
            bigMesh.updateXPositions(dataX);
        }

        /// <summary>
        /// Updates vertices positions on Y
        /// </summary>
        /// <param name="dataY"></param>
        public void UpdateYPositions(float[] dataY)
        {
            bigMesh.updateYPositions(dataY);
        }

        /// <summary>
        /// Updates vertices positions on Z
        /// </summary>
        /// <param name="dataY"></param>
        public void UpdateZPositions(float[] dataZ)
        {
            bigMesh.updateZPositions(dataZ);
        }

        /// <summary>
        /// returns all the positions
        /// </summary>
        /// <returns></returns>
        public Vector3[] GetPositions()
        {
            return bigMesh.getBigMeshVertices();
        }

        /// <summary>
        /// Tweens - animates the vertices
        /// </summary>
        public void TweenPosition()
        {
            bigMesh.Tween(BigMesh.TweenType.Position);
        }

        public void TweenSize()
        {
            bigMesh.Tween(BigMesh.TweenType.Size);
        }

        /// <summary>
        /// Gets the brushed texture that contains brushed indices
        /// </summary>
        /// <returns></returns>
        public Texture2D GetBrushTexture()
        {
            return bigMesh.SharedMaterial.GetTexture("_BrushedTexture") as Texture2D;
        }

        /// <summary>
        /// Sets the brush texture
        /// </summary>
        /// <param name="brushedIndicesTexture"></param>
        public void SetBrushTexture(Texture brushedIndicesTexture)
        {
            bigMesh.SharedMaterial.SetTexture("_BrushedTexture", brushedIndicesTexture);
        }

       
        /// <summary>
        /// sets data width for brushing textures
        /// </summary>
        /// <param name="texSize"></param>
        public void SetDataWidthBrushingTexture(float width)
        {
            bigMesh.SharedMaterial.SetFloat("_DataWidth", width);
        }

        /// <summary>
        /// sets data width for brushing textures
        /// </summary>
        /// <param name="texSize"></param>
        public void SetDataHeightBrushingTexture(float height)
        {
            bigMesh.SharedMaterial.SetFloat("_DataWidth", height);
        }

        /// <summary>
        /// Sets a boolean that shows/hides the brush
        /// </summary>
        /// <param name="showBrush"></param>
        public void SetShowBrush(bool showBrush)
        {
            bigMesh.SharedMaterial.SetFloat("showBrush", Convert.ToSingle(showBrush));
        }

        /// <summary>
        /// Sets the Color of the brush
        /// </summary>
        /// <param name="brushColor"></param>
        public void SetBrushColor(Color brushColor)
        {
            bigMesh.SharedMaterial.SetColor("brushColor", brushColor);
        }

        /// <summary>
        /// recalculates the bounds of the view 
        /// </summary>
        public void RecalculateBounds()
        {
            bigMesh.recalculateBounds();
        }


    }

}