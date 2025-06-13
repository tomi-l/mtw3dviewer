# mtw3dviewer
Viewer for medieval total war 3d files using [Godot](https://godotengine.org/) 

Use Mouse2 to rotate camera and WASD to move

Model files are located in Steam\steamapps\common\Total War Medieval 1 Gold\Models\Western European directory

![image](https://github.com/user-attachments/assets/ceaba967-aca1-4ecc-8dd4-b31ac1d4a583)


# 3xx File format
![image](https://github.com/user-attachments/assets/e7c4dc20-733d-46cc-bc37-51e0c9f0dea1)

.3xx file starts with count of incoming vertices.

![image](https://github.com/user-attachments/assets/9af1ea61-e449-475c-8cc1-d3255fb434bc)

Each vertex is a struct of 3 floats
```
struct Vertex {
    float x;
    float y;
    float z;
};
```
After vertices, next information is count of normals

![image](https://github.com/user-attachments/assets/84ac43f3-0677-4684-8e1a-7b10cc20a241)

Each normal is a struct of 3 floats
```
struct Normal {
    float x;
    float y;
    float z;
};
```
After normals, next information is count of triangles

![image](https://github.com/user-attachments/assets/02c68f68-2f7a-499b-af91-be75d9cdfef1)

Each triangle consists of TextureID of the triangle and 3 blocks of UV data for each vertex of triangle
```
struct Uv {
    u32 vertexIndex;
    u32 normalIndex;
    float uvx;
    float uvy;
};
struct Triangle {
    s32 textureIndex;
    Uv uvs[3];
};
```
VertexIndex and NormalIndex refer to the earlier blocks respectively

# Textures
Texture filenames are defined in .txx file. Actual texture image files are located in Models directory. These are mostly stored in .LBM format. 
I use https://github.com/HexaEngine/Hexa.NET.SDL.Image to extract pixel data and convert it into Godot image.
