# IATK 1.0 (Mala): Immersive Analytics Toolkit 
<img width="280" alt="iatk_menu" src="https://user-images.githubusercontent.com/11532065/35493346-921c8e32-0506-11e8-8471-f010b9e37f5a.JPG"> <img width="280" alt="iatk_menu" src="https://user-images.githubusercontent.com/11532065/35493577-8f67a4b8-0508-11e8-9e08-240a9631bcfd.JPG"> <img width="280" alt="scatterplot" src="https://user-images.githubusercontent.com/11532065/35493918-ed5610d0-050a-11e8-909f-d0ba9c7ac534.PNG">


The Immersive Analytics Toolkit (IATK) is a Unity plugin to help you build high quality, interactive and robust data visualisation in Immersive Environements (Virtual / Augmented Reality). Use the *Visualisation* script to create data vizs interactively in the editor, press play and view your data in VR. Write simple code to use the *IATK* core graphics components to make your own interactive visualisations programitcally.

%With IATK, load CSV/TSV files and write simple code using the library to create immersive data visualisations.
%IATK provides a *Visualisation* script to help you create and design visualizations within the Unity editor.

IATK is an open project! We setup a roadmap with features that we would like to see in IATK. 
Feel free to contribute :)

## IATK editor components
1. Data Source: import data into the scene
First, create a **Data Source** object (right click in the hierarchy, IATK/DataSouce). Drag and drop a CSV/TSV in the Data field. Your data is in the scene

2. Visualisation: an editor menu helper
Create a **Visualisation** object (right click in the hierarchy, IATK/DataSouce). Drag and drop a **Data Source** object 
For now in IATK you can create:

- a Simple 2D/3D visualisation

-- 2D and 3D scatterplots


-- 2D and 3D trajectories, trailsets 

- Parallel Coordinates
- a Scatterplot Matrix

3. Custom Interactive Brushing and Linking 

4. View linker

## IATK core data and graphics scripting
1. Data Source

2. Visualisation

3. Graphics toolkit
The toolkit contains facilities to create high quality graphics dsigned for data visualisation in Virtual and Augmented Reality. The IATK core graphics tools include:

- a **BigMesh** script that allows the creation of visualisations 
- several shaders to render a lot of data rapidly and efficiently, and custom graphics for data visualisations (thick lines, dots/spheres, cubes/bars)
- a selection tool (brushing) that enables the selection of data accross several data visualizations.



<img width="500" alt="iatk_menu" src="https://user-images.githubusercontent.com/11532065/35493494-ee5358c4-0507-11e8-874f-c96f0c9c36de.PNG">

3. Scripting

4. Template Library
 - Histograms 
 - 2D/3D scatterplots
 - 2D/3D Scatterplot matrices (SPLOM)
   
5. Interactive visualisation components support
 - Brushing and linking
 - Scale
 - Grab & Move
  
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
