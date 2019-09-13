# Plane Calibration

![Banner Calibration World](../Images/Toolkit/PlaneCalibration/Banner_CalibrationWorld.gif)

(To Be Replaced by a gif)

## Interactions

This page will focus here on the plane calibration but if you are interested in the other elements of the scene, we encourage you to visit [the Surface Drawing Documentation,](SurfaceDrawing.md) or [the Menu Interaction Documentation.](menuInteraction.md)

The implementation of the plan calibration can be found in the example scene `9_Example_PlaneCalibration` in the `PlaneCalibrationMenu` GameObject.
You can also see it in action the demo experience.

![Hierarchy Calibration Interaction](../Images/Toolkit/PlaneCalibration/Hierarchy_CalibrationInteraction.png)

## Implementation

All the heavy lifting happens in the `PlaneCalibration.cs` script attached to the `PlaneCalibrationMenu` GameObject.

![Inspector Plane Calibration](../Images/Toolkit/PlaneCalibration/Inspector_PlaneCalibration.png)

To make it easier to start the plane calibration in the scene we just added a button that triggers the calibration.

![Inspector Calibration Button](../Images/Toolkit/PlaneCalibration/Inspector_CalibrationButton.png)

### Plane Calibration

The Tracked device is the device you use to position the calibration points. This scene as it is, allow you to quickly create some plane in 3D anywhere you want. The script will not create a drawing surface from scratch, it will resize and rotate an existing one, the one you pass in Drawing Plane. The orientation of the drawing surface will be determined by the forward vector of the HMD. Be sure to look at the right direction.

Part of the magic of the VR Ink is its ability to draw on real physical surface. You can change the `PlaneCalibration` component New Point Trigger to use the Tip instead of the primary button. Now you can just press the stylus on three edges of you desk. To also be able to write with the Tip, be sure to change the trigger and the Stylus Axis input in the `ShaderDrawing` component.

![Inspector Shader Drawing](../Images/Toolkit/PlaneCalibration/Inspector_ShaderDrawing.png)





