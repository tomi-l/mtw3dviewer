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
    private Dictionary<MapTexture, Texture2D> _textures = new();
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

            _textures.TryAdd(t, img);
        }
        //for (int y = map.MainNodes.GetLength(1) - 1; y > 0; y--)
        for (int y = 0; y < map.MainNodes.GetLength(0); y++)
        {
            for (int x = 0; x < map.MainNodes.GetLength(0); x++)
            {
                var plane = new GridPlane();
                var node = map.Nodes[0, x, y];
                var tx = Math.Clamp(node.Column, 0, map.Textures.GetLength(0) - 1);
                var ty = Math.Clamp(node.Row, 0, map.Textures.GetLength(1) - 1);
                var tex = map.Textures[tx, ty];
                plane.Create(node, map, _textures[tex], tex);
                this.AddChild(plane);
                _planes[x, y] = plane;
            }
        }
        //AddSurface(fileName, map, this.Mesh as ArrayMesh);
    }
}
