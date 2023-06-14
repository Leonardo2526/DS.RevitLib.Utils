using Autodesk.Revit.DB;
using DS.RevitLib.Utils;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.MEP.Models;
using DS.RevitLib.Utils.Models;
using DS.RevitLib.Utils.Connections.PointModels;
using DS.RevitLib.Utils.Solids.Models;
using DS.RevitLib.Utils.Elements.Transfer.TransformBuilders;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DS.RevitLib.Utils.Elements.Transfer
{
    internal class LineModelBuilder
    {
        private readonly MEPCurve _startMEPCurve;
        private readonly List<XYZ> _path;
        private readonly double _elbowRadius;

        public LineModelBuilder(MEPCurve startMEPCurve, List<XYZ> path, double elbowRadius)
        {
            _startMEPCurve = startMEPCurve;
            _path = path;
            _elbowRadius = elbowRadius;
        }

        public List<Line> Lines { get; private set; } = new List<Line>();
        public List<LineModel> LineModels { get; private set; } = new List<LineModel>();
        public MEPCurveModel BaseMEPCurveModel { get; private set; }


        public List<Line> GetLines()
        {
            for (int i = 0; i < _path.Count - 1; i++)
            {
                XYZ dir = (_path[i + 1] - _path[i]).Normalize();

                XYZ p1 = _path[i] + dir.Multiply(_elbowRadius);
                if (!p1.IsBetweenPoints(_path[i], _path[i + 1]))
                {
                    throw new InvalidOperationException($"Point {p1} is not between points {_path[i]} and {_path[i + 1]}");
                }
                XYZ p2 = _path[i + 1] - dir.Multiply(_elbowRadius);
                if (!p2.IsBetweenPoints(p1, _path[i + 1]))
                {
                    throw new InvalidOperationException($"Point {p2} is not between points {p1} and {_path[i + 1]}");
                }

                var line = Line.CreateBound(p1, p2);
                //line.Show(_p1.Element.Document);
                Lines.Add(line);
            }

            return Lines;
        }

        public List<LineModel> Build()
        {
            GetLines();

            var startLineModel = GetStartLineModel();
            Basis initBasis = startLineModel.Basis.Clone();
            //initBasis.Show(DocModel.Doc);
            //DocModel.UiDoc.RefreshActiveView();

            foreach (var line in Lines)
            {
                var trModel = new BasisLineTransformBuilder(initBasis, line).Build();
                initBasis.Transform(trModel.Transforms);

                var lineModel = new LineModel(line, initBasis);
                //lineModel.Basis.Show(DocModel.Doc);
                LineModels.Add(lineModel);
                initBasis = initBasis.Clone();
            }

            return LineModels;
        }

        private LineModel GetStartLineModel()
        {
            BaseMEPCurveModel = new MEPCurveModel(_startMEPCurve, new SolidModel(ElementUtils.GetSolid(_startMEPCurve)));
            return new LineModel(_startMEPCurve);
        }

    }
}
