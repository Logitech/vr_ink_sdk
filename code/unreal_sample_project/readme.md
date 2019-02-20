# Logitech Stylus UE4 SDK

![logo](./../../resources/unreal/LogiPenUnrealSDK.png)

## How to use

Keep note that this SDK is a reference implementation rather than a toolkit. While you are more than welcome to re-use parts of this (provided you follow the terms of this repository's licence), we supply this project as a simple starter example only. **The start project has been packaged using Unreal Engine 4.21.2 , the project assets will not open if you are using an older version of Unreal Engine.**

Unzip [LogiPenUnrealSDK.zip](https://github.com/Logitech/labs_vr_pen_sdk/releases) file on your machine and double-click on `LogiPenUnrealSDK.uproject`. Unreal Engine will then open the project, and after compiling everything, you will be able to try out the Logitech Stylus in UE4! Make sure to use VR Preview, as shown below:

![Play in VR](./../../resources/unreal/vr_preview.png)

## Content

This section describes the main components, pieces of code, Blueprints, and assets contained in the SDK.

### Pen models

We provide you with two models of the pen: a full mesh and a simplified one.

![pen models](./../../resources/unreal/pen_models.png)

If your application doesn't require the user to quickly move around--at the risk of hitting something with the tracking geometry of the pen--we recommend using the simple model. We suggest you try to use the tab at the end of the pen as an additional UI element: for example, this could be the current ink colour, or pulse when the user's attention is needed elsewhere in your app.

### VR Player With Stylus

This Blueprint is a Pawn and was created following the step-by-step guide outlined below.

### Create Child if Stylus

A rather simple Blueprint function graph determining if a given Motion Controller is a Logitech Stylus, in which case it hides its default model and replaces it with the pen model specified in the Pawn.

### C++ code

Intentionally kept very simple, the few lines of code included in this SDK can be found in `Source/LogiPenUnrealSDK/LogiPenModelOverride.cpp`. There are two functions, called in the function graph described above.

## Adding Logitech Stylus Motion controller to your scene

This is how to add the Logitech Stylus to a scene using the assets contained in this project:

* Configure a VR Player Pawn: [UE Doc](https://docs.unrealengine.com/en-US/Platforms/SteamVR/HowTo/StandingCamera)
* Configure left and right motion controller, including component visualization (this is important to detect which controller is the Logitech Stylus): [UE Doc](https://docs.unrealengine.com/latest/INT/Platforms/VR/MotionController)
* In the Pawn's Event Graph, after `Set Tracking Origin`, add a `Sequence`, and add a `Create Child if Pen` to both outputs:

  ![add action](./../../resources/unreal/add_action.png)

* Under My Blueprint, add a new variable, call it LogiPenActor, make it public, and of type Actor > Class Reference. Then drag & drop it on the event graph, select "Get LogiPenActor".
* Drag & drop your left and right Motion Controllers, also select "Get ..."
* Link these items in the following way:

  ![overall BP](./../../resources/unreal/create_child_if_pen.png)

* Return to your map, drop the VR Pawn, make sure it is at the origin, set it as Auto Possess Player 0, and use LogiPenModel_simple as Logitech Stylus Actor. This Actor is the one that will override the default model coming from SteamVR.

  ![pawn config](./../../resources/unreal/vr_player_pawn_config_highlight.png)
