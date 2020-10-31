using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IATK;
using System.Linq;

public class Activity1_lines : MonoBehaviour {

    public TextAsset dataSource;

	// Use this for initialization
	void Start () {
        CSVDataSource csvds = createCSVDataSource(dataSource.text);

        Gradient g = new Gradient();
        GradientColorKey[] gck = new GradientColorKey[2];
        gck[0] = new GradientColorKey(Color.blue, 0);
        gck[1] = new GradientColorKey(Color.red, 1);
        g.colorKeys = gck;

        //stock,date,open

        // create a view builder with the point topology
        ViewBuilder vb = new ViewBuilder(MeshTopology.Lines, "Uber pick up point visualisation").
                initialiseDataView(csvds.DataCount).
                setDataDimension(csvds["stock"].Data, ViewBuilder.VIEW_DIMENSION.X).
                setDataDimension(csvds["date"].Data, ViewBuilder.VIEW_DIMENSION.Y).
                setDataDimension(csvds["open"].Data, ViewBuilder.VIEW_DIMENSION.Z).
                setSize(csvds["volume"].Data).
                setColors(csvds["volume"].Data.Select(x => g.Evaluate(x)).ToArray()).
                createIndicesConnectedLineTopology(csvds["stock"].Data);

        // create a view builder with the point topology

        Material mt = IATKUtil.GetMaterialFromTopology(AbstractVisualisation.GeometryType.Lines);
        //Material mt = new Material(Shader.Find("IATK/LinesShader"));
        mt.mainTexture = Resources.Load("circle-outline-basic") as Texture2D;
        mt.renderQueue = 3000;
        mt.SetFloat("_MinSize", 0.01f);
        mt.SetFloat("_MaxSize", 0.05f);

        View view = vb.updateView().apply(gameObject, mt);
	}

    CSVDataSource createCSVDataSource(string data)
    {
        CSVDataSource dataSource;
        dataSource = gameObject.AddComponent<CSVDataSource>();
        dataSource.load(data, null);
        return dataSource;
    }

	// Update is called once per frame
	void Update () {
		
	}
}
