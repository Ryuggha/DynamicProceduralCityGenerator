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
**Step 1**. Import the _prefab_ called "DynamicProceduralUrbanGeneratorManager.prefab" to the scene.
**Step 2**. Add one or more buildings and signal the probability of each one on the appropiate parameter.
**Step 3**. Signal the appropiate _layers_ on each component (further explained below).
**Step 4**. At least one terrain layer must be referenced (further explained below).
**Step 5**. Configure the components of the object following the desired design. The configuration of each component is explained in the following sections.
**Step 6**. Enable the created object.
**Step 7**. Run the Scene.

## Building Creation
