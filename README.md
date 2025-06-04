# InMirror

日本語版のREADMEは[README.ja.md](README.ja.md)をご覧ください。

This repository contains a simple AR/VR portal demonstration created with Unity. The demo shows how to switch between augmented reality and virtual reality worlds through a portal effect.

## Opening the Project

1. Install **Unity 6000.0.36f1** or later.
2. Clone this repository and launch Unity Hub.
3. In Unity Hub choose **Open**, browse to the cloned directory and select it. Unity will import the required packages on first load.

## Required Packages

The project uses several packages defined in [`Packages/manifest.json`](Packages/manifest.json). Important ones include:

- `com.meta.xr.sdk.all` (Meta XR Integration)
- `com.unity.xr.management`
- `com.unity.xr.oculus`
- `com.unity.inputsystem`
- `com.unity.visualscripting`

Make sure these packages are installed when Unity prompts you about missing dependencies.

## Build and Run

1. Open **Build Settings** in Unity (File > Build Settings...).
2. Add the desired scene (e.g. `Main.unity`) to the build list.
3. Choose your target platform (for Oculus devices select **Android**).
4. Click **Build** or **Build and Run**.

The sample has been tested on Oculus devices but can be run in the Unity editor as well.

## Main Scenes and Scripts

The project contains three scenes located in `Assets/Scenes`:

- `Main.unity`
- `BuildTest.unity`
- `MRTK_scene.unity`

Core logic for the portal behaviour is implemented in two scripts located in `Assets/Script`:

- `PortalManager.cs` – manages entering/exiting the portal and switching between AR and VR rendering.
- `ClippingPlane_Origin.cs` – handles the clipping plane used to hide or reveal objects when passing through the portal.

Feel free to explore and modify these scripts to adapt the portal behaviour to your needs.


