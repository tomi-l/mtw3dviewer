using Godot;
using System;
using System.IO;

public partial class ModelViewer : Node3D
{

    private FileDialog _dialog;
    private Model _mesh;
    private MapPlane _mapPlane;
    private CameraController _camera;
    public override void _Ready()
    {
        _dialog = GetNode<FileDialog>("FileDialog");
        _mesh = GetNode<Model>("Model");
        _mapPlane = GetNode<MapPlane>("Terrain");
        _dialog.FileSelected += LoadFile;
        _camera = GetNode<CameraController>("CameraController");
        GetNode<MenuBar>("MenuBar").GetNode<Button>("OpenButton").Pressed += () =>
        {
            _dialog.Show();
        };
        base._Ready();
       
    }
    public override void _UnhandledKeyInput(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent)
        {
            if (keyEvent.Pressed)
            {
                if (keyEvent.PhysicalKeycode == Key.F1)
                {
                    _dialog.Show();
                }
            }
        }
        base._UnhandledKeyInput(@event);
    }
    private void LoadFile(string path)
    {
        //Input.MouseMode = Input.MouseModeEnum.Captured;
        if (path.ToLower().EndsWith(".3xx"))
        {
            _mesh.LoadModel(path);
        }
        else if (path.ToLower().EndsWith(".jjm"))
        {
            //GetViewport().DebugDraw = Viewport.DebugDrawEnum.Wireframe;
            _mapPlane.LoadMap(path);
            //var n = _mapPlane.Map.Nodes[5, 5];
            //_camera.GlobalPosition = new Vector3(0, n.Height, 0);
        }

    }
}
