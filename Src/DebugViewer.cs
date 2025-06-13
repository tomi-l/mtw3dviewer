using Godot;
using System;
using System.Collections.Generic;

public partial class DebugViewer : GridContainer
{
    private static Dictionary<string, string> _info;
    private static Dictionary<string, string> _oldInfo;
    private static DebugViewer _instance;
    public override void _Ready()
    {
        _instance = this;
        base._Ready();
    }
    public static void WriteInfo(string key, string value)
    {
        if (_info == null)
            _info = new Dictionary<string, string>();
        _info[key] = value;
    }
    public static void Clear()
    {
        _info = new Dictionary<string, string>();
        foreach (var c in _instance.GetChildren())
            c.QueueFree();
    }
    public override void _Process(double delta)
    {
        if (_info != _oldInfo)
        {
            foreach (var i in _info)
            {
                this.AddChild(new Label { Text = i.Key + ":" });
                this.AddChild(new Label { Text = i.Value});
            }
        }
        _oldInfo = _info;
        base._Process(delta);
    }
}
