# PDSim Animation System — Developer Guide

This guide covers everything you need to write fluent visualizers for PDSim simulations.

---

## Table of Contents

1. [Overview](#overview)
2. [How the System Works](#how-the-system-works)
3. [Creating an Animation Script](#creating-an-animation-script)
4. [IFluentVisualizer Interface](#ifluentvisualizer-interface)
5. [Reading Fluent Values (GeTAtom)](#reading-fluent-values-getatom)
6. [PDSimAnimator API Reference](#pdsimanimator-api-reference)
7. [Easing Types](#easing-types)
8. [PDSimMetadata — Anchors and Renders](#pdsimmetadata--anchors-and-renders)
9. [Examples by Pattern](#examples-by-pattern)
10. [PDSimMetadata Deep-Dive Example](#pattern-10--pdsimmetadata-deep-dive)
11. [Common Mistakes](#common-mistakes)

---

## Overview

In PDSim, every PDDL fluent change during plan execution triggers a **visualizer** — a C# script that animates the corresponding Unity objects to reflect the new world state.

**Example:** If the PDDL plan contains the effect `(on block1 block2)`, PDSim will call your visualizer for `on(block, block)` with:
- `objects[0]` → the Unity GameObject representing `block1`
- `objects[1]` → the Unity GameObject representing `block2`
- `value` → a `GeTAtom` holding `true` (boolean fluent)
- `duration` → how long this action takes in the plan timeline

Your visualizer decides what to do visually — move, color, show, hide, or any combination.

---

## How the System Works

```
Plan action executes
       │
       ▼
GeTStateVariable change detected
       │
       ▼
AnimationsController.UpdateQueue()
       │
       ├─ Animations.AnimationCheck() matches fluent name + parameter types
       │
       ▼
AnimationQueueElement enqueued
(value, objects[], duration, className)
       │
       ▼
AnimationMachineLoop processes queue
       │
       ▼
TriggerAnimation()
  ├─ Clones animation GameObject from SimpleObjectPool
  ├─ Finds IFluentVisualizer (or auto-attaches via scriptClassName)
  └─ Calls Animate(args, value, objects, duration, onComplete)
             │
             ▼
        Your visualizer runs
             │
             ▼
        onComplete() MUST be called
             │
             ▼
        Next animation dequeues
```

The system supports **concurrent animations** — each action runs its own queue identified by the action ID. Multiple fluent changes from the same action play in sequence within that action's queue; different actions' animations can overlap.

---

## Creating an Animation Script

### Via the Editor (Recommended)

1. Select the **Animations** GameObject in your simulation scene.
2. Find the `FluentAnimation` component for your fluent (e.g., `on`).
3. Click **+** to open the Create Animation window.
4. Select the concrete parameter types for this variant (e.g., `block`, `block`).
5. Click **Create** — PDSim generates a C# script and a GameObject, and compiles.
6. Open the generated script and implement the `AnimateRoutine` coroutine.

### Manually

Create a C# script in **namespace `GeneratedVisualizers`** that implements `IFluentVisualizer`:

```csharp
using System;
using System.Collections;
using System.Collections.Generic;
using PDSim.Components;
using PDSim.Utils.Animation;
using GeTModel;
using UnityEngine;

namespace GeneratedVisualizers
{
    public class On_Block_BlockVisualizer : MonoBehaviour, IFluentVisualizer
    {
        public void Animate(List<string> args, GeTAtom value, GameObject[] objects,
                            float duration, Action onComplete)
        {
            StartCoroutine(AnimateRoutine(objects, value, duration, onComplete));
        }

        private IEnumerator AnimateRoutine(GameObject[] objects, GeTAtom value,
                                           float duration, Action onComplete)
        {
            // ... your animation logic ...

            onComplete?.Invoke(); // REQUIRED
        }
    }
}
```

> **Namespace is mandatory.** `AnimationsController` looks up scripts as
> `"GeneratedVisualizers." + scriptClassName` across all loaded assemblies.
> Scripts outside this namespace will never be found.

---

## IFluentVisualizer Interface

```csharp
public interface IFluentVisualizer
{
    void Animate(List<string> args, GeTAtom value, GameObject[] objects,
                 float duration, Action onComplete);
}
```

| Parameter | Type | Description |
|-----------|------|-------------|
| `args` | `List<string>` | Fluent argument names (currently always passed as empty list; use `objects[]` for GameObjects) |
| `value` | `GeTAtom` | The new value of the fluent (Symbol / Int / Real / Boolean) |
| `objects` | `GameObject[]` | Unity GameObjects for each fluent parameter, in declaration order |
| `duration` | `float` | Action duration in seconds (from plan timeline). `0` for init-block fluents |
| `onComplete` | `Action` | **MUST be called exactly once** when the animation finishes |

### The onComplete Contract

`onComplete` is the signal that tells the queue to advance. **Failing to call it will freeze the entire simulation.**

```csharp
// ✅ Correct — always called, even on early return
private IEnumerator AnimateRoutine(GameObject[] objects, GeTAtom value,
                                   float duration, Action onComplete)
{
    if (objects.Length == 0) { onComplete?.Invoke(); yield break; }

    yield return PDSimAnimator.Sequence()
        .Move(objects[0]).To(objects[1]).Duration(duration)
        .Play();

    onComplete?.Invoke();
}

// ❌ Wrong — onComplete never called if objects is empty → simulation hangs
private IEnumerator AnimateRoutine(GameObject[] objects, GeTAtom value,
                                   float duration, Action onComplete)
{
    if (objects.Length == 0) yield break; // BUG: queue stalls

    yield return PDSimAnimator.Sequence()
        .Move(objects[0]).To(objects[1]).Duration(duration)
        .Play();

    onComplete?.Invoke();
}
```

---

## Reading Fluent Values (GeTAtom)

`GeTAtom` is a tagged union — exactly one of its four fields is set.

```csharp
public class GeTAtom
{
    public string?  Symbol       { get; }  // e.g. "red", "table"
    public long?    IntValue     { get; }  // e.g. 3
    public GeTReal? RealValue    { get; }  // e.g. 0.5 (numerator/denominator)
    public bool?    BooleanValue { get; }  // true / false
}
```

### Reading each type

```csharp
// Boolean fluent — e.g., (clear ?x) := true
if (value.BooleanValue.HasValue)
{
    bool isSet = value.BooleanValue.Value;
}

// Symbol fluent — e.g., (color ?x) := red
if (value.Symbol != null)
{
    string symbol = value.Symbol; // "red"
}

// Integer fluent — e.g., (count ?x) := 5
if (value.IntValue.HasValue)
{
    long count = value.IntValue.Value;
}

// Real fluent — e.g., (weight ?x) := 1.5
if (value.RealValue != null)
{
    double weight = value.RealValue.ToDouble(); // 1.5
}
```

### Helper method (use in every visualizer)

```csharp
private object GetNativeValue(GeTAtom atom)
{
    if (atom.BooleanValue.HasValue) return atom.BooleanValue.Value;
    if (atom.Symbol != null)        return atom.Symbol;
    if (atom.IntValue.HasValue)     return atom.IntValue.Value;
    if (atom.RealValue != null)     return atom.RealValue.ToDouble();
    return null;
}
```

---

## PDSimAnimator API Reference

`PDSimAnimator` is a static factory that produces a builder chain. Call `.Play()` at the end and `yield return` the result.

```csharp
// Sequential — actions run one after another
yield return PDSimAnimator.Sequence()
    /* ... actions ... */
    .Play();

// Parallel — all actions run simultaneously, waits for all to finish
yield return PDSimAnimator.Parallel()
    /* ... actions ... */
    .Play();
```

### Move

```csharp
// Move to a world-space position
.Move(gameObject).To(new Vector3(1, 0, 0)).Duration(1f)

// Move to another GameObject's position (dynamic — updates every frame)
.Move(gameObject).To(targetGameObject).Duration(duration)

// Move to a named anchor on the SOURCE object (resolved via PDSimMetadata on the mover)
.Move(gameObject, "GripPoint").To(targetGameObject).Duration(1f)

// Move to a named anchor on the DESTINATION object (resolved via PDSimMetadata on the target)
.Move(gameObject).To(targetGameObject, "Top").Duration(1f)

// Both — source anchor moves to destination anchor
.Move(gameObject, "Base").To(targetGameObject, "Top").Duration(1f)

// Move in local space
.Move(gameObject).To(new Vector3(0, 1, 0)).Duration(0.5f).InLocalSpace()

// With easing
.Move(gameObject).To(target).Duration(1f).WithEasing(EasingType.OutBack)
```

> **NavMesh automatic dispatch:** if the moving GameObject has a `VisualisationObject` component
> with `useNavMeshAgent = true`, `MoveAction` bypasses the lerp and drives the `NavMeshAgent`
> instead. No code change needed in the visualizer — the builder handles it transparently.
> Pause state and the animation speed slider are both respected during NavMesh traversal.

### Rotate

```csharp
// Rotate to an absolute quaternion
.Rotate(gameObject).To(Quaternion.Euler(0, 180, 0)).Duration(1f)

// Rotate to Euler angles (vector overload of To)
.Rotate(gameObject).To(new Vector3(0, 180, 0)).Duration(1f)

// Rotate by a delta (relative)
.Rotate(gameObject).By(new Vector3(0, 360, 0)).Duration(2f)

// Rotate in local space
.Rotate(gameObject).By(new Vector3(0, 90, 0)).Duration(0.5f).InLocalSpace()
```

### Scale

```csharp
// Scale to a target scale
.Scale(gameObject).To(new Vector3(2, 2, 2)).Duration(0.5f)

// Scale with easing
.Scale(gameObject).To(Vector3.one * 1.5f).Duration(1f).WithEasing(EasingType.InOutQuad)
```

### Color

```csharp
// Change the object's renderer color (animates over time)
.Color(gameObject, Color.red).Duration(0.5f)

// Change a specific renderer identified by an aesthetic tag (PDSimMetadata)
.Color(gameObject, "Body", new Color(1f, 0.5f, 0f)).Duration(1f)
```

> Sets both `_Color` and `_BaseColor` on the material property block — works with both Legacy and URP shaders.

### Show / Hide

```csharp
// Instant (no duration)
.Show(gameObject)
.Hide(gameObject)
```

### Attach / Detach

```csharp
// Reparent child to parent (maintains world position)
.Attach(child, parent)

// Unparent (world position preserved)
.Detach(child)
```

### Text

```csharp
// Update a TMP_Text or UI Toolkit TextElement identified by a UI tag (PDSimMetadata)
.Text(gameObject, "label", "Hello World")
```

### Wait

```csharp
// Pause for N seconds (respects animation speed and pause state)
.Wait(1.5f)
```

### Then

```csharp
// Readability alias — does nothing, returns the same builder
.Then()
```

### Nested Parallel inside a Sequence

```csharp
yield return PDSimAnimator.Sequence()
    .Move(obj).To(target).Duration(1f)      // step 1
    .Parallel()                              // step 2: run simultaneously
        .Rotate(obj).By(new Vector3(0, 360, 0)).Duration(1f)
        .Scale(obj).To(Vector3.one * 1.5f).Duration(1f)
    .End()                                   // back to sequence
    .Color(obj, Color.green)                 // step 3
    .Play();
```

### Full builder chain reference

| Method | Description |
|--------|-------------|
| `Move(go)` / `Move(go, tag)` | Translate GameObject (optionally via source anchor tag). Auto-uses NavMesh if `VisualisationObject.useNavMeshAgent` is true. |
| `Rotate(go)` / `Rotate(go, tag)` | Rotate GameObject (optionally via anchor tag) |
| `Scale(go)` | Scale GameObject |
| `Color(go, color)` | Animate renderer color |
| `Color(go, tag, color)` | Animate named renderer color (PDSimMetadata) |
| `Text(go, tag, text)` | Update TMP or UIToolkit text |
| `Show(go)` | `SetActive(true)` — instant |
| `Hide(go)` | `SetActive(false)` — instant |
| `Attach(child, parent)` | Reparent — instant |
| `Detach(child)` | Unparent — instant |
| `Wait(secs)` | Pause execution — speed-aware |
| `Then()` | Readability no-op |
| `Parallel()` | Begin a nested parallel block |
| `End()` | Close a nested parallel block |
| `.To(Vector3)` | Set move/scale/rotate target |
| `.To(GameObject)` | Move toward a GameObject's root |
| `.To(GameObject, string)` | Move toward a named anchor on the destination (PDSimMetadata) |
| `.To(Quaternion)` | Set rotation target |
| `.By(Vector3)` | Set relative rotation delta |
| `.Duration(float)` | Tween duration in seconds |
| `.WithEasing(type)` | Easing function |
| `.InLocalSpace()` | Use local-space coordinates |
| `.Play()` | Build and return `IEnumerator` — must be yielded |

---

## Easing Types

```csharp
EasingType.Linear       // Constant speed (default)
EasingType.InQuad       // Starts slow, accelerates
EasingType.OutQuad      // Starts fast, decelerates
EasingType.InOutQuad    // Slow-fast-slow
EasingType.InCubic      // Stronger acceleration in
EasingType.OutCubic     // Stronger deceleration out
EasingType.InOutCubic   // Strong slow-fast-slow
EasingType.InBack       // Slight pullback before moving forward
EasingType.OutBack      // Overshoots then settles
EasingType.SmoothStep   // Smooth S-curve (good general purpose)
```

---

## PDSimMetadata — Anchors and Renders

`PDSimMetadata` is a component you add to your prefabs to expose named sub-objects to the animation system. Configure it in the Inspector; access it at runtime.

```
PDSimMetadata
 ├── anchors  → named Transform references (attach points, snap targets)
 ├── renders  → named Renderer references (color targets)
 ├── ui       → named Component references (TMP, UIDocument)
 └── attributes → named string key-value pairs
```

### In your visualizer

```csharp
// Access directly — useful when you need the Transform or Renderer for non-builder logic
var meta = objects[0].GetComponent<PDSimMetadata>();

Transform topAnchor   = meta.GetAnchor("Top");
Renderer bodyRenderer = meta.GetRender("Body");
string typeAttr       = meta.GetAttribute("pddl_type");

// Builder source-anchor tag — the moving object's named point does the moving
.Move(objects[0], "Base").To(objects[1]).Duration(1f)

// Builder destination-anchor tag — snap to a named point on the target object
.Move(objects[0]).To(objects[1], "Top").Duration(1f)

// Both — a named point on the mover travels to a named point on the target
.Move(objects[0], "Base").To(objects[1], "Top").Duration(1f)

// Color a specific renderer by name
.Color(objects[0], "Body", Color.red).Duration(0.5f)
```

> Both source and destination anchor tags resolve via `PDSimMetadata.GetAnchor()`. If the tag
> is missing the builder logs a warning and falls back to the root transform — it never throws.

### Use cases for anchors

- **Stacking:** a `"Top"` anchor on each block lets `.To(block, "Top")` snap precisely without manual position math
- **Grip points:** move a held object to `"GripPoint"` on a robot without resolving the transform yourself
- **Attachment:** combine `.To(robot, "GripPoint")` with `.Attach(obj, gripGO)` for a pick-up sequence
- **Pivot rotation:** pass a source anchor tag to `.Rotate(robot, "ArmBase")` so only that joint rotates

---

## Examples by Pattern

### Pattern 1 — Boolean Show/Hide

**Fluent:** `(active ?robot) := boolean`
**Effect:** Show the robot when active, hide when inactive.

```csharp
namespace GeneratedVisualizers
{
    public class Active_RobotVisualizer : MonoBehaviour, IFluentVisualizer
    {
        public void Animate(List<string> args, GeTAtom value, GameObject[] objects,
                            float duration, Action onComplete)
        {
            bool isActive = value.BooleanValue ?? false;
            StartCoroutine(Run(objects[0], isActive, onComplete));
        }

        private IEnumerator Run(GameObject robot, bool isActive, Action onComplete)
        {
            if (isActive)
            {
                robot.SetActive(true);
                yield return PDSimAnimator.Sequence()
                    .Scale(robot).To(Vector3.one).Duration(0.4f).WithEasing(EasingType.OutBack)
                    .Play();
            }
            else
            {
                yield return PDSimAnimator.Sequence()
                    .Scale(robot).To(Vector3.zero).Duration(0.3f).WithEasing(EasingType.InQuad)
                    .Play();
                robot.SetActive(false);
            }

            onComplete?.Invoke();
        }
    }
}
```

---

### Pattern 2 — Symbol-Driven Color Change

**Fluent:** `(color ?block) := symbol`
**Effect:** Recolor the block to match the symbol value (e.g., "red", "blue", "green").

```csharp
namespace GeneratedVisualizers
{
    public class Color_BlockVisualizer : MonoBehaviour, IFluentVisualizer
    {
        private static readonly Dictionary<string, Color> Palette = new Dictionary<string, Color>
        {
            { "red",    new Color(0.9f, 0.2f, 0.2f) },
            { "green",  new Color(0.2f, 0.8f, 0.3f) },
            { "blue",   new Color(0.2f, 0.4f, 0.9f) },
            { "yellow", new Color(1.0f, 0.85f, 0.1f) },
        };

        public void Animate(List<string> args, GeTAtom value, GameObject[] objects,
                            float duration, Action onComplete)
        {
            string symbol = value.Symbol ?? "red";
            Color target = Palette.TryGetValue(symbol, out var c) ? c : Color.white;
            StartCoroutine(Run(objects[0], target, onComplete));
        }

        private IEnumerator Run(GameObject block, Color target, Action onComplete)
        {
            yield return PDSimAnimator.Sequence()
                .Color(block, target).Duration(0.5f).WithEasing(EasingType.SmoothStep)
                .Play();

            onComplete?.Invoke();
        }
    }
}
```

---

### Pattern 3 — Integer-Driven Scale

**Fluent:** `(stack-height ?loc) := integer`
**Effect:** Scale a height indicator at a location to reflect the stack height.

```csharp
namespace GeneratedVisualizers
{
    public class StackHeight_LocationVisualizer : MonoBehaviour, IFluentVisualizer
    {
        public void Animate(List<string> args, GeTAtom value, GameObject[] objects,
                            float duration, Action onComplete)
        {
            long height = value.IntValue ?? 0;
            StartCoroutine(Run(objects[0], (float)height, onComplete));
        }

        private IEnumerator Run(GameObject location, float height, Action onComplete)
        {
            // Find a child named "HeightBar" to resize
            var bar = location.transform.Find("HeightBar")?.gameObject;
            if (bar == null) { onComplete?.Invoke(); yield break; }

            Vector3 targetScale = new Vector3(1f, Mathf.Max(0.05f, height * 0.5f), 1f);

            yield return PDSimAnimator.Sequence()
                .Scale(bar).To(targetScale).Duration(0.6f).WithEasing(EasingType.OutCubic)
                .Play();

            onComplete?.Invoke();
        }
    }
}
```

---

### Pattern 4 — Move Between Objects (Stack)

**Fluent:** `(on ?x ?y) := boolean`
**Effect:** When `on(x, y)` becomes true, move block x on top of block y.
The `"Top"` anchor on y is resolved automatically by `.To(y, "Top")` — no manual metadata lookup needed.

```csharp
namespace GeneratedVisualizers
{
    public class On_Block_BlockVisualizer : MonoBehaviour, IFluentVisualizer
    {
        public void Animate(List<string> args, GeTAtom value, GameObject[] objects,
                            float duration, Action onComplete)
        {
            // objects[0] = x (block to move)
            // objects[1] = y (destination block)
            bool isOn = value.BooleanValue ?? false;
            StartCoroutine(Run(objects[0], objects[1], isOn, duration, onComplete));
        }

        private IEnumerator Run(GameObject x, GameObject y, bool isOn,
                                float duration, Action onComplete)
        {
            if (!isOn) { onComplete?.Invoke(); yield break; }

            // Arc move: lift x up, slide over, then descend onto y's "Top" anchor.
            // .To(y, "Top") resolves the anchor via PDSimMetadata automatically;
            // falls back to y's root if the anchor is not configured.
            Vector3 start = x.transform.position;
            Vector3 peak  = new Vector3(start.x, start.y + 2f, start.z);

            yield return PDSimAnimator.Sequence()
                .Move(x).To(peak).Duration(duration * 0.4f).WithEasing(EasingType.OutQuad)
                .Move(x).To(y, "Top").Duration(duration * 0.6f).WithEasing(EasingType.InQuad)
                .Attach(x, y)   // make x a child of y so it moves with it later
                .Play();

            onComplete?.Invoke();
        }
    }
}
```

---

### Pattern 5 — Real-Value Progress Bar

**Fluent:** `(fuel ?vehicle) := real`
**Effect:** Animate a fuel gauge (scaled bar child) to reflect the fuel level [0..1].

```csharp
namespace GeneratedVisualizers
{
    public class Fuel_VehicleVisualizer : MonoBehaviour, IFluentVisualizer
    {
        public void Animate(List<string> args, GeTAtom value, GameObject[] objects,
                            float duration, Action onComplete)
        {
            float level = value.RealValue != null ? (float)value.RealValue.ToDouble() : 0f;
            StartCoroutine(Run(objects[0], level, duration, onComplete));
        }

        private IEnumerator Run(GameObject vehicle, float level, float duration, Action onComplete)
        {
            var gauge = vehicle.transform.Find("FuelGauge")?.gameObject;
            if (gauge == null) { onComplete?.Invoke(); yield break; }

            // Set color: green when full, red when empty
            Color gaugeColor = Color.Lerp(Color.red, Color.green, level);

            yield return PDSimAnimator.Sequence()
                .Scale(gauge).To(new Vector3(level, 1f, 1f)).Duration(duration)
                .Color(gauge, gaugeColor)
                .Play();

            // Show an optional warning UI if below 20%
            var warning = vehicle.transform.Find("LowFuelWarning")?.gameObject;
            if (warning != null)
            {
                if (level < 0.2f) warning.SetActive(true);
                else              warning.SetActive(false);
            }

            onComplete?.Invoke();
        }
    }
}
```

---

### Pattern 6 — Parallel Animations

**Fluent:** `(holding ?robot ?object) := boolean`
**Effect:** When the robot picks up an object, simultaneously animate the robot arm raising and the object attaching to the grip point.

```csharp
namespace GeneratedVisualizers
{
    public class Holding_Robot_ObjectVisualizer : MonoBehaviour, IFluentVisualizer
    {
        public void Animate(List<string> args, GeTAtom value, GameObject[] objects,
                            float duration, Action onComplete)
        {
            // objects[0] = robot, objects[1] = object being held
            bool isHolding = value.BooleanValue ?? false;
            StartCoroutine(Run(objects[0], objects[1], isHolding, duration, onComplete));
        }

        private IEnumerator Run(GameObject robot, GameObject obj, bool isHolding,
                                float duration, Action onComplete)
        {
            if (isHolding)
            {
                // .To(robot, "Grip") resolves the "Grip" anchor via PDSimMetadata automatically.
                // We still need the Transform directly for Attach(), so fetch it once.
                var gripTransform = robot.GetComponent<PDSimMetadata>()?.GetAnchor("Grip");
                var gripParent    = gripTransform != null ? gripTransform.gameObject : robot;

                // Run arm raise and object move at the same time
                yield return PDSimAnimator.Parallel()
                    .Rotate(robot, "Arm").To(new Vector3(-45, 0, 0)).Duration(duration)
                    .Move(obj).To(robot, "Grip").Duration(duration).WithEasing(EasingType.InOutQuad)
                    .Play();

                // After parallel finishes, attach the object to the grip
                yield return PDSimAnimator.Sequence()
                    .Attach(obj, gripParent)
                    .Play();
            }
            else
            {
                // Drop: detach and let fall
                yield return PDSimAnimator.Sequence()
                    .Detach(obj)
                    .Rotate(robot, "Arm").To(new Vector3(0, 0, 0)).Duration(duration * 0.5f)
                    .Play();
            }

            onComplete?.Invoke();
        }
    }
}
```

---

### Pattern 7 — Multi-Phase Sequence with Text Update

**Fluent:** `(at ?truck ?location) := boolean`
**Effect:** Move the truck to a location, wait briefly, then update its destination label.

```csharp
namespace GeneratedVisualizers
{
    public class At_Truck_LocationVisualizer : MonoBehaviour, IFluentVisualizer
    {
        public void Animate(List<string> args, GeTAtom value, GameObject[] objects,
                            float duration, Action onComplete)
        {
            // objects[0] = truck, objects[1] = location
            bool arrived = value.BooleanValue ?? false;
            StartCoroutine(Run(objects[0], objects[1], arrived, duration, onComplete));
        }

        private IEnumerator Run(GameObject truck, GameObject location, bool arrived,
                                float duration, Action onComplete)
        {
            if (!arrived) { onComplete?.Invoke(); yield break; }

            string locationName = location.name; // e.g. "depot"

            yield return PDSimAnimator.Sequence()
                // 1. Drive toward the location
                .Move(truck).To(location).Duration(duration * 0.8f).WithEasing(EasingType.InOutQuad)
                // 2. Brief pause on arrival
                .Wait(0.2f)
                // 3. Bounce-settle
                .Scale(truck).To(new Vector3(1.05f, 0.95f, 1.05f)).Duration(0.1f)
                .Scale(truck).To(Vector3.one).Duration(0.15f).WithEasing(EasingType.OutBack)
                // 4. Update destination label
                .Text(truck, "Destination", locationName)
                .Play();

            onComplete?.Invoke();
        }
    }
}
```

---

### Pattern 8 — Conditional Animation on Value

**Fluent:** `(door-state ?room) := symbol`
Values: `"open"`, `"closed"`, `"locked"`
**Effect:** Rotate the door to show its state.

```csharp
namespace GeneratedVisualizers
{
    public class DoorState_RoomVisualizer : MonoBehaviour, IFluentVisualizer
    {
        public void Animate(List<string> args, GeTAtom value, GameObject[] objects,
                            float duration, Action onComplete)
        {
            string state = value.Symbol ?? "closed";
            StartCoroutine(Run(objects[0], state, duration, onComplete));
        }

        private IEnumerator Run(GameObject room, string state, float duration, Action onComplete)
        {
            var door = room.transform.Find("Door")?.gameObject;
            if (door == null) { onComplete?.Invoke(); yield break; }

            Quaternion targetRot = state switch
            {
                "open"   => Quaternion.Euler(0, -90, 0),
                "locked" => Quaternion.Euler(0, 0, 0),
                _        => Quaternion.Euler(0, 0, 0), // closed
            };

            Color indicatorColor = state switch
            {
                "open"   => Color.green,
                "locked" => Color.red,
                _        => Color.grey,
            };

            yield return PDSimAnimator.Parallel()
                .Rotate(door).To(targetRot).Duration(duration).WithEasing(EasingType.InOutCubic)
                .Color(room, "DoorIndicator", indicatorColor).Duration(duration * 0.5f)
                .Play();

            onComplete?.Invoke();
        }
    }
}
```

---

### Pattern 9 — Custom Coroutine Logic (no PDSimAnimator)

For full manual control — physics integration, procedural paths, sampling curves, etc.

> **NavMesh is handled automatically.** If the moving object has `VisualisationObject.useNavMeshAgent = true`,
> `.Move(obj).To(target)` uses the `NavMeshAgent` instead of lerping — no custom code needed.
> Use a raw coroutine only when you need logic the builder cannot express (e.g. physics forces,
> custom path curves, or querying external state each frame).

```csharp
namespace GeneratedVisualizers
{
    public class Position_Robot_LocationVisualizer : MonoBehaviour, IFluentVisualizer
    {
        public void Animate(List<string> args, GeTAtom value, GameObject[] objects,
                            float duration, Action onComplete)
        {
            StartCoroutine(Run(objects[0], objects[1], duration, onComplete));
        }

        private IEnumerator Run(GameObject robot, GameObject location,
                                float duration, Action onComplete)
        {
            Vector3 start = robot.transform.position;
            Vector3 end   = location.transform.position;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                // Respect pause and speed
                if (Controller.Instance != null && Controller.Instance.IsPaused)
                {
                    yield return null;
                    continue;
                }

                float speed = Controller.Instance != null
                    ? Controller.Instance.animationSpeed : 1f;

                elapsed += Time.deltaTime * speed;
                float t = Easing.Apply(Mathf.Clamp01(elapsed / duration), EasingType.InOutQuad);
                robot.transform.position = Vector3.Lerp(start, end, t);
                yield return null;
            }

            robot.transform.position = end;
            onComplete?.Invoke();
        }
    }
}
```

---

### Pattern 10 — PDSimMetadata Deep-Dive

This pattern shows every `PDSimMetadata` category (anchors, renders, UI, attributes) used together in a single visualizer, with a step-by-step explanation of the setup.

**Scenario:** A warehouse robot picks up a crate and loads it onto a conveyor belt. The crate has a color-coded status indicator light, a TMP label showing its ID, and a slot-based attachment point. The robot exposes its gripper position as an anchor, and an attribute encodes whether it is left- or right-handed.

---

#### Step 1 — Set up PDSimMetadata on the prefabs

**Robot prefab** — add `PDSimMetadata` and populate:

| Category | Name | Reference |
|----------|------|-----------|
| `anchors` | `"GripPoint"` | the child Transform at the tip of the gripper |
| `anchors` | `"ArmBase"` | the shoulder pivot Transform used for rotation |
| `attributes` | `"handedness"` | `"right"` (plain string value) |

**Crate prefab** — add `PDSimMetadata` and populate:

| Category | Name | Reference |
|----------|------|-----------|
| `renders` | `"StatusLight"` | the small indicator Renderer on top of the crate |
| `ui` | `"CrateLabel"` | the TMP_Text component showing the crate ID |
| `anchors` | `"SnapBase"` | the Transform at the crate's bottom-centre |

**Conveyor prefab** — add `PDSimMetadata` and populate:

| Category | Name | Reference |
|----------|------|-----------|
| `anchors` | `"LoadSlot"` | the Transform where crates land on the belt |

---

#### Step 2 — The visualizer

**Fluent:** `(loaded ?robot ?crate ?conveyor) := boolean`
**Parameters:** `objects[0]` = robot, `objects[1]` = crate, `objects[2]` = conveyor

```csharp
using System;
using System.Collections;
using System.Collections.Generic;
using GeTModel;
using PDSim.Components;
using PDSim.Utils.Animation;
using UnityEngine;

namespace GeneratedVisualizers
{
    public class Loaded_Robot_Crate_ConveyorVisualizer : MonoBehaviour, IFluentVisualizer
    {
        public void Animate(List<string> args, GeTAtom value, GameObject[] objects,
                            float duration, Action onComplete)
        {
            if (objects.Length < 3) { onComplete?.Invoke(); return; }
            bool isLoaded = value.BooleanValue ?? false;
            StartCoroutine(Run(objects[0], objects[1], objects[2], isLoaded, duration, onComplete));
        }

        private IEnumerator Run(GameObject robot, GameObject crate, GameObject conveyor,
                                bool isLoaded, float dur, Action onComplete)
        {
            // ── Resolve metadata ───────────────────────────────────────────────

            var robotMeta    = robot.GetComponent<PDSimMetadata>();
            var crateMeta    = crate.GetComponent<PDSimMetadata>();
            var conveyorMeta = conveyor.GetComponent<PDSimMetadata>();

            // Anchors — fall back to the root transform when not configured
            Transform gripPoint = robotMeta?.GetAnchor("GripPoint") ?? robot.transform;
            Transform armBase   = robotMeta?.GetAnchor("ArmBase")   ?? robot.transform;
            Transform loadSlot  = conveyorMeta?.GetAnchor("LoadSlot") ?? conveyor.transform;

            // Renderer — used to animate the crate's status light color
            Renderer statusLight = crateMeta?.GetRender("StatusLight");

            // Attribute — drive a mirrored offset for left-handed robots
            string handedness   = robotMeta?.GetAttribute("handedness") ?? "right";
            float  handSign     = handedness == "left" ? -1f : 1f;

            if (isLoaded)
            {
                yield return PickAndPlace(robot, crate, conveyor,
                                          gripPoint, armBase, loadSlot,
                                          statusLight, handSign, dur, onComplete);
            }
            else
            {
                yield return Unload(robot, crate, gripPoint, statusLight, dur, onComplete);
            }
        }

        // ── Loading sequence ───────────────────────────────────────────────────

        private IEnumerator PickAndPlace(
            GameObject robot, GameObject crate, GameObject conveyor,
            Transform gripPoint, Transform armBase, Transform loadSlot,
            Renderer statusLight, float handSign, float dur, Action onComplete)
        {
            float reach  = dur * 0.35f;
            float carry  = dur * 0.40f;
            float settle = dur * 0.25f;

            // 1. Extend arm toward crate (rotate arm base toward pick position)
            //    Using "ArmBase" anchor tag so only the arm pivot rotates, not the whole robot.
            Vector3 crateDir   = (crate.transform.position - armBase.position).normalized;
            Quaternion armRot  = Quaternion.LookRotation(crateDir) * Quaternion.Euler(handSign * -30f, 0, 0);

            yield return PDSimAnimator.Sequence()
                .Rotate(robot, "ArmBase").To(armRot).Duration(reach).WithEasing(EasingType.OutQuad)
                .Play();

            // 2. Simultaneously: move crate to grip point + flash status light yellow (grasping)
            if (statusLight != null)
            {
                // Direct renderer tint — used when we want an instant color not managed by the builder
                statusLight.material.color = new Color(1f, 0.85f, 0.1f); // yellow
            }

            // .To(robot, "GripPoint") resolves the anchor automatically — no manual position math.
            yield return PDSimAnimator.Parallel()
                .Move(crate).To(robot, "GripPoint").Duration(reach * 0.6f).WithEasing(EasingType.InOutQuad)
                .Color(crate, "StatusLight", new Color(1f, 0.85f, 0.1f)).Duration(reach * 0.6f)
                .Play();

            // Parent crate to grip so it follows the robot arm.
            // Attach() needs a GameObject, so we still resolve the Transform directly here.
            yield return PDSimAnimator.Sequence()
                .Attach(crate, gripPoint.gameObject)
                .Play();

            // 3. Carry to conveyor — arm sweeps back, robot drives forward.
            //    If robot has useNavMeshAgent=true the Move will use NavMesh automatically.
            yield return PDSimAnimator.Parallel()
                .Rotate(robot, "ArmBase").To(Quaternion.identity).Duration(carry * 0.4f)
                .Move(robot).To(conveyor).Duration(carry).WithEasing(EasingType.InOutCubic)
                .Play();

            // 4. Place on load slot — detach crate, lower it onto the conveyor's "LoadSlot" anchor.
            yield return PDSimAnimator.Sequence()
                .Detach(crate)
                .Move(crate).To(conveyor, "LoadSlot").Duration(settle).WithEasing(EasingType.InQuad)
                .Attach(crate, loadSlot.gameObject)   // snap to slot so belt carries it
                .Play();

            // 5. Status light → green (loaded OK) and update the crate label
            yield return PDSimAnimator.Sequence()
                .Color(crate, "StatusLight", new Color(0.2f, 0.85f, 0.3f)).Duration(0.3f)
                .Text(crate, "CrateLabel", $"ON BELT")
                .Play();

            onComplete?.Invoke();
        }

        // ── Unloading sequence ─────────────────────────────────────────────────

        private IEnumerator Unload(
            GameObject robot, GameObject crate,
            Transform gripPoint, Renderer statusLight,
            float dur, Action onComplete)
        {
            // Status light → red while detaching
            yield return PDSimAnimator.Sequence()
                .Color(crate, "StatusLight", new Color(0.9f, 0.18f, 0.18f)).Duration(0.2f)
                .Detach(crate)
                .Text(crate, "CrateLabel", "REMOVED")
                .Play();

            // Arm resets
            yield return PDSimAnimator.Sequence()
                .Rotate(robot, "ArmBase").To(Quaternion.identity)
                                         .Duration(dur * 0.5f)
                                         .WithEasing(EasingType.OutBack)
                .Play();

            onComplete?.Invoke();
        }
    }
}
```

---

#### What each metadata category did here

| Category | Entry | Builder syntax used | Purpose |
|----------|-------|---------------------|---------|
| `anchors` | `robot / "GripPoint"` | `.Move(crate).To(robot, "GripPoint")` | Destination anchor — crate snaps to gripper tip |
| `anchors` | `robot / "ArmBase"` | `.Rotate(robot, "ArmBase")` | Source anchor — only the arm pivot rotates |
| `anchors` | `conveyor / "LoadSlot"` | `.Move(crate).To(conveyor, "LoadSlot")` | Destination anchor — crate lands on the belt slot |
| `anchors` | `gripPoint.gameObject` | `.Attach(crate, gripPoint.gameObject)` | Direct use — `Attach()` needs a `GameObject`, resolved manually |
| `renders` | `crate / "StatusLight"` | `.Color(crate, "StatusLight", …)` | Named renderer — only the indicator light changes color |
| `ui` | `crate / "CrateLabel"` | `.Text(crate, "CrateLabel", …)` | Named UI element — TMP label updated with status text |
| `attributes` | `robot / "handedness"` | `meta.GetAttribute("handedness")` | Plain string — drives arm rotation sign; no builder overload for attributes |

---

#### Key rules when working with metadata

```csharp
// ── Prefer builder tag overloads — less code, same safety ─────────────────────

// Destination anchor: move to "Top" on the target (no manual GetAnchor call needed)
.Move(block).To(table, "Top").Duration(1f)

// Source anchor: a specific point on the mover travels to the target
.Move(robot, "GripPoint").To(crate).Duration(1f)

// Both anchors: named point on mover → named point on target
.Move(robot, "GripPoint").To(table, "LoadSlot").Duration(1f)

// All tag overloads fall back to root and log a warning if the key is missing — never throw.

// ── Use direct access when you need the reference itself ──────────────────────

// Attach() takes a GameObject, so you still need to resolve it manually
var gripGO = robot.GetComponent<PDSimMetadata>()?.GetAnchor("GripPoint")?.gameObject ?? robot;
yield return PDSimAnimator.Sequence()
    .Move(crate).To(robot, "GripPoint").Duration(1f)
    .Attach(crate, gripGO)
    .Play();

// ── Attributes have no builder overload — read and parse yourself ─────────────

var meta   = obj.GetComponent<PDSimMetadata>();
string raw = meta?.GetAttribute("speed_factor") ?? "1.0";
float factor = float.TryParse(raw, out var f) ? f : 1f;

// ── NavMesh is automatic — no metadata or custom code needed ──────────────────

// If the moving object has VisualisationObject.useNavMeshAgent = true,
// the builder switches to NavMesh automatically. Same syntax either way:
.Move(robot).To(waypoint).Duration(5f)           // lerp if no NavMeshAgent
.Move(navRobot).To(waypoint).Duration(5f)        // NavMesh if useNavMeshAgent = true
```

---

## Common Mistakes

| Mistake | Symptom | Fix |
|---------|---------|-----|
| Not calling `onComplete()` | Simulation freezes at that fluent | Always call it, even in early-return paths |
| Calling `onComplete()` before animation ends | Next animation starts immediately, overlapping | Call it only after all `yield return` statements |
| Wrong namespace | `No IFluentVisualizer found` warning, animation skipped | Must be `namespace GeneratedVisualizers` |
| Using `Time.deltaTime` without speed factor | Animation ignores speed slider and pause | Use `Controller.Instance.animationSpeed` and check `IsPaused` |
| `objects[N]` without bounds check | `IndexOutOfRangeException` crash | Guard with `if (objects.Length > N)` |
| Modifying GameObject after it's returned to pool | Corrupts next use from pool | Do all cleanup before `onComplete()`, never after |
| `duration` is 0 for init fluents | Tween completes instantly (fine) | Handle gracefully; `PDSimAnimator` already skips tween if `duration <= 0` |
| Passing `.To(target, "tag")` to `Rotate` or `Scale` | Tag silently ignored — those actions only accept the source tag on the mover | Destination anchor tags only work with `Move`; for rotate/scale targets always pass a `Vector3` or `Quaternion` |
| Calling `.Attach(obj, gripGO)` with a manually fetched `GameObject` but using a stale reference | Object parented to a destroyed or pooled instance | Always resolve the anchor `GameObject` immediately before `Attach`, not at the top of the coroutine |
| Expecting NavMesh to follow `Duration()` like lerp | NavMesh duration sets agent speed, not a hard time limit — arrival time varies with path length | Use `Duration` as a speed hint; let the agent complete naturally, or use a fixed speed via `MovementSettings` |
