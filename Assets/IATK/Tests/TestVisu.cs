using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IATK;
using System.IO;
using System;
using System.Linq;


public class TestVisu : MonoBehaviour
{
    View v;
    public TextAsset trajectories;
    public TextAsset uberData;
    CSVDataSource csvdata;

    public Transform t0;
    public Transform t1;

    // Use this for initialization
    void Start()
    {
        csvdata = createCSVDataSource(uberData.text);
        //flights(createCSVDataSource(trajectories.text));
       v = uber(csvdata);
    }

    CSVDataSource createCSVDataSource(string data)
    {
        CSVDataSource dataSource;
        dataSource = gameObject.AddComponent<CSVDataSource>();
        dataSource.load(data, null);
        return dataSource;
    }

    // a space time cube
    View uber(CSVDataSource csvds)
    {
        // header
        // Date,Time,Lat,Lon,Base
        Gradient g = new Gradient();
        GradientColorKey[] gck = new GradientColorKey[2];
        gck[0] = new GradientColorKey(Color.blue, 0);
        gck[1] = new GradientColorKey(Color.red, 1);
        g.colorKeys = gck;

        // create a view builder with the point topology
        ViewBuilder vb = new ViewBuilder(MeshTopology.Points, "Uber pick up point visualisation").
            initialiseDataView(csvds.DataCount).
            setDataDimension(csvds["Lat"].Data, ViewBuilder.VIEW_DIMENSION.X).
            setDataDimension(csvds["Base"].Data, ViewBuilder.VIEW_DIMENSION.Y).
            setDataDimension(csvds["Lon"].Data, ViewBuilder.VIEW_DIMENSION.Z).
            setSize(csvds["Base"].Data).
            setColors(csvds["Time"].Data.Select(x => g.Evaluate(x)).ToArray());

        // initialise the view builder wiith thhe number of data points and parent GameOBject

        //Enumerable.Repeat(1f, dataSource[0].Data.Length).ToArray()
        Material mt = new Material(Shader.Find("IATK/OutlineDots"));
        //Material mt = new Material(Shader.Find("IATK/LinesShader"));
        mt.mainTexture = Resources.Load("circle-outline-basic") as Texture2D;
        mt.renderQueue = 3000;
        mt.SetFloat("_MinSize", 0.01f);
        mt.SetFloat("_MaxSize", 0.05f);

      return vb.updateView().apply(gameObject, mt);


    }

    View flights(CSVDataSource csvds)
    {
        // create a view builder with a Lines topology
        ViewBuilder vb = new ViewBuilder(MeshTopology.Lines, "Flights visualisation");

        //initialise the view builder with the number of data points parent GameObejct
        vb.initialiseDataView(csvds.DataCount);

        //Gradient g = new Gradient();

        //GradientColorKey[] gck = new GradientColorKey[2];
        //gck[0] = new GradientColorKey(Color.blue, 0);
        //gck[1] = new GradientColorKey(Color.blue, 1);
        //g.colorKeys = gck;

        var logT = csvds["y"].Data.Select(x => Mathf.Log(x)).ToArray();

        vb.setDataDimension(csvds["X"].Data, ViewBuilder.VIEW_DIMENSION.X).
        setDataDimension(csvds["y"].Data, ViewBuilder.VIEW_DIMENSION.Z).
        setSize(csvds["X"].Data).
        createIndicesConnectedLineTopology(csvds["vol"].Data).
        setDataDimension(csvds["modec"].Data.Select(x=>x).ToArray(), ViewBuilder.VIEW_DIMENSION.Y);

        //Enumerable.Repeat(1f, dataSource[0].Data.Length).ToArray()
        //   Material mt = new Material(Shader.Find("IATK/OutlineDots"));
        //  mt.mainTexture = Resources.Load("circle-outline-basic") as Texture2D;
        //  mt.renderQueue = 3000;

        Material mt = new Material(Shader.Find("IATK/LineAndDotsShader"));
        mt.renderQueue = 3000;
        mt.SetFloat("_MinSize", 0.01f);
        mt.SetFloat("_MaxSize", 0.05f);
        View view = vb.updateView().apply(gameObject, mt);//.UpdateViewColors(Color.red);
        return view;
    }

    
    // Update is called once per frame
    void Update()
    {
        v.SetSize(Vector3.Distance(t1.transform.position, t0.transform.position));
    }

    #region old test
    //void test2()
    //{
    //    v = gameObject.AddComponent<Visualisation>();
    //    v.dataSource = dataSource;

    //    v.visualisationType = AbstractViualisation.VisualisationTypes.SIMPLE_VISUALISATION;

    //    v.xDimension.Attribute = "fixed acidity";
    //    v.yDimension.Attribute = "citric acid";
    //    v.geometry = AbstractViualisation.GeometryType.Points;
    //    v.sizeDimension = "citric acid";
    //    v.colourDimension = "Undefined";
    //    v.colour = Color.red;

    //    v.size = 0.5f;
    //    //v.minSize = 0.1f;
    //    //v.maxSize = 0.3f;

    //    v.CreateVisualisation(AbstractViualisation.VisualisationTypes.SIMPLE_VISUALISATION);
    //    v.updateProperties();

    //}

    //void testVisuBostonBikePath()
    //{
    //    v = gameObject.AddComponent<Visualisation>();
    //    v.dataSource = dataSource;

    //    v.name = "Bike paths in Boston";
    //    v.linkingDimension = "Route";
    //    v.geometry = AbstractViualisation.GeometryType.Lines;

    //    //Latitude,Longitude,Activity,Date,Time,Route

    //    v.xDimension.Attribute = dataSource["Latitude"].Identifier;
    //    v.yDimension.Attribute = dataSource["Longitude"].Identifier;
    //    //v.colourDimension = dataSource[i].Identifier;
    //    v.colour = Color.red;
    //    v.updateViewProperties(IATK.AbstractViualisation.PropertyType.Colour);

    //    //      v.transform.localPosition = new Vector3(i, j, 0f);
    //    ////  }
    //    //   }

    //    // v.zDimension = "step";

    //    //v.colourDimension = "X";
    //    v.colour = Color.black;


    //    //        v.size = 0.1f;

    //    //    v.updateView();
    //}

    //void testVisualisationAircraftTrajs()
    //{
    //    v = gameObject.AddComponent<Visualisation>();
    //    v.dataSource = dataSource;

    //    v.linkingDimension = "VOL";
    //    v.geometry = AbstractViualisation.GeometryType.Lines;

    //    v.xDimension.Attribute = "X";
    //    v.yDimension.Attribute = "Y";
    //    v.zDimension.Attribute = "MODEC";
    //    v.sizeDimension = "X";
    //    //v.colourDimension = "X";
    //    v.colour = Color.black;


    //    //        v.size = 0.1f;

    //    //v.updateView();
    //    print("created from test script >> ");

    //    //v2 = gameObject.AddComponent<Visualisation>();
    //    //v2.dataSource = dataSource;

    //    //v2.geometry = Visualisation.GeometryType.Points;

    //    //v2.xDimension = "X";
    //    //v2.yDimension = "Y";
    //    //v2.zDimension = "MODEC";

    //    //v2.colourDimension = "X";
    //    //v2.colour = Color.red;
    //    //v2.size = 0.1f;
    //}

    //void testVisualisationUBER_Light()
    //{
    //    v = gameObject.AddComponent<Visualisation>();
    //    v.dataSource = dataSource;

    //    //v.linkingDimension = "Time";
    //    v.geometry = AbstractViualisation.GeometryType.Points;

    //    v.xDimension.Attribute = "Lat";
    //    v.yDimension.Attribute = "Lon";
    //    //v.zDimension = "MODEC";

    //    //v.colourDimension = "X";
    //    v.size = 0.1f;

    //    //Debug.Log("Loaded " + dataSource.DataCount + " data points, and " + dataSource.DimensionCount + " dimensions");

    //    //v.updateView();

    //    //v2 = gameObject.AddComponent<Visualisation>();
    //    //v2.dataSource = dataSource;

    //    //v2.geometry = Visualisation.GeometryType.Points;

    //    //v2.xDimension = "X";
    //    //v2.yDimension = "Y";
    //    //v2.zDimension = "MODEC";

    //    //v2.colourDimension = "X";
    //    //v2.colour = Color.red;
    //    //v2.size = 0.1f;

    //    //find the outliers
    //    //float Minxs = dataSource["Lat"].MetaData.minValue;
    //    //float Minys = dataSource["Lon"].MetaData.minValue;

    //    //float Maxs = dataSource["Lat"].MetaData.maxValue;
    //    //float Maxys = dataSource["Lon"].MetaData.maxValue;


    //    //float[] xs = dataSource["Lat"].Data;
    //    //float[] ys = dataSource["Lon"].Data;


    //    //List<int> indxs = new List<int>();
    //    //List<int> indys = new List<int>();

    //    //for (int i = 0; i < xs.Length; i++)
    //    //{
    //    //    float x = UtilMath.normaliseValue(xs[i], 0f, 1f, Minxs, Maxs);
    //    //    float y = UtilMath.normaliseValue(ys[i], 0f, 1f, Minys, Maxys);

    //    //    if (x < 39f || x > 42f)
    //    //        indxs.Add(i);
    //    //    if (y < -74f || y > 73f)
    //    //        indxs.Add(i);

    //    //}

    //    //   Debug.Log(indxs.ToString());

    //}
    #endregion


}
