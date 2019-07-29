using Godot;
using System;
using System.Collections.Generic;

public class Board : Area2D
{

    [Export]
    float ViewRadius = 300f;
    [Export]
    float CellSize = 30f;
    [Export]
    float selectionRadius = 0.02f;

    public List<Point> points = new List<Point>();
    private Point selected = null;
    private bool mouseOver = false;
    public float boardScale = 1f;

    private PackedScene PointNode = (PackedScene)ResourceLoader.Load("res://Point.tscn");
    private Label label;

    public override void _Ready()
    {
        label = (Label)GetNode("Label");
    }

    public override void _Process(float delta)
    {
        if(mouseOver)
        {
            Vector2 mouseWorldPos = GetViewport().GetMousePosition() - Position;
            Vector2 mouseGridPos = ConvertToGridPos(mouseWorldPos);
            float offset = 10f;
            label.RectPosition = mouseWorldPos - Vector2.One * offset;
            label.Text = String.Format("({0:G2},{1:G2})",mouseGridPos.x, mouseGridPos.y);

            if(Input.IsActionPressed("LeftClick"))
            {
                //Drag selected
                if(selected != null)
                {
                    selected.pos = mouseGridPos;
                    selected.Position = mouseWorldPos;
                }
                //Add new point
                else
                {
                    selected = findSelected(mouseGridPos);
                    if(selected == null)
                    {
                        selected = (Point)PointNode.Instance();
                        points.Add(selected);
                        selected.pos = mouseGridPos;
                        AddChild(selected);
                        selected.Position = mouseWorldPos;
                    }
                }
            }
            else if(Input.IsActionPressed("RightClick"))
            {
                //Remove selected
                if(selected != null)
                {
                    selected.QueueFree();
                    points.Remove(selected);
                    selected = null;
                }
                //Delete nearby point
                else
                {
                    Point p = findSelected(mouseGridPos);
                    if(p != null)
                    {
                        points.Remove(p);
                        p.QueueFree();
                    }
                }
            }
            else
            {
                selected = null;
            }
        }
    }

    public Point findSelected(Vector2 mousePosition)
    {
        foreach(Point p in points)
        {
            if(p.pos.DistanceTo(mousePosition) < selectionRadius*boardScale)
            {
                return p;
            }
        }
        return null;
    }

    public override void _Draw()
    {
        for(float x = -ViewRadius; x <= ViewRadius; x += CellSize)
            DrawLine(new Vector2(x, ViewRadius), new Vector2(x, -ViewRadius), Colors.Cyan);
        for(float y = -ViewRadius; y <= ViewRadius; y += CellSize)
            DrawLine(new Vector2(ViewRadius, y), new Vector2(-ViewRadius, y), Colors.Cyan);
    }

    public void _on_Board_mouse_exited()
    {
        mouseOver = false;
        label.Visible = false;
    }

    public void _on_Board_mouse_entered()
    {
        mouseOver = true;
        label.Visible = true;
    }

    public Vector2 ConvertToWorldPos(Vector2 gridPos)
    {
        float x = 2*ViewRadius*gridPos.x/boardScale - ViewRadius;
        float y = 2*ViewRadius*(1 - (gridPos.y/boardScale)) - ViewRadius;
        return new Vector2(x, y);
    }

    public Vector2 ConvertToGridPos(Vector2 worldPos)
    {
        float x = (worldPos.x + ViewRadius)/(ViewRadius*2)*boardScale;
        float y = (1 - (worldPos.y + ViewRadius)/(ViewRadius*2))*boardScale;
        return new Vector2(x, y);
    }
}
