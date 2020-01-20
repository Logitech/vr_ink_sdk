#pragma once
#define VR_INK_API __declspec(dllexport)
#include <string>
namespace VrInkApi {
    typedef enum _ETouchstripMode
    {
        Smart = 0,
        Default,
        Disabled,
        SmartPreprocessed,
        MaxElements
    }ETouchstripMode;

    typedef enum _ESensor
    {
        Tip = 0,
        Primary = 1,
        Grip = 2
    }ESensor;

    typedef enum _EGripActivationMode
    {
        Soft = 0,
        Medium = 1,
        Hard = 2
    }EGripActivationMode;

    typedef struct _SensorCurve {
        float p0x = 0.0f;
        float p1x = 0.0f;
        float p1y = 0.0f;
        float p2x = 1.0f;
    } SensorCurve;

    typedef struct _DeviceInfo {
        int handler = 0;
        std::string serialNumber = "";
        std::string modelNumber = "";
        std::string renderModel = "";
    } DeviceInfo;

    typedef struct _InkStatus {
        float poseMatrix[16];
        float touchstripX = 0;
        float touchstripY = 0;
        bool touchstripTouch = false;
        bool touchstripClick = false;
        float primaryValue = 0;
        bool primaryTouch = false;
        bool primaryClick = false;
        float tipValue = 0;
        bool tipTouch = false;
        bool tipClick = false;
        bool gripTouch = false;
        bool gripClick = false;
        float gripValue = 0;
        float gripRawValue = 0;
        bool applicationMenu = false;
		uint8_t sensorValue[30];
	} InkStatus;

    extern "C" {
        VR_INK_API void GetApiVersion(uint8_t &major, uint8_t &minor);
        VR_INK_API void GetDriverVersion(uint8_t &major, uint8_t &minor);
        VR_INK_API void SetSensorCurve(ESensor input, SensorCurve curve);
        VR_INK_API void GetSensorCurve(ESensor input, SensorCurve &curve);
        VR_INK_API void SetTouchstripRejectionMode(ETouchstripMode tsMode);
        VR_INK_API void GetTouchstripRejectionMode(ETouchstripMode &tsMode);
        VR_INK_API void SetGripActivationMode(EGripActivationMode mode);
        VR_INK_API void GetGripActivationMode(EGripActivationMode &mode);
        VR_INK_API void GetDeviceStatus(InkStatus &status);
    }
}
