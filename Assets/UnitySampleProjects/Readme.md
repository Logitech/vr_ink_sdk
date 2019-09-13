# Before you start

We recommend that you:
- Follow [these **design guidelines**](../../Documentation/DesignGuidelines) when integrating or creating interactions for the VR Ink. 
- Look at the examples of their implementation in the [**VR Ink Toolkit**](../../Assets/Toolkit).

# Unity Integration

We are providing examples for the different ways that a developed has to integrate the VR Ink using Unity.

## 1. Using SteamVR 2.0 Input System (recommended)
SteamVR has moved to implement the OpenXR standard with the SteamVR 2.0 Input mapping: If your project is using the SteamVR 2.0 input system, you can use our SteamVR 2.0 Unity sample to integrate the VR Ink.
[Get started here](./UnitySample_SteamVR2.0).

## 2. Using Legacy SteamVR
We are aware that many applications have yet to make the jump to the new SteamVR 2.0 Input system, so if your application is still using the legacy SteamVR approach [SteamVR Plugin](https://github.com/ValveSoftware/steamvr_unity_plugin/releases/tag/1.2.3), our legacy SteamVR Unity sample will help you integrate the VR Ink.
[Get started here](./UnitySample_LegacySteamVR).


## 3. Using Unity input abstracton plugin
If you app is using Unity's own input abstraction layer [see doc](https://docs.unity3d.com/Manual/OpenVRControllers.html), most functionality should be supported



