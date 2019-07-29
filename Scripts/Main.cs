using Godot;
using System;
using System.Collections.Generic;

public class Polynomial
{
    public int degree = 1;
    public List<double> coefs = new List<double>();

    public Polynomial(int deg)
    {
        degree = deg;
        for(int i = 0; i <= degree; ++i)
        {
            coefs.Add(0.1);
        }
    }

    public double solve(double x)
    {
        double ret = 0;
        for(int i = 0; i <= degree; ++i)
        {
            ret += Math.Pow(x, i) * coefs[i];
        }
        return ret;
    }
}

public class Main : Node
{
    private Line2D RedLine;
    private Line2D OrangeLine;
    private Line2D YellowLine;
    private Line2D GreenLine;

    private Board board;

    private Polynomial RedPolynomial;
    private Polynomial OrangePolynomial;
    private Polynomial YellowPolynomial;
    private Polynomial GreenPolynomial;

    private int degree = 1;

    private bool running = false;
    private float rate = 0.3f;
    private float clock = 0f;

    public override void _Ready()
    {
        board = (Board)GetNode("./Board");

        RedLine = (Line2D)GetNode("./Board/RedRegressionLine");
        OrangeLine = (Line2D)GetNode("./Board/OrangeRegressionLine");
        YellowLine = (Line2D)GetNode("./Board/YellowRegressionLine");
        GreenLine = (Line2D)GetNode("./Board/GreenRegressionLine");
        
        RedPolynomial = new Polynomial(1);
        OrangePolynomial = new Polynomial(1);
        YellowPolynomial = new Polynomial(1);
        GreenPolynomial = new Polynomial(1);

        UpdateLine(RedLine, RedPolynomial);
        UpdateLine(OrangeLine, OrangePolynomial);
        UpdateLine(YellowLine, YellowPolynomial);
        UpdateLine(GreenLine, GreenPolynomial);
    }

    public void UpdateLine(Line2D line, Polynomial polynomial)
    {
        const int pointCount = 100;
        Vector2[] points = new Vector2[pointCount+1];
        for(int i = 0; i <= pointCount; ++i)
        {
            float x = (i/(float)pointCount)*board.boardScale;
            float y = (float)polynomial.solve(x);
            if(y > board.boardScale)
                y = board.boardScale;
            if(y < 0)
                y = 0;
            points[i] = board.ConvertToWorldPos(new Vector2(x, y));
        }
        line.SetPoints(points);
    }

    public override void _Process(float delta)
    {
        if(running)
        {
            clock += delta;
            if(clock >= rate)
            {
                clock -= rate;
                Train();
            }
        }
    }

    public void Start()
    {
        running = true;
    }

    public void Stop()
    {
        running = false;
    }

    public void Step()
    {
        running = false;
        Train();
        GD.Print(MSE());
    }

    public void CyclePolynomials()
    {
        RedPolynomial = OrangePolynomial;
        OrangePolynomial = YellowPolynomial;
        YellowPolynomial = GreenPolynomial;

        Polynomial newPoly = new Polynomial(degree);
        for(int i = 0; i <= Mathf.Min(GreenPolynomial.degree, degree); ++i)
        {
            newPoly.coefs[i] = GreenPolynomial.coefs[i];
        }

        GreenPolynomial = newPoly;
    }

    public void UpdateLines()
    {
        UpdateLine(RedLine, RedPolynomial);
        UpdateLine(OrangeLine, OrangePolynomial);
        UpdateLine(YellowLine, YellowPolynomial);
        UpdateLine(GreenLine, GreenPolynomial);
    }

    public void Train()
    {
        CyclePolynomials();

        if(board.points.Count == 0)
            return;

        List<double> deltas = new List<double>(new double[degree+1]);
        foreach(Point p in board.points)
        {
            
            double yHat = GreenPolynomial.solve((double)p.pos.x);
            for(int d = 0; d <= degree; ++d)
            {
                deltas[d] += Math.Pow((double)p.pos.x, d) * (yHat - (double)p.pos.y) / (double)board.points.Count;
            }
        }

        for(int d = 0; d <= degree; ++d)
        {
            GreenPolynomial.coefs[d] -= deltas[d];
        }

        UpdateLines();
    }

    public void Nudge()
    {
        CyclePolynomials();

        for(int d = 0; d <= degree; ++d)
        {
            GreenPolynomial.coefs[d] += (float)GD.RandRange(-1,1);
        }

        UpdateLines();
    }

    public double MSE()
    {
        double mse = 0.0;
        foreach(Point p in board.points)
        {
            double yHat = GreenPolynomial.solve((double)p.pos.x);
            mse += Math.Pow(yHat-(double)p.pos.y, 2)/(double)board.points.Count;
        }
        return mse;
    }

    public void Reset()
    {
        GreenPolynomial = new Polynomial(degree);
    }

    public void SetDegree(string text)
    {
        if(int.TryParse(text, out int value))
        {
            degree = value;
        }
    }
}
