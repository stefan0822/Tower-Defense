# Unity Low-Poly Terrain Tile Kit — 96 individual 3D tiles

This kit contains actual editable/importable geometry (OBJ meshes with UVs) and individual PNG textures, NOT just concept-art pictures. Terrain style is inspired by supplied images, but generated meshes are original approximations rather than 1:1 reconstructed copies.

## Contents
- 96 individual models, each 10 x 10 Unity world units (X and Z), Y-up, 0 at the outer edges for easier placement together.
- 96 individual 256 x 256 diffuse PNG textures, UV mapped to each model.
- 96 accompanying .MTL material-definition files.
- Unity Editor C# utility to create materials, prefabs, MeshColliders and demo scene.
- 8 terrain categories x 12 variants each: Grass_Flat; Grass_Hills; Grass_RockyGround; Grass_RockyMountains; Desert_Flat; Desert_Mountains; Snow_Flat; Snow_Mountains.

## Unity installation
1. Unzip the package.
2. Drag the **Assets/TerrainTileKit** folder into the **Assets** folder of your Unity project (or copy its contents into <your-project>/Assets/TerrainTileKit).
3. Wait for the asset import to finish.
4. Click Unity menu **Tools > Terrain Tile Kit > 1. Generate Materials and Prefabs**.
5. Click **Tools > Terrain Tile Kit > 2. Create Demo Scene**.
6. Open **Assets/TerrainTileKit/Demo/TerrainTileKit_Demo.unity**. You can drag any prefab from **Prefabs** into your scene.

These steps work with Unity's built-in renderer and URP (Shader selection tries URP Lit and falls back to Standard). HDRP projects may need changing the prefab material shader to HDRP/Lit if the editor detects a different shader.

## Notes
- Each mesh is a separately usable OBJ file. No Blender required.
- If tiles are placed edge-to-edge use a spacing of exactly **10 units**. The demo scene uses 11.5 units to display a gap between assets.
- For performance, disable unnecessary MeshColliders on distant terrain; or use simpler collision meshes when appropriate.
- The PNG files are diffuse/color textures. They are not heightmaps, normal maps, or PBR material sets. Height is baked into the OBJ geometry.
- You may adapt model scale, collider settings and materials in Unity.
