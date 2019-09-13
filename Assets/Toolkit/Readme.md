<img src="../../Documentation/Images/Toolkit/VRInkToolkitBanner.jpg" alt="Logitech VR Ink">

# Logitech VR Ink Toolkit
The Logitech VR Ink Toolkit demonstrates some key interactions and capabilities of the VR Ink and presents best practices to partners and developers. It also includes accompanying code examples.

The Toolkit is built using Unity, but the concepts and interactions presented here may be transferred to another platform such as Unreal Engine.

## Design Guidelines
As a starting point, we recommend that you follow our [design guidelines](../../Documentation/DesignGuidelines) when integrating or creating interactions for the VR Ink. You can find examples of their implementation in the Toolkit modules below.

## Getting Started
You can get the Toolkit by heading to the 'Assets/Toolkit' folder in the SDK, or by downloading and importing the Unity package from the [source code archive](https://github.com/Logitech/labs_vr_stylus_sdk/releases). 
Note that the Toolkit is not compatible with the Unity Integration section of the SDK and uses the Unity SteamVR plugin version 2.0+ that has the action bindings.

## Interaction Modules
These modules of the Toolkit showcase how to create some key interactions with the VR Ink:

[<img src="../../Documentation/Images/Toolkit/Grab&Scale.png" width="290" alt="Toolkit">](../../Documentation/Toolkit/Grab&Scale.md)
[<img src="../../Documentation/Images/Toolkit/MenuInteraction.png" width="290" alt="Toolkit">](../../Documentation/Toolkit/MenuInteraction.md)
[<img src="../../Documentation/Images/Toolkit/AirDrawing.png" width="290" alt="Toolkit">](../../Documentation/Toolkit/AirDrawing.md)
[<img src="../../Documentation/Images/Toolkit/SurfaceDrawing.png" width="290" alt="Toolkit">](/../../Documentation/Toolkit/SurfaceDrawing.md)
[<img src="../../Documentation/Images/Toolkit/Teleport.png" width="290" alt="Toolkit">](../../Documentation/Toolkit/Teleport.md)
[<img src="../../Documentation/Images/Toolkit/Haptics.png" width="290" alt="Toolkit">](../../Documentation/Toolkit/Haptics.md)
[<img src="../../Documentation/Images/Toolkit/PlaneCalibration.png" width="290" alt="Toolkit">](../../Documentation/Toolkit/PlaneCalibration.md)
[<img src="../../Documentation/Images/Toolkit/Tooltips.png" width="290" alt="Toolkit">](../../Documentation/Toolkit/Tooltips.md)

## Example Scenes
Along with example scenes for each module, there are two more scenes.

### All-in-One Scene
The All-in-One scene is designed to show all the key interactions and how they can work together to build creative applications. It can be found in `Toolkit/Examples/ExampleInteractions/1_Example_All_In_One`.

### Simple Interaction
If you have a clear design direction already in mind and just want to get started from the bare minimum, we provide a bare bones scene that features the VR Ink model and the correct pose information, and is ready to begin building custom interactions and experiences. It contains a basic interaction using the Toolkit framework to help you get started building your own interactions. It can be found in `Toolkit/Examples/ExampleInteractions/2_Example_SimpleInteraction`.

