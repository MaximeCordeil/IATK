using UnityEngine;
using IATK;
using System.Linq;


public class Activity0_Uber : MonoBehaviour {
    
    // input data
    public TextAsset uberData;

    //IATK elements
    CSVDataSource csvdata;
    View v;
    //inputs

    public Transform t0;
    public Transform t1;

    View[] accordion;

    // Use this for initialization
    void Start () {
        csvdata = createCSVDataSource(uberData.text);
        Uber(csvdata);
        //FacetBy("Base");
    }

    void FacetBy(string attribute)
    {
        // categories
        // "B02598";
        // "B02512"

        float[] uniqueValues = csvdata[attribute].MetaData.categories;
        accordion = new View [uniqueValues.Length];

        for (int i = 0; i < uniqueValues.Length; i++)
        {
            View view = Facet(csvdata,
            csvdata.getOriginalValue(uniqueValues[i], attribute).ToString(), "Base", Random.ColorHSV());
            view.transform.position = new Vector3(i,0,0);
            accordion[i] = view;
        }

        //View v1 = Faceting(csvdata, "B02598");
        //v1.transform.position = Vector3.zero;

        //View v2 = Faceting(csvdata, "B02512");
        //v2.transform.position = Vector3.right;


    }

    CSVDataSource createCSVDataSource(string data)
    {
        CSVDataSource dataSource;
        dataSource = gameObject.AddComponent<CSVDataSource>();
        dataSource.load(data, null);
        return dataSource;
    }

    public Vector3 Spherical(float r, float theta, float phi)
    {
        Vector3 pt = new Vector3();
        float snt = (float)Mathf.Sin(theta * Mathf.PI / 180);
        float cnt = (float)Mathf.Cos(theta * Mathf.PI / 180);
        float snp = (float)Mathf.Sin(phi * Mathf.PI / 180);
        float cnp = (float)Mathf.Cos(phi * Mathf.PI / 180);
        pt.x = r * snt * cnp;
        pt.y = r * cnt;
        pt.z = -r * snt * snp;
        return pt;
    }

    // a space time cube
    View Uber(CSVDataSource csvds)
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
            setSize(csvds["Base"].Data).
            setColors(csvds["Time"].Data.Select(x => g.Evaluate(x)).ToArray());

        // initialise the view builder wiith thhe number of data points and parent GameOBject

        //Enumerable.Repeat(1f, dataSource[0].Data.Length).ToArray()
        Material mt = IATKUtil.GetMaterialFromTopology(AbstractVisualisation.GeometryType.Points);            
        mt.SetFloat("_MinSize", 0.01f);
        mt.SetFloat("_MaxSize", 0.05f);

        View v = vb.updateView().apply(gameObject, mt);
        return v;
    }

    delegate float[] Filter(float[] ar, CSVDataSource csvds, string fiteredValue, string filteringAttribute);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="csvds">CSV data source</param>
    /// <param name="filteringValue"> filtered value</param>
    /// <param name="filteringAttribute"> filtering attribute </param>
    /// <param name="color"></param>
    /// <returns></returns>
    View Facet(CSVDataSource csvds, string filteringValue, string filteringAttribute, Color color)
    {
        //B02598
        //B02512

        // filters the array on a particular value in another dimension
        Filter baseFilter = (array, datasource, value, dimension) =>
        {
            return array.Select((b, i) => new { index = i, _base = b })
            .Where(b => datasource.getOriginalValuePrecise(csvds[dimension].Data[b.index],dimension).ToString() == value)
            .Select(b => b._base).ToArray();
        };

        Filter identity = (ar,ds,fv,fa) => { return ar; };
       // baseFilter = identity;

        var xData = baseFilter(csvds["Lat"].Data,csvds, filteringValue,filteringAttribute);
        var yData = baseFilter(csvds["Lon"].Data, csvds, filteringValue, filteringAttribute);
        var zData = baseFilter(csvds["Base"].Data, csvds, filteringValue, filteringAttribute);
        
        ViewBuilder vb = new ViewBuilder(MeshTopology.Points, "Uber pick up point visualisation").
            initialiseDataView(xData.Length).
            setDataDimension(xData, ViewBuilder.VIEW_DIMENSION.X).
            setDataDimension(yData, ViewBuilder.VIEW_DIMENSION.Y).
            //setDataDimension(zData, ViewBuilder.VIEW_DIMENSION.Z).
            setSize(baseFilter(csvds["Date"].Data,csvds, filteringValue,filteringAttribute)).
            setColors(xData.Select(x => color).ToArray());

        Material mt = IATKUtil.GetMaterialFromTopology(AbstractVisualisation.GeometryType.Points);
        mt.SetFloat("_MinSize", 0.01f);
        mt.SetFloat("_MaxSize", 0.05f);

        return vb.updateView().apply(gameObject, mt);
    }

    void accordionPosition (ref Transform toAccordion, Transform left, Transform right, float pos, float nbOfViews)
    {
        toAccordion.position = Vector3.Lerp(left.position, right.position, pos/ nbOfViews);
        toAccordion.rotation = Quaternion.Lerp(left.rotation, right.rotation, pos / nbOfViews);
    }

    // Update is called once per frame
    void Update () {

        //accordion
        if (t0 != null && t1 != null && accordion!=null)
        {
            for (int i = 0; i < accordion.Length; i++)
            {
                accordion[i].transform.position = Vector3.Lerp(t0.position - new Vector3(0, 0.5f, 0), t1.position - new Vector3(0, 0.5f, 0), (float)i / (float)(accordion.Length - 1));
                accordion[i].transform.rotation = Quaternion.Lerp(t0.rotation, t1.rotation, (float)i / (float)(accordion.Length - 1));
            }
        }

    }
}
