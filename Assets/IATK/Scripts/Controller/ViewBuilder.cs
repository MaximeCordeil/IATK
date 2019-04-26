using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

namespace IATK
{

    /// <summary>
    /// View class to generate a mesh containing the visualisation data
    /// </summary>
    public class ViewBuilder
    {
        // DATA

        public enum VIEW_DIMENSION { X, Y, Z, LINKING_FIELD };      // View dimension

        public string Name { get; private set; }                    // The name assigned to this view

        public List<int> Indices
        {
            get
            {
                return indices;
            }

            set
            {
                indices = value;
            }
        }

        public List<int> LineLength
        {
            get
            {
                return lineLength;
            }

            set
            {
                lineLength = value;
            }
        }

        private MeshTopology myMeshTopolgy;                        // The topology to use for the mesh

        private List<Vector3> positions;                            // The positions of the data
        private List<int> indices;                                  // The indices for the positions
        private List<Color> colours;                                // The colour at each position
        private List<Vector3> normals;                              // 
        private List<Vector3> uvs;                                  // Holds extra data such as id, size
        private List<Vector3> uv2s;                                 // Holds extra data such as id, size

        private List<int> lineLength;                               // contains the size of each line when 
        // making a line topology mesh
        private List<int> chunkSizeList;                            // size of each block of indices when the
        // mesh contains more than 65k vertices
        
        private int numberOfDataPoints;                             // number of points in the dataset

        private BigMesh bigMesh;                                    // the big mesh


        // PUBLIC

        /// <summary>
        /// Initializes a new instance of the <see cref="Staxes.View"/> class.
        /// </summary>
        /// <param name="type">Mesh topology type</param>
        /// <param name="viewName">View name.</param>
        public ViewBuilder(MeshTopology type, string viewName)
        {
            myMeshTopolgy = type;
            Name = viewName;
        }

        /// <summary>
        /// Initialises the data view.
        /// </summary>
        /// <param name="numberOfPoints">Number of points.</param>
        /// <param name="parent">Parent.</param>
        public ViewBuilder initialiseDataView(int numberOfPoints)
        {
            // Initialise
            positions = new List<Vector3>(numberOfPoints);
            Indices = new List<int>();
            colours = new List<Color>(numberOfPoints);
            normals = new List<Vector3>(numberOfPoints);
            uvs = new List<Vector3>(numberOfPoints);
            uv2s = new List<Vector3>(numberOfPoints);

            LineLength = new List<int>();
            chunkSizeList = new List<int>();

            numberOfDataPoints = numberOfPoints;
            
            // Fill
            for (int i = 0; i < numberOfPoints; i++)
            {
                positions.Add(new Vector3());
                colours.Add(Color.white);
                normals.Add(new Vector3());
                uvs.Add(new Vector3());
                uv2s.Add(new Vector3());
                
            }

            return this;
        }


        /// <summary>
        /// Sets the data dimension.
        /// </summary>
        /// <param name="data">Data.</param>
        /// <param name="dimension">Dimension.</param>
        public ViewBuilder setDataDimension(float[] data, VIEW_DIMENSION dimension)
        {
            Debug.Assert((int)dimension < 3);
            Debug.Assert(data.Length <= positions.Count);

            float minValue = data.Min();
            float maxValue = data.Max();

            for (int i = 0; i < data.Length; i++)
            {
                Vector3 p = positions[i];
                Vector3 n = normals[i];

                p[(int)dimension] = data[i];
                n[(int)dimension] = data[i];

                positions[i] = p;
                normals[i] = n;
            }

            return this;
        }

        /// <summary>
        /// Updates the view.
        /// </summary>
        /// <param name="linking">Linking.</param>
        public ViewBuilder updateView()
        {
            updateMeshPositions();
            return this;
        }

        /// <summary>
        /// Maps a single color to all the data points
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>

        public ViewBuilder setSingleColor(Color color)
        {
            for (int i = 0; i < numberOfDataPoints; i++)
                colours.Add(color);

            return this;
        }

        /// <summary>
        /// Sets the colors.
        /// </summary>
        /// <param name="colors">Colors.</param>
        public ViewBuilder setColors(Color[] newColours)
        {
            Debug.Assert(newColours != null && newColours.Length == numberOfDataPoints);
            colours = newColours.ToList();
            return this;
        }


        /// <summary>
        /// Maps the colours in a continuous gradient.
        /// </summary>
        /// <param name="data">Data.</param>
        /// <param name="fromColor">From color.</param>
        /// <param name="toColor">To color.</param>
        public ViewBuilder mapColorContinuous(float[] data, Color fromColor, Color toColor)
        {
            Color[] newColours = new Color[data.Length];

            for (int i = 0; i < newColours.Length; i++)
            {
                newColours[i] = Color.Lerp(fromColor, toColor, data[i]);
            }

            //Debug.Log("vertices count: " + myMesh.vertices.Length + " colors count: " + newColours.Length);
            colours = newColours.ToList();

            return this;
        }

        /// <summary>
        /// Maps the data to a color palette.
        /// </summary>
        /// <param name="data">Data.</param>
        /// <param name="palette">Palette.</param>
        public ViewBuilder mapColorCategory(float[] data, Color[] palette)
        {
            Color[] colorSet = new Color[data.Length];

            float? prevData = null;
            int cat = 0;
            for (int i = 0; i < colorSet.Length; i++)
            {
                if ((prevData != null) && (data[i] != prevData.Value))
                {
                    cat++;

                    Debug.Assert(cat < palette.Length);
                }

                colorSet[i] = palette[cat];

                prevData = data[i];
            }

            setColors(colorSet);

            return this;
        }

        /// <summary>
        /// Generates the meshes and attaches them to the given parent GameObject
        /// Call to finalise the view being built
        /// </summary>
        /// <param name="parent">Parent.</param>
        /// <param name="material">Material.</param>
        public View apply(GameObject parent, Material material)
        {
            parent.name = Name;
            
            GameObject viewGameObject = new GameObject();

            viewGameObject.transform.SetParent(parent.transform, false);

            View view = viewGameObject.AddComponent<View>();
            view.gameObject.tag = "View";
            view.gameObject.name = "View";


            var data = new BigMesh.BigMeshData(myMeshTopolgy,
                                               positions.ToArray(),
                                               Indices.ToArray(),
                                               colours.ToArray(),
                                               normals.ToArray(),
                                               uvs.ToArray(),
                                               uv2s.ToArray(),
                                               chunkSizeList.ToArray(),
                                               material,
                                               LineLength.ToArray());

            BigMesh meshObject = BigMesh.createBigMesh(data);
            meshObject.gameObject.name = "BigMesh";
            meshObject.transform.SetParent(viewGameObject.transform, false);

            bigMesh = meshObject;
            view.BigMesh = bigMesh;

            return view;
        }

        public void update()
        {

        }
        // PRIVATE
        
        /// <summary>
        /// Updates the mesh from positions.
        /// </summary>
        /// <param name="linkingField">Linking field.</param>
        private void updateMeshPositions()
        {
            switch (myMeshTopolgy)
            {
                case MeshTopology.LineStrip:
                case MeshTopology.Lines:
                    {
                        //createIndicesLines(linkingField);
                        break;
                    }
                case MeshTopology.Points:
                    {
                        createIndicesScatterPlot(positions.Count);
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }

        /// <summary>
        /// Creates the indices for a scatter plot.
        /// </summary>
        /// <returns>The indices for a scatter plot.</returns>
        /// <param name="numberOfPoints">Number of points.</param>
        private void createIndicesScatterPlot(int numberOfPoints)
        {
            Indices.Clear();
            Indices.Capacity = numberOfPoints;

            for (int i = 0; i < numberOfPoints; i++)
            {
                Indices.Add(i);
            }
        }

        /// <summary>
        /// Use the normal to store the vertex index, size
        /// </summary>
        public ViewBuilder createIndicesPointTopology()
        {
            for (int i = 0; i < numberOfDataPoints; i++)
            {
                Vector3 n = uvs[i];
                n[0] = (float)i;
                uvs[i] = n;
            }
            return this;
        }

        /// <summary>
        /// Use the normal to store the vertex index, size
        /// </summary>
        public ViewBuilder createIndicesPointTopology(int totalPoints)
        {
            for (int i = 0; i < totalPoints; i++)
            {
                Vector3 n = uvs[i];
                n[0] = (float)i;
                uvs[i] = n;
            }
            return this;
        }

        public ViewBuilder createIndicesPointTopology(float[] vertexIndices)
        {

            for (int i = 0; i < vertexIndices.Length; i++)
            {
                Vector3 n = uvs[i];
                n[0] = vertexIndices[i];
                uvs[i] = n;
            }

            return this;
        }

        // for graphs, OD data
        public ViewBuilder createIndicesGraphTopology(Dictionary<int, List<int>> nodeEdges)
        {

            foreach (var nodEdg in nodeEdges)
            {
                int node = nodEdg.Key;
                List<int> edges = nodEdg.Value;

                foreach (var edgeNode in edges)
                {
                    Indices.Add(node-1);
                    Indices.Add(edgeNode);
                }

                Vector3 n = uvs[node-1];
                n.x = (float)(node -1);
                uvs[node-1] = n;
            }
            
            return this;
        }

        // for trajectories, time series
        public ViewBuilder createIndicesConnectedLineTopology(float[] linkingField)
        {
            //the first member of the Tuple is the index in the index buffer, 
            // the second member is the index in the Vertex Buffer.

            List<Tuple<int, int>> indexToVertexIndex = new List<Tuple<int, int>>();

            int count = 0;
            int previousIndexCount = 0;
            for (int i = 0; i < linkingField.Length - 1; i++)
            {
                Vector3 n = uvs[i];
                n.x = (float)i;
                uvs[i] = n;

                if (linkingField[i] == linkingField[i + 1])
                {
                    Indices.Add(i);
                    Indices.Add(i + 1);
                    indexToVertexIndex.Add(new Tuple<int, int>(i, i));
                    indexToVertexIndex.Add(new Tuple<int, int>(i + 1, i + 1));
                    count++;
                }
                else
                {
                    LineLength.Add(count);
                    count = 0;
                }
            }

            int deltaSizeLast = Indices.Count - previousIndexCount;
            if (deltaSizeLast > 0) chunkSizeList.Add(deltaSizeLast);

            return this;
        }

        /// <summary>
        /// maps sizes to individual points
        /// </summary>
        /// <param name="sizeArray"></param>
        /// <returns></returns>         
        public ViewBuilder setSize(float[] sizeArray)
        {
            Debug.Assert(sizeArray != null && sizeArray.Length == numberOfDataPoints);
            for (int i = 0; i < numberOfDataPoints; i++)
            {
                Vector3 n = uvs[i];
                n.y = sizeArray[i];
                uvs[i] = n;
            }

            return this;
        }
    }

}   // Namespace