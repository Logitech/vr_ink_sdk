## Troubleshooting

### 1. After installing the Driver and pairing my VR Ink, I can see the 3D model moving in the SteamVR shell. However, when pressing any button, nothing shows on the Status tab of the Driver UI.
This could happen if your device has a legacy configuration (you got one of the first VR Ink devices!). We provide a configuration tool that will automatically change the Driver settings to support all device versions.

How to use the configuration tool:
- Verify that the VR Ink 3D model is correctly tracked in the SteamVR VR view and that the Driver UI is running
- Go to the Driver installation files folder and run the `ConfigurationToolBatch.bat` file to launch the tool:

    ![Configuration Tool Batch](./../Images/Driver/ConfigurationToolBatch.png)

- Press the "Detect Device and Configure Driver" button

    ![Configuration Tool Start](./../Images/Driver/ConfigurationToolStart.png)

- The tool will let you know if your device had a supported configuration and will ask you to restart SteamVR to apply the changes. After restarting SteamVR you should be able to see the inputs working in the Status tab of the Driver UI.

    ![Configuration Tool Finished](./../Images/Driver/ConfigurationToolFinished.png)

- If the configuration tool was not able to detect your device or its configuration is not currently supported : please contact our technical support with a screenshot of the configuration tool and the serial number of your unit so we can investigate how to solve the issue.

### 2. The stylus doesn't show in the shell and I can't see input on the STATUS window

One of the biggest limitation of the driver is that input **does not work** when the SteamVR Dashboard is up. Button press will not go trough the STATUS window of the driver and the 3D model of VR Ink will disappear in the shell.

Your SteamVR status will show the virtual device as disconnected:

![SteamVR Status](./../Images/Driver/SteamVRStatusDisconnected.png)

As a reference here is what the SteamVR Dashboard look like this:

![SteamVR dashboard](./../Images/Driver/SteamVRDashboard.png)

Quitting the SteamVR Dashboard by **pressing the system button a paired controller other than VR Ink or by clicking the power button on the side of the HMD** will fix the issue.
