# Air Drawing

![Banner AirDrawing World](../Images/Toolkit/AirDrawing/Banner_AirDrawingWorld.gif)

## Interactions

We will go trough 2 different interactions:

* Air Drawing : inking in 3D
* Undo/Redo : undo or redo a line that has just been drawn

An implementation of these 2 interactions can be found in the  scene `5_Example_AirDrawing`. The Air Drawing interaction will be on the `AirDrawing` GameObject whilst the Undo/Redo is found on the `LogitechVRInkSimple` prefab.

![Hierarchy Air Drawing](../Images/Toolkit/AirDrawing/Hierarchy_AirDrawing.png)

## Implementation

The `AirDrawing.cs` script is a classic Toolkit interaction. It has a trigger, `DrawingTrigger`, which is an `InputTrigger` allowing us to get input from either the primary or non dominant hand tracked device. The `AirDrawingAction`is in charge of creating the line.

![Inspector Air Drawing](../Images/Toolkit/AirDrawing/Inspector_AirDrawing.png)

### Brush Modes

For the `AirDrawing` Interaction to be able to work, we need to have the script component `DrawingVariables.cs` somewhere in the scene. By default it's attached to our `LogitechVRInkSimple` prefab. In this example, the different brush mode use the same asset, but you you can imagine building different asset and then easily switch between them with this script.

This also allows us to have a reference to current brush size and brush color. Changing the color here will impact all the drawing colors, as well as any visual feedback tied to the active color. You can see this in action in our demo application.

![Inspector Drawing Variables](../Images/Toolkit/AirDrawing/Inspector_DrawingVariables.png)

### The Lines

To produce the drawing, `AirDrawingAction.cs` will create a new  GameObject with a `LineRenderer` component. The `LineRenderer` point follows the tracked device position and the width of the line is determined by the pressure you are applying on the primary button of the VR Ink. Every stroke will create a new GameObject.

Though you can decide to have a fixed width when drawing using the `Line Settings` of the `AirDrawingAction`.

![Interaction Line Width](../Images/Toolkit/AirDrawing/Interaction_LineWidth.png)

We found that the `LineRenderer` comes with some limitations. For instance, when drawing slowly you can create points too close to each other for the `LineRenderer` component to determine in which direction the line is supposed to go: that line segment would just be transparent.

To avoid this issue, we had to define a minimum distance at which a new point can be created.

```csharp
private void AddPoint(LineRenderer line, WidthCurve curve, Vector3 newPosition, float width)
{
	float distance = Vector3.Distance(_lastPosition, newPosition);
	if (distance < MinimalDrawingDistance && curve.Distances.Count > 0)
	{
		line.widthCurve = curve.GetCurve();
		line.SetPosition(line.positionCount - 1, line.transform.InverseTransformPoint(newPosition));
		return;
	}
	_lastPosition = newPosition;
	curve.AddPoint(width, distance);
	line.widthCurve = curve.GetCurve();
	line.positionCount++;
	line.SetPosition(line.positionCount - 1, line.transform.InverseTransformPoint(newPosition));
}
```

#### Line smoothing

By default the drawing will use the raw tracked device position, resulting in a line without filtering.  We found that smoothing the line created a better representation of the intent of the user. As such we provide a smoothing algorithm that you can activate in the `LineSettings`. We compute the average of the last X point, where X is the `Window Size` defined in the `LineSettings`, and apply this new average position to the last drawn point.

### Air Drawing prevention

In real use case scenario there's several cases where you don't want to draw. We have implemented two ways to contextually prevent drawing.

* Prevent Air Drawing on Raycast
  * In this case we use a `RaycastTrigger`. If you are pointing at a element that has the UIInteractable tag you cannot draw. This is expected behaviour; when the user is pointing at UI element you should not be able to draw but instead select the UI
* Prevent Air Drawing on Collision
  * We use a `CollisionTrigger` for this case. This allows you to define zones with Colliders where you do not want to activate AirDrawing.

![AirDrawingPrevention](../Images/Toolkit/AirDrawing/Inspector_AirDrawingPrevention.png)

### Undo Redo

As you sketch on paper, you might erase parts of your sketch and do it again until it's perfect. That's something we should also be able to do while drawing/sketching in VR. Each new stroke created by the `AirDrawingAction` also has `UndoRedoGameObject` component.

 The Undo Redo works as a stack of actions. Trigger a Undo and you will call the defined `Undo()` method for the last action on the stack. For the line it will call `SetActive(false)` on the GameObject. Trigger a Undo again you will continue to go down the stack. And trigger a Redo, you will go up once on stack and call the defined `Redo()` method for the given stack action.

```csharp
public class UndoRedoGameObject : MonoBehaviour, IUndoRedo
{
	public void Clear()
	{
		Destroy(this.gameObject);
	}

	public void Redo()
	{
		gameObject.SetActive(true);
	}

	public void Undo()
	{
		gameObject.SetActive(false);
	}

	void Start()
	{
		UndoRedoManager.Instance.RegisterNewAction(this);
	}
}
```

The `LogitechVRInkSimple` prefab come with the `UndoRedoManager.cs` component that allow you to define buttons to trigger Undo and Redo. By default we use buttons from the Non Dominant Hand Controller.

![Inspector Undo Redo Manager](../Images/Toolkit/AirDrawing/Inspector_UndoRedoManager.png)
