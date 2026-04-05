<div align="center">
<!--
<div align="center">
-->

# Custom URP Post-Processing Effects
<!--
# Custom URP Post-Processing Effects
-->

**A collection of highly optimized custom post-processing effects (FXAA and PBR Neutral Tonemapping) for Unity's Universal Render Pipeline (URP).**
<!--
**A collection of highly optimized custom post-processing effects (FXAA and PBR Neutral Tonemapping) for Unity's Universal Render Pipeline (URP).**
-->

---
<!--
---
-->

## Overview
<!--
## Overview
-->

This repository provides custom; efficient post-processing solutions for Unity URP. It includes a custom implementation of Fast Approximate Anti-Aliasing (FXAA) designed to run correctly after tonemapping; and a Khronos PBR Neutral Tonemapping feature to preserve color fidelity and integrity under intense lighting conditions.
<!--
This repository provides custom; efficient post-processing solutions for Unity URP. It includes a custom implementation of Fast Approximate Anti-Aliasing (FXAA) designed to run correctly after tonemapping; and a Khronos PBR Neutral Tonemapping feature to preserve color fidelity and integrity under intense lighting conditions.
-->

## Features
<!--
## Features
-->

**Post-Processing Effects**
<!--
**Post-Processing Effects**
-->
*   **Custom FXAA:** Fast Approximate Anti-Aliasing implemented as a custom render feature; correctly running after tonemapping in the 0-1 LDR color space for optimal edge smoothing.
<!--
*   **Custom FXAA:** Fast Approximate Anti-Aliasing implemented as a custom render feature; correctly running after tonemapping in the 0-1 LDR color space for optimal edge smoothing.
-->
*   **PBR Neutral Tonemap:** Built-in tone mapping to preserve color integrity under intense lighting; utilizing the Khronos PBR Neutral Tonemapping Math.
<!--
*   **PBR Neutral Tonemap:** Built-in tone mapping to preserve color integrity under intense lighting; utilizing the Khronos PBR Neutral Tonemapping Math.
-->

**URP Integration**
<!--
**URP Integration**
-->
*   Fully compatible with Unity's Universal Render Pipeline.
<!--
*   Fully compatible with Unity's Universal Render Pipeline.
-->
*   Both effects are implemented as `ScriptableRendererFeature` and integrate seamlessly with the URP post-processing stack and Volume framework.
<!--
*   Both effects are implemented as `ScriptableRendererFeature` and integrate seamlessly with the URP post-processing stack and Volume framework.
-->

## Installation
<!--
## Installation
-->

1. Drop the `CustomFXAA` and `CustomPBRNeutralTonemap` folders into your Unity project's `Assets` folder.
<!--
1. Drop the `CustomFXAA` and `CustomPBRNeutralTonemap` folders into your Unity project's `Assets` folder.
-->
2. Add the `CustomFXAARendererFeature` and `PBRNeutralTonemapFeature` to your URP Forward Renderer Data asset.
<!--
2. Add the `CustomFXAARendererFeature` and `PBRNeutralTonemapFeature` to your URP Forward Renderer Data asset.
-->
3. Assign the respective shaders (`Hidden/CustomFXAA` and `Hidden/Custom/PBRNeutralTonemap`) to the renderer features.
<!--
3. Assign the respective shaders (`Hidden/CustomFXAA` and `Hidden/Custom/PBRNeutralTonemap`) to the renderer features.
-->
4. Add the corresponding Volume Components to your Post-Processing Volumes to enable and control the effects.
<!--
4. Add the corresponding Volume Components to your Post-Processing Volumes to enable and control the effects.
-->

</div>
<!--
</div>
-->