# Teleport

![Banner Grab&ScaleWorld](../Images/Toolkit/Teleport/Banner_TeleportWorld.gif)

The Teleport module provides a simple way to teleport yourself into your virtual world.

## Teleportation Components

There are 4 distinct components used to create the Teleportation Interaction in the Toolkit:

- The teleportation script
- The teleportation arc
- The landing area
- The fading camera

The implementation of the Toolkit teleportation can be found in the example scene `7_Example_Teleport` in the `TeleportInteraction` GameObject.

![Hierarchy Teleport Interaction](../Images/Toolkit/Teleport/Hierarchy_TeleportInteraction.png)

## Implementation

Every component needed for teleportation interaction are generated at runtime from the `TeleportCamera.cs` script.

![Inspector Teleport Camera](../Images/Toolkit/Teleport/Inspector_TeleportCamera.png)

Once you play the scene, you can see that a `Quad` GameObject is created under the `Camera` and a `LandingArea` GameObject is created under the `Controller (Right)`. The scripts `TeleportBeam.cs` and the `Line Renderer` Component are also added to the `Controller (Right)` GameObject.

![Hierarchy Teleport Interaction Play](../Images/Toolkit/Teleport/Hierarchy_TeleportInteractionPlay.png)
![Inspector Example Beam](../Images/Toolkit/Teleport/Inspector_ExampleBeam.png)

### Teleport Camera

We attach the `TeleportBeam.cs` and `Line Renderer` component to the selected tracked device. We have then defined 2 separate `InputTrigger`: the first one to show the teleportation destination and the second to trigger the teleport. We decided to use the touchstrip in the example scene because it has the both a `TouchEvent` and a `ClickEvent` but you could also use the primary button with the following setup:

![FallingEdgeSetup](../Images/Toolkit/Teleport/Inspector_FallingEdgeExample.png)

 The Camera Parent Transform is the actual object that will be teleported, ensure that each GameObject you want to be teleported is a child of this GameObject.

### Camera Fading

The `TeleportCamera` script also uses the `CameraParentTransform` to find the main camera active in the scene. Once we have found the main camera we  place a `Quad` in front of it that will be used to fade the screen to black during teleportation.

It's quite jarring to just teleport without the fading! Once the `TeleportTrigger` is valid, a coroutine that fade the screen to black is started, then we teleport the user and finally the view fade to normal again.

(Teleportation gif TBD)

### Possible Improvements

At the moment, the teleportation will always teleport you with the same camera direction and at the same height. Here are some though to improve the current teleportation:

- Create a tag for which ground you can teleport on. Then instead of teleporting to height zero, teleport at the position of the intersection of the beam and the ground tagged.
- Add an indicator (an Arrow?) on the landing area pointing forward. And make it turning around the landing area while you rotate the stylus on itself while keep pointing at the same direction. The direction the indicator point at will define which direction the camera should look at when the teleportation is finished.
