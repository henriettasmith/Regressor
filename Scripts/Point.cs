using Godot;
using System;

[Tool]
public class Point : Node2D
{
    public Vector2 pos;

    public override void _Draw()
    {
        DrawCircle(Vector2.Zero, 4f, Colors.Blue);
    }
}