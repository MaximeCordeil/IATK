# IATK 1.1 (Mala): Immersive Analytics Toolkit 
<img align="left" width="390" alt="2dscatterplot" src=https://user-images.githubusercontent.com/11532065/46409367-9bd0c500-c758-11e8-9485-c92d532d97b8.jpg> **IATK: Immersive Analytics Toolkit** is a Unity project to help you build **high quality**, **interactive** and **scalable** **data visualisations** in **Immersive Environments (Virtual/Augmented Reality)**. Use the *Visualisation* script to create data visualisations interactively in the editor, press play and view and interact with your data in VR/AR. Write simple code to use the *IATK* core graphics components to make your own interactive visualisations programmatically.

IATK is an ongoing and open project! We setup a roadmap with features that we would like to see in IATK in a near future.

# Publication
IATK was published at IEEE VR 2019 - Please reference using the [reference file](CITATION.bib) or the reference below:

@inproceedings{cordeil2019iatk,<br/>
  title={IATK: An Immersive Analytics Toolkit},<br/>
  author={Cordeil, Maxime and Cunningham, Andrew and Bach, Benjamin and Hurter, Christophe and Thomas, Bruce H and Marriott, Kim and Dwyer, Tim},<br/>
  booktitle={2019 IEEE Conference on Virtual Reality and 3D User Interfaces (VR)},<br/>
  pages={200--209},<br/>
  year={2019},<br/>
  organization={IEEE} }


# Requirements
- This repository
- [Unity 2021.3.4f1](https://unity3d.com/get-unity/)
- [MapBox Unity Plugin v2.1.1](https://www.mapbox.com/install/unity/)
  - A MapBox account and access token are not required unless working directly with map visualisations
  - Only the main package is required <br/><img width="265" alt="image" src="https://user-images.githubusercontent.com/1362512/169925401-44bfc78f-205d-4830-9be3-da257ec88368.png">



# Tutorial
A video tutorial of IATK [can be found here](https://vimeo.com/320646218)
The actual tutorial starts at 01:12
<!--[the slides of our tutorial are available here](https://drive.google.com/open?id=1-uBezockeTltw1x-5gcNsQrAPJ9v08up) -->

A written tutorial explaining how to a scatterplot visualisation can be found here: [How to create a Visualisation (Non-VR)](https://github.com/MaximeCordeil/IATK/blob/master/Documentation/Visualisation_Tutorial.md)

***IATK works on Windows machines -  Mac users please [install bootcamp](https://support.apple.com/en-au/HT201468)***

Setup a VR scene with the Oculus Rift in Unity: https://www.youtube.com/watch?v=psPVNddjgGw

Setup a VR scene with the HTC Vive in Unity: https://www.youtube.com/watch?v=tyFV9oBReqg

Our repository contains sample datasets. Come with your CSV data to the tutorial if you want to give it a go!

A written tutorial explaining how to setup Unity and get an example scene working with a VR interactable visualisation can be found here: [How to create a VR Visualisation](https://github.com/MaximeCordeil/IATK/blob/master/Documentation/VR_TUTORIAL.md)

# IATK editor components
## Data Source: import data into the scene
First, create a **Data Source** object (right click in the hierarchy, IATK/DataSouce). Drag and drop a CSV/TSV in the *Data field*. Your data is now imported into the Unity scene!

## Visualisation: an editor menu helper
IATK has a number of predefined, controllable data visualisation templates that are accessible directly in the editor. The **Visualisation** object allows you to access those templates.

Create a **Visualisation** object (right click in the hierarchy, IATK/Visualisation). Drag and drop a **Data Source** object in the *Data Source* field of the Visualisation object. You are now ready to design a data visualisation with the following templates:


# Visualisations

## VR Visualisation
<img align="right" width="230" alt="Pre-configured VR visualisation" src="https://user-images.githubusercontent.com/1362512/173139106-914f7ce0-14cc-4f92-8c47-cab728505e42.png">

#### VRTK VR/AR Visualisation with interactable handles for scaling
To create a pre-configured **VR Visualisation** object: right click in the hierarchy, IATK/VR Visualisation. Drag and drop a **Data Source** object in the *Data Source* field of the VR Visualisation object. VR Visualisation use [VRTK v4 Tilia packages](https://www.vrtk.io/tilia.html) and support any [OpenXR compatible headset](https://www.khronos.org/conformance/adopters/conformant-products/openxr), including Oculus Quest, Oculus Quest 2, Microsoft HoloLens, and Valve Index.

Please note that VR Visualisations are currently only configured to work with scatterplot visualisations, other visualisation types will need to be manually configured.

## Simple visualisations: 
#### 2D/3D Scatterplots
<img height="200" alt="2dscatterplot" src=https://user-images.githubusercontent.com/11532065/46408763-8eb2d680-c756-11e8-813f-e3114a63215d.png> <img height="200" alt="3dscatterplot" src=https://user-images.githubusercontent.com/11532065/46408760-89ee2280-c756-11e8-9fa2-add36ce6bdda.png>
 
#### 2D/3D Bar Charts
<img height="200" alt="2dbarchart" src=https://user-images.githubusercontent.com/11532065/46408758-89ee2280-c756-11e8-8d18-b1a6a5997276.png> <img height="200" alt="3dbarchart" src=https://user-images.githubusercontent.com/11532065/46408759-89ee2280-c756-11e8-99e6-8ea0339d1b7d.png>
 
#### 2D/3D Trails/Trajectories (use linking field to bind a *linking attribute*)
<img height="200" alt="3dsparklines" src=  https://user-images.githubusercontent.com/11532065/46444864-4677d080-c7b7-11e8-9b54-9f7191841c51.JPG > <img height="200" alt="3dsparklines" src=https://user-images.githubusercontent.com/11532065/46444863-45df3a00-c7b7-11e8-88ef-b88cad461efa.JPG> <img height="200"  alt="3dsparklines" src=https://user-images.githubusercontent.com/11532065/46444862-45df3a00-c7b7-11e8-8168-4c3d81047730.JPG> 
  
#### 2D/3D Connected Dots (use linking field to bind a *linking attribute)
<img height="200" alt="3dsparklines" src=  https://user-images.githubusercontent.com/11532065/46441718-b9c71580-c7aa-11e8-8c80-3b0d1122f078.JPG > <img height="200" alt="3dsparklines" src=https://user-images.githubusercontent.com/11532065/46408616-0a605380-c756-11e8-93d9-e0ec49b3b2a7.png>
  
#### Parallel Coordinates Plots (PCPs)
<img alt="parallelcoord" src=https://user-images.githubusercontent.com/11532065/46409268-43012c80-c758-11e8-9484-3fc9a7ecd783.JPG> 

## Scatterplot Matrices:
#### 2D Scatterplot Matrix
<img height="200" alt="3dsplom" src=https://user-images.githubusercontent.com/11532065/46442039-bda76780-c7ab-11e8-829a-3becf85efcf4.JPG>
  
#### 3D Scatterplot Matrix
<img height="200" alt="3dsplom" src=https://user-images.githubusercontent.com/11532065/46408791-a9854b00-c756-11e8-8c05-21ce94e2f463.png> <img height="200" alt="3dsplomcloseup" src=https://user-images.githubusercontent.com/11532065/46442262-5b029b80-c7ac-11e8-8abc-beeda5040efa.JPG> <img height="200" alt="3dsplomcloseup" src=https://user-images.githubusercontent.com/11532065/46408792-aa1de180-c756-11e8-9fb8-5f92e5b0fa9e.png>

# Visualisation designer in the Unity Editor
<img align="left" width="280" alt="3dsparklines" src=  https://user-images.githubusercontent.com/11532065/46445150-acb12300-c7b8-11e8-98b1-22cd2f1eba65.png>  The visualisation component allows the design of the visualisation inside the Unity Editor. Visual variables can be attached to data dimension attributes.

- Geometry: defines the geometry of the visualisation. **Points, Quads, Bars** and **Cubes** are a single point topology. **Lines and Connected Lines and Dots** are a line topology and they require you to specify a *Linking dimension* (see below)
- Colour dimension: use the dropdown to bind a data attribute to a continuous colour gradient. Click the Colour gradient to edit it.
- Bind Colour palette: use the dropdown to bind a discrete data attribute to a discrete colour palette. Click the corresponding colour values to edit the palette.
- Blending Mode Source, Destination: lets you specify the blending mode. By default it's set to SrcAlpha, OneMinusSrcAlpha that allows for traditional blending with transparency. Use One, One to do visual accumation effects.
- Colour: if *Colour dimension* and *Bind Colour palette* are *Undefined*, sets the same colour to all the glyphs.
- Size dimension: use the dropdown to bind an attribute to the size of the glyphs. The Size slider sets the global size, the Min/Max Size slider sets the scale.
- Linking dimension: use the dropdown to link datapoints by id. **/!\ It requires that your data is ordered in sequence in the CSV source**
- Attribute Filters: type in an attribute name and use filters to filter ranges of values. This is an additional visual query facility.

## Linked Visualisations
<img width="280" alt="3dsparklines" src=  https://user-images.githubusercontent.com/11532065/46444861-4546a380-c7b7-11e8-99a6-3e90300cac71.JPG> <img width="280" alt="3dsparklines" src= https://user-images.githubusercontent.com/11532065/46445099-7e334800-c7b8-11e8-962d-747c236c1fe4.JPG> 

Create visual links between two visualisations (right click IATK>LinkedVisualisation). Drag and drop two visualisations objects (source, target) into the *Linked Visualisation* component. Set the ShowLinks (true/false) variable to show/hide the links between the 2 visualisations.

## Brushing and Linking
<img align = "left" width="280" alt="brushinglinking" src="https://user-images.githubusercontent.com/11532065/46593142-3e03fa80-cb15-11e8-8fe0-bc56d62c10af.PNG"> It is easy to setup brushing and linking visualisations with IATK. First create a *BrushingAndLinking* object (right click in hierarchy, IATK>Brushing And Linking). 
You now have to drag and drop a *Visualisation* object that will act as the brush controller: the Brushing Visualisation. Then drag and drop how many visualisations you wish to link to this brushing visualisation in the Brushed Visualisations list. You can also attach a *Linked Visualisation* object to show highlights of the brushed data. The visual elements brushed (by attaching *Transforms* into Input1 and Input2) in the Brushing Visualisation will be highlighted in the Brushed Visualisations and the Brushed Linked Visualisations.

<!--Define a Brushing visualisation and a list of Brushed visualisation. Define the input shape and the colour of the brush. -->

<!--## IATK VR interaction ##
Use VRTK to interact with the data visualisations. -->


# IATK core data and graphics scripting
## DataSource
The DataSource object is the starting point of your immersive data visualisation project. It allows you to import your text data into Unity and has a set of methods to access the data by attribute, id etc. Further details (methods, properties) are available in the documentation [to come].
<!-- TODO: Add Further details (methods, properties) -->

Usage:
```csharp
// Use Unity Text assets to import text data (e.g. csv, tsv etc.)
TextAsset myDataSource;
CSVDataSource myCSVDataSource;
myCSVDataSource = createCSVDataSource(myDataSource.text);

// A reusable method to create an IATK CSVDataSource object.
CSVDataSource createCSVDataSource(string data) {
CSVDataSource dataSource;
dataSource = gameObject.AddComponent<CSVDataSource>();
dataSource.load(data, null);
return dataSource;
}
```


## ViewBuilder
IATK uses a fluent design pattern that lets you chain commands to design a visualisation in a single instruction.
Example in the [**Scripting**](#scripting) section

## View
Once you have built a View object with the Viewbuilder, you can change the view attributes (colours, positions, sizes, filters ...) . See documentation [to come]. 
<!-- TODO: Add further documentation -->

## Graphics toolkit
The toolkit contains facilities to create high quality graphics designed for data visualisation in Virtual and Augmented Reality. The IATK core graphics tools include:

- A **BigMesh** script that allows the creation of visualisations 
- Several **shaders** to render a lot of data rapidly and efficiently, and custom graphics for data visualisations (thick lines, dots/spheres, cubes/bars)
- A selection tool (brushing) that enables the selection of data across several data visualizations.

## Scripting
Example of building a visualisation from code. The *ViewBuilder* class allows the design of the visualisation. When *applying* (i.e. using the .apply(GameObject go, Material) method) the ViewBuilder returns a View. The View object can then be later updated (e.g. set positions, colours, etc...).

```csharp
// Date,Time,Lat,Lon,Base
Gradient g = new Gradient();
GradientColorKey[] gck = new GradientColorKey[2];
gck[0] = new GradientColorKey(Color.blue, 0);
gck[1] = new GradientColorKey(Color.red, 1);
g.colorKeys = gck;

// Create a view builder with the point topology
ViewBuilder vb = new ViewBuilder (MeshTopology.Points, "Uber pick up point visualisation").
                     initialiseDataView(csvds.DataCount).
                     setDataDimension(csvds["Lat"].Data, ViewBuilder.VIEW_DIMENSION.X).
                     setDataDimension(csvds["Base"].Data, ViewBuilder.VIEW_DIMENSION.Y).
                     setDataDimension(csvds["Lon"].Data, ViewBuilder.VIEW_DIMENSION.Z).
                     setSize(csvds["Base"].Data).
                     setColors(csvds["Time"].Data.Select(x => g.Evaluate(x)).ToArray());
                     
// Use the "IATKUtil" class to get the corresponding Material mt 
Material mt = IATKUtil.GetMaterialFromTopology(AbstractVisualisation.GeometryType.Points);
mt.SetFloat("_MinSize", 0.01f);
mt.SetFloat("_MaxSize", 0.05f);

// Create a view builder with the point topology
View view = vb.updateView().apply(gameObject, mt);
```


# Known issues
IATK is a prototype for research and there are known issues that we will fix in the near future.
- Unrecognised symbols in data source can break the scripts 
- Scatterplot matrices can be slow due to the amount of points to display (if you have a very large dataset and a lot of dimension, scatterplot matrices will be slow)


# Roadmap
In a near feature we will support:
- Fix number of labels for discrete values (delegate method)
- Details on demand
- 2D/3D graph visualisation (node-link diagrams) directly in the *Visualisation* editor
- More type of data sources (including real time data)
- More geometry (e.g. surfaces / volumes)


# Team
The toolkit is developed by:
- Maxime Cordeil (project leader, University of Queensland)
- Andrew Cunningham (University of South Australia)
- Benjamin Lee (Monash University)
- Daniel Harris (University of South Wales)

Past developpers:
- Kingsley Stephens (Monash University)
