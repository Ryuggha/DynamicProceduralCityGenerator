using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexagonalGrid : MonoBehaviour
{
    public static HexagonalGrid instance;

    [SerializeField] [Range(0, 1)] float relaxValue;
    [SerializeField] [Range(0, 5)] int size;
    [SerializeField] float HexagonSize = 25;

    Dictionary<Vector2Int, HexagonChunk> hexagonChunks;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }

        hexagonChunks = new Dictionary<Vector2Int, HexagonChunk>();

        generateChunk(0, 0);
    }

    private void OnDestroy()
    {
        if (instance == this) instance = null;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            drawAllPolygons(8);
        }
    }

    public void generateChunk(int x, int y)
    {
        Vector2Int position = new Vector2Int(x, y);

        if (hexagonChunks.ContainsKey(position)) Debug.LogWarning("This chunk has already been generated");
        else
        {
            hexagonChunks.Add(position, new HexagonChunk(HexagonSize, size, relaxValue, position));
        }
    }


    public Vector2Int getChunkByPoint(Vector3 point)
    {
        Vector2Int indexChunk = Vector2Int.zero;
        foreach (Vector2Int index in hexagonChunks.Keys)
        {
            if (!hexagonChunks.ContainsKey(indexChunk)) indexChunk = index;
            else
            {
                if ((hexagonChunks[index].getStartingPositionVector3() - point).magnitude < (hexagonChunks[indexChunk].getStartingPositionVector3() - point).magnitude)
                {
                    indexChunk = index;
                }
            }
        }

        return indexChunk;
    }

    public Point getFirstPoint()
    {
        Point auxPoint;
        HexagonChunk chunk = hexagonChunks[new Vector2Int(0, 0)];
        auxPoint = chunk.points[0];

        for (int i = 1; i < chunk.points.Count; i++)
        {
            if ((chunk.points[i].location - Vector2.zero).magnitude < (auxPoint.location - Vector2.zero).magnitude) auxPoint = chunk.points[i];
        }

        return auxPoint;
    }

    /*private void drawLines()
    {
        float refreshTime = .1f;

        foreach (var point in points)
        {
            point.Draw(refreshTime);
            foreach (var line in point.lines)
            {
                line.Draw(refreshTime);
            }
        }
        Invoke("drawLines", refreshTime);
    }*/

    private void drawAllPolygons(float f)
    {
        bool refresh = false;
        float refreshTime = .1f;
        if (f == -1) refresh = true;
        else refreshTime = f;

        foreach (var chunk in hexagonChunks.Values)
        {
            foreach (var poly in chunk.polygons)
            {
                poly.Draw(refreshTime);
            }
        }
        if (refresh) Invoke("drawAllPolygons", refreshTime);
    }

    private void drawAllPolygons()
    {
        drawAllPolygons(-1);
    }

    private class HexagonChunk
    {
        public List<Point> points;
        public List<Line> lines;
        public List<Poly> polygons;
        float W, H, relaxValue;
        Vector2 startingPosition;
        public Point[] outerPoints;
        private float size;
                
        public HexagonChunk (float HexagonSize, int size, float relaxValue, Vector2Int index)
        {
            this.size = size;
            W = HexagonSize * 2;
            H = HexagonSize * Mathf.Sqrt(3f);
            this.relaxValue = relaxValue;

            startingPosition = new Vector2((size + 1) * W * index.x * .75f, (size + 1) * H * index.y + (index.x % 2 == 0 ? 0 : (size + 1) * .5f * H));

            points = new List<Point>();
            //createHexOfHexes(3);

            createHexOfTriangles(size);
            //drawLines();

            countLines();
            countTriangles();

            deleteRandomLines();
            subdivideToQuads();
            countLines();

            for (int i = 0; i < 15; i++)
            {
                relaxVertex();
            }

            outerPoints = new Point[(size + 1) * 2 * 6];

            organizeOuterPoints();
        }

        public Vector3 getStartingPositionVector3()
        {
            return new Vector3(startingPosition.x, 0, startingPosition.y);
        }

        private void organizeOuterPoints()
        {
            Point topPoint = null;
            for (int i = 0; i < points.Count && topPoint == null; i++)
            {
                if (points[i].location == startingPosition + new Vector2(0, (size + 1) * H / 2))
                    topPoint = points[i];
            }

            outerPoints[0] = topPoint;

            foreach (var line in topPoint.lines)
            {
                if (line.p1.isExterior && !line.p1.Equals(topPoint) && line.p1.location.x > topPoint.location.x) 
                {
                    outerPoints[1] = line.p1;
                    break;
                }
                else if (line.p2.isExterior && !line.p2.Equals(topPoint) && line.p2.location.x > topPoint.location.x)
                {
                    outerPoints[1] = line.p2;
                    break;
                }
            }

            for(int i = 2; i < outerPoints.Length; i++)
            {
                foreach (var line in outerPoints[i-1].lines)
                {
                    if (line.p1.isExterior && !line.p1.Equals(outerPoints[i-1]) && !line.p1.Equals(outerPoints[i - 2]))
                    {
                        outerPoints[i] = line.p1;
                        break;
                    }
                    else if (line.p2.isExterior && !line.p2.Equals(outerPoints[i - 1]) && !line.p2.Equals(outerPoints[i - 2]))
                    {
                        outerPoints[i] = line.p2;
                        break;
                    }
                }
            }
        }

        private void createTriangleGrid()
        {
            for (int i = -2; i <= 2; i++)
            {
                for (int j = -2; j < 2; j++)
                {
                    points.Add(new Point(i, j));
                }
            }
            points.Add(new Point(0, 2));
        }

        private void createHexOfTriangles(int size)
        {
            for (int i = 0; i <= size; i++)
            {
                generateHexInTriangleGrid(0, i, true);
                if (i != 0) generateHexInTriangleGrid(0, -i, true);

                for (int j = 0; j < size - i / 2; j++)
                {
                    generateHexInTriangleGrid(-(j + 1), i, true);
                    generateHexInTriangleGrid(-(j + 1), -i, true);
                }

                for (int j = 0; j < size - (i + 1) / 2; j++)
                {
                    generateHexInTriangleGrid(j + 1, i, true);
                    generateHexInTriangleGrid(j + 1, -i, true);
                }
            }
        }

        private void createHexOfHexes(int size)
        {
            for (int i = 0; i <= size; i++)
            {
                generateHexInGrid(i, 0, false);
                if (i != 0) generateHexInGrid(-i, 0, false);

                for (int j = 0; j < size - i / 2; j++)
                {
                    generateHexInGrid(i, -(j + 1), false);
                    generateHexInGrid(-i, -(j + 1), false);
                }

                for (int j = 0; j < size - (i + 1) / 2; j++)
                {
                    generateHexInGrid(i, j + 1, false);
                    generateHexInGrid(-i, j + 1, false);
                }
            }
        }

        private void countLines()
        {
            lines = new List<Line>();

            foreach (var point in points)
            {
                foreach (var line in point.lines)
                {
                    if (!lines.Contains(line)) lines.Add(line);
                }
            }
        }

        private void countTriangles()
        {
            polygons = new List<Poly>();

            foreach (var point in points)
            {
                foreach (var line1 in point.lines)
                {
                    foreach (var line2 in point.lines)
                    {
                        if (!line1.Equals(line2))
                        {
                            Point p1 = line1.p1.Equals(point) ? line1.p2 : line1.p1;
                            Point p2 = line2.p1.Equals(point) ? line2.p2 : line2.p1;

                            foreach (var sharedLine in p1.lines)
                            {
                                if (sharedLine.p1.Equals(p2) || sharedLine.p2.Equals(p2))
                                {

                                    if (!polygons.Exists(x => x.Similar(new List<Point> { point, p1, p2 })))
                                    {
                                        Poly aux = new Poly(new List<Point> { point, p1, p2 });
                                        polygons.Add(aux);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void generateHexInGrid(int row, int col, bool withCenter)
        {
            Vector2 centre = new Vector2(W * row * .75f, H * col + (row % 2 == 0 ? 0 : .5f * H));

            generateHex(centre.x, centre.y, withCenter);
        }

        private void generateHexInTriangleGrid(int row, int col, bool withCenter)
        {
            Vector2 centre = new Vector2(W * row * .5f + (col % 2 == 0 ? 0 : .25f * W), H * col * .5f) + startingPosition;

            generateHex(centre.x, centre.y, withCenter);
        }

        private void generateHex(float x, float y, bool withCenter)
        {


            Vector2 centre = new Vector2(x, y);

            Vector2[] points = new Vector2[6];

            var p1 = addPoint(centre.x + W * .25f, centre.y + H * .5f);
            var p2 = addPoint(centre.x + W * .5f, centre.y);
            var p3 = addPoint(centre.x + W * .25f, centre.y - H * .5f);
            var p4 = addPoint(centre.x - W * .25f, centre.y - H * .5f);
            var p5 = addPoint(centre.x - W * .5f, centre.y);
            var p6 = addPoint(centre.x - W * .25f, centre.y + H * .5f);

            new Line(p1, p2);
            new Line(p2, p3);
            new Line(p3, p4);
            new Line(p4, p5);
            new Line(p5, p6);
            new Line(p6, p1);

            if (withCenter)
            {
                var pc = addPoint(centre.x, centre.y);

                new Line(p1, pc);
                new Line(p2, pc);
                new Line(p3, pc);
                new Line(p4, pc);
                new Line(p5, pc);
                new Line(p6, pc);
            }
        }

        private Point addPoint(float x, float y)
        {
            Point r = new Point(x, y);
            if (points.Contains(r))
            {
                return points[points.IndexOf(r)];
            }

            points.Add(r);
            return r;
        }

        private void deleteRandomLines()
        {
            List<Line> randomArray = new List<Line>();
            for (int i = 0; i < lines.Count; i++)
            {
                bool f = false;
                do
                {
                    int r = Random.Range(0, lines.Count);
                    if (!randomArray.Contains(lines[r]))
                    {
                        f = true;
                        randomArray.Add(lines[r]);
                    }

                } while (!f);
            }

            for (int i = 0; i < lines.Count; i++)
            {
                Line line = randomArray[i];
                if (line.polygons.Count > 1)
                {
                    bool allTriangles = true;
                    foreach (Poly poly in line.polygons)
                    {
                        if (poly.points.Count != 3) allTriangles = false;
                    }

                    if (allTriangles) deleteLine(line);
                }
            }
        }

        private void deleteLine(Line line)
        {
            Point p1 = line.p1;
            Point p2 = new Point(0, 0);
            Point p3 = line.p2;
            Point p4 = new Point(0, 0);

            List<Poly> auxPolyList = new List<Poly>(line.polygons);

            if (!auxPolyList[0].points[0].Equals(p1) && !auxPolyList[0].points[0].Equals(p3)) p2 = auxPolyList[0].points[0];
            else if (!auxPolyList[0].points[1].Equals(p1) && !auxPolyList[0].points[1].Equals(p3)) p2 = auxPolyList[0].points[1];
            else if (!auxPolyList[0].points[2].Equals(p1) && !auxPolyList[0].points[2].Equals(p3)) p2 = auxPolyList[0].points[2];
            if (!auxPolyList[1].points[0].Equals(p1) && !auxPolyList[1].points[0].Equals(p3)) p4 = auxPolyList[1].points[0];
            else if (!auxPolyList[1].points[1].Equals(p1) && !auxPolyList[1].points[1].Equals(p3)) p4 = auxPolyList[1].points[1];
            else if (!auxPolyList[1].points[2].Equals(p1) && !auxPolyList[1].points[2].Equals(p3)) p4 = auxPolyList[1].points[2];


            lines.Remove(line);

            for (int i = 0; i < auxPolyList.Count; i++)
            {
                Poly poly = auxPolyList[i];
                polygons.Remove(poly);
                foreach (var point in poly.points)
                {
                    point.polygons.Remove(poly);
                }
                foreach (var l in poly.lines)
                {
                    l.polygons.Remove(poly);
                }

            }

            p1.lines.Remove(line);
            p3.lines.Remove(line);

            polygons.Add(new Poly(new List<Point> { p1, p2, p3, p4 }));
        }

        private void subdivideToQuads()
        {
            List<Poly> toSubdivide = new List<Poly>(polygons);

            foreach (var poly in toSubdivide)
            {
                List<Point> polyPoints = new List<Point>(poly.points);
                List<Line> polyLines = new List<Line>(poly.lines);

                polygons.Remove(poly);

                float midPointX = 0, midPointY = 0;
                foreach (var point in polyPoints)
                {
                    point.polygons.Remove(poly);
                    midPointX += point.location.x;
                    midPointY += point.location.y;
                }

                Point midPoint = addPoint(midPointX / polyPoints.Count, midPointY / polyPoints.Count);

                foreach (var line1 in polyLines)
                {
                    line1.polygons.Remove(poly);
                    line1.p1.lines.Remove(line1);
                    line1.p2.lines.Remove(line1);


                    foreach (var line2 in polyLines)
                    {
                        if (line2.Equals(line1)) continue;

                        Point commonPoint = new Point(0, 0);
                        bool sharedPoint = false;
                        if (line1.p1.Equals(line2.p1) || line1.p1.Equals(line2.p2))
                        {
                            sharedPoint = true;
                            commonPoint = line1.p1;
                        }
                        else if (line1.p2.Equals(line2.p1) || line1.p2.Equals(line2.p2))
                        {
                            sharedPoint = true;
                            commonPoint = line1.p2;
                        }

                        if (sharedPoint)
                        {
                            Point midPointLine1 = addPoint((line1.p1.location.x + line1.p2.location.x) / 2, (line1.p1.location.y + line1.p2.location.y) / 2);
                            Point midPointLine2 = addPoint((line2.p1.location.x + line2.p2.location.x) / 2, (line2.p1.location.y + line2.p2.location.y) / 2);

                            Line auxL = new Line(midPointLine1, commonPoint);
                            if (!lines.Contains(auxL)) lines.Add(auxL);
                            auxL = new Line(commonPoint, midPointLine2);
                            if (!lines.Contains(auxL)) lines.Add(auxL);
                            auxL = new Line(midPointLine2, midPoint);
                            if (!lines.Contains(auxL)) lines.Add(auxL);
                            auxL = new Line(midPoint, midPointLine1);
                            if (!lines.Contains(auxL)) lines.Add(auxL);

                            Poly auxP = new Poly(new List<Point>() { midPointLine1, commonPoint, midPointLine2, midPoint });
                            if (polygons.FindIndex(x => x.Similar(auxP)) == -1) polygons.Add(auxP);
                        }
                    }
                }
            }
        }


        private void relaxVertex()
        {
            List<Point> deepCopy = new List<Point>();

            foreach (var point in points)
            {
                deepCopy.Add(point.copy());
            }


            foreach (var vertex in deepCopy)
            {
                Point point = points[points.IndexOf(vertex)];

                foreach (var line in point.lines)
                {
                    if (line.polygons.Count < 2)
                    {
                        point.isExterior = true;
                    }
                }

                if (!point.isExterior)
                {
                    Vector2 target = Vector2.zero;

                    foreach (var line in vertex.lines)
                    {
                        Point otherPoint = line.p1;

                        if (line.p1.Equals(vertex)) otherPoint = line.p2;

                        target += otherPoint.location;
                    }

                    target /= vertex.lines.Count;

                    point.location = Vector2.Lerp(point.location, target, relaxValue);
                }
            }
        }
    }

    
}

