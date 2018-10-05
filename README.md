# IATK 1.0 (Mala): Immersive Analytics Toolkit 
<p> <img align="left" width="410" alt="2dscatterplot" src=https://user-images.githubusercontent.com/11532065/46409367-9bd0c500-c758-11e8-9485-c92d532d97b8.jpg>  The **Immersive Analytics Toolkit (IATK)** is a Unity plugin to help you build high quality, interactive and robust data visualisation in Immersive Environements (Virtual / Augmented Reality). Use the *Visualisation* script to create data vizs interactively in the editor, press play and view your data in VR. Write simple code to use the *IATK* core graphics components to make your own interactive visualisations programitcally.
<!-- With IATK, load CSV/TSV files and write simple code using the library to create immersive data visualisations.
%IATK provides a *Visualisation* script to help you create and design visualizations within the Unity editor -->
IATK is an open project! We setup a roadmap with features that we would like to see in IATK.  
</p>  
  
## IATK editor components
1. ***Data Source: import data into the scene***
First, create a **Data Source** object (right click in the hierarchy, IATK/DataSouce). Drag and drop a CSV/TSV in the Data field. Your data is now imported in the Unity scene!

2. ***Visualisation: an editor menu helper***
IATK has a number of predefined, controlable data visualisation template that are accessible directly in the editor. The **Visualisation** object allows you to access those templates.

Create a **Visualisation** object (right click in the hierarchy, IATK/DataSouce). Drag and drop a **Data Source** object in the *Data Source* field of the Visualisation object. You are now ready to design a data visualisation with the following templates:

- Simple visualisation: 
  * 2D/3D scatterplots
  
 <img width="280" height="280" alt="2dscatterplot" src=https://user-images.githubusercontent.com/11532065/46408763-8eb2d680-c756-11e8-813f-e3114a63215d.png> <img width="280" height="280" alt="3dscatterplot" src=https://user-images.githubusercontent.com/11532065/46408760-89ee2280-c756-11e8-9fa2-add36ce6bdda.png>
 
  * 2D/3D barcharts
  
 <img width="280" height="280" alt="2dbarchart" src=https://user-images.githubusercontent.com/11532065/46408758-89ee2280-c756-11e8-8d18-b1a6a5997276.png> <img width="280" height="280" alt="3dbarchart" src=https://user-images.githubusercontent.com/11532065/46408759-89ee2280-c756-11e8-99e6-8ea0339d1b7d.png>
 
  * 2D/3D trails/trajectories (use linkning field to bind a *linking attribute*)
  
  <img width="280" height="280" alt="3dsparklines" src=  https://user-images.githubusercontent.com/11532065/46444864-4677d080-c7b7-11e8-9b54-9f7191841c51.JPG > <img width="280" height="280" alt="3dsparklines" src=https://user-images.githubusercontent.com/11532065/46444863-45df3a00-c7b7-11e8-88ef-b88cad461efa.JPG> <img width="280" height="280" alt="3dsparklines" src=https://user-images.githubusercontent.com/11532065/46444862-45df3a00-c7b7-11e8-8168-4c3d81047730.JPG> 
  
  * 2D/3D Connnected dots (use linkning field to bind a *linking attribute*)
  
  <img width="280" height="280" alt="3dsparklines" src=  https://user-images.githubusercontent.com/11532065/46441718-b9c71580-c7aa-11e8-8c80-3b0d1122f078.JPG > <img width="280" height="280" alt="3dsparklines" src=https://user-images.githubusercontent.com/11532065/46408616-0a605380-c756-11e8-93d9-e0ec49b3b2a7.png>
  
 
- Parallel Coordinates Plots (PCPs):

<img alt="parallelcoord" src=https://user-images.githubusercontent.com/11532065/46409268-43012c80-c758-11e8-9484-3fc9a7ecd783.JPG> 

- Scaptterplot Matrix
  - 2D Scatterplot Matrix
  
  <img width="280" height="280" height="280" alt="3dsplom" src=https://user-images.githubusercontent.com/11532065/46442039-bda76780-c7ab-11e8-829a-3becf85efcf4.JPG>
  
  - 3D Scatterplot Matrix
  
  <img width="280" height="280" height="280" alt="3dsplom" src=https://user-images.githubusercontent.com/11532065/46408791-a9854b00-c756-11e8-8c05-21ce94e2f463.png> <img width="280" height="280" alt="3dsplomcloseup" src=https://user-images.githubusercontent.com/11532065/46442262-5b029b80-c7ac-11e8-8abc-beeda5040efa.JPG> <img width="280" height="280" alt="3dsplomcloseup" src=https://user-images.githubusercontent.com/11532065/46408792-aa1de180-c756-11e8-9fb8-5f92e5b0fa9e.png>


Visualisation designer in the Unity Editor

<!--<p align="center"> 
</p> -->

<img align="left" width="280" alt="3dsparklines" src=  https://user-images.githubusercontent.com/11532065/46445150-acb12300-c7b8-11e8-98b1-22cd2f1eba65.png>  The visualisation component allows the design of the visualisation inside the Unity Editor. Visual variables can be bound by dimension attributes.

- Geometry: defines the geometry of the visualisation. **Points, Quads, Bars** and **Cubes** are a single point topology. **Lines and Connected Lines and Dots** are a line topology and they require you to specify a *Linking dimension* (see below)
- Colour dimension: use the dropdown to bind a data attribute to a continuous colour gradient. Click the Colour gradient to edit it.
- Bind Colour palette: use the dropdown to bind a discrete data attribute to a discrete colour palette. Click the corresponding colour values to edit the palette.
- Blending Mode Source, Destination: lets you specify the blending mode. By default it's set to SrcAlpha,OneMinusSrcAlpha that allows for traditional blending with transparency. Use One,One to do visual accumation effects.
- Colour: if *Colour dimension* and *Bind Coolour palette* are *Undefined*, sets the same colour to all the glyphs.
- Size dimension: use the dropdown to bind an attribute to the size of the glyphs. The Size slider sets the global size, the Min/Max Size slider sets the scale.
- Linking dimension: use the dropdown to link datapoints by id. **/!\ It requires that your data is ordered in sequence in the CSV source**
- Attribute Filters: type in an attribute name and use filters to filter ranges of values. This is an additional visual query facility.

3. ***View linker***

<img width="280" alt="3dsparklines" src=  https://user-images.githubusercontent.com/11532065/46444861-4546a380-c7b7-11e8-99a6-3e90300cac71.JPG> <img width="280" alt="3dsparklines" src= https://user-images.githubusercontent.com/11532065/46445099-7e334800-c7b8-11e8-962d-747c236c1fe4.JPG> 

Create visual links between two visualisations. Drag and drop two visualisations objects (source,target) into the Linked Visualisation component. Set the (boolean) ShowLinks to show the links between the 2 visualisations.

4. ***Brushing and Linking***
It's easy to setup brushing and linking visualisation with IATK. First create a BrushingAndLinking object (right click in hierarchy, IATK>Brushing And Linking). You now have to drag and drop a *Visualisation* object that will act as the controller. 
<!--Define a Brushing visualisation and a list of Brushed visualisation. Define the input shape and the color of the brush. -->

<!--## IATK VR interaction ##
Use VRTK to interact with the data visualisations. -->

## IATK core data and graphics scripting
1. DataSource
The DataSource object is the starting point of your immersive data visualisation project. It allows you to import your text data into Unity and has a set of methods to access the data by attribute, id etc.

Usage:
```go
\\ Use Unity Test assets to import text data (e.g. csv, tsv etc.)
TextAsset myDataSource;
CSVDataSource myCSVDataSource;
myCSVDataSource = createCSVDataSource(myDataSource.text);
```
Further details (methods, properties) are available in the documention [to come].

2. ViewBuilder
IATK uses a fluent design pattern that lets you chain commands to design a visualisation in a single instruction.
Example:

3. View
Once you have built a View object with the Viewbuilder, you can change the view attributes (colours, positions, sizes, filters ...) . See documentation [to come].

4. Graphics toolkit
The toolkit contains facilities to create high quality graphics dsigned for data visualisation in Virtual and Augmented Reality. The IATK core graphics tools include:

- a **BigMesh** script that allows the creation of visualisations 
- several shaders to render a lot of data rapidly and efficiently, and custom graphics for data visualisations (thick lines, dots/spheres, cubes/bars)
- a selection tool (brushing) that enables the selection of data accross several data visualizations.

3. Scripting
```go
// create a view builder with the point topology
ViewBuilder vb = new ViewBuilder (MeshTopology.Points, "Uber pick up point visualisation").
        initialiseDataView(csvds.DataCount).
        setDataDimension(csvds["Lat"].Data, ViewBuilder.VIEW_DIMENSION.X).
        setDataDimension(csvds["Base"].Data, ViewBuilder.VIEW_DIMENSION.Y).
        setDataDimension(csvds["Lon"].Data, ViewBuilder.VIEW_DIMENSION.Z).
        setSize(csvds["Base"].Data).
     setColors(csvds["Time"].Data.Select(x => g.Evaluate(x)).ToArray());

// create a view builder with the point topology
View view = vb.updateView().apply(gameObject, mt);
```

## Known issues
IATK is a prototype for research and there are known issues that we will fix in the near future.

- unrecognised symbols in data source can break the scripts 
- scatterplot matrices can be slow due to the amount of points to display (if you have a very large dataset and a lot of dimension, scatetrplot matrices will be slow)


## IEEE VIS 2018 tutorial 
We will give an Immersive Visualisation tutorial with IATK at IEEE VIS 2018.
For attendees, please download:

- this repository
- [Unity 2017](https://unity3d.com/get-unity/download?thank-you=update&download_nid=49126&os=Win)
- [Virtual reality toolkit VRTK](https://github.com/thestonefox/VRT)

Setup a VR scene with the Oculus Rift in Unity: https://www.youtube.com/watch?v=psPVNddjgGw&t
Setup a VR scene with the HTC Vive in Unity: https://www.youtube.com/watch?v=tyFV9oBReqg&list=RDtyFV9oBReqg&start_radio=1

Our repository contains sample datasets. Come with your CSV data to the tutorial if you want to give it a go!

## Roadmap
In a near feature we will support:

- fix number of labels for discrete values (delegate method)
- details on demand
- 2D/3D graph visualisation (node-link diagrams) directly in the *Visualisation* editor
- more type of data sources (including real time data)
- more geometry (e.g. surfaces / volumes)

## Official members and developpers
The toolkit is developped by Maxime Cordeil (project leader, Monash University), Kingsley Stephens (Monash University) and Andrew Cunningham (University of South Australia).

Contributors: Tim Dwyer (Monash University), Yalong Yang (Monash University)
