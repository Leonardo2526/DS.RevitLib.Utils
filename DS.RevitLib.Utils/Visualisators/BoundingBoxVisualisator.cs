using Autodesk.Revit.DB;
using DS.RevitLib.Utils.ModelCurveUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.Visualisators
{
    public class BoundingBoxVisualisator : IVisualisator
    {
        private readonly BoundingBoxXYZ _boundingBoxXYZ;
        private readonly Document _document;

        public BoundingBoxVisualisator(BoundingBoxXYZ boundingBoxXYZ, Document document)
        {
            _boundingBoxXYZ = boundingBoxXYZ;
            _document = document;
        }

        public void Visualise()
        {
            var transform = _boundingBoxXYZ.Transform;

            _boundingBoxXYZ.get_MaxEnabled(1);
            XYZ minPoint = _boundingBoxXYZ.Min;
            XYZ maxPoint = _boundingBoxXYZ.Max;

            XYZ minTrPoint = transform.OfPoint(minPoint);
            XYZ maxTrPoint = transform.OfPoint(maxPoint);

            List<Line> lines = GetLines(minTrPoint, maxTrPoint);

            var lineCreator = new ModelCurveCreator(_document);

            foreach (var line in lines)
            {
                lineCreator.Create(line);
            }
        }

        private List<Line> GetLines(XYZ minPoint, XYZ maxPoint)
        {
            List<Line> lines = new List<Line>();

            var minPoints = GetOrthPoints(minPoint, maxPoint);
            var maxPoints = GetOrthPoints(maxPoint, minPoint);

            lines.AddRange(GetSideLines(minPoints));
            lines.AddRange(GetSideLines(maxPoints));
            lines.AddRange(GetIntermediateLines(minPoints, maxPoints));

            return lines;
        }

        private List<Line> GetSideLines(List<XYZ> points)
        {
            var lines = new List<Line>();

            for (int i = 1; i < points.Count; i++)
            {
                lines.Add(Line.CreateBound(points[0], points[i]));
            }

            lines.Add(Line.CreateBound(points.First(), points.Last()));

            return lines;
        }

        private List<Line> GetIntermediateLines(List<XYZ> minPoints, List<XYZ> maxPoints)
        {
            var lines = new List<Line>()
            {
                Line.CreateBound(minPoints[3], maxPoints[1]),
                Line.CreateBound(minPoints[3], maxPoints[2]),
                Line.CreateBound(minPoints[1], maxPoints[2]),
                Line.CreateBound(maxPoints[3], minPoints[1]),
                Line.CreateBound(maxPoints[3], minPoints[2]),
                Line.CreateBound(maxPoints[1], minPoints[2]),

            };

            return lines;
        }

        private List<XYZ> GetOrthPoints(XYZ basePoint, XYZ point)
        {
            var points = new List<XYZ>()
            {
                new XYZ(basePoint.X, basePoint.Y, basePoint.Z),
                new XYZ(point.X, basePoint.Y, basePoint.Z),
                new XYZ(basePoint.X, point.Y, basePoint.Z),
                new XYZ(basePoint.X, basePoint.Y, point.Z)
            };
            return points;
        }
    }
}
