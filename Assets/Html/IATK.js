var IATK = {};
IATK.GetNewChartObjectIATK = internal_GetNewChartObjectIATK;

var WEBAPI = {};
WEBAPI.uiurl = "http://someip:someport";

/**
 * 
 * @param {number} mId
 * @param {number} mOrder
 * @param {string} mPrefabName
 * @param {number} mCanvasSize
 * @param {number} mCanvasSpacedSize
 * @returns {ChartDataIATK}
 */
function internal_GetNewChartObjectIATK(mId, mOrder, mCanvasSize, mCanvasSpacedSize) {

    var chartData = {};
    chartData.lib = "IATK";
    chartData.id = mId;
    chartData.xpos = mOrder * mCanvasSpacedSize;
    chartData.prefix = "UCanvas";
    chartData.chartGoName = chartData.prefix + "_" + mId;
    chartData.panelGoName = chartData.chartGoName + "_Panel";
    chartData.dataGoName = "CGOVIS-" + chartData.chartGoName;
    chartData.panelContentUrl = WEBAPI.uiurl + "/chartsub.html";
    chartData.chartSize = mCanvasSize;
    chartData.panelInitDelayMs = 1000;
    chartData.chartGo = null;
    chartData.panelGo = null;
    chartData.dataGo = null;
    chartData.storedDimensions = [];
    chartData.countPerDimension = [];

    //IATK
    chartData.rtds = null; //IATKExt.RealTimeDataSource
    chartData.vis = null; //IATK.Visualisation
    chartData.abstractVisualisation = null;

    /**
     * updates the chart-id in case of re-init
     * @param {number} mId
     */
    chartData.fctUpdateId = function (mId) {
        chartData.id = mId;
        chartData.chartGoName = chartData.prefix + "_" + mId;
        chartData.panelGoName = chartData.chartGoName + "_Panel";
        chartData.dataGoName = "CGOVIS-" + chartData.chartGoName;
    };

    /**
     * update GameObject refs in case of name changes or re-hooking
     * */
    chartData.fctUpdateRefs = function () {
        try {
            var UE = importNamespace("UnityEngine");
            chartData.chartGo = UE.GameObject.Find(chartData.chartGoName);
            chartData.panelGo = UE.GameObject.Find(chartData.panelGoName);
            chartData.dataGo = UE.GameObject.Find(chartData.dataGoName);
        } catch (err) {
            console.log("chartData.fctUpdateRefs ERROR => " + err);
        }
    };

    /**
     * 
     * @param {number} mOrder
     */
    chartData.fctUpdatePos = function (mOrder) {
        console.log("chartData.fctInit @ mXpos=" + mOrder);
        this.xpos = mOrder * mCanvasSpacedSize;
        var cs = this.chartSize;
        //RT.Unity.SetLocalPose(this.chartGo,
        //    [this.xpos, 0, 0, 0], // T
        //    [0, 0, 0, 1], // R
        //    [cs, cs, cs] //S
        //);
    };

    chartData.fctDestroy = function () {
        console.log("chartData.fctDestroy @ " + this.id +
            ", " + this.chartGoName + ", " + this.panelGoName);

        this.id = -1;
        if (this.chartGo != null) {
            //RT.Unity.DestroyGO(this.dataGo);
            //RT.Unity.DestroyGO(this.chartGo);
        }

        if (this.panelGo != null) {
            this.panelGo.Expire();
        }
    };

    //IATK
    chartData.GeometryType = { //AbstractVisualisation.GeometryType
        "Undefined": 0,
        "Points": 1,
        "Lines": 2,
        "Quads": 3,
        "LinesAndDots": 4,
        "Cubes": 5,
        "Bars": 6,
        "Spheres": 7
    };

    chartData.PropertyType = { //AbstractVisualisation.PropertyType
        "None": 0,
        "X": 1,
        "Y": 2,
        "Z": 3,
        "Colour": 4,
        "Size": 5,
        "GeometryType": 6,
        "LinkingDimension": 7,
        "OriginDimension": 8,
        "DestinationDimension": 9,
        "GraphDimension": 10,
        "DimensionFiltering": 11,
        "Scaling": 12,
        "BlendSourceMode": 13,
        "BlendDestinationMode": 14,
        "AttributeFiltering": 15,
        "DimensionChange": 16,
        "VisualisationType": 17,
        "SizeValues": 18,
        "DimensionChangeFiltering": 19,
        "VisualisationWidth": 20,
        "VisualisationHeight": 21,
        "VisualisationLength": 22
    };

    chartData.fctInitDataProvider = function () {
        try {
            var IATK = importNamespace('IATK');
            this.dataGo = new MAIN.UE.GameObject(this.dataGoName);
            this.rtds = this.dataGo.AddComponent(IATK.RealtimeDataSource);

            if (this.rtds != null) {
                this.rtds.AddDimension("id", 0, 1);
                for (var i = 0; i < 100; i++) {
                    this.rtds.AddDataByStr("id", i);
                }
            }
        } catch (err) {
            console.log("fctInitDataProvider ERROR => " + err);
        }
    }

    chartData.fctInit = function () {
        console.log("chartData.fctInit @ " + this.id + ", " + this.chartGoName);
        try {

            if (this.chartGo == null) {
                var IATK = importNamespace('IATK');
                this.chartGo = new importNamespace("UnityEngine").GameObject(this.chartGoName);
                this.vis = this.chartGo.AddComponent(IATK.Visualisation);

                if (this.vis.theVisualizationObject == null) {
                    this.vis.CreateVisualisation(0); //AbstractVisualisation.VisualisationTypes.SCATTERPLOT 0
                    this.abstractVisualisation = this.vis.theVisualizationObject;
                }

                this.vis.dataSource = this.rtds;
                this.abstractVisualisation.visualisationReference.xDimension.Attribute = "id";
                this.abstractVisualisation.UpdateVisualisation(this.PropertyType.X);
                this.abstractVisualisation.visualisationReference.yDimension.Attribute = "id";
                this.abstractVisualisation.UpdateVisualisation(this.PropertyType.Y);
                this.abstractVisualisation.visualisationReference.zDimension.Attribute = "id";
                this.abstractVisualisation.UpdateVisualisation(this.PropertyType.Z);
                this.abstractVisualisation.visualisationReference.sizeDimension = "id";
                this.abstractVisualisation.UpdateVisualisation(this.PropertyType.Size);

                this.abstractVisualisation.visualisationReference.size = 0.03;
                this.abstractVisualisation.visualisationReference.minSize = 0.01;
                this.abstractVisualisation.visualisationReference.maxSize = 0.2;
                //this.abstractVisualisation.visualisationReference.fontAxesSize = 800;
                this.abstractVisualisation.UpdateVisualisation(this.PropertyType.SizeValues);



                this.abstractVisualisation.visualisationReference.linkingDimension = "names";
                this.abstractVisualisation.UpdateVisualisation(this.PropertyType.LinkingDimension);

                //this.abstractVisualisation.visualisationReference.colourDimension = "id";
                //this.abstractVisualisation.UpdateVisualisation(this.PropertyType.Colour);

                //this.abstractVisualisation.UpdateVisualisation(this.PropertyType.SizeValues);
                //this.abstractVisualisation.UpdateVisualisation(this.PropertyType.GraphDimension);
                this.abstractVisualisation.UpdateVisualisation(this.PropertyType.None);

                this.fctChangeStyle(this.GeometryType.Points);
                this.fctUpdateChart();
            }

            var cs = this.chartSize;
            //RT.Unity.SetParent(this.chartGo, MAIN.usercanvas.rootGo);
            //RT.Unity.SetLocalPose(this.chartGo,
            //    [this.xpos, 0, 0, 0], // T
            //    [0, 0, 0, 1], // R
            //    [cs, cs, cs] //S
            //);
        } catch (err) {
            console.log("fctInit ERROR => " + err);
        }
    };

    chartData.fctUpdateChart = function () {
        console.log("chartData.fctUpdateChart @ " + this.id +
            ", " + this.chartGoName + ", " + this.panelGoName);
        try {
            if (this.vis != null) {
                this.vis.updateView(0);
            }
        } catch (err) {
            console.log("internal_GetNewChartObject fctUpdateChart ERROR => " + err);
        }
    };

    /**
     * @typedef {Object} RealtimeDimensionData
     * @property {Object.<string,string>} data
     */
    /**
     * 
     * @param {RealtimeDimensionData} mData
     * @param {number} mAxisIndicator
     */
    chartData.fctAddRealtimeDimension = function (mData, mAxisIndicator) {

        try {
            console.log("chartData.fctAddRealtimeDimension @ " + this.id +
                ", " + this.chartGoName + ", " + this.panelGoName);
            console.log("chartData.fctAddRealtimeDimension mData => " + mData);

            var dataObj = JSON.parse(mData);
            //add dimesion
            //add mqtt hook

            var fieldname = dataObj.data.fieldname;
            var topic = dataObj.data.mqtt;

            console.log("chartData.fctAddRealtimeDimension fieldname => " + fieldname + "|" + typeof fieldname);
            console.log("chartData.fctAddRealtimeDimension topic => " + topic + "|" + typeof topic);

            var uniqueFieldName = fieldname;
            var splitTopic = topic.split('/');
            if (splitTopic.length > 3) {
                uniqueFieldName = fieldname + "-" + splitTopic[4]; // telemetry/buidling/floor/room/device/part/skill -> take device
            }
            //var uniqueFieldName = fieldname + "-" + topic.split('/')[4]; // telemetry/buidling/floor/room/device/part/skill -> take device //eg telemetry/inffeld16/2nd/id2068/bigserver/DC-BIG/dcin
            this.storedDimensions.push(uniqueFieldName);
            //this.storedDimensions.push(fieldname);
            var dimensionId = this.storedDimensions.length - 1;
            console.log("chartData.fctAddRealtimeDimension storedDimensions=" + this.storedDimensions + ", dimensionId=" + dimensionId);


            if (this.rtds != null) {
                this.rtds.AddDimension(uniqueFieldName, 0, 100); //fix normalization problem
                //this.rtds.AddStrDataByStr("names", uniqueFieldName);
                for (var i = 0; i < 100; i++) {
                    this.rtds.AddDataByStr("id", i);
                }
            }

            this.countPerDimension[uniqueFieldName] = {};
            this.countPerDimension[uniqueFieldName].id = 0;


            //TODO add function to manipulate dimensions

            if (mAxisIndicator == 0) {
                this.abstractVisualisation.visualisationReference.xDimension.Attribute = uniqueFieldName;
                this.abstractVisualisation.UpdateVisualisation(this.PropertyType.X);
            } else if (mAxisIndicator == 1) {
                this.abstractVisualisation.visualisationReference.yDimension.Attribute = uniqueFieldName;
                this.abstractVisualisation.UpdateVisualisation(this.PropertyType.Y);
            } else if (mAxisIndicator == 2) {
                this.abstractVisualisation.visualisationReference.zDimension.Attribute = uniqueFieldName;
                this.abstractVisualisation.UpdateVisualisation(this.PropertyType.Z);
            } else if (mAxisIndicator == 3) {
                this.abstractVisualisation.visualisationReference.sizeDimension = uniqueFieldName;
                this.abstractVisualisation.UpdateVisualisation(this.PropertyType.Size);
            } else if (mAxisIndicator == 4) { //color
                var UE = importNamespace("UnityEngine");
                var gradient = new UE.Gradient();

                // Populate the color keys at the relative time 0 and 1 (0 and 100%)
                var colorKey = [new UE.GradientColorKey(), new UE.GradientColorKey()];
                colorKey[0].color = new UE.Color(1, 0, 0);
                colorKey[0].time = 0.0;
                colorKey[1].color = new UE.Color(0, 1, 0)
                colorKey[1].time = 1.0;

                // Populate the alpha  keys at relative time 0 and 1  (0 and 100%)
                var alphaKey = [new UE.GradientAlphaKey(), new UE.GradientAlphaKey()];
                alphaKey[0].alpha = 1.0;
                alphaKey[0].time = 0.0;
                alphaKey[1].alpha = 0.0;
                alphaKey[1].time = 1.0;
                gradient.SetKeys(colorKey, alphaKey);

                this.abstractVisualisation.visualisationReference.colourDimension = uniqueFieldName;
                //this.abstractVisualisation.UpdateVisualisation(this.PropertyType.Colour);

                this.abstractVisualisation.visualisationReference.dimensionColour = gradient;
                this.abstractVisualisation.UpdateVisualisation(this.PropertyType.Colour);
            }
            this.fctUpdateChart();

			/*
            //RT.MQTT.Subscribe(topic);
            RT.MQTT.RegisterCallbackTopic(function (mTopic, mPayload) {
                //console.log("chartData.fctAddRealtimeDimension mqtt cb => " + mTopic + "|" + mPayload);
                var dobj = JSON.parse(mPayload);
                if (dobj.hasOwnProperty(fieldname)) {
                    try {
                        var uniqueFieldName = fieldname;
                        var splitTopic = topic.split('/');
                        if (splitTopic.length > 3) {
                            uniqueFieldName = fieldname + "-" + splitTopic[4]; // telemetry/buidling/floor/room/device/part/skill -> take device
                        }
                        var mVal = dobj[fieldname];
                        mVal = parseFloat(mVal);

                        var dirty = chartData.rtds.AddDataByStr(uniqueFieldName, mVal);
                        chartData.fctUpdateChart();

                    } catch (err) {
                        console.log("chartData.fctAddRealtimeDimension mqtt cb ERROR => " + err);
                    }
                }
            }, topic);
			*/
        } catch (err) {
            console.log("chartData.fctAddRealtimeDimension ERROR => " + err);
        }
    };

    chartData.fctChangeStyle = function (mStyle) {
        if (this.vis != null) {
            this.vis.geometry = mStyle;
            this.fctUpdateChart();
        }
    }


    chartData.fctInitDataProvider();
    chartData.fctInit();
    chartData.fctInitWorldUi();

    //chartData.fctInitChartPanel();
    return chartData;
}