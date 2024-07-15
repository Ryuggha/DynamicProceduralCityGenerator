# DynamicProceduralCityGenerator
This project is a Tool developed as a Final Degree Project that aims to generate procedural cities (or intrincate structures) in runtime, given a set of ampliable data.
The documentation of the research and development process can be found here: https://drive.google.com/file/d/19Qys58agLmkSDtchciYuTNX5Shs9ISn3/view?usp=sharing

[Click to watch the Demo](https://youtu.be/Ubeb8ppgGjQ)<br>
[![Watch the video](https://i.imgur.com/gj2CZ7L.png)](https://youtu.be/Ubeb8ppgGjQ)

# Use Guide
The following seccion describes both the instructions to use the tool, and the description of each field the inspector offers.

## Instalation
This Tool has been designed to be used in the Unity Engine environment.
**Step 1**. Download the tool using the following link: https://drive.google.com/file/d/1CJ2DCpTZ3su0GTCtyC889WOkzlml5TR_/view?usp=sharing
**Step 2**. Download and install the Unity Engine Editor.
**Step 3**. Start the editor.
**Step 4**. Import the tool to the editor. This can be done opening the file downloaded, or dragging the file to the editor.

## Layer Definition
This tool uses various _layers_ to make searches and physics calculations more efficiently. Therefore, the user must define those _layers_ and apply them to the configuration of derivate objects.
Those _layers_ are:
- **Terrain**: Used by Unity's terrains.
- **Road Colliders**: Used to detect roads.
- **Building Colliders**: Used to detect and place buildings.

## Initial Configuration
To use the tool, first the scene on which the tool is being used must be configured.
<br>**Step 1**. Import the _prefab_ called "DynamicProceduralUrbanGeneratorManager.prefab" to the scene.
<br>**Step 2**. Add one or more buildings and signal the probability of each one on the appropiate parameter.
<br>**Step 3**. Signal the appropiate _layers_ on each component (further explained below).
<br>**Step 4**. At least one terrain layer must be referenced (further explained below).
<br>**Step 5**. Configure the components of the object following the desired design. The configuration of each component is explained in the following sections.
<br>**Step 6**. Enable the created object.
<br>**Step 7**. Run the Scene.

## Building Creation
Buildings are the main component of the tool. To create a new building follow the steps:
<br>**Step 1**. Duplicate the "BuildingPreset" prefab template.
<br>**Step 2**. Rename the prefab.
<br>**Step 3**. Open the prefab to edit it.
<br>**Step 4**. Add the desired building object as a child of the "_prefab_/Element/Mesh" object.
<br>**Step 5**. Modify the "_prefab_/Elements/GenerationColliders" object's colliders to better adapt the building model. Colliders can be added or removed, but a collider component must always exist. The valid colliders are: Box Collider, Sphere Collider, and Capsule Collider, and the "Is Trigger" box must be always enabled.
<br>**Step 6**. Modify the "GenerationColliders" object's layer to the _layer_ assigned to "Building Colliders".
<br>**Step 7**. Modify and add empty objects as children of "_prefab_/Elements/Entrances" in the relative positions to the parent object, where the main object is desired to join a road, and with the forward vecotr pointing the direction in which the road is expected to be. As an important note, an entrance can not be inside an area of a collider with the layer "Building Colliders".
<br>**Step 8**. Modify the "_prefab_/Elements/Floor" object's Box Collider to signal the size of the terrain that's going to be adapted for the building foundations.
<br>**Step 9**. In the "Building Bounding Box" component of the parent objects, three parameters must be assigned: 
- **Collider Holder**: A reference to the container object of the building's _colliders_, created and modified during the **step 5**. This object must have the "Building Colliders" layer.
- **Entrance Game Object List**: In this list, all objects created or modified in **step 7** must be referenced.
- **Floor Holder**: A reference to the object modified in **step 8**.

# Parameters
## Dynamica Procedural Urban Generator Manager
This component is the administrator of all other components that the tool uses, ensuring a single entry point. <br>
The only parameter it has it's the "enable" checkbox, that toggles the tool on and off.

## Hexagonal Grid
This component builds an _organic irregular grid_ that will hold the foundations of the roads generated.
<br> _Parameters_:
