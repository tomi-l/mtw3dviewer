using Godot;
using System;

public partial class CameraController : Node3D
{
    private const float MOVESPEED = 2500f;
    private const float CAMERA_MOUSE_ROTATION_SPEED = 0.003f;
    private Camera3D _camera;
    private bool _mouseLook;
    public override void _Ready()
    {
        _camera = GetNode<Camera3D>("Camera3D");
        base._Ready();
    }
    public override void _Process(double delta)
    {
        // movement
        var fdelta = (float)delta;
        var moveDir = Vector3.Zero;
        if (Input.IsKeyPressed(Key.W))
            moveDir -= _camera.GlobalTransform.Basis.Z;
        if (Input.IsKeyPressed(Key.S))
            moveDir += _camera.GlobalTransform.Basis.Z;
        if (Input.IsKeyPressed(Key.A))
            moveDir -= _camera.GlobalTransform.Basis.X;
        if (Input.IsKeyPressed(Key.D))
            moveDir += _camera.GlobalTransform.Basis.X;

        if (moveDir != Vector3.Zero)
            this.Position += moveDir.Normalized() * MOVESPEED * fdelta;
    }
    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseMotion mm && _mouseLook)
        {
            float scaleFactor = Mathf.Min(
            DisplayServer.WindowGetSize().X / GetViewport().GetVisibleRect().Size.X,
            DisplayServer.WindowGetSize().Y / GetViewport().GetVisibleRect().Size.Y
            );
            _camera.RotateY(Mathf.DegToRad(-mm.Relative.X * CAMERA_MOUSE_ROTATION_SPEED));
            RotateCamera(mm.Relative * CAMERA_MOUSE_ROTATION_SPEED * scaleFactor);
        }
        if (@event is InputEventMouseButton mb)
        {
            if (mb.Pressed && mb.ButtonIndex == MouseButton.Right)
            {
                _mouseLook = true;
                Input.MouseMode = Input.MouseModeEnum.Captured;
            }
            else
            {
                _mouseLook = false;
                Input.MouseMode = Input.MouseModeEnum.Visible;
            }
        }
        base._Input(@event);
    }
    private void RotateCamera(Vector2 move)
    {
        this.RotateY(-move.X);
        this.Orthonormalize();

        var rot = _camera.Rotation - new Vector3(move.Y, 0, 0);
        _camera.Rotation = rot;
        _camera.Orthonormalize();
    }
}
