# Logitech VR Ink Driver

The Logitech VR Ink driver will allow you to modify the you are interacting with the device. You will be able to define force response curve for the tip and primary button analog as well as the threshold you want the grip to activate. Ultimately the driver will be automatically downloaded when you first pair the stylus to your system.

<br>

## Still in development

For early version of the VR Ink, we issue a driver that listens to the physical unit and advertises a virtual device. Unlike the physical device, the virtual one registers as either left or right hand and can be used by applications as if it were the real Stylus, with only ~10ms latency.
In the near future, our device firmware will be ready to do most of the heavylifting currently taking place in the driver, at which point we will seamlessly and transparently switch back to using a single device. **If your app can use our virtual device now, it will be 100% compatible with the next generation.**

## Installing the driver

Download the Logitech VR Ink driver on the [releases page](https://github.com/Logitech/labs_vr_pen_sdk/releases), run the application and follow the instructions on screen.

![Installer Console](resources/driver/startingConsole.PNG)

Press 1, then Enter and choose yes to any overwrite permissions.

If you want to uninstall the driver you can run the logitech_stylus_installer again and choose the uninstall function.

## Features

This is the driver UI:

![driver UI](resources/driver/driverUI.png)

You can change the curve response for both the tip and the primary analog button. Today both of these are linked, pressing on the tip of the stylus or the analog button at the top of the stylus will activate the 2 preview sliders.

The Driver UI should normally automatically close when you quit SteamVR, but if you want to manually close the driver UI you wil have to go to the system tray and right click on the Logi Icon and select Quit.

