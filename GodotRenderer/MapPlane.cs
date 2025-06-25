using Godot;
using mtw3dviewer.DataTypes;
using mtw3dviewer.FileFormats;
using mtw3dviewer.GodotRenderer;
using System;
using System.Collections.Generic;
using System.IO;

public partial class MapPlane : MeshInstance3D
{
    public Jjm Map { get; private set; }
    private GridPlane[,] _planes;
    private Dictionary<int, Texture2D> _textures = new();
    public override void _Ready()
    {
        base._Ready();
    }

    public void LoadMap(string fileName)
    {
        this.Mesh = new ArrayMesh();
        Jjm map = new Jjm();
        map.LoadMap(fileName);
        Map = map;
        _planes = new GridPlane[map.MainNodes.GetLength(0), map.MainNodes.GetLength(1)];
        foreach (var c in this.GetChildren())
        {
            c.QueueFree();
        }
        foreach (var t in map.DistinctTextures)
        {
            string path = t.Path(fileName, TerrainType.LUSH);
            var img = ImageTexture.CreateFromImage(Image.LoadFromFile(path));
            if (img == null)
                throw new FileLoadException($"Texture load failed {path}");

            _textures.TryAdd(t.GetHashCode(), img);
        }
        for (int y = map.MainNodes.GetLength(1) - 1; y > 0; y--)
        {
            for (int x = 0; x < map.MainNodes.GetLength(0) - 2; x++)
            {
                var plane = new GridPlane();
                plane.Create(map.Nodes[0, x, y], map, _textures[map.Textures[x, y - 1].GetHashCode()]);
                this.AddChild(plane);
                _planes[x, y] = plane;
            }
        }
        //AddSurface(fileName, map, this.Mesh as ArrayMesh);
    }

    /*private void AddSurface(string fileName, Jjm map, ArrayMesh mesh)
    {
        Godot.Collections.Array surfaceArray = [];
        surfaceArray.Resize((int)Mesh.ArrayType.Max);
        var verts = new List<Vector3>();
        var indices = new List<int>();
        var index = 0;
        for (int y = 0; y < map.Nodes.GetLength(1); y++)
        {
            for (int x = 0; x < map.Nodes.GetLength(0); x++)
            {
                var node = map.Nodes[x, y];
                verts.Add(new Vector3(node.X, node.Height * -1, node.Y));

                if (x < map.Nodes.GetLength(0) - 1 && y < map.Nodes.GetLength(1) - 1)
                {
                    indices.Add(index); // kolmio1
                    indices.Add(index + 1);
                    indices.Add(index + map.Nodes.GetLength(0));

                    indices.Add(index + 1); // kolmio2
                    indices.Add(index + map.Nodes.GetLength(0) + 1);
                    indices.Add(index + map.Nodes.GetLength(0));
                }
                index++;
            }
        }
        DebugViewer.WriteInfo("Tris: ", $"{indices.Count / 3}");
        DebugViewer.WriteInfo("Quads: ", $"{indices.Count / 6}");
        DebugViewer.WriteInfo("Verts: ", $"{verts.Count}");
        DebugViewer.WriteInfo("Indices: ", $"{indices.Count}");
        surfaceArray[(int)Mesh.ArrayType.Vertex] = verts.ToArray();
        //surfaceArray[(int)Mesh.ArrayType.TexUV] = uvs.ToArray();
        //surfaceArray[(int)Mesh.ArrayType.Normal] = normals.Select(v => new Vector3(v.X, v.Y, v.Z)).ToArray();
        surfaceArray[(int)Mesh.ArrayType.Index] = indices.ToArray();

        mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, surfaceArray);
    }*/
}
