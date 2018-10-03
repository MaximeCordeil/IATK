# IATK 1.0 (Mala): Immersive Analytics Toolkit 
<img width="280" alt="iatk_menu" src="https://user-images.githubusercontent.com/11532065/35493346-921c8e32-0506-11e8-8471-f010b9e37f5a.JPG"> <img width="280" alt="iatk_menu" src="https://user-images.githubusercontent.com/11532065/35493577-8f67a4b8-0508-11e8-9e08-240a9631bcfd.JPG"> <img width="280" alt="scatterplot" src="https://user-images.githubusercontent.com/11532065/35493918-ed5610d0-050a-11e8-909f-d0ba9c7ac534.PNG">


The Immersive Analytics Toolkit (IATK) is a Unity plugin to help you build high quality, interactive and robust data visualisation in Immersive Environements (Virtual / Augmented Reality). Use the *Visualisation* script to create data vizs interactively in the editor, press play and view your data in VR. Write simple code to use the *IATK* core graphics components to make your own interactive visualisations programitcally.

%With IATK, load CSV/TSV files and write simple code using the library to create immersive data visualisations.
%IATK provides a *Visualisation* script to help you create and design visualizations within the Unity editor.

IATK is an open project! We setup a roadmap with features that we would like to see in IATK. 
Feel free to contribute :)

## IATK editor components
1. ***Data Source: import data into the scene***
First, create a **Data Source** object (right click in the hierarchy, IATK/DataSouce). Drag and drop a CSV/TSV in the Data field. Your data is now imported in the Unity scene!

2. ***Visualisation: an editor menu helper***
IATK has a number of predefined, controlable data visualisation template that are accessible directly in the editor. The **Visualisation** object allows you to access those templates.
<img width="280" alt="2dscatterplot" src=https://user-images.githubusercontent.com/11532065/46408763-8eb2d680-c756-11e8-813f-e3114a63215d.png> <img width="280" alt="3dscatterplot" src=https://user-images.githubusercontent.com/11532065/46408760-89ee2280-c756-11e8-9fa2-add36ce6bdda.png> <img width="280" alt="2dbarchart" src=https://user-images.githubusercontent.com/11532065/46408758-89ee2280-c756-11e8-8d18-b1a6a5997276.png> <img width="280" alt="3dbarchart" src=https://user-images.githubusercontent.com/11532065/46408759-89ee2280-c756-11e8-99e6-8ea0339d1b7d.png> <img width="280" alt="snip1" src=https://user-images.githubusercontent.com/11532065/46408616-0a605380-c756-11e8-93d9-e0ec49b3b2a7.png> <img width="280" alt="3dsplom" src=https://user-images.githubusercontent.com/11532065/46408791-a9854b00-c756-11e8-8c05-21ce94e2f463.png> <img width="280" alt="3dsplomcloseup" src=https://user-images.githubusercontent.com/11532065/46408792-aa1de180-c756-11e8-9fb8-5f92e5b0fa9e.png>

Create a **Visualisation** object (right click in the hierarchy, IATK/DataSouce). Drag and drop a **Data Source** object in 


3. ***Custom Interactive Brushing and Linking***

4. ***View linker***

## IATK VR interaction ##
Use VRTK to interact with the data visualisations.

## IATK core data and graphics scripting
1. DataSource

2. ViewBuilder

3. View

4. Graphics toolkit
The toolkit contains facilities to create high quality graphics dsigned for data visualisation in Virtual and Augmented Reality. The IATK core graphics tools include:

- a **BigMesh** script that allows the creation of visualisations 
- several shaders to render a lot of data rapidly and efficiently, and custom graphics for data visualisations (thick lines, dots/spheres, cubes/bars)
- a selection tool (brushing) that enables the selection of data accross several data visualizations.

<img width="500" alt="iatk_menu" src="https://user-images.githubusercontent.com/11532065/35493494-ee5358c4-0507-11e8-874f-c96f0c9c36de.PNG">

3. Scripting

  
## Virtual/Augmented Reality Support
The toolkit has been tested and is fully supported by the following Virtual/Augmented reality devices:
 - Oculus
 - Vive
 - Hololens
 - Meta

## Roadmap
- Support 2D/3D graph visualisation (node-link diagrams) directly in the *Visualisation* editor
- Support more data sources (including real time data)


## Official members and developpers
The toolkit is developped by Maxime Cordeil (project leader, Monash University), Kingsley Stephens (Monash University) and Andrew Cunningham (University of South Australia).

Contributors: Tim Dwyer (Monash University), Yalong Yang (Monash University)
