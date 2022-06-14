using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace IATK
{

    /// <summary>
    /// Big mesh class. Feed it as many vertices as you want and it will aggregate the meshes to fit Unity's mesh index limit.
    /// </summary>
    [ExecuteInEditMode]
    public class BigMesh : MonoBehaviour
    {
        // CLASSES

        /// <summary>
        /// Big mesh data class
        /// </summary>
        public class BigMeshData
        {
            public MeshTopology meshTopology;   // How the indices are to be interpreted

            public Vector3[] vertices;          // The vertices of the mesh
            public int[] indices;               // The indices of the mesh
            public Color[] colours;             // The colours for each vertex
            public Vector3[] normals;           // The normals of the mesh
            public Vector3[] uvs;               // The uvs of the mesh
            public Vector3[] uvsAnimation;      // Store the information for animation  

            public int[] lineLength;
            public int[] chunkIndicesSize;

            public Material material;           // The material to apply to the mesh
            public Dictionary<int, int> indexToVertexDictionary;         // Dictionnary that links index buffer indices to vertex buffer indices

            public BigMeshData(MeshTopology meshTopology, Vector3[] vertices, int[] indices, Color[] colours, Vector3[] normals, Vector3[] uvs, Vector3[] uvsAnimation, int[] chunkIndicesSize, Material material, int[] lineLength)
            {
                this.meshTopology = meshTopology;
                this.vertices = vertices;
                this.indices = indices;
                this.colours = colours;
                this.normals = normals;
                this.uvs = uvs;
                this.uvsAnimation = uvsAnimation;
                this.chunkIndicesSize = chunkIndicesSize;
                this.material = material;
                this.lineLength = lineLength;
                

            }
        }

        // DATA

        private int NB_VERTTICES;

        public static readonly int VERTEX_LIMIT = 65000000;       // Unity's internal vertex limit

        private List<Mesh> meshList = new List<Mesh>();         // The list of meshes

        // PROPERTIES

        [SerializeField] // needs to be serialized for build support
        private Material sharedMaterial;

        // The common material shared between all submeshes
        public Material SharedMaterial {                              
            get
            {
                return sharedMaterial;
            }
            private set
            {
                sharedMaterial = value;
            }
        }

        // PUBLIC

        public void recalculateBounds()
        {
            foreach (var mesh in meshList)
            {
                mesh.RecalculateBounds();
            }
        }

        public int GetNumberVertices()
        {
            int v = 0;
            foreach (var item in meshList)
            {
                v += item.vertexCount;
            }
            return v;
        }

        /// <summary>
        /// sets to zero the provided dimension
        /// </summary>
        /// <param name="indexDimension"></param>
        public void zeroPosition(int component)
        {
            foreach (var mesh in meshList)
            {
                Vector3[] vertices = mesh.vertices;
                Vector3[] normals = mesh.normals;

                for (int i = 0; i < vertices.Length; i++)
                {
                    Vector3 norm = normals[i];
                    Vector3 val = vertices[i];

                    norm = val;
                    val[component] = 0;

                    normals[i] = norm;
                    vertices[i] = val;
                }
                mesh.SetNormals(normals.ToList());
                mesh.SetVertices(vertices.ToList());
                mesh.RecalculateBounds();
            }
        }

        /// <summary>
        /// sets the x positions of the vertices in the bigmesh
        /// </summary>
        /// <param name="values"></param>
        public void updateXPositions(float[] values)
        {
            UpdateNPositions(0, values);
        }

        /// <summary>
        /// sets the y positions of the vertices in the bigmesh
        /// </summary>
        /// <param name="values"></param>
        public void updateYPositions(float[] values)
        {
            UpdateNPositions(1, values);
        }

        /// <summary>
        /// sets the y positions of the vertices in the bigmesh
        /// </summary>
        /// <param name="values"></param>
        public void updateZPositions(float[] values)
        {
            UpdateNPositions(2, values);
        }

        private void UpdateNPositions(int component, float[] values)
        {
            int cpt = 0;
            foreach (var mesh in meshList)
            {
                float[] xvals = HelperUtils.SubArray(values, cpt, mesh.vertexCount);
                Vector3[] vertices = mesh.vertices;
                Vector3[] normals = mesh.normals;

                for (int i = 0; i < vertices.Length; i++)
                {
                    Vector3 norm = normals[i];
                    Vector3 val = vertices[i];

                    norm = val;
                    val[component] = xvals[i];

                    normals[i] = norm;
                    vertices[i] = val;
                }
                mesh.SetNormals(normals.ToList());
                mesh.SetVertices(vertices.ToList());
                mesh.RecalculateBounds();
                cpt += mesh.vertexCount;
            }
        }

        /// <summary>
        /// returns the vertex positions of the big mesh
        /// </summary>
        /// <returns></returns>
        public Vector3[] getBigMeshVertices()
        {
            List<Vector3> poses = new List<Vector3>();
            foreach (var mesh in meshList)
            {
                poses.AddRange(mesh.vertices);
            }

            return poses.ToArray();
        }

        /// <summary>
        /// updates the big mesh positions
        /// </summary>
        /// <param name="vertices"></param>
        private void updateBigMeshVertices(Vector3[] vertices)
        {
            int cpt = 0;
            foreach (var mesh in meshList)
            {
                mesh.SetVertices(HelperUtils.SubArray(vertices, cpt, mesh.vertexCount).ToList());
                cpt += mesh.vertexCount;
                mesh.RecalculateBounds();
            }
        }

        /// <summary>
        /// updates the big mesh colours
        /// </summary>
        /// <param name="vertices"></param>
        public void updateBigMeshColour(Color colour)
        {
            int cpt = 0;
            foreach (var mesh in meshList)
            {
                List<Color> lc = new List<Color>();
                foreach (var vp in mesh.vertices)
                {
                    lc.Add(colour);
                }
                mesh.SetColors(HelperUtils.SubArray(lc.ToArray(), cpt, mesh.vertexCount).ToList());
                cpt += mesh.vertexCount;
                mesh.RecalculateBounds();
            }
        }

        /// <summary>
        /// updates the big mesh colours
        /// </summary>
        /// <param name="vertices"></param>
        public void updateBigMeshColours(Color[] colours)
        {
            int cpt = 0;
            foreach (var mesh in meshList)
            {
                mesh.SetColors(HelperUtils.SubArray(colours, cpt, mesh.vertexCount).ToList());
                cpt += mesh.vertexCount;
                mesh.RecalculateBounds();
            }
        }

        /// <summary>
        /// Gets the colours.
        /// </summary>
        /// <returns>The colours.</returns>
        public Color[] getColors()
        {
            List<Color> colours = new List<Color>();
            foreach (var mesh in meshList)
            {
                colours.AddRange(mesh.colors);
            }
            return colours.ToArray();
        }

        /// <summary>
        /// updates the big mesh normals
        /// </summary>
        /// <param name="vertices"></param>
        public void updateBigMeshNormals(Vector3[] normals)
        {
            int cpt = 0;
            foreach (var mesh in meshList)
            {
                mesh.SetNormals(HelperUtils.SubArray(normals, cpt, mesh.vertexCount).ToList());
                cpt += mesh.vertexCount;
                mesh.RecalculateBounds();
            }
        }

        /// <summary>
        /// Gets the normals.
        /// </summary>
        /// <returns>The normals.</returns>
        public Vector3[] getNormals()
        {
            List<Vector3> normals = new List<Vector3>();
            foreach (var mesh in meshList)
            {
                normals.AddRange(mesh.normals);
                mesh.RecalculateBounds();
            }
            return normals.ToArray();
        }

        List<Vector4> GetUVList(int channel, Mesh mesh)
        {
            List<Vector4> l = new List<Vector4>();
            mesh.GetUVs(channel, l);
            return l;
        }

        public List<Vector4> GetUVs(int channel)
        {
            List<Vector4> uvs = new List<Vector4>();
            foreach (var mesh in meshList)
            {
                uvs.AddRange(GetUVList(channel, mesh));
            }

            return uvs;
        }

        public void MapUVChannel(int channel, int component, float[] data)
        {
            int total = meshList.Sum(x => x.vertexCount);
            int dataPtr = 0;
            int meshIdx = 0;
            int meshVertIdx = 0;
            var meshUVs = GetUVList(channel, meshList[meshIdx]);

            for (int i = 0; i < total; ++i)
            {
                if (dataPtr >= data.Length)
                {
                    dataPtr = 0;
                }
                if (meshVertIdx == meshList[meshIdx].vertexCount)
                {
                    meshList[meshIdx].SetUVs(channel, meshUVs);
                    meshIdx++;
                    meshVertIdx = 0;
                    if (meshIdx >= meshList.Count)
                    {
                        break;
                    }
                    meshUVs = GetUVList(channel, meshList[meshIdx]);
                }


                var v = meshUVs[meshVertIdx];
                var prev = v[component];
                v[component] = data[dataPtr];
                if (component == (int)AbstractVisualisation.NormalChannel.Size)
                {
                    v[3] = prev;
                }
                meshUVs[meshVertIdx] = v;
                
                meshVertIdx++;
                dataPtr++;
            }
            
            // map the final UVs
            meshList[meshIdx].SetUVs(channel, meshUVs);

            foreach (var mesh in meshList)
            {
                mesh.RecalculateBounds();
            }
        }

        /// <summary>
        /// Creates a big mesh. Only handles meshes composed of triangles, points, and lines.
        /// </summary>
        /// <returns>The big mesh.</returns>
        /// <param name="meshData">Mesh data.</param>
        public static BigMesh createBigMesh(BigMeshData meshData)
        {
            List<Mesh> localMeshList = new List<Mesh>();
            GameObject parentObject = null;
            
            // Optimisation for when there is only one mesh
            //if (meshData.vertices.Length <= VERTEX_LIMIT)
            //{
                parentObject = createMesh(meshData.vertices,
                    meshData.indices,
                    meshData.colours,
                    meshData.normals,
                    meshData.uvs,
                    meshData.meshTopology,
                    meshData.material);
                localMeshList.Add(parentObject.GetComponent<MeshFilter>().sharedMesh);
            //}
            //else // Split it up
//            {
//                parentObject = new GameObject();
//                parentObject.name = "BigMesh";

//                List<Vector3> vertexBuffer = new List<Vector3>();
//                List<int> indexBuffer = new List<int>();
//                List<Color> colourBuffer = new List<Color>();
//                List<Vector3> normalBuffer = new List<Vector3>();
//                List<Vector3> uvBuffer = new List<Vector3>();

//                // Create all the meshes
//                int offset0 = 0;
//                ///
//                ///  >>>>>>>>> POINT TOPOLOGY <<<<<<<<<<<<<<
//                ///
//                if (meshData.meshTopology == MeshTopology.Points)
//                {
//                    for (int i = 0; i < meshData.vertices.Length; i++)
//                    {
//                        if ((vertexBuffer.Count) >= VERTEX_LIMIT)
//                        {
//                            GameObject meshObject = createMesh(vertexBuffer.ToArray(), 
//                                indexBuffer.ToArray(), 
//                                colourBuffer.ToArray(),
//                                normalBuffer.ToArray(),
//                                uvBuffer.ToArray(),
//                                meshData.meshTopology, 
//                                meshData.material);
//                            meshObject.transform.parent = parentObject.transform;

//                            vertexBuffer.Clear();
//                            indexBuffer.Clear();
//                            colourBuffer.Clear();
//                            normalBuffer.Clear();
//                            uvBuffer.Clear();

//                            localMeshList.Add(meshObject.GetComponent<MeshFilter>().sharedMesh);
//                            offset0 += VERTEX_LIMIT;
//                        }

//                        indexBuffer.Add(i - offset0);
//                        vertexBuffer.Add(meshData.vertices[i]);
//                        colourBuffer.Add(meshData.colours[i]);
//                        normalBuffer.Add(meshData.normals[i]);
//                        uvBuffer.Add(meshData.uvs[i]);                    }
//                }
//                ///
//                ///  >>>>>>>>> LINE TOPOLOGY <<<<<<<<<<<<<<
//                ///
//                else if (meshData.meshTopology == MeshTopology.Lines)
//                {
//                    int MaxIndex = meshData.indices.Max(); //should correspond to the size of the dataset - 1
//                    Dictionary<int, int> indexBtoVertexBIndices = new Dictionary<int, int>();

//                    int currentLine = 0;
//                    int currentLineCount = meshData.lineLength[0] * 2;
//                    HashSet<int> globalIndices = new HashSet<int>();

//                    for (int i = 0; i < meshData.indices.Length; i++)
//                    {
//                        //get the index of the vertex to access
//                        int vbIndex = meshData.indices[i];
                        
//                        if (currentLineCount == 0 && currentLine + 1 < meshData.lineLength.Length)
//                        {
//                            int newVertexCounter = 0;

//                            //look up next vertices in the coming line
//                            for (int k = i; k < i + meshData.lineLength[currentLine + 1] * 2; k++)
//                            {
//                                if (k < meshData.indices.Length)
//                                    if (!globalIndices.Contains(meshData.indices[k])) newVertexCounter++;
//                            }

//                            if (vertexBuffer.Count + newVertexCounter > VERTEX_LIMIT)
//                            {
//                                GameObject meshObject = createMesh(vertexBuffer.ToArray(), 
//                                                                   indexBuffer.ToArray(), 
//                                                                   colourBuffer.ToArray(), 
//                                                                   normalBuffer.ToArray(),
//                                                                   uvBuffer.ToArray(),
//                                                                   meshData.meshTopology, meshData.material);
//                                meshObject.transform.parent = parentObject.transform;
//                                localMeshList.Add(meshObject.GetComponent<MeshFilter>().sharedMesh);

//                                //print("created mesh " + localMeshList.Count + " containing # vertices: " + vertexBuffer.Count);

//                                vertexBuffer.Clear();
//                                indexBuffer.Clear();
//                                colourBuffer.Clear();
//                                normalBuffer.Clear();
//                                uvBuffer.Clear();
//                                indexBtoVertexBIndices.Clear();
//                                globalIndices.Clear();

//                            }

//                            currentLine++;
//                            currentLineCount = meshData.lineLength[currentLine] * 2;

//                        }
//                        //else
//                        {
//                            //in any case keep filling up the index buffer
//                            // normalise the index
//                            int localIndexBufferIndex = 0;

//                            if (!indexBtoVertexBIndices.ContainsKey(vbIndex))
//                            {
//                                localIndexBufferIndex = vertexBuffer.Count;

//                                vertexBuffer.Add(meshData.vertices[vbIndex]);
//                                colourBuffer.Add(meshData.colours[vbIndex]);
//                                normalBuffer.Add(meshData.normals[vbIndex]);
//                                uvBuffer.Add(meshData.uvs[vbIndex]);

//                                indexBtoVertexBIndices.Add(vbIndex, localIndexBufferIndex);

//                            }
//                            else
//                            {
//                                localIndexBufferIndex = indexBtoVertexBIndices[vbIndex];
//                            }

//                            indexBuffer.Add(localIndexBufferIndex);

//                            globalIndices.Add(vbIndex);
//                            //decrease line count
//                            currentLineCount--;

//                        }
//                    }

//                }

//                if (vertexBuffer.Count > 0)
//                {
//                    //print("creeating ")
//                    GameObject meshObject = createMesh(vertexBuffer.ToArray(), 
//                                                       indexBuffer.ToArray(), 
//                                                       colourBuffer.ToArray(), 
//                                                       normalBuffer.ToArray(),
//                                                       uvBuffer.ToArray(),
//                                                       meshData.meshTopology, meshData.material);
//                    meshObject.transform.parent = parentObject.transform;
//                    localMeshList.Add(meshObject.GetComponent<MeshFilter>().sharedMesh);

////                    print("created mesh " + localMeshList.Count + " containing # vertices: " + vertexBuffer.Count);

//                }

//                #region broken code for connected topology with 65k+ lines
//                //else if (meshData.meshTopology == MeshTopology.Lines)
//                //{
//                //    int previousChunkOffset = 0;

//                //    for (int chunk=0;chunk<meshData.chunkIndicesSize.Length; chunk++)
//                //    {
//                //        int chunkOffset = previousChunkOffset + meshData.chunkIndicesSize[chunk];// *(BigMesh.VERTEX_LIMIT);
//                //        print("chunk offset " + chunkOffset);

//                //        vertexBuffer.Clear();
//                //        indexBuffer.Clear();
//                //        colourBuffer.Clear();
//                //        normalBuffer.Clear();

//                //        //read 65k vertices
//                //        for (int i=0;i<=BigMesh.VERTEX_LIMIT;i++)
//                //        {
//                //            vertexBuffer.Add(meshData.vertices[i + chunkOffset]);
//                //            colourBuffer.Add(meshData.colours[i + chunkOffset]);
//                //            normalBuffer.Add(meshData.normals[i + chunkOffset]);
//                //        }

//                //        //read the index buffer
//                //        for (int i = 0; i < meshData.chunkIndicesSize[chunk]; i++)
//                //        {
//                //            indexBuffer.Add(meshData.indices[i]);
//                //        }

//                //        print("at 0: " + indexBuffer[0]);
//                //        print("at 1: " + indexBuffer[1]);
//                //        print("at 2: " + indexBuffer[2]);

//                //        print("at last -2: " + indexBuffer[indexBuffer.Count -3]);
//                //        print("at last -1: " + indexBuffer[indexBuffer.Count - 2]);
//                //        print("at last: " + indexBuffer[indexBuffer.Count - 1]);

//                //        GameObject meshObject = createMesh(vertexBuffer.ToArray(), indexBuffer.ToArray(), colourBuffer.ToArray(), normalBuffer.ToArray(), meshData.meshTopology, meshData.material);
//                //        meshObject.transform.parent = parentObject.transform;

//                //        previousChunkOffset = chunkOffset;
//                //    }
//                //}
//                #endregion

//                #region old bugged code for big mesh creation
//                //for (int i = 0; i < meshData.vertices.Length - 1; i += 2)
//                //{
//                //    if ((vertexBuffer.Count) > VERTEX_LIMIT)
//                //    {
//                //        GameObject meshObject = createMesh(vertexBuffer.ToArray(), indexBuffer.ToArray(), colourBuffer.ToArray(), normalBuffer.ToArray(), meshData.meshTopology, meshData.material);
//                //        meshObject.transform.parent = parentObject.transform;

//                //        vertexBuffer.Clear();
//                //        indexBuffer.Clear();
//                //        colourBuffer.Clear();
//                //        normalBuffer.Clear();

//                //        localMeshList.Add(meshObject.GetComponent<MeshFilter>().sharedMesh);
//                //        //Debug.Log("completed line from " + meshData.indices[i] + " to " + meshData.indices[i + 1]);
//                //        //if(i+2 < meshData.vertices.Length)
//                //        //Debug.Log("next indice is " + meshData.indices[i+2]);

//                //        //if (localMeshList.Count > 1) goto debuglines;
//                //    }

//                //    int index1 = meshData.indices[i];// + offset];
//                //    int index2 = meshData.indices[i + 1];
//                //    //indexBuffer.Add(vertexBuffer.Count);
//                //    indexBuffer.Add(index1 % (VERTEX_LIMIT));// + offset]);
//                //    indexBuffer.Add(index2 % (VERTEX_LIMIT));

//                //    //if(i%2 ==0)
//                //    vertexBuffer.Add(meshData.vertices[i]);
//                //    vertexBuffer.Add(meshData.vertices[i + 1]);

//                //    if (meshData.colours != null)
//                //    {
//                //        // if (i % 2 == 0)
//                //        colourBuffer.Add(meshData.colours[index1]);
//                //        colourBuffer.Add(meshData.colours[index2]);

//                //    }

//                //    if (meshData.normals != null)
//                //    {
//                //        // if (i % 2 == 0)
//                //        normalBuffer.Add(meshData.normals[index1]);
//                //        normalBuffer.Add(meshData.normals[index2]);

//                //    }

//                //}
//                #endregion

//                //// Create remaining
//                //if (vertexBuffer.Count >= stride)
//                //{
//                //    GameObject meshObject = createMesh(vertexBuffer.ToArray(), indexBuffer.ToArray(), colourBuffer.ToArray(), normalBuffer.ToArray(), meshData.meshTopology, meshData.material);
//                //    meshObject.transform.parent = parentObject.transform;

//                //    localMeshList.Add(meshObject.GetComponent<MeshFilter>().sharedMesh);
//                //}
//            }


            BigMesh bigMeshComponent = parentObject.AddComponent<BigMesh>();
            bigMeshComponent.SharedMaterial = meshData.material;

            bigMeshComponent.meshList = localMeshList;

            return bigMeshComponent;
        }

        // PRIVATE

        private static int getNumberOfTopologyChunk(BigMeshData meshData)
        {
            //if (meshData.meshTopology == MeshTopology.Points)
            //{
                return meshData.vertices.Length; 
            //}
            //else 
            //{
             //   return meshData.lineLength.Length;
            //}
        }

        private static int getChunkStride(BigMeshData meshData, int chunkIndex)
        {
            //if (meshData.meshTopology == MeshTopology.Points)
            { return 1; }
            //else
            //{
             //   return meshData.lineLength[chunkIndex]*2;
            //}
        }

        /// <summary>
        /// Creates a mesh.
        /// </summary>
        /// <returns>The mesh.</returns>
        /// <param name="vertices">Vertices.</param>
        /// <param name="indices">Indices.</param>
        /// <param name="colours">Colours.</param>
        /// <param name="normals">Normals.</param>
        /// <param name="MeshTopology">Mesh topology.</param>
        /// <param name="material">Material.</param>
        private static GameObject createMesh(Vector3[] vertices, int[] indices, Color[] colours, Vector3[] normals, Vector3[] uvs, MeshTopology meshTopology, Material material)
        {
            GameObject meshObject = new GameObject();

            MeshTopology mtp = meshTopology;
           // if (mtp == MeshTopology.Lines) mtp = MeshTopology.LineStrip;
            // Create the mesh
            Mesh mesh = new Mesh();

            //enables bigmesh
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

            mesh.vertices = vertices;
            mesh.SetIndices(indices, mtp, 0);
            mesh.normals = normals;
            mesh.colors = colours;
            mesh.SetUVs(0, uvs.ToList());
            
            mesh.RecalculateBounds();

            if (normals == null || normals.Length == 0)
            {
                mesh.RecalculateNormals(); 
            }

            // Assign to GameObject
            MeshFilter meshFilter = meshObject.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = meshObject.AddComponent<MeshRenderer>();

            meshFilter.mesh = mesh;
            meshRenderer.material = material;
            mesh.RecalculateBounds();

            return meshObject;
        }

        /// <summary>
        /// saves the mesh(es) on the disk
        /// </summary>
        /// <param name="theMesh"></param>
        /// <param name="indexCount"></param>
        /// <param name="material"></param>
        public void saveMesh(GameObject theMesh, ref int indexCount, Material material)
        {
            MeshFilter mf = theMesh.GetComponent<MeshFilter>();
            MeshRenderer mr = theMesh.GetComponent<MeshRenderer>();

            if (mf != null && mr !=null)
            {
                Mesh mesh = mf.sharedMesh;
                mr.sharedMaterial = material;

                string meshNameInPath = "Assets/SavedMeshes/" + "Mesh_" + indexCount.ToString() + ".asset";

#if UNITY_EDITOR
                AssetDatabase.CreateAsset(mesh, meshNameInPath);
                AssetDatabase.SaveAssets();

                Mesh m = AssetDatabase.LoadAssetAtPath<Mesh>(meshNameInPath);
                
                mf.mesh = m;
#endif
                indexCount++;
            }
            foreach (Transform child in theMesh.transform)
            {
                saveMesh(child.gameObject, ref indexCount, material);
            }
        }

        public void SaveBigMesh()
        {
            #if UNITY_EDITOR

            if (!Directory.Exists("Assets/SavedMeshes")) Directory.CreateDirectory("Assets/SavedMeshes");

            int countMeshRef = 0;
            MeshRenderer mr = gameObject.GetComponentInChildren<MeshRenderer>();

            if (mr != null && mr.sharedMaterial != null)
            {
                string rendererNameInPath = "Assets/SavedMeshes/" + mr.sharedMaterial.name.Replace("/","_")  + ".asset";

                AssetDatabase.CreateAsset(mr.sharedMaterial, rendererNameInPath);
                AssetDatabase.SaveAssets();
                Material m = AssetDatabase.LoadAssetAtPath<Material>(rendererNameInPath);

                saveMesh(this.gameObject, ref countMeshRef, m);
            }
            PrefabUtility.SaveAsPrefabAsset(this.gameObject, "Assets/Prefabs/BigMesh.prefab");
            
            #endif
        }

        /// <summary>
        /// Gets the stride from a mesh topology
        /// </summary>
        /// <returns>The stride.</returns>
        /// <param name="meshTopology">Mesh topology.</param>
        private static int getStride(MeshTopology meshTopology)
        {
            switch (meshTopology)
            {
            case MeshTopology.Triangles:
                {
                    return 3;
                }
            case MeshTopology.Lines:
                {
                    return 2;
                }
            case MeshTopology.Points:
                {
                    return 1;
                }
            default:
                {
                    Debug.Assert(false, "Unsupported MeshTopology");
                    break;
                }
            }

            return 0;
        }

        // handle animations
        float _tweenPosition = 0.0f;
        float _tweenSize = 0.0f;

        public enum TweenType
        {
            Position,
            Size
        }

        // update tween delegate. Returns false when the tween is complete
        delegate bool UpdateTweenDelegate();

        // global list of all tweens callbacks
        static List<UpdateTweenDelegate> updateTweens = new List<UpdateTweenDelegate>();

        #pragma warning disable 0414
        // Used when outside Unity editor
        //  causes warnings in Unity as it does not think it is used
        private bool hasTweens = false;
        #pragma warning disable 0414
        
        private void Update()
        {
            #if !UNITY_EDITOR
            if (hasTweens)
            {
                UpdateTweens();
            }
            #endif
        }

        public void Tween(TweenType type)
        {
            if (type == TweenType.Position)
            {
                _tweenPosition = 0.0f;
                this.SharedMaterial.SetFloat("_Tween", 0);
                QueueTween();
            }
            else if (type == TweenType.Size)
            {
                _tweenSize = 0.0f;
                this.SharedMaterial.SetFloat("_TweenSize", 0);
                QueueTween();
            }
        }

        void UpdateTweens()
        {
            List<UpdateTweenDelegate> deleteList = new List<UpdateTweenDelegate>();
            for (int i = updateTweens.Count() - 1; i >= 0; --i)
            {
                bool isTweening = updateTweens[i]();
                if (!isTweening)
                {
                    updateTweens.RemoveAt(i);
                }
            }
            #if UNITY_EDITOR
            if (updateTweens.Count() == 0)
            {
                EditorApplication.update = null;
            }
            #else
            if (updateTweens.Count() == 0)
            {
                hasTweens = false;
            }
            #endif
        }

        void QueueTween()
        {
            updateTweens.Add(DoTheTween);
            #if UNITY_EDITOR
            EditorApplication.update = UpdateTweens;
            #else
            hasTweens = true;
            #endif
        }

        // returns false if complete, else true
        private bool DoTheTween()
        {
            if (this.SharedMaterial == null) return false;
            
            bool isTweening = false;
            
            _tweenPosition += Time.deltaTime;
            if (_tweenPosition < 1.0f)
            {
                float v = Mathf.Pow(_tweenPosition, 3) * (_tweenPosition * (6f * _tweenPosition - 15f) + 10f);
                this.SharedMaterial.SetFloat("_Tween", v);
                isTweening = true;
            }
            else
            {
                _tweenPosition = 1.0f;
                this.SharedMaterial.SetFloat("_Tween", 1);
            }

            _tweenSize += Time.deltaTime;
            if (_tweenSize < 1.0f)
            {
                float v = Mathf.Pow(_tweenSize, 3) * (_tweenSize * (6f * _tweenSize - 15f) + 10f);
                this.SharedMaterial.SetFloat("_TweenSize", v);
                isTweening = true;
            }
            else
            {
                _tweenSize = 1.0f;
                this.SharedMaterial.SetFloat("_TweenSize", 1);
            }
            return isTweening;
        }
    }

}   // Namespace