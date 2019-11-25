## Troubleshooting

### 1. After installing the Driver and pairing my VR Ink, I can see the 3D model moving in the SteamVR shell. However, when pressing any buttons, nothing shows on the status tab in the driver window.

This could happen if your device has a legacy configuration (you got one of the first VR Ink devices!). We provide a configuration tool that will automatically change the Driver settings to support all device versions.

How to use the configuration tool:
- Verify that the VR Ink 3D model is correctly tracked in the SteamVR view and that the Driver UI is running.
- Go to the Driver installation files folder and run the `ConfigurationToolBatch.bat` file to launch the tool:
    <br>
    ![Configuration Tool Batch](./../Images/Driver/ConfigurationToolBatch.png)

- Press the "Detect Device and Configure Driver" button.
    <br>
    ![Configuration Tool Start](./../Images/Driver/ConfigurationToolStart.png)

- The tool will let you know if your device had a supported configuration and will ask you to restart SteamVR to apply the changes. After restarting SteamVR, you should be able to see the inputs working in the Status tab of the Driver window.
    <br>
    ![Configuration Tool Finished](./../Images/Driver/ConfigurationToolFinished.png)

- If the configuration tool was not able to detect your device or its configuration is not currently supported, please contact our technical support with a screenshot of the configuration tool and the serial number of your unit so we can investigate how to resolve the issue.

### 2. The stylus doesn't appear in the SteamVR shell and I can't see input on the status tab in the driver window.

One of the current biggest limitations of the driver is that input **does not work** when the SteamVR Dashboard is open. Button presses will not go trough the status window of the driver and the 3D model of VR Ink will disappear in the SteamVR shell.

Your SteamVR status window will show the virtual device as disconnected:
<br>
![SteamVR Status](./../Images/Driver/SteamVRStatusDisconnected.png)

As a reference, here is what the SteamVR Dashboard looks like:
<br>
![SteamVR dashboard](./../Images/Driver/SteamVRDashboard.png)

Quitting the SteamVR Dashboard by **pressing the system button on a paired controller other than VR Ink or by clicking the power button on the side of the HMD** will fix the issue.

### 3. I cannot use a Vive tracker as an external camera for mixed reality capture.

The following procedure is only valid if the application has implemented SteamVR mixed reality mode. More details [here](https://vr.arvilab.com/blog/capturing-mixed-reality-video-unity-and-steamvr).

Let's look at how to setup VR Ink to capture mixed reality content for Tilt Brush.

The Mixed Reality capture in SteamVR seems to always select the 3rd tracked device connected. The work around consists of turning on the devices in the correct sequence in order to have the tracker detect as the 3rd device.

#### Pairing the Vive tracker to its dongle

If you are Vive tracker is already paired to a dongle please skip to the next section.

In the case of VR Ink and Vive Controller the procedure goes like this:

1. All devices should be turned off Vive Controllers, Tracker and VR Ink
2. Plug in the dongle in the computer
3. Turn on SteamVR, you should see this
<br>
![SteamVR Everything Off](./../Images/FAQ/MixedRealityCapture/SteamVRControllersOff.png)
4. Turn on VR Ink and a Vive Controller
<br>
![SteamVR VR Ink + Controller](./../Images/FAQ/MixedRealityCapture/SteamVROn.png)
5. Select *Pair Controller* by right-clicking on an active controller, then select *I want to pair a different type of controller*, select Vive Tracker.
<br>
![SteamVR Pair Controller](./../Images/FAQ/MixedRealityCapture/SteamVRPairController.png)
<br>
![SteamVR Pair Tracker](./../Images/FAQ/MixedRealityCapture/PairTracker.png)
6. Put the tracker in pairing mode by pressing the system button
7. You should now see this:
<br>
![SteamVR Status everything paired](./../Images/FAQ/MixedRealityCapture/SteamVREverythinOnWrongOrder.png)
<br>
![SteamVR Shell](./../Images/FAQ/MixedRealityCapture/SteamVRShellWithTracker.png)
The tracker is now paired to its dongle!

#### Setting up the mixed reality capture

You should have a Vive tracker paired to its dongle and the dongle plugged into the PC for next part.

This part assumes that you have put the  externalcamera.cfg in Tilt Brush folder already.

1. All devices should be turned off Vive Controllers, Tracker and VR Ink
2. Turn on SteamVR, you should see this
<br>
![SteamVR Everything Off](./../Images/FAQ/MixedRealityCapture/SteamVRControllersOff.png)
3. Turn on the previously paired vive controller first
<br>
![SteamVR Controller Paired](./../Images/FAQ/MixedRealityCapture/SteamVRControllerOnly.png)
4. Turn on the tracker
<br>
![SteamVR Tracker Paired](./../Images/FAQ/MixedRealityCapture/SteamVRControllerAndTracker.png)
5. Right Click on the tracker, select Manage Tracker and make sure it is set as *Camera*
![Manage tracker role](./../Images/FAQ/MixedRealityCapture/ManageTrackerWindow.png)
6. Turn on VR Ink
<br>
![SteamVR Everything on](./../Images/FAQ/MixedRealityCapture/SteamVREverythinOn.png)
7. Now turning on TiltBrush:
<br>
![Tilt Brush Window](./../Images/FAQ/MixedRealityCapture/TiltBrushMixedRealityMode.png)
The tracker correctly moves the camera in the bottom left quadrant. The setup for mixed reality capture is finished

### 4. Can I use 2 VR Inks at the same time ?

Currently it is impossible to use two VR Inks at the same time.

### 5. I have no input after I have paired a second VR Ink on my system.

Limitations in our driver prevent us to have a multiple VR Ink recognized during the same session of SteamVR. As a rule of thumb, **everytime you pair a new VR Ink to a system, restart SteamVR**.

For instance in this case, my first VR Ink ran out of battery, I decided to pair a second VR Ink, my SteamVR status window looks like the following:
<br>
![two VR Inks](./../Images/FAQ/MultipleVRInk/PairingAfterOff.png)

You notice that the Virtual Device icon stays greyed out, to get it work again I need to restart steamVR.

It now looks like this and all inputs are working:
<br>
![SteamVR after restart](./../Images/FAQ/MultipleVRInk/SteamVRAfterRestart.png)

