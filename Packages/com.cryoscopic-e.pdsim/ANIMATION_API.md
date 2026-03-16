# PDSim Animation API Documentation

The **PDSim Animation API** is a fluent, chainable C# library designed to help you quickly create complex Unity animations for PDDL fluents and actions. It is built directly into the PDSim package and requires no external dependencies.

---

## 🚀 Quick Start

Every generated `FluentVisualizer` script now includes a boilerplate `AnimateRoutine`. You can use the `PDSimAnimator` within this routine to define your visuals.

```csharp
using PDSim.Utils.Animation;

// ... inside your visualizer class
private IEnumerator AnimateRoutine(GameObject[] objects, bool value, float duration, Action onComplete)
{
    if (value) 
    {
        yield return PDSimAnimator.Sequence()
            .Move(objects[0]).To(objects[1]).Duration(duration)
            .Play();
    }
    onComplete?.Invoke();
}
```

---

## 🏗 Core Concepts

### 1. The Builder Pattern
The API uses a **Fluent Builder**. You chain methods together to describe "what" happens, and then call `.Play()` to receive an `IEnumerator` that Unity can execute.

### 2. Execution Blocks
*   **`Sequence()`**: Actions run one after another.
*   **`Parallel()`**: Actions run at the same time. You must call `.End()` to close a parallel block if it's nested inside a sequence.

---

## 🛠 Available Actions

### Transformations (Tweenable)
These actions take time and support `.Duration(seconds)` and `.WithEasing(type)`.

| Method | Description |
| :--- | :--- |
| `.Move(target).To(position)` | Moves target to a `Vector3` or another `GameObject`. |
| `.Rotate(target).To(rotation)` | Rotates target to a `Quaternion` or `Vector3` (Euler). |
| `.Rotate(target).By(axis)` | Rotates target relative to its current rotation. |
| `.Scale(target).To(scale)` | Scales target to a new `Vector3`. |
| `.Color(target, color)` | Smoothly lerps the material color (supports URP). |

**Modifiers:**
*   `.InLocalSpace()`: Use local coordinates instead of world coordinates (available for Move and Rotate).

### Hierarchy & Visuals (Instant)
These actions happen immediately at their point in the sequence.

| Method | Description |
| :--- | :--- |
| `.Attach(child, parent)` | Sets the parent of the child object (maintains world position). |
| `.Detach(child)` | Removes the parent of the child object. |
| `.Show(target)` | Sets the target GameObject to active. |
| `.Hide(target)` | Sets the target GameObject to inactive. |

### Control
| Method | Description |
| :--- | :--- |
| `.Wait(seconds)` | Adds a pause to the sequence. |
| `.Then()` | Purely cosmetic; used to separate actions in a sequence for readability. |

---

## 📈 Easing Functions

You can make your animations feel more natural by using `.WithEasing(EasingType)`.

*   `Linear` (Default)
*   `InQuad`, `OutQuad`, `InOutQuad`
*   `InCubic`, `OutCubic`, `InOutCubic`
*   `InBack`, `OutBack`
*   `SmoothStep`

---

## ⏳ Handling Durative Actions

PDSim automatically calculates the duration of the PDDL action causing a state change and passes it to the `AnimateRoutine`. 

**Tip:** Always prefer using the `duration` parameter for your primary animation. This ensures that if a PDDL action takes 5 seconds, your Unity animation will take exactly 5 seconds, staying perfectly in sync with the plan.

---

## 🌟 Advanced Example

```csharp
yield return PDSimAnimator.Sequence()
    // Move to the target over the full action duration
    .Move(objects[0]).To(objects[1]).Duration(duration).WithEasing(EasingType.SmoothStep)
    .Then()
    // Simultaneously rotate the object and change its color
    .Parallel()
        .Rotate(objects[0]).By(new Vector3(0, 180, 0)).Duration(0.5f)
        .Color(objects[0], Color.green).Duration(0.5f)
    .End()
    // Finally, attach it to the robot
    .Attach(objects[1], objects[0])
    .Play();
```
