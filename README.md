# Logitech VR Stylus SDK
This SDK will allow your app to take advantage of all the features of the Logitech VR stylus and give you all the necessary information to help with integration in your app.

The Logitech Stylus is a simple and powerful creative input device for VR. It allows for creation of precise, controlled lines and handwriting on virtual surfaces, physical surfaces, and in three dimensions.

See below an image of the device and a description of the controls:
![Pen Button Mapping](resources/LogiStylus.jpg?raw=true)
### Inputs & Controls

The device features several unique controls, these allow for a large range of potential experiences and interactions.
The key controls are:

![Button Layout](resources/buttonLayout.png)

| Control | Description | Input Type |
|---|---|---|
| Analog Button | The Analog Button allows for modulated input that can present a range of values. This can be used for creating variable line widths based on the pressure applied to the button. | Returns Value between 0 - 1 |
| Analog Tip | The Analog Tip allows for the creation of lines on physical surfaces that are mapped in VR. This can be used to recreate a drawing surface on a desk or a whiteboard. | Returns Value between 0 - 1 |
| Touch Strip | The Touch Strip allows for the adjustment of a control using up and down swipes. The touch strip is capacitive sensing and has a button to allow for multiple methods of input. | Returns Y Value between -1 - 1 The TouchStrip can be clicked |
| Grip Button | The Grip Button on the sides of the device allow users to intuitively pick objects up in VR, and to scale and move objects using a system controller in the non-dominant hand. | Returns True or False |
| Menu Button | The Menu Button is a simple switch that allows menus to be opened and closed. The button can be long-pressed to access the 6Dof Gestural Control | Returns True or False |
| System Button | The System button allows for access to the SteamVR shell. | Returns True or False |

## Development Kit Contents
You should have received a sample of our device, if not ask for it. 

Available Directly in the Repo:
- [Sample Unity application](https://github.com/Logitech/labs_vr_pen_sdk/tree/master/code/unity_sample_app) open source showing best practice to integrate the device in your application
- [Unreal Engine documentation](https://github.com/Logitech/labs_vr_pen_sdk/tree/master/code/unreal_sample_project) for device support
- [SteamVR render Models](https://github.com/Logitech/labs_vr_pen_sdk/tree/master/code/steamVR_renderModels) for different versions of the device

Available in the [Release](https://github.com/Logitech/labs_vr_pen_sdk/releases):
- SteamVR render model and icon installer
- Unity package source code to be used for the integration of the stylus in your app
- Unreal Engine full project
- 3D Models


## Setting Up The Stylus
### Add the RenderModel to SteamVR
This step will allow you to see the device inside the SteamVR shell.

#### Using the Installer (Recommended)
Download the Logitech Stylus Model Installer on the [releases page](https://github.com/Logitech/labs_vr_pen_sdk/releases), run the application, and follow the instructions on screen! You will have access to updated icons as well as the device rendermodel in the shell. 

#### Copy Files Manually
Copy the [RenderModel folder](https://github.com/Logitech/labs_vr_pen_sdk/tree/master/code/steamVR_renderModels) that corresponds to the version of your Logitech Stylus in the appropriate SteamVR sub-folder. Typically the path is something like this : `C:\Program Files (x86)\Steam\steamapps\common\SteamVR\resources\rendermodels`
### Pairing the Stylus
![Pair Pairing](resources/pairPen.png?raw=true)

<br> 
First turn on the Logitech Stylus by pressing the system button, status LED will go blue. Then you go into SteamVR right click on greyed-out controller icon and then select *Pair Controller*. Press both the Menu and Power buttons together, the status LED will blink BLUE, and it will be solid GREEN on the Pen once pairing is complete.
<br>

### Trying out the Stylus
You are now ready to go. The pen should work with existing SteamVR applications in the same was as a Vive controller.
If you want to try a fully integrated application I recommend you to try out the Logitech Sample app available in the [Release](https://github.com/Logitech/labs_vr_pen_sdk/releases).
<br>
You can also get started with the device in Unity by either cloning this repository or downloading the Unity Package available in the [Release](https://github.com/Logitech/labs_vr_pen_sdk/releases). 



## Feedback & Bugs

We are working constantly to improve and address issues with the device, so please make sure you have the latest release of SDK installed and running.

We also value your input on:
- possible bugs
- shortcomings
- issues
- incompatibilities

as well as:
- enhancements ideas
- possible new features

We strongly suggest to use our private GitHub repository for bug reports and features requests. Follow this [link](https://github.com/Logitech/labs_vr_pen_sdk/issues) and post it there.

<br>

## License
Copyright (c) Logitech Corporation. All rights reserved.
Licensed under the MIT License.

