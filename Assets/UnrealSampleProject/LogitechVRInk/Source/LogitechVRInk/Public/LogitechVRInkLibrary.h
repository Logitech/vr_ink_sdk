/* Copyright (c) Logitech Corporation. All rights reserved. Licensed under the MIT License.*/

#pragma once

#include "CoreMinimal.h"
#include "Kismet/BlueprintFunctionLibrary.h"
#include "MotionControllerComponent.h"
#include "LogitechVRInkLibrary.generated.h"

DECLARE_LOG_CATEGORY_EXTERN(Logitech, Log, All);

UCLASS()
class LOGITECHVRINK_API ULogitechVRInkLibrary : public UBlueprintFunctionLibrary
{
	GENERATED_BODY()

    /**
    * Tells if the motion controller component is a Logitech VR Ink
    * @return  Whether the controller is a Logi Pen or not.
    */
    UFUNCTION(BlueprintCallable, Category = "Logitech VR Ink")
    static bool IsVRInk(const UMotionControllerComponent* motionController);
};
