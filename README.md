# VR DataViz 

An immersive VR/AR data visualization tool built with Unity and OpenXR for Meta Quest.

<!-- ![VR DataViz Demo](demo.gif) -->

## Features

- **Load CSV Data**: Import any CSV file and visualize it in 3D space
- **Axis Mapping**: Dynamically map columns to X, Y, Z axes
- **Color Coding**: Color points by numeric gradient or categorical values
- **Interactive Filtering**: Filter data points by min/max values
- **VR Drawing**: Annotate data with 3D drawings
- **Grab & Move**: Reposition visualization in space
- **Hover Tooltips**: Point at data to see all values
- **Save/Load Views**: Persist your visualization state
- **AR Mode**: Toggle passthrough for augmented reality view

## Tech Stack

- Unity 6 (6000.x)
- OpenXR
- XR Interaction Toolkit
- Meta Quest 3/3S/Pro

## Demo Dataset

Includes the classic Iris dataset showing 3 flower species clustered by petal/sepal measurements.

## Controls

| Action | Input |
|--------|-------|
| Point/Select | Ray from controller |
| Grab Visualization | G key / Grip button |
| Draw Annotation | F key / Trigger button |
| Move | WASD |
| Look | Mouse |

## Installation

1. Clone this repository
2. Open in Unity 6
3. Open `Assets/Scenes/DataVizMainScene`
4. Press Play or build to Quest

## Project Structure
```
Assets/
├── Scripts/
│   ├── Core/           # Data management, CSV parsing
│   ├── Visualization/  # Point cloud, coordinate system
│   ├── Interaction/    # VR pointer, grabbing, drawing
│   └── UI/             # Axis mapping, filter panels
├── StreamingAssets/    # CSV data files
└── Scenes/             # Main scene
```

## Skills Demonstrated

- XR Development (OpenXR, Meta Quest)
- Data Visualization
- Unity C# Programming
- Custom VR Interaction Systems
- UI/UX for VR

## Author

**Anjali Kan**
- GitHub: [@Anjali-Kan](https://github.com/Anjali-Kan)

## License

MIT License