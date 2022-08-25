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

            Line line = Line.CreateUnbound(minPoint, maxPoint);
            lines.Add(line);

            return lines;
        }
    }
}
