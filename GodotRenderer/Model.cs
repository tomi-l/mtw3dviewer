using Godot;
using mtw3dviewer.FileFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

public partial class Model : MeshInstance3D
{
    public override void _Ready()
    {
        base._Ready();
    }
    public void LoadModel(string fileName)
    {
        this.Mesh = new ArrayMesh();
        Model3xx twModel = new Model3xx();
        twModel.LoadModel(fileName);

        foreach (var tex in twModel.TextureIds)
        {
            AddSurface(fileName, twModel, this.Mesh as ArrayMesh, tex);
        }
        DebugViewer.Clear();
        DebugViewer.WriteInfo("File", Path.GetFileNameWithoutExtension(twModel.FileName));
        DebugViewer.WriteInfo("Verts", twModel.Vertices.Count().ToString());
        DebugViewer.WriteInfo("Tris", twModel.Triangles.Count().ToString());
        DebugViewer.WriteInfo("Textures", twModel.TextureIds.Count().ToString());
    }

    private void AddSurface(string fileName, Model3xx model, ArrayMesh mesh, int textureId)
    {
        Godot.Collections.Array surfaceArray = [];
        surfaceArray.Resize((int)Mesh.ArrayType.Max);

        // Data
        var verts = model.VerticesBy(x => x.TextureIndex == textureId);
        var normals = model.NormalsBy(x => x.TextureIndex == textureId);
        var indices = model.IndicesBy(x => x.TextureIndex == textureId);
        List<Vector2> uvs = [];
        surfaceArray[(int)Mesh.ArrayType.Vertex] = verts.Select(v => new Vector3(v.X, v.Y, v.Z)).ToArray();

        foreach (var triangle in model.Triangles.Where(x => x.TextureIndex == textureId))
        {
            uvs.AddRange(triangle.Uvs.Select(x => new Vector2(x.X, x.Y)));
        }
        surfaceArray[(int)Mesh.ArrayType.TexUV] = uvs.ToArray();
        surfaceArray[(int)Mesh.ArrayType.Normal] = normals.Select(v => new Vector3(v.X, v.Y, v.Z)).ToArray();
        surfaceArray[(int)Mesh.ArrayType.Index] = indices.ToArray();

        // Textures
        string texture = model.Textures[textureId];
        string path = Regex.Match(fileName, @".+?(?<=Models)").ToString();
        var imageName = Directory.GetFiles(path, $"{texture}.*", SearchOption.TopDirectoryOnly).FirstOrDefault();
        if (imageName == null)
            throw new FileLoadException($"Could not find image {texture} from {path}");

        // Materials
        var material = new StandardMaterial3D();
        if (imageName.ToLower().EndsWith(".lbm"))
        {
            material.AlbedoTexture = ImageTexture.CreateFromImage(LBM.LoadImage(imageName));
        }
        else
        {
            material.AlbedoTexture = GD.Load<Texture2D>(imageName);
        }
        mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, surfaceArray);
        mesh.SurfaceSetMaterial(textureId, material);
    }

}
