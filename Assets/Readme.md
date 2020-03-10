# Get to Know Your Device
Here are descriptions of the different buttons and inputs available with Logitech VR Ink. They will be referenced throughout this guide as well as in the design guidelines, code examples and in other parts of the SDK.

[<img src="../Documentation/Images/ButtonLayout.png" width="600" alt="VR Ink controls">]()

| <div style="width:120px">Control</div> | Description |
|----|---------------|
| **Primary Button** | The Primary Button allows for modulated input that can report a range of values. This can be used for creating variable line widths in the air based on the pressure applied to the button, or interacting with UI. |
| **Analog Tip** | The Analog Tip allows for the creation of lines on physical surfaces that are mapped in VR. This can be used to recreate a drawing surface on a desk or a whiteboard. |
| **Touchstrip** | The Touchstrip is capacitive sensing, and has a button, to allow for multiple methods of input. It can be used for the adjustment of controls using up and down swipes or executing mapped controls based on touch position. |
| **Grip Button** | The Grip Button on the sides of VR Ink allows users to intuitively pick objects up in VR, and to scale and move objects using a system controller (Vive controller or Index Controller) in the non-dominant hand. |
| **Menu Button** | The Menu Button is a simple switch button, and can be used for tertiary controls like opening a menu. |
| **System Button** | The System button allows for access to the SteamVR shell, and powering the device on and off. |


# Setting up VR Ink
To ensure VR Ink works in applications and appears correctly in the SteamVR shell and status window, an initial installation is required.

## 1. Install the Driver
1. Download the latest [logitech VR Ink Driver](https://store.steampowered.com/app/1068300/Logitech_VR_Ink_Driver/) from the **Steam Store**.<br>
[<img src="../Documentation/Images/SteamVR/steamstore_page_small.png" width="500" >](../Documentation/Images/SteamVR/steamstore_page_small.png)<br><br>
2. After successful install the SteamVR status window should look like the one below: you'll note an additional icon (greyed out) for the Logitech VR Ink driver. <br>
[<img src="../Documentation/Images/SteamVR/steamvr_dash_after_install.png">]()<br><br>
Some verifications before you move on:
   * Make sure your **HMD is connected** and turned on.
   * Verify that SteamVR is **NOT in Safe mode**, as the driver won't be loaded, if that is the case **restart SteamVR**.<br><br>
3. Then **pair your VR Ink** following the steps described below.


<br>
<br>

## 2. Pairing VR Ink
1. If you already have two controllers paired and connected, **first turn OFF the one you want to replace** the VR Ink with (ex: the right one if you're right-handed pen user)<br>
[<img src="../Documentation/Images/SteamVR/steamvr_dash_off_before_pair.png">]()

2. In the SteamVR status window, right click on (any) controller icon and select **Pair Controller**.<br>

3. Click on the button **"I want to pair a different type of controller ..."**

4. Click on the **Logitech VR Ink** section (instructions available starting from SteamVR 1.9 version) and follow the instructions. <br>

5. Press (and hold) both the **MENU** and **SYSTEM** buttons on VR Ink for a few seconds until the status LED on the stylus starts blinking blue.<br>
[<img src="../Documentation/Images/SteamVR/steamvr_pair_vrink.png" width="500" alt="Pair: select controller">]()

6. When VR Ink is successfully paired, the LED will appear as a **solid green**, click **Done**.<br>
[<img src="../Documentation/Images/SteamVR/steamvr_pair_vrink_success.png" width="500" alt="Pair: select controller">]()

7. You should also see a new stylus icon in SteamVR status window alongside the VR Ink Driver one. You'll note now that both the icons for VR Ink stylus are active.
   <br>
[<img src="../Documentation/Images/SteamVR/steamvr_dash_ok_full.png" alt="SteamVR status all good">]()

8. When it first start, the VR Ink Driver will suggest to opt in for some analytics: <br>**Rest reassured**: We won't be able to see what you draw or write, but only measure the amount of time that the stylus is being used. This will allow us to **improve the HW and the SW experience** in the future !<br>
[<img src="../Documentation/Images/Driver/DriverAnalytics.png" alt="Driver Analytics opt in" width="500">]()

<br>
<br>

## 3. Using VR Ink
* Now, If you wear the HMD you should see the VR Ink model in the SteamVR Shell: try to move it !
* You can bring up the **Logitech VR Ink settings UI** either from clicking on its icon in the **taskbar** or from the windows **tray**:<br>[<img src="../Documentation/Images/Driver/task_bar_icon.png" width="167" alt="Task Bar">]() [<img src="../Documentation/Images/Driver/vrink driver tray.png" width="214" alt="Windows tray">]()<br>
* The UI allows you to **verify the status** of your stylus as well as **select specific pressure sensitivity** for the PRIMARY, TIP and GRIP force sensing buttons.<br>
[<img src="../Documentation/Images/Driver/driver_ui.png" width="500" alt="Driver UI">]()

* After your VR Ink is set up, we highly recommend that you check out our Demo App:<br>
[<img src="../Documentation/Images/LandingPage/DemoExperience.png" width="214" alt="Demo Experience">](../Documentation/DemoExperience)

* or have a look at our integration tutorials:<br>
[<img src="../Documentation/Images/LandingPage/UnityIntegration.png" width="214" alt="Unity Integration">](./UnitySampleProjects)
[<img src="../Documentation/Images/LandingPage/UnrealIntegration.png" width="214" alt="Unreal Integration">](./UnrealSampleProject)
[<img src="../Documentation/Images/LandingPage/NativeIntegration.png" width="214" alt="Native Integration">](./NativeSampleProject)

* If you want to know more about the features of the VR Ink driver, click on Driver Icon here below:<br>
[<img src="../Documentation/Images/LandingPage/Driver.png" width="214" alt="Demo Experience">](../Documentation/Driver)
