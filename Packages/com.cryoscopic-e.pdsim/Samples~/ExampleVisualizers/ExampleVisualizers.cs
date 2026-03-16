// ============================================================
//  ExampleVisualizers.cs — PDSim Animation Examples
// ============================================================
//
//  This file contains ready-to-use visualizers for common PDDL
//  fluent patterns. Copy the class that matches your domain,
//  rename it, and register it via the FluentAnimation Inspector.
//
//  PDDL patterns covered:
//    1.  BooleanToggle     — show/hide with scale pop
//    2.  SymbolColor       — recolor by symbol value
//    3.  IntegerCounter    — display numeric value with text + scale
//    4.  RealGauge         — animate a progress bar by real value
//    5.  MoveToObject      — translate one object to another (stacking)
//    6.  ArcMove           — parabolic lift-then-place (pick & place)
//    7.  HoldAttach        — pick up an object and parent to grip anchor
//    8.  DriveToLocation   — vehicle movement with settle + label update
//    9.  DoorState         — symbol-driven rotation + indicator color
//   10.  MultiFluentParallel — parallel rotate + color simultaneously
//
//  All visualizers:
//    • Live in namespace GeneratedVisualizers (required)
//    • Implement IFluentVisualizer
//    • Respect pause state and animationSpeed slider
//    • ALWAYS call onComplete() exactly once
//
// ============================================================

using System;
using System.Collections;
using System.Collections.Generic;
using GeTModel;
using PDSim.Components;
using PDSim.Utils.Animation;
using UnityEngine;

namespace GeneratedVisualizers
{
    // ----------------------------------------------------------
    // 1. BooleanToggle
    //
    //  PDDL:  (active ?obj) := boolean
    //  Effect: Pops the object in when true, shrinks it out when false.
    //  Usage:  Suitable for any on/off state (power, existence, clear).
    // ----------------------------------------------------------
    public class BooleanToggle_ObjectVisualizer : MonoBehaviour, IFluentVisualizer
    {
        public void Animate(List<string> args, GeTAtom value, GameObject[] objects,
                            float duration, Action onComplete)
        {
            if (objects.Length == 0) { onComplete?.Invoke(); return; }
            bool isActive = value.BooleanValue ?? false;
            StartCoroutine(Run(objects[0], isActive, duration, onComplete));
        }

        private IEnumerator Run(GameObject obj, bool isActive, float dur, Action onComplete)
        {
            float tweenTime = Mathf.Max(0.35f, dur * 0.4f);

            if (isActive)
            {
                obj.SetActive(true);
                yield return PDSimAnimator.Sequence()
                    .Scale(obj).To(Vector3.zero).Duration(0f)   // start invisible
                    .Scale(obj).To(Vector3.one).Duration(tweenTime).WithEasing(EasingType.OutBack)
                    .Play();
            }
            else
            {
                yield return PDSimAnimator.Sequence()
                    .Scale(obj).To(Vector3.zero).Duration(tweenTime).WithEasing(EasingType.InQuad)
                    .Play();
                obj.SetActive(false);
                obj.transform.localScale = Vector3.one; // reset for next time
            }

            onComplete?.Invoke();
        }
    }

    // ----------------------------------------------------------
    // 2. SymbolColor
    //
    //  PDDL:  (color ?block) := symbol   (values: red/green/blue/yellow/white)
    //  Effect: Cross-fades the object's material color to match the symbol.
    //  Note:   Extend the Palette dictionary with your domain's symbols.
    // ----------------------------------------------------------
    public class SymbolColor_BlockVisualizer : MonoBehaviour, IFluentVisualizer
    {
        private static readonly Dictionary<string, Color> Palette =
            new Dictionary<string, Color>(StringComparer.OrdinalIgnoreCase)
            {
                { "red",    new Color(0.90f, 0.18f, 0.18f) },
                { "green",  new Color(0.18f, 0.80f, 0.30f) },
                { "blue",   new Color(0.18f, 0.40f, 0.90f) },
                { "yellow", new Color(1.00f, 0.85f, 0.10f) },
                { "orange", new Color(1.00f, 0.55f, 0.10f) },
                { "purple", new Color(0.60f, 0.18f, 0.80f) },
                { "white",  Color.white },
                { "black",  Color.black },
            };

        public void Animate(List<string> args, GeTAtom value, GameObject[] objects,
                            float duration, Action onComplete)
        {
            if (objects.Length == 0) { onComplete?.Invoke(); return; }
            string symbol = value.Symbol ?? "white";
            Color target  = Palette.TryGetValue(symbol, out var c) ? c : Color.white;
            StartCoroutine(Run(objects[0], target, duration, onComplete));
        }

        private IEnumerator Run(GameObject obj, Color target, float dur, Action onComplete)
        {
            float tweenTime = Mathf.Max(0.4f, dur * 0.5f);

            yield return PDSimAnimator.Sequence()
                .Color(obj, target).Duration(tweenTime).WithEasing(EasingType.SmoothStep)
                .Play();

            onComplete?.Invoke();
        }
    }

    // ----------------------------------------------------------
    // 3. IntegerCounter
    //
    //  PDDL:  (count ?loc) := integer
    //  Effect: Scales a child "Counter" bar and updates a TMP label.
    //  Requires: PDSimMetadata with a UI entry "CountLabel" (TMP_Text).
    // ----------------------------------------------------------
    public class IntegerCounter_LocationVisualizer : MonoBehaviour, IFluentVisualizer
    {
        public void Animate(List<string> args, GeTAtom value, GameObject[] objects,
                            float duration, Action onComplete)
        {
            if (objects.Length == 0) { onComplete?.Invoke(); return; }
            long count = value.IntValue ?? 0;
            StartCoroutine(Run(objects[0], (int)count, duration, onComplete));
        }

        private IEnumerator Run(GameObject loc, int count, float dur, Action onComplete)
        {
            var bar = loc.transform.Find("Counter")?.gameObject;
            float barHeight = Mathf.Max(0.05f, count * 0.3f);
            float tweenTime = Mathf.Max(0.3f, dur * 0.6f);

            // Scale bar and update label simultaneously
            yield return PDSimAnimator.Parallel()
                .Scale(bar ?? loc).To(new Vector3(1f, barHeight, 1f))
                                  .Duration(tweenTime)
                                  .WithEasing(EasingType.OutCubic)
                .Text(loc, "CountLabel", count.ToString())
                .Play();

            // Brief attention-grab bounce on the bar
            if (bar != null)
            {
                yield return PDSimAnimator.Sequence()
                    .Scale(bar).To(new Vector3(1.1f, barHeight * 1.05f, 1.1f)).Duration(0.08f)
                    .Scale(bar).To(new Vector3(1f, barHeight, 1f)).Duration(0.12f).WithEasing(EasingType.OutBack)
                    .Play();
            }

            onComplete?.Invoke();
        }
    }

    // ----------------------------------------------------------
    // 4. RealGauge
    //
    //  PDDL:  (fuel ?vehicle) := real   (expected range 0.0 – 1.0)
    //  Effect: Scales a "FuelBar" child along X and cross-fades from
    //          red (empty) to green (full).  Shows "LowFuel" warning below 20%.
    // ----------------------------------------------------------
    public class RealGauge_VehicleVisualizer : MonoBehaviour, IFluentVisualizer
    {
        public void Animate(List<string> args, GeTAtom value, GameObject[] objects,
                            float duration, Action onComplete)
        {
            if (objects.Length == 0) { onComplete?.Invoke(); return; }
            float level = value.RealValue != null ? Mathf.Clamp01((float)value.RealValue.ToDouble()) : 0f;
            StartCoroutine(Run(objects[0], level, duration, onComplete));
        }

        private IEnumerator Run(GameObject vehicle, float level, float dur, Action onComplete)
        {
            var bar     = vehicle.transform.Find("FuelBar")?.gameObject;
            var warning = vehicle.transform.Find("LowFuelWarning")?.gameObject;

            Color gaugeColor = Color.Lerp(Color.red, Color.green, level);
            float tweenTime  = Mathf.Max(0.5f, dur * 0.7f);

            if (bar != null)
            {
                yield return PDSimAnimator.Parallel()
                    .Scale(bar).To(new Vector3(Mathf.Max(level, 0.02f), 1f, 1f))
                               .Duration(tweenTime)
                               .WithEasing(EasingType.InOutQuad)
                    .Color(bar, gaugeColor).Duration(tweenTime)
                    .Play();
            }

            if (warning != null)
                warning.SetActive(level < 0.2f);

            onComplete?.Invoke();
        }
    }

    // ----------------------------------------------------------
    // 5. MoveToObject
    //
    //  PDDL:  (on ?x ?y) := boolean   (x placed on y)
    //  Effect: Moves x to y's "Top" anchor (or y's position).
    //          Attaches x as a child of y so they move together later.
    //  Requires: PDSimMetadata on y with anchor "Top".
    // ----------------------------------------------------------
    public class MoveToObject_BlockBlockVisualizer : MonoBehaviour, IFluentVisualizer
    {
        public void Animate(List<string> args, GeTAtom value, GameObject[] objects,
                            float duration, Action onComplete)
        {
            if (objects.Length < 2) { onComplete?.Invoke(); return; }
            bool isOn = value.BooleanValue ?? false;
            if (!isOn)           { onComplete?.Invoke(); return; } // false → nothing to show
            StartCoroutine(Run(objects[0], objects[1], duration, onComplete));
        }

        private IEnumerator Run(GameObject x, GameObject y, float dur, Action onComplete)
        {
            // Resolve snap target
            var meta = y.GetComponent<PDSimMetadata>();
            Transform snap = meta?.GetAnchor("Top");
            Vector3 dest   = snap != null ? snap.position : y.transform.position;

            float tweenTime = Mathf.Max(0.5f, dur);

            yield return PDSimAnimator.Sequence()
                .Move(x).To(dest).Duration(tweenTime).WithEasing(EasingType.InOutQuad)
                .Attach(x, y)
                .Play();

            onComplete?.Invoke();
        }
    }

    // ----------------------------------------------------------
    // 6. ArcMove (Pick & Place)
    //
    //  PDDL:  (at ?object ?location) := boolean
    //  Effect: Lifts the object in a parabolic arc to the destination.
    //          Three-phase move: rise → traverse → descend.
    // ----------------------------------------------------------
    public class ArcMove_ObjectLocationVisualizer : MonoBehaviour, IFluentVisualizer
    {
        [Tooltip("How high above both endpoints the arc peaks (world units).")]
        public float arcHeight = 2f;

        public void Animate(List<string> args, GeTAtom value, GameObject[] objects,
                            float duration, Action onComplete)
        {
            if (objects.Length < 2) { onComplete?.Invoke(); return; }
            bool arrived = value.BooleanValue ?? false;
            if (!arrived)         { onComplete?.Invoke(); return; }
            StartCoroutine(Run(objects[0], objects[1], duration, onComplete));
        }

        private IEnumerator Run(GameObject obj, GameObject dest, float dur, Action onComplete)
        {
            Vector3 start = obj.transform.position;
            Vector3 end   = dest.transform.position;
            float peak    = Mathf.Max(start.y, end.y) + arcHeight;

            Vector3 midAir = new Vector3(
                (start.x + end.x) * 0.5f,
                peak,
                (start.z + end.z) * 0.5f
            );

            float phase = Mathf.Max(0.15f, dur / 3f);

            yield return PDSimAnimator.Sequence()
                .Move(obj).To(new Vector3(start.x, peak, start.z))
                          .Duration(phase).WithEasing(EasingType.OutQuad)
                .Move(obj).To(new Vector3(end.x, peak, end.z))
                          .Duration(phase)
                .Move(obj).To(end)
                          .Duration(phase).WithEasing(EasingType.InQuad)
                .Play();

            onComplete?.Invoke();
        }
    }

    // ----------------------------------------------------------
    // 7. HoldAttach (Gripper / Carrying)
    //
    //  PDDL:  (holding ?robot ?object) := boolean
    //  Effect: TRUE  → rotate robot arm down, move object to grip anchor, parent it.
    //          FALSE → detach object, rotate arm back up.
    //  Requires: PDSimMetadata on robot with anchor "Grip" and anchor "Arm".
    // ----------------------------------------------------------
    public class HoldAttach_RobotObjectVisualizer : MonoBehaviour, IFluentVisualizer
    {
        public void Animate(List<string> args, GeTAtom value, GameObject[] objects,
                            float duration, Action onComplete)
        {
            if (objects.Length < 2) { onComplete?.Invoke(); return; }
            bool isHolding = value.BooleanValue ?? false;
            StartCoroutine(Run(objects[0], objects[1], isHolding, duration, onComplete));
        }

        private IEnumerator Run(GameObject robot, GameObject obj, bool isHolding,
                                float dur, Action onComplete)
        {
            var meta   = robot.GetComponent<PDSimMetadata>();
            Transform grip = meta?.GetAnchor("Grip");
            Vector3 gripPos = grip != null ? grip.position : robot.transform.position;

            float tweenTime = Mathf.Max(0.4f, dur * 0.6f);

            if (isHolding)
            {
                yield return PDSimAnimator.Parallel()
                    .Rotate(robot, "Arm").To(new Vector3(-45f, 0f, 0f)).Duration(tweenTime)
                    .Move(obj).To(gripPos).Duration(tweenTime).WithEasing(EasingType.InOutQuad)
                    .Play();

                if (grip != null)
                {
                    yield return PDSimAnimator.Sequence()
                        .Attach(obj, grip.gameObject)
                        .Play();
                }
            }
            else
            {
                yield return PDSimAnimator.Sequence()
                    .Detach(obj)
                    .Rotate(robot, "Arm").To(new Vector3(0f, 0f, 0f))
                                        .Duration(tweenTime * 0.6f)
                                        .WithEasing(EasingType.OutBack)
                    .Play();
            }

            onComplete?.Invoke();
        }
    }

    // ----------------------------------------------------------
    // 8. DriveToLocation (Vehicle + Label)
    //
    //  PDDL:  (at ?truck ?location) := boolean
    //  Effect: Moves the truck to the location, settle-bounce on arrival,
    //          then updates the destination label.
    //  Requires: PDSimMetadata on truck with UI entry "Destination" (TMP_Text).
    // ----------------------------------------------------------
    public class DriveToLocation_TruckLocationVisualizer : MonoBehaviour, IFluentVisualizer
    {
        public void Animate(List<string> args, GeTAtom value, GameObject[] objects,
                            float duration, Action onComplete)
        {
            if (objects.Length < 2) { onComplete?.Invoke(); return; }
            bool arrived = value.BooleanValue ?? false;
            if (!arrived)          { onComplete?.Invoke(); return; }
            StartCoroutine(Run(objects[0], objects[1], duration, onComplete));
        }

        private IEnumerator Run(GameObject truck, GameObject location,
                                float dur, Action onComplete)
        {
            string label    = location.name;
            float driveTime = Mathf.Max(0.5f, dur * 0.8f);

            yield return PDSimAnimator.Sequence()
                // Drive
                .Move(truck).To(location).Duration(driveTime).WithEasing(EasingType.InOutQuad)
                // Arrive settle
                .Wait(0.1f)
                .Scale(truck).To(new Vector3(1.06f, 0.94f, 1.06f)).Duration(0.08f)
                .Scale(truck).To(Vector3.one).Duration(0.18f).WithEasing(EasingType.OutBack)
                // Update label
                .Text(truck, "Destination", label)
                .Play();

            onComplete?.Invoke();
        }
    }

    // ----------------------------------------------------------
    // 9. DoorState (Symbol-driven Rotation + Color Indicator)
    //
    //  PDDL:  (door-state ?room) := symbol   (open / closed / locked)
    //  Effect: Rotates a "Door" child and recolors a "DoorIndicator" child.
    //  Requires: Children named "Door" and "DoorIndicator" on the room object.
    // ----------------------------------------------------------
    public class DoorState_RoomVisualizer : MonoBehaviour, IFluentVisualizer
    {
        public void Animate(List<string> args, GeTAtom value, GameObject[] objects,
                            float duration, Action onComplete)
        {
            if (objects.Length == 0) { onComplete?.Invoke(); return; }
            string state = value.Symbol ?? "closed";
            StartCoroutine(Run(objects[0], state, duration, onComplete));
        }

        private IEnumerator Run(GameObject room, string state, float dur, Action onComplete)
        {
            var door      = room.transform.Find("Door")?.gameObject;
            var indicator = room.transform.Find("DoorIndicator")?.gameObject;

            Quaternion targetRot = state switch
            {
                "open"   => Quaternion.Euler(0f, -90f, 0f),
                "locked" => Quaternion.Euler(0f,   0f, 0f),
                _        => Quaternion.Euler(0f,   0f, 0f), // closed
            };

            Color indicatorColor = state switch
            {
                "open"   => new Color(0.2f, 0.85f, 0.3f),
                "locked" => new Color(0.9f, 0.18f, 0.18f),
                _        => new Color(0.5f, 0.5f,  0.5f),
            };

            float tweenTime = Mathf.Max(0.4f, dur * 0.6f);

            yield return PDSimAnimator.Parallel()
                .Rotate(door ?? room).To(targetRot).Duration(tweenTime).WithEasing(EasingType.InOutCubic)
                .Color(indicator ?? room, indicatorColor).Duration(tweenTime * 0.5f)
                .Play();

            onComplete?.Invoke();
        }
    }

    // ----------------------------------------------------------
    // 10. MultiFluentParallel (Spin + Flash)
    //
    //  PDDL:  (processing ?machine) := boolean
    //  Effect: While true, spin the machine and flash it blue.
    //          This shows how to combine multiple simultaneous tweens.
    // ----------------------------------------------------------
    public class MultiFluentParallel_MachineVisualizer : MonoBehaviour, IFluentVisualizer
    {
        public void Animate(List<string> args, GeTAtom value, GameObject[] objects,
                            float duration, Action onComplete)
        {
            if (objects.Length == 0) { onComplete?.Invoke(); return; }
            bool isProcessing = value.BooleanValue ?? false;
            StartCoroutine(Run(objects[0], isProcessing, duration, onComplete));
        }

        private IEnumerator Run(GameObject machine, bool isProcessing, float dur, Action onComplete)
        {
            float tweenTime = Mathf.Max(0.6f, dur);

            if (isProcessing)
            {
                yield return PDSimAnimator.Parallel()
                    .Rotate(machine).By(new Vector3(0f, 360f, 0f)).Duration(tweenTime)
                    .Color(machine, new Color(0.3f, 0.6f, 1.0f)).Duration(tweenTime * 0.3f)
                    .Play();

                // Flash back to neutral
                yield return PDSimAnimator.Sequence()
                    .Color(machine, Color.white).Duration(0.25f)
                    .Play();
            }
            else
            {
                // Wind down: slow-stop rotation + gray out
                yield return PDSimAnimator.Parallel()
                    .Rotate(machine).By(new Vector3(0f, 90f, 0f))
                                    .Duration(tweenTime * 0.4f)
                                    .WithEasing(EasingType.InQuad)
                    .Color(machine, Color.grey).Duration(tweenTime * 0.3f)
                    .Play();
            }

            onComplete?.Invoke();
        }
    }
}
