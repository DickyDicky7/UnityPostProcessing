# Custom SMAA Port for Unity URP
<!--
# Custom SMAA Port for Unity URP
-->

This folder contains a 1:1 faithful port of SMAA to Unity 2022 URP v14 Post Processing Volume.
<!--
This folder contains a 1:1 faithful port of SMAA to Unity 2022 URP v14 Post Processing Volume.
-->

## Instructions for use in your Unity project
<!--
## Instructions for use in your Unity project
-->

1. **Move the Folder:** Move this `CustomSMAA` folder into your Unity project's `Assets` folder.
<!--
1. **Move the Folder:** Move this `CustomSMAA` folder into your Unity project's `Assets` folder.
-->
2. **Get SMAA.hlsl:** Download or clone the original SMAA repository (https://github.com/iryoku/smaa) and copy `SMAA.hlsl` into this `CustomSMAA` folder, replacing the placeholder file.
<!--
2. **Get SMAA.hlsl:** Download or clone the original SMAA repository (https://github.com/iryoku/smaa) and copy `SMAA.hlsl` into this `CustomSMAA` folder, replacing the placeholder file.
-->
3. **Texture Import Settings:** Ensure `AreaTexDX10.tga` and `SearchTex.tga` are imported into Unity with the following settings:
<!--
3. **Texture Import Settings:** Ensure `AreaTexDX10.tga` and `SearchTex.tga` are imported into Unity with the following settings:
-->
   - **sRGB (Color Texture)** is **unchecked**.
<!--
   - **sRGB (Color Texture)** is **unchecked**.
-->
   - **Generate Mip Maps** is **unchecked**.
<!--
   - **Generate Mip Maps** is **unchecked**.
-->
   - **Filter Mode** is set to **Bilinear** (or as required by SMAA).
<!--
   - **Filter Mode** is set to **Bilinear** (or as required by SMAA).
-->
4. **Renderer Feature Setup:**
<!--
4. **Renderer Feature Setup:**
-->
   - Locate your URP Renderer Data asset (usually in your Settings folder).
<!--
   - Locate your URP Renderer Data asset (usually in your Settings folder).
-->
   - Add the `CustomSMAARendererFeature` to the Renderer Features list.
<!--
   - Add the `CustomSMAARendererFeature` to the Renderer Features list.
-->
   - In the settings for this feature, assign the `CustomSMAA.shader`, `AreaTexDX10` texture, and `SearchTex` texture.
<!--
   - In the settings for this feature, assign the `CustomSMAA.shader`, `AreaTexDX10` texture, and `SearchTex` texture.
-->
5. **Volume Setup:**
<!--
5. **Volume Setup:**
-->
   - Add a Volume component to your scene (e.g., a Global Volume).
<!--
   - Add a Volume component to your scene (e.g., a Global Volume).
-->
   - Add the override: **Custom Post-processing/Custom SMAA**.
<!--
   - Add the override: **Custom Post-processing/Custom SMAA**.
-->
   - Check the **isEnabled** box to activate the effect.
<!--
   - Check the **isEnabled** box to activate the effect.
-->
