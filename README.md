# Logitech VR Pen SDK
This SDK will allow your app to take advantage of all the features of the Logitech VR pen and give you all the necessary information to help the integration in your app.
<br>
![Pen Button Mapping](resources/penButtonMapping.png?raw=true)

<br>

## Development Kit Contents
You should have received a sample of our pen, if not ask for it. 

Available Directly in the Repo:
- [Sample Unity application](https://github.com/Logitech/labs_vr_pen_sdk/tree/master/code/unity_sample_app) open source showing best practice to integrate the pen in your application
- [SteamVR render Models](https://github.com/Logitech/labs_vr_pen_sdk/tree/master/code/steamVR_renderModels) for different versions of the pen

Available in the [Release](https://github.com/Logitech/labs_vr_pen_sdk/releases):
- Demo app 
- Unity package source code to be used for the integration of the keyboard in your app
- 3D Models


## Setting Up The Pen
### Add the RenderModel to SteamVR
This step will allow you to see the pen inside the SteamVR shell.
Copy the [RenderModel folder](https://github.com/Logitech/labs_vr_pen_sdk/tree/master/code/steamVR_renderModels) that corresponds to the version of your Logitech Pen in the appropriate SteamVR sub-folder. Typically the path is something like this : `C:\Program Files (x86)\Steam\steamapps\common\SteamVR\resources\rendermodels`
### Pairing the pen
![Pair Pairing](resources/pairPen.png?raw=true)

Pairing works the same as per a regular Vive controller. Turn on SteamVR, if you already have two controller paired to your Vive Headset you should turn off one of them before proceeding. 
<br> 
First turn on the Logitech Pen by pressing the power button, status LED will go blue. Then you go into SteamVR right click on greyed-out controller icon and then select *Pair Controller*. Press both the Menu and Power buttons together, the status LED will blink BLUE, and it will be solid GREEN on the Pen once pairing is complete.
### Trying out the Pen
You are now ready to go. The pen should work with existing SteamVR applications in the same was as a Vive controller.
If you want to try a fully integrated application I recommend you to try out the Logitech Sample app available in the [Release](https://github.com/Logitech/labs_vr_pen_sdk/releases).
<br>
You can also get started with the Pen in Unity by either cloning this repository or downloading the Unity Package available in the [Release](https://github.com/Logitech/labs_vr_pen_sdk/releases). 



## Feedback & Bugs
We strongly suggest to use our private GitHub repository for bug reports and features requests. Follow this [link](https://github.com/Logitech/labs_vr_pen_sdk/issues) and post it there.

<br>

## License
Copyright (c) Logitech Corporation. All rights reserved.
Licensed under the MIT License.

