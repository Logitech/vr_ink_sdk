# Logitech VR Ink Integration

There's a couple of consideration to have when you start working with the VR Ink. This section will go in details into several of those.

## VR Ink Models
We offer two variation on the VR Ink 3D model, one with the tracking geometry and one without. It is up to you to decide if you want to keep the tracking geometry in your application.

![VR Ink Model options](resources/GettingStarted/stylus_models.png)

Currently the FBX assets are not final, and we will be providing a better quality model (low poly count, better UV) in the coming months as we zero in on the final physical shape of the device.

### Input Highlight

The Logitech VR Ink offers 3 different pressure sensitive input to interact, as such it you cannot show button travel when interacting with the stylus. We recommend to use a color fill on the button to show that it is being pressed:
<br>
![Button Highlight](resources/GettingStarted/color_change.gif)
<br>

You could also work with visual feedback that communicates how hard the user is pressing on each analog input, for instance we found the following to work well for the main analog button at the top of the stylus :
<br>
![Analog Button Highlight](resources/GettingStarted/analogButtonFill.gif)
<br>

For the touchstrip we recommend the use of the single dot that shows to the user where the touchpoint currently is.
<br>
![Touchstrip Highlight ](resources/GettingStarted/touchStripAnimation.gif)
<br>

Sample code for both of these highlighting scheme will be available in the Unity and Unreal Sample soon.
