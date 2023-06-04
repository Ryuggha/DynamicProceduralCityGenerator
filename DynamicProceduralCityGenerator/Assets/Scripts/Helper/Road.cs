using UnityEngine;
using System.Collections.Generic;

public class Road 
{
    Intersection intersectionStart, intersectionEnd;
    float width;
    Line line;
    bool populated;

    public Road(Line line, float width)
    {
        this.line = line;
        line.usedByCity = true;
        this.width = width;
        intersectionStart = new Intersection(line.p1, this);
        intersectionEnd = new Intersection(line.p2, this);
        RoadGeneration.instance.generateRoadColliders(this);
    }

    protected Road(Intersection start, Line line, float width)
    {
        this.line = line;
        line.usedByCity = true;
        this.width = width;
        intersectionStart = start;
        start.addRoad(this);
        Point end = line.p1;
        if (start.point.Equals(end)) end = line.p2;
        intersectionEnd = new Intersection(end, this);
        RoadGeneration.instance.generateRoadColliders(this);
    }

    protected Road(Intersection start, Intersection end, Line line, float width)
    {
        this.line = line;
        line.usedByCity = true;
        this.width = width;
        intersectionStart = start;
        start.addRoad(this);
        intersectionEnd = end;
        end.addRoad(this);
        RoadGeneration.instance.generateRoadColliders(this);
    }

    public List<Road> expand()
    {
        List<Road> newRoads = new List<Road>();

        if (!intersectionStart.alreadyPopulated) newRoads.AddRange(intersectionStart.expand(this));
        if (!intersectionEnd.alreadyPopulated) newRoads.AddRange(intersectionEnd.expand(this));

        Populate();

        return newRoads;
    }

    public void Populate()
    {
        BuildingGeneration.instance.populateRoad(this);
        populated = true;
    }

    public bool getPopulated() { return this.populated; }

    public override bool Equals(object obj)
    {
        if (obj.GetType() != this.GetType()) return false;
        var other = obj as Road;
        if (other == null) return false;
        if ((other.intersectionStart == null) != (this.intersectionStart == null)) return false;
        if ((other.intersectionEnd == null) != (this.intersectionEnd == null)) return false;
        if (this.intersectionStart != null && !other.intersectionStart.Equals(this.intersectionStart)) return false;
        if (this.intersectionEnd != null && !other.intersectionEnd.Equals(this.intersectionEnd)) return false;
        return true;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public Vector3 getPositionStart() { return intersectionStart.getPosition(); }
    public Vector3 getPositionEnd() { return intersectionEnd.getPosition(); }
    public List<Road> getStartAdjacentRoads() { return intersectionStart.adjacentRoads; }
    public List<Road> getEndAdjacentRoads() { return intersectionEnd.adjacentRoads; }
    public float getWidth() { return this.width; }
    public bool hasOpenRoads() { return !(intersectionStart.alreadyPopulated && intersectionEnd.alreadyPopulated); }
    public bool getAlreadyPopulatedStart() { return intersectionStart.alreadyPopulated; }
    public bool getAlreadyPopulatedEnd() { return intersectionEnd.alreadyPopulated; }
    protected Intersection getIntersectionStart() { return intersectionStart; }
    protected Intersection getIntersectionEnd() { return intersectionEnd; }

    protected class Intersection
    {
        public Point point;

        public List<Road> adjacentRoads;
        public bool alreadyPopulated;

        public Intersection(Point point, List<Road> adjacentRoads)
        {
            this.point = point;
            point.usedByCity = true;
            this.adjacentRoads = adjacentRoads;
        }

        public Intersection(Point point, Road adjacentRoad)
        {
            this.point = point;
            point.usedByCity = true;
            adjacentRoads = new List<Road>();
            adjacentRoads.Add(adjacentRoad);
        }

        public bool addRoad(Road road)
        {
            if (!adjacentRoads.Contains(road))
            {
                adjacentRoads.Add(road);
                return true;
            }
            return false;
        }

        public List<Road> expand(Road road)
        {
            List<Road> newRoads = new List<Road>();
            List<Line> possibleLines = new List<Line>();
            List<float> linesProbabilities = new List<float>();
            List<Point> lineOtherPoint = new List<Point>();

            for (int i = 0; i < point.lines.Count; i++)
            {
                Line line = point.lines[i];
                if (!line.Equals(road.line) && !line.usedByCity)
                {
                    
                    Point end = line.p2.Equals(this.point) ? line.p1 : line.p2;
                    Vector3 actualRoadOtherPosition = this.getPosition().Equals(road.getPositionEnd()) ? road.getPositionStart() : road.getPositionEnd();
                    float angle = Vector3.Angle((actualRoadOtherPosition - this.getPosition()).normalized, (end.getLocationVector3() - this.getPosition()).normalized);

                    possibleLines.Add(line);
                    lineOtherPoint.Add(end);
                    linesProbabilities.Add(Random.Range(0, Mathf.Pow(angle, RoadGeneration.instance.straightRoadsBias)));
                }
            }

            for (int i = 0; i < linesProbabilities.Count - 1; i++)
            {
                for (int j = 0; j < linesProbabilities.Count - i - 1; j++)
                {
                    if (linesProbabilities[j] < linesProbabilities[j + 1])
                    {
                        Line auxLine = possibleLines[j];
                        possibleLines[j] = possibleLines[j + 1];
                        possibleLines[j + 1] = auxLine;

                        float auxProbability = linesProbabilities[j];
                        linesProbabilities[j] = linesProbabilities[j + 1];
                        linesProbabilities[j + 1] = auxProbability;

                        Point auxEnd = lineOtherPoint[j];
                        lineOtherPoint[j] = lineOtherPoint[j + 1];
                        lineOtherPoint[j + 1] = auxEnd;
                    }
                }
            }

            int numberOfRoadsToCreate = RoadGeneration.instance.curveSampler.random(0, linesProbabilities.Count + 1);

            for (int i = 0; i < numberOfRoadsToCreate; i++)
            {
                Point end = lineOtherPoint[i];
                Line line = possibleLines[i];
                if (end.usedByCity)
                {
                    Road auxRoad = RoadGeneration.instance.findIntersection(end);
                    Intersection endIntersection = auxRoad.getIntersectionStart();
                    if (!endIntersection.getPosition().Equals(end.getLocationVector3())) endIntersection = auxRoad.getIntersectionEnd();

                    newRoads.Add(new Road(this, endIntersection, line, RoadGeneration.instance.calculateNewWidth(road.width, Vector3.Angle(road.getPositionEnd() - road.getPositionStart(), end.getLocationVector3() - this.getPosition()))));
                }
                else
                {
                    newRoads.Add(new Road(this, line, RoadGeneration.instance.calculateNewWidth(road.width, Vector3.Angle(road.getPositionEnd() - road.getPositionStart(), end.getLocationVector3() - this.getPosition()))));
                }
            }

            this.alreadyPopulated = true;

            return newRoads;
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() != this.GetType()) return false;
            var other = obj as Intersection;
            if (other == null) return false;
            if (other.point.Equals(this.point)) return true;
            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public Vector3 getPosition()
        {
            return this.point.getLocationVector3();
        }
    }
}



