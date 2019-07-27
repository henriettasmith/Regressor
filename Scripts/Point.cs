using Godot;
using System;

[Tool]
public class Point : Node2D
{
    public Vector2 pos;

    public override void _Draw()
    {
        float radius = 6f;
        DrawCircle(Vector2.Zero, radius, Colors.Blue);
    }
}