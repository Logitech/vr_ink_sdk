# Native Integration
Here we provide a sample VR Ink integration using **OpenGL** and **SteamVR Input System 2.0**.

Note that this is not an OpenGL tutorial. This sample focuses only on the VR Ink Integration; **prior knowledge of OpenGL is strongly recommended.**

## Before You Start
We recommend that you:
- Follow these [**design guidelines**](../../Documentation/DesignGuidelines) when integrating or creating interactions for VR Ink.
- Look at the examples of their implementation in the [**VR Ink Toolkit**](../../Assets/Toolkit).

## Using SteamVR 2.0 Input System (recommended)
SteamVR has moved to implement the OpenXR standard with the SteamVR 2.0 Input mapping. If your project is using the SteamVR 2.0 input system, you can follow the default implementation in this example.

### VR Ink Integration Challenges
SteamVR uses indexes to describe the different tracked devices it is using. By default the Head Mounted Display will always be index 0. Then starting from index 1 any controller, Base Station or tracked device will use a new index. Unfortunately there's no way to know in advance which index will be assigned to VR Ink.

The SteamVR implementation of left and right hand controller is problematic when using VR Ink in conjunction with another controller (for instance a Vive controller). A user can to take VR Ink in either the right or left hand without distinction and will expect the primary interactions to be on VR Ink regardless. We need to be able detect if VR Ink is assigned to the left or right hand and update the UI in the application accordingly.

### VR Ink Integration Pipeline
Note that all the further steps for VR Ink integration also describe, with some small changes, how to integrate the non-dominant hand controller. In this sample we define it as a Vive controller (either Vive or Vive Pro), but by following these steps you should be able to infer what needs to be done to use a different controller.

#### 1) Find the StreamVR ID for VR Ink.
From `main` we create an object `CMainApplication` and then call the `CMainApplication::BInit()` method. `BInit` creates a `VRSystem`, the main component from OpenVR. From `Binit` we create the window, and we initialize OpenGL with a call of `CMainApplication::BInitGL()` method.
From here, we want to create and render the controllers. The first step is to get the SteamVR index of both VR Ink and the non-dominant hand controller. To do this, we have the following method that takes the name of a controller as an argument and returns its index:

```
uint32_t CMainApplication::GetTrackedDeviceVRIndex(std::string modelName)
{
    for (uint32_t i = 0; i < vr::k_unMaxTrackedDeviceCount; i++)
    {
        if (!m_pHMD->IsTrackedDeviceConnected(i))
        {
            continue;
        }
        vr::ETrackedPropertyError eError = vr::TrackedProp_Success;
        vr::ETrackedDeviceProperty eProperty = vr::Prop_ModelNumber_String;
        char deviceName[32];
        m_pHMD->GetStringTrackedDeviceProperty(i, eProperty, deviceName, 1000, &eError);
        std::string deviceNameStr(deviceName);
        // We remove any dot to deal seamlessly between a Vive and a Vive pro controller.
        deviceNameStr.erase(std::remove(deviceNameStr.begin(), deviceNameStr.end(), '.'), deviceNameStr.end());
        std::transform(deviceNameStr.begin(), deviceNameStr.end(), deviceNameStr.begin(), [](unsigned char c)
        {
            return std::tolower(c);
        });
        if (deviceNameStr.find(modelName) != std::string::npos)
        {
            return i;
        }
    }
    return -1;
}
```

Here we loop through all possible SteamVR indexes which you get with `k_unMaxTrackedDeviceCount` (holds the value 64).

As most of these indexes will not be used, we check that the current index is from a connected device with `m_pHMD->IsTrackedDeviceConnected(i)`. Note that `m_pHMD` is the OpenVR `VRSystem` element. Also note that along with the active controllers, the Base Stations will also return True with `IsTrackedDeviceConnected`.

We're looking for which index `i` is assigned to VR Ink. To determine this, we will compare the name we passed in the argument `modelName` to the name of the device we're checking at the moment. To do that, we call `m_pHMD->GetStringTrackedDeviceProperty(i, eProperty, deviceName, 1000, &eError);`.
The interesting part here is the `eProperty`, which we define just above this call with `vr::ETrackedDeviceProperty eProperty = vr::Prop_ModelNumber_String;`. The Model Number here is the `model_number` in the config file of that controller.
We remove any periods in the name, replace uppercase letters with lowercase letters and compare the result to the given name in the `modelName` argument. If there is a match, we have found the desired controller and can return the index. If there is no match, the controller we're looking for is not in use with the SteamVR system at the moment.

#### 2) Assign VR Ink to the Correct Hand
As we know the index of each controller, the next phase is to assign the action bindings. Before we can do that we have to know if VR Ink is set as the Right or Left controller in the SteamVR system.

From `BInitGL` method we call the `CMainApplication::InitSteamVRInput(uint32_t inkIndex, uint32_t viveIndex)` method. The first part here consists of setting the action.json manifest and controller's bindings files (available [here](dependencies/defaultResources/)). We then create the global Action Handle. Note that there is only one Action Handle in the programm and it will deal with all actions of all controllers.
Next we have to provide a source handle and then define the bindings to use. To assign the source handle, we need to know which hand VR Ink is assigned to. We solve this by calling `CMainApplication::AssignHandHandle(uint32_t inkIndex)`. This method checks if the index of VR Ink passed in the argument corresponds to the right hand controller with `m_pHMD->GetTrackedDeviceIndexForControllerRole(vr::ETrackedControllerRole::TrackedControllerRole_RightHand);`.
From the result of the comparison it will assign VR Ink and the non-dominant hand controller to their respective roles.

#### 3) Assign the Bindings for VR Ink
With the source handle set up, we can now proceed with the bindings assignment. This consists of linking an action defined in the [Action manifest](dependencies/defaultResources/) to a `VRActionHandle_t`. We have already prepared an array of two `ControllerManager` Objects which each have a Struct `DeviceHandlesInfo` defining all the `VRActionHandle_t` we need.
For both controllers, we define each binding of the `DeviceHandlesInfo` with the 1st element of the array being VR Ink and the second one the non-dominant hand controller.

#### 4) Create The VrInkManager
Continuing with the `BInitGL` method, it now calls the `SceneRenderer::SetupScene(...)` method which will create the `VrInkManager` Object and assign the previously defined handles.
At this point, VR Ink is properly initialized.

#### 5) Update the Data for VR Ink
When everything is initialized correctly, the program enters a routine which consists of getting new input from SteamVR and updating accordingly.
The `main` will call the `CMainApplication::RunMainLoop()` method which will call the `SceneRenderer::UpdateScene(...)` method. The `SceneRenderer` will take care of calling the `VrInkManager::UpdateControllerStatus()` method.
We start with a call from `vr::EVRInputError eError = vr::VRInput()->UpdateActionState(&actionSet, sizeof(actionSet), 1);` which updates the status of the global Action Handle. From there, if the controller is active, we begin by updating the position. We also verify that the Render Model Name was already defined, which will not be the case on the first call. If not, we get the Render Model Name from `VrInkManager::GetStringProperty(vr::TrackedDeviceIndex_t unDevice, vr::TrackedDeviceProperty prop, vr::TrackedPropertyError *peError)`. This method will return the path to where the render model file is located. It's the same render model which is used in the SteamVR shell. We can then load the render model with `ControllerManager::LoadRenderModel(std::string renderModelName)`. The model only needs to be loaded once.
Once the position is updated and the model loaded, if needed, the method will check all the handles and will update the values of everything.

As a basic input usage example, you can look at the `VrInkManager::UpdateButtonHighlights()` that will highlight button's actions on VR Ink.

At this point, you have a VR Ink that will render in your scene and report every input. Then it's up to you to be creative!

## Use the Provided Logitech Driver API as Input
If you don't want to use the SteamVR 2.0 Input mapping, the VR Ink Driver comes with an API that is also available here.

To use the API version of this example, change the class variable `m_bUseVRInkApi` from `false` to `true` in the `CMainApplication::CMainApplication(...)` constructor.
At this point, you don't need to care about the bindings and actions; everything is taken care of by our Driver.

Note that the driver is still using SteamVR itself, so it will add a small delay in the input acquisition.

The API doesn't provide data for other controllers. In this example, the HTC Vive controller will use the SteamVR 2.0 Input mapping either way.

## Troubleshooting
When running this application, you may encounter the following exception:

`Exception thrown at 0x00007FFB5F26D93E (nvoglv64.dll) in VRInkNative.exe: 0xC0000005: Access violation reading location 0x0000000000000000.`

This is a known issue that seems to happen on some machines and not others. Closing the process and starting the application will eventually make it work.

If you experience any problem with your VR Ink device, have a look at the [FAQ section](./../FAQ/Readme.md).
If this does not solve your problem you can contact us at [vrinksupport@logitech.com](mailto:vrinksupport@logitech.com).
