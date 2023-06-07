using System.Collections.Generic;
using UnityEngine;
using static Road;

public class Point
{
    public Vector2 location;
    public List<Line> lines;
    public List<Poly> polygons;
    public bool isExterior;

    public bool usedByCity;

    public Point(float x, float y)
    {
        this.location = new Vector2(x, y);
        lines = new List<Line>();
        polygons = new List<Poly>();
    }

    public void Draw(float time)
    {
        //Debug.Draw(new Vector3(location.x, 0, location.y), "o");
    }

    public override bool Equals(object obj)
    {
        if (obj == null) return false;
        if (obj.GetType() != this.GetType()) return false;
        var other = obj as Point;
        if (other == null) return false;
        return (this.location-other.location).magnitude < HexagonalGrid.instance.pointEqualityMaxDistance;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public Vector3 getLocationVector3()
    {
        return new Vector3(location.x, 0, location.y);
    }

    public Point copy()
    {
        Point r = new Point(location.x, location.y);

        r.lines = new List<Line>(this.lines);
        r.polygons = new List<Poly>(this.polygons);

        return r;
    }

    public List<Line> fuse(Point other)
    {
        List<Line> r = new List<Line>();

        if (!this.Equals(other) || this == other) return r;

        if (other.usedByCity) this.usedByCity = true;

        foreach (var line in other.lines)
        {
            if (line.p1 == other) line.p1 = this;
            else if (line.p2 == other) line.p2 = this;
            if (lines.Contains(line))
            {
                Line realLine = lines[lines.IndexOf(line)];

                Point otherPointRealLine = realLine.p1;
                if (realLine.p1 == this) otherPointRealLine = realLine.p2;

                Point otherPointOldLine = line.p1;
                if (line.p1 == other) otherPointOldLine = line.p2;

                if (otherPointRealLine == otherPointOldLine)
                {
                    if (line.usedByCity) realLine.usedByCity = true;
                    for (int i = otherPointOldLine.lines.Count - 1; i >= 0; i--)
                    {
                        if (otherPointOldLine.lines[i] == line) 
                        {
                            otherPointOldLine.lines[i].p1 = null;
                            otherPointOldLine.lines[i].p2 = null;
                            otherPointOldLine.lines.RemoveAt(i);
                        }
                    }
                    r.Add(line);
                }
                else
                {
                    if (line.p1 == other) line.p1 = this;
                    else line.p2 = this;
                }
            }

            lines.Add(line);

            foreach (var poly in line.polygons)
            {
                if (!polygons.Contains(poly))
                {
                    polygons.Add(poly);
                    for (int i = 0; i < poly.points.Count; i++)
                    {
                        if (poly.points[i] == other) poly.points[i] = this;
                    }
                }
            }
        }

        other.lines.Clear();
        other.polygons.Clear();

        return r;
    }
}

public class Line
{
    public Point p1;
    public Point p2;

    public List<Poly> polygons;

    public bool usedByCity;

    public Line(Point p1, Point p2)
    {
        polygons = new List<Poly>();

        if (p1.Equals(p2)) return;
        this.p1 = p1;
        this.p2 = p2;

        if (!p1.lines.Contains(this)) p1.lines.Add(this);
        if (!p2.lines.Contains(this)) p2.lines.Add(this);
    }

    public void Draw(float time)
    {
        Debug.DrawLine(new Vector3(p1.location.x, 0, p1.location.y), new Vector3(p2.location.x, 0, p2.location.y), Color.red, time);
    }

    public override bool Equals(object obj)
    {
        if (obj.GetType() != this.GetType()) return false;
        var other = obj as Line;
        if (other == null) return false;
        if (other.p1.Equals(this.p1) && other.p2.Equals(this.p2)) return true;
        if (other.p1.Equals(this.p2) && other.p2.Equals(this.p1)) return true;
        return false;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}

public class Poly
{
    public List<Point> points;
    public List<Line> lines;

    public Poly(List<Point> points)
    {
        if (points.Count < 3) return;

        lines = new List<Line>();
        this.points = points;

        List<Line> possibilitySpaceLines = new List<Line>();

        for (int i = 0; i < points.Count; i++)
        {
            possibilitySpaceLines.AddRange(points[i].lines);
            if (points.Count == 3)
            {
                if (points[i].polygons.Find(x => x.Similar(this)) == null) points[i].polygons.Add(this);
            }
            else
            {
                if (!points[i].polygons.Contains(this)) points[i].polygons.Add(this);
            }
        }

        for (int i = 0; i < points.Count; i++)
        {
            int index = -1;
            if (i != points.Count - 1)
            {
                index = possibilitySpaceLines.IndexOf(new Line(points[i], points[i + 1]));
            }
            else index = possibilitySpaceLines.IndexOf(new Line(points[i], points[0]));

            if (index == -1) Debug.LogError("No Line found for this Poly");
            else
            {
                lines.Add(possibilitySpaceLines[index]);
            }
        }

        foreach (var line in lines)
        {
            if (line.polygons.FindIndex(x => x.Similar(this)) == -1) line.polygons.Add(this);
        }

        foreach (var point in points)
        {
            if (point.polygons.FindIndex(x => x.Similar(this)) == -1) point.polygons.Add(this);
        }
    }

    public void Draw(float time)
    {
        for (int i = 0; i < points.Count; i++)
        {
            if (i != points.Count - 1)
            {
                Debug.DrawLine(new Vector3(points[i].location.x, 0, points[i].location.y), new Vector3(points[i + 1].location.x, 0, points[i + 1].location.y), Color.white, time);
            }
            else Debug.DrawLine(new Vector3(points[i].location.x, 0, points[i].location.y), new Vector3(points[0].location.x, 0, points[0].location.y), Color.white, time);
        }
    }

    public bool Similar(Poly other)
    {
        if (other.points.Count != points.Count) return false;
        for (int i = 0; i < points.Count; i++)
        {
            if (!other.points.Contains(points[i])) return false;
        }

        return true;
    }

    public bool Similar(List<Point> otherPoints)
    {
        if (otherPoints.Count != points.Count) return false;
        for (int i = 0; i < points.Count; i++)
        {
            if (!otherPoints.Contains(points[i])) return false;
        }

        return true;
    }

    public override bool Equals(object obj)
    {
        if (obj.GetType() != this.GetType()) return false;
        var other = obj as Poly;
        if (other == null) return false;
        return points.Equals(other.points);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}
