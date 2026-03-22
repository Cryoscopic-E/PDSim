# Interactive Sensing & Authoring System

PDSim now supports a **Physics-to-Logic** bridge, allowing Unity's physics engine to automatically update the planning state and enabling users to author domain logic directly within the Unity Inspector.

## 1. Getting Started

### 1.1 Automatic Setup
To set up an interactive environment:
1.  Go to the Unity menu: **PDSim > Create Interactive Planning Scene**.
2.  This will create a new scene with the necessary managers and a **Live Dashboard** HUD.

### 1.2 Making Objects Interactive
Instead of adding components manually, you can use the right-click menu in the Hierarchy on any GameObject:
- **PDSim > Make Interactive Object**: Adds `PDSimMetadata` and `VisualisationObject`.
- **PDSim > Add Semantic Sensor (Raycast)**: Adds a pre-configured `SemanticSensor`.
- **PDSim > Add Logical Action**: Adds a `LogicalAction` component for authoring behaviors.

## 2. Core Components

### 2.1 PDSimWorldObserver
The central hub for the interactive simulation. It maintains the "Live State" of the world and a registry of all interactive objects.
- **OnStateChanged**: An event fired whenever a sensor updates a fluent.
- **GenerateDomain/Problem**: Dynamically builds GeTPlan models from the scene components.

### 2.2 SemanticSensor
Attach this to any GameObject to sense other objects and update fluents.
- **Mode**: 
    - `Raycast`: Senses objects in front of the transform.
    - `Trigger`: Senses all valid objects within an attached Trigger Collider.
- **Mapping Expression**: Use the Natural DSL (e.g., `at[self, hit]`).
- **Target Filter**: Uses LayerMask to identify scannable objects.
- **Auto-Revert**: Automatically sets the predicate to `false` when the object leaves the sensor range.

### 2.3 LogicalAction
Define planning actions directly on GameObjects.
- **Action Name**: The name used in the planning domain.
- **Parameters**: Define the lifted parameters (Name and Type).
- **Preconditions**: DSL expressions that must be true for the action to execute.
- **Effects**: DSL expressions applied when the action is performed.

## 3. Natural Expression DSL (Unity)

The Unity components use a simplified string-based version of the GeTPlan DSL with special keywords:
- **`self`**: Refers to the name of the GameObject holding the component.
- **`hit`**: Refers to the name of the object detected by a sensor.

**Examples:**
- `at[self, hit]` (Sensor mapping)
- `holding[self] := hit` (Sensor effect)
- `!holding[self] & at[self, target]` (Action precondition)

## 4. Interactive Dashboard

The Dashboard UI (active in Play Mode) is auto-loaded by the `InteractiveDashboard` component. It provides:
- **Live State Feed**: A reactive list of all fluents currently being sensed in the Unity world.
- **Goal Input**: Type a goal expression (e.g., `on[BlockA, BlockB]`).
- **Solve & Execute**: Takes a snapshot of the live state, generates a problem, solves it via the Python backend, and plays the resulting animation in Unity.

> **Note**: Ensure you have a `UIDocument` and `PanelSettings` assigned to the `PDSim Managers` object for the UI to render correctly.

## 5. Requirements for Objects
For an object to be "seen" by sensors or used in actions, it **must** have one of the following components:
- `PDSimMetadata`
- `VisualisationObject`
