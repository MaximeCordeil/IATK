using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IATK;
using System.Linq;
namespace IATK
{
    [ExecuteInEditMode]
    public class LinkingVisualisations : MonoBehaviour
    {

        enum simpleVisualisationAxis { NONE, X, Y, Z, XY, XZ, YZ, XYZ };

        Vector3 fbl1 = Vector3.zero;
        Vector3 ftl1 = Vector3.zero;
        Vector3 ftr1 = Vector3.zero;
        Vector3 fbr1 = Vector3.zero;

        Vector3 btl1 = Vector3.zero;
        Vector3 bbl1 = Vector3.zero;
        Vector3 btr1 = Vector3.zero;
        Vector3 bbr1 = Vector3.zero;

        Vector3 ftr2 = Vector3.zero;
        Vector3 fbr2 = Vector3.zero;
        Vector3 ftl2 = Vector3.zero;
        Vector3 fbl2 = Vector3.zero;

        Vector3 btr2 = Vector3.zero;
        Vector3 bbr2 = Vector3.zero;
        Vector3 btl2 = Vector3.zero;
        Vector3 bbl2 = Vector3.zero;

        public Visualisation visualisationSource = null;
        public Visualisation visualisationTarget = null;

        public bool showLinks;
        [Range(0f,1f)]
        public float linkTransparency;

        bool toggleShow;

        Material linkerMaterial;

        View view;

        public View View
        {
            get { return view; }
            set { view = value; }
        }

        // Use this for initialization
        void OnEnable()
        {
            Visualisation.OnUpdateViewAction += Visualisation_OnUpdateViewAction;

            linkerMaterial = new Material(Shader.Find("IATK/Linked-Views-Material"))
            {
                renderQueue = 3000,
                enableInstancing = true
            };

            if(visualisationSource != null && visualisationTarget !=null) LinkVisualisations();
            toggleShow = showLinks;
            
        }

        private void OnDestroy()
        {
            Visualisation.OnUpdateViewAction -= Visualisation_OnUpdateViewAction;
        }

        private void Visualisation_OnUpdateViewAction(AbstractVisualisation.PropertyType propertyType)
        {
            if ((visualisationSource != null && visualisationTarget != null) &&
                (propertyType == AbstractVisualisation.PropertyType.DimensionChange
                || propertyType == AbstractVisualisation.PropertyType.X
                || propertyType == AbstractVisualisation.PropertyType.Y
                || propertyType == AbstractVisualisation.PropertyType.Z
                || propertyType == AbstractVisualisation.PropertyType.Colour))
            {
                LinkVisualisations();
            }
        }

        
        // Update is called once per frame
        void Update()
        {
            if(toggleShow != showLinks)
            {
                if (GetComponentInChildren<View>() == null)
                {
                    LinkVisualisations();
                    GetComponentInChildren<View>().Show(showLinks);
                }
                else
                    GetComponentInChildren<View>().Show(showLinks);
            }
            if (GetComponentInChildren<View>() != null && showLinks)
            {
                //calculate the visualisation corners
                CalculateCorners(visualisationSource, ref ftl1, ref ftr1, ref fbl1, ref fbr1, ref btl1, ref btr1, ref bbl1, ref bbr1);
                CalculateCorners(visualisationTarget, ref ftl2, ref ftr2, ref fbl2, ref fbr2, ref btl2, ref btr2, ref bbl2, ref bbr2);

                Vector3 p = new Vector3();
                p = (visualisationSource.transform.position + visualisationTarget.transform.position) / 2;
                transform.position = p;

                //calculate the visualisation corners
                CalculateCorners(visualisationSource, ref ftl1, ref ftr1, ref fbl1, ref fbr1, ref btl1, ref btr1, ref bbl1, ref bbr1);
                CalculateCorners(visualisationTarget, ref ftl2, ref ftr2, ref fbl2, ref fbr2, ref btl2, ref btr2, ref bbl2, ref bbr2);

                linkerMaterial.SetVector("_ftl1", transform.InverseTransformPoint(ftl1));
                linkerMaterial.SetVector("_fbl1", transform.InverseTransformPoint(fbl1));
                linkerMaterial.SetVector("_ftr1", transform.InverseTransformPoint(ftr1));
                linkerMaterial.SetVector("_fbr1", transform.InverseTransformPoint(fbr1));

                linkerMaterial.SetVector("_btl1", transform.InverseTransformPoint(btl1));
                linkerMaterial.SetVector("_bbl1", transform.InverseTransformPoint(bbl1));
                linkerMaterial.SetVector("_btr1", transform.InverseTransformPoint(btr1));
                linkerMaterial.SetVector("_bbr1", transform.InverseTransformPoint(bbr1));

                linkerMaterial.SetVector("_ftr2", transform.InverseTransformPoint(ftr2));
                linkerMaterial.SetVector("_fbr2", transform.InverseTransformPoint(fbr2));
                linkerMaterial.SetVector("_ftl2", transform.InverseTransformPoint(ftl2));
                linkerMaterial.SetVector("_fbl2", transform.InverseTransformPoint(fbl2));

                linkerMaterial.SetVector("_btr2", transform.InverseTransformPoint(btr2));
                linkerMaterial.SetVector("_bbr2", transform.InverseTransformPoint(bbr2));
                linkerMaterial.SetVector("_btl2", transform.InverseTransformPoint(btl2));
                linkerMaterial.SetVector("_bbl2", transform.InverseTransformPoint(bbl2));

                linkerMaterial.SetFloat("_MinXFilter1", visualisationSource.xDimension.minFilter);
                linkerMaterial.SetFloat("_MinXFilter2", visualisationTarget.xDimension.minFilter);
                linkerMaterial.SetFloat("_MinYFilter1", visualisationSource.yDimension.minFilter);
                linkerMaterial.SetFloat("_MinYFilter2", visualisationTarget.yDimension.minFilter);
                linkerMaterial.SetFloat("_MinZFilter1", visualisationSource.zDimension.minFilter);
                linkerMaterial.SetFloat("_MinZFilter2", visualisationTarget.zDimension.minFilter);

                linkerMaterial.SetFloat("_MaxXFilter1", visualisationSource.xDimension.maxFilter);
                linkerMaterial.SetFloat("_MaxXFilter2", visualisationTarget.xDimension.maxFilter);
                linkerMaterial.SetFloat("_MaxYFilter1", visualisationSource.yDimension.maxFilter);
                linkerMaterial.SetFloat("_MaxYFilter2", visualisationTarget.yDimension.maxFilter);
                linkerMaterial.SetFloat("_MaxZFilter1", visualisationSource.zDimension.maxFilter);
                linkerMaterial.SetFloat("_MaxZFilter2", visualisationTarget.zDimension.maxFilter);

                linkerMaterial.SetFloat("_MinNormX1", visualisationSource.xDimension.minScale);
                linkerMaterial.SetFloat("_MinNormX2", visualisationTarget.xDimension.minScale);
                linkerMaterial.SetFloat("_MinNormY1", visualisationSource.yDimension.minScale);
                linkerMaterial.SetFloat("_MinNormY2", visualisationTarget.yDimension.minScale);
                linkerMaterial.SetFloat("_MinNormZ1", visualisationSource.zDimension.minScale);
                linkerMaterial.SetFloat("_MinNormZ2", visualisationTarget.zDimension.minScale);

                linkerMaterial.SetFloat("_MaxNormX1", visualisationSource.xDimension.maxScale);
                linkerMaterial.SetFloat("_MaxNormX2", visualisationTarget.xDimension.maxScale);
                linkerMaterial.SetFloat("_MaxNormY1", visualisationSource.yDimension.maxScale);
                linkerMaterial.SetFloat("_MaxNormY2", visualisationTarget.yDimension.maxScale);
                linkerMaterial.SetFloat("_MaxNormZ1", visualisationSource.zDimension.maxScale);
                linkerMaterial.SetFloat("_MaxNormZ2", visualisationTarget.zDimension.maxScale);

                //set alpha
                linkerMaterial.SetFloat("_Alpha", linkTransparency);

                // Set scaling matrices based on visualisation size
                linkerMaterial.SetMatrix("_ScaleMatrix1", Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(visualisationSource.width, visualisationSource.height, visualisationSource.depth)));
                linkerMaterial.SetMatrix("_ScaleMatrix2", Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(visualisationTarget.width, visualisationTarget.height, visualisationTarget.depth)));
            }
            toggleShow = showLinks;
        }

        simpleVisualisationAxis GetVisualisationAxisType(Visualisation v)
        {
            if (v.theVisualizationObject.X_AXIS != null
                && v.theVisualizationObject.Y_AXIS == null
                && v.theVisualizationObject.Z_AXIS == null)
                return simpleVisualisationAxis.X;
            else if (v.theVisualizationObject.X_AXIS == null
                && v.theVisualizationObject.Y_AXIS != null
                && v.theVisualizationObject.Z_AXIS == null)
                return simpleVisualisationAxis.Y;
            else if (v.theVisualizationObject.X_AXIS == null
                && v.theVisualizationObject.Y_AXIS == null
                && v.theVisualizationObject.Z_AXIS != null)
                return simpleVisualisationAxis.Z;
            else if (v.theVisualizationObject.X_AXIS != null
                && v.theVisualizationObject.Y_AXIS != null
                && v.theVisualizationObject.Z_AXIS == null)
                return simpleVisualisationAxis.XY;
            else if (v.theVisualizationObject.X_AXIS != null
                && v.theVisualizationObject.Y_AXIS == null
                && v.theVisualizationObject.Z_AXIS != null)
                return simpleVisualisationAxis.XZ;
            else if (v.theVisualizationObject.X_AXIS == null
                && v.theVisualizationObject.Y_AXIS != null
                && v.theVisualizationObject.Z_AXIS != null)
                return simpleVisualisationAxis.YZ;
            else if (v.theVisualizationObject.X_AXIS != null
                && v.theVisualizationObject.Y_AXIS != null
                && v.theVisualizationObject.Z_AXIS != null)
                return simpleVisualisationAxis.XYZ;
            else return simpleVisualisationAxis.NONE;
        }

        void CalculateCorners(Visualisation v, ref Vector3 ftl, ref Vector3 ftr, ref Vector3 fbl, ref Vector3 fbr,
            ref Vector3 btl, ref Vector3 btr, ref Vector3 bbl, ref Vector3 bbr)
        {
            switch (GetVisualisationAxisType(v))
            {
                case simpleVisualisationAxis.NONE:
                    break;
                case simpleVisualisationAxis.X:
                    CalculateCorners3(v.theVisualizationObject.X_AXIS.GetComponent<Axis>(),
                                      v.theVisualizationObject.X_AXIS.GetComponent<Axis>(),
                                      v.theVisualizationObject.X_AXIS.GetComponent<Axis>(),
                                      ref ftl, ref ftr, ref fbl, ref fbr, ref btl, ref btr, ref bbl, ref bbr);
                    break;
                case simpleVisualisationAxis.Y:
                    CalculateCorners3(v.theVisualizationObject.Y_AXIS.GetComponent<Axis>(),
                                      v.theVisualizationObject.Y_AXIS.GetComponent<Axis>(),
                                      v.theVisualizationObject.Y_AXIS.GetComponent<Axis>(),
                                      ref ftl, ref ftr, ref fbl, ref fbr, ref btl, ref btr, ref bbl, ref bbr);
                    break;
                case simpleVisualisationAxis.Z:
                    CalculateCorners3(v.theVisualizationObject.Z_AXIS.GetComponent<Axis>(),
                                      v.theVisualizationObject.Z_AXIS.GetComponent<Axis>(),
                                      v.theVisualizationObject.Z_AXIS.GetComponent<Axis>(),
                                      ref ftl, ref ftr, ref fbl, ref fbr, ref btl, ref btr, ref bbl, ref bbr);
                    break;
                case simpleVisualisationAxis.XY:
                    CalculateCorners3(v.theVisualizationObject.X_AXIS.GetComponent<Axis>(),
                                      v.theVisualizationObject.Y_AXIS.GetComponent<Axis>(),
                                      v.theVisualizationObject.Y_AXIS.GetComponent<Axis>(),
                                      ref ftl, ref ftr, ref fbl, ref fbr, ref btl, ref btr, ref bbl, ref bbr);
                    break;
                case simpleVisualisationAxis.XZ:
                    CalculateCorners3(v.theVisualizationObject.X_AXIS.GetComponent<Axis>(),
                                      v.theVisualizationObject.Z_AXIS.GetComponent<Axis>(),
                                      v.theVisualizationObject.Z_AXIS.GetComponent<Axis>(),
                                      ref ftl, ref ftr, ref fbl, ref fbr, ref btl, ref btr, ref bbl, ref bbr);
                    break;
                case simpleVisualisationAxis.YZ:
                    CalculateCorners3(v.theVisualizationObject.Z_AXIS.GetComponent<Axis>(),
                                      v.theVisualizationObject.Y_AXIS.GetComponent<Axis>(),
                                      v.theVisualizationObject.Z_AXIS.GetComponent<Axis>(),
                                      ref ftl, ref ftr, ref fbl, ref fbr, ref btl, ref btr, ref bbl, ref bbr);
                    break;
                case simpleVisualisationAxis.XYZ:
                    CalculateCorners3(v.theVisualizationObject.X_AXIS.GetComponent<Axis>(),
                                      v.theVisualizationObject.Y_AXIS.GetComponent<Axis>(),
                                      v.theVisualizationObject.Z_AXIS.GetComponent<Axis>(),
                                      ref ftl, ref ftr, ref fbl, ref fbr, ref btl, ref btr, ref bbl, ref bbr);
                    break;
                default:
                    break;
            }

            ftl = v.transform.TransformPoint(v.transform.InverseTransformVector((ftl)));
            ftr = v.transform.TransformPoint(v.transform.InverseTransformVector((ftr)));
            fbl = v.transform.TransformPoint(v.transform.InverseTransformVector((fbl)));
            fbr = v.transform.TransformPoint(v.transform.InverseTransformVector((fbr)));

            btl = v.transform.TransformPoint(v.transform.InverseTransformVector((btl)));
            btr = v.transform.TransformPoint(v.transform.InverseTransformVector((btr)));
            bbl = v.transform.TransformPoint(v.transform.InverseTransformVector((bbl)));
            bbr = v.transform.TransformPoint(v.transform.InverseTransformVector((bbr)));
        }

        public void LinkVisualisations()
        {
            View pv = GetComponentInChildren<View>();

            if (pv != null)
                DestroyImmediate(pv.gameObject);

            ViewBuilder viewLinked;

            Vector3[] vSource = visualisationSource.theVisualizationObject.viewList[0].GetPositions();// bigMesh.getBigMeshVertices();
            Vector3[] vDest = visualisationTarget.theVisualizationObject.viewList[0].GetPositions();// bigMesh.getBigMeshVertices();
            List<Color> colors = new List<Color>();
            Color[] cSource = visualisationSource.theVisualizationObject.viewList[0].GetColors();// bigMesh.getColors();
            Color[] cDest = visualisationTarget.theVisualizationObject.viewList[0].GetColors();// bigMesh.getColors();

            List<Vector3> vertexBuffer = new List<Vector3>();
            List<int> indexBuffer = new List<int>();

            List<float> positionsLocalX = new List<float>();
            List<float> positionsLocalY = new List<float>();
            List<float> positionsLocalZ = new List<float>();
            List<Vector3> normalsBuffer = new List<Vector3>();

            for (int i = 0; i < vSource.Length; i++)
            {
                positionsLocalX.Add((vSource[i]).x);
                positionsLocalX.Add((vDest[i]).x);
                positionsLocalY.Add((vSource[i]).y);
                positionsLocalY.Add((vDest[i]).y);
                positionsLocalZ.Add((vSource[i]).z);
                positionsLocalZ.Add((vDest[i]).z);

                colors.Add(cSource[i]);
                colors.Add(cDest[i]);

                normalsBuffer.Add(new Vector3((float)i, 0, 0));
                normalsBuffer.Add(new Vector3((float)i, 0, 1));
            }

            for (int i = 0; i < positionsLocalX.Count; i += 2)
            {
                indexBuffer.Add(i);
                indexBuffer.Add(i + 1);
            }

            viewLinked = new ViewBuilder(MeshTopology.Lines, "Linked Visualisation");
            viewLinked.initialiseDataView(positionsLocalX.Count);
            viewLinked.setDataDimension(positionsLocalX.ToArray(), ViewBuilder.VIEW_DIMENSION.X);
            viewLinked.setDataDimension(positionsLocalY.ToArray(), ViewBuilder.VIEW_DIMENSION.Y);
            viewLinked.setDataDimension(positionsLocalZ.ToArray(), ViewBuilder.VIEW_DIMENSION.Z);

            viewLinked.Indices = indexBuffer;
            viewLinked.updateView();
            view = viewLinked.apply(this.gameObject, linkerMaterial);
            view.SetColors(colors.ToArray());
            view.SetAllChannels(normalsBuffer.ToArray());

            view.RecalculateBounds();

            transform.position = Vector3.zero;
            GetComponentInChildren<View>().transform.position = Vector3.zero;

        }

        void CalculateCorners1(Axis ax, ref Vector3 ftl, ref Vector3 ftr, ref Vector3 fbl, ref Vector3 fbr,
            ref Vector3 btl, ref Vector3 btr, ref Vector3 bbl, ref Vector3 bbr)
        {
            Vector3 up = ax.transform.TransformVector(Vector3.up);
            Vector3 down = ax.transform.TransformVector(Vector3.down);
            Vector3 forward = ax.transform.TransformVector(Vector3.up);
            Vector3 back = ax.transform.TransformVector(Vector3.zero);

            ftl = up; //+ forward;
            ftr = up + ax.transform.TransformVector(Vector3.up) + forward;
            fbl = down + forward;
            fbr = down + ax.transform.TransformVector(Vector3.up) + forward;

            btl = up; //+ back;
            btr = up + ax.transform.TransformVector(Vector3.up) + back;
            bbl = down + back;
            bbr = down + ax.transform.TransformVector(Vector3.up) + back;
        }

        void CalculateCorners3(Axis axisA, Axis axisB, Axis axisC, ref Vector3 ftl, ref Vector3 ftr, ref Vector3 fbl, ref Vector3 fbr, ref Vector3 btl, ref Vector3 btr, ref Vector3 bbl, ref Vector3 bbr)
        {
            Vector3 up = axisA.transform.TransformVector(Vector3.up);
            Vector3 down = axisA.transform.TransformVector(Vector3.zero);
            Vector3 forward = axisC.transform.TransformVector(Vector3.up);
            Vector3 back = axisC.transform.TransformVector(Vector3.zero);

            ftl = up + forward;
            ftr = up + axisB.transform.TransformVector(Vector3.up) + forward;
            fbl = down + forward;
            fbr = down + axisB.transform.TransformVector(Vector3.up) + forward;

            btl = up + back;
            btr = up + axisB.transform.TransformVector(Vector3.up) + back;
            bbl = down + back;
            bbr = down + axisB.transform.TransformVector(Vector3.up) + back;
        }

        //public override void CreateVisualisation()
        //{
        //    throw new System.NotImplementedException();
        //}

        //public override void UpdateVisualisation(PropertyType propertyType)
        //{
        //    throw new System.NotImplementedException();
        //}

        //public override void SaveConfiguration()
        //{
        //    throw new System.NotImplementedException();
        //}

        //public override void LoadConfiguration()
        //{
        //    throw new System.NotImplementedException();
        //}

        //public override Color[] mapColoursContinuous(float[] data)
        //{
        //    throw new System.NotImplementedException();
        //}
    }
}