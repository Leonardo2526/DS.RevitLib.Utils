using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils;
using DS.RevitLib.Utils.Creation.Transactions;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.Lines;
using DS.RevitLib.Utils.ModelCurveUtils;
using DS.RevitLib.Utils.Visualisators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.Geometry.Points
{
    /// <summary>
    /// An object that represents visualisator for see cref="Autodesk.Revit.DB.XYZ"/>.
    /// </summary>
    public class XYZVisualizator : IPointVisualisator<XYZ>
    {
        private readonly UIDocument _uiDoc;
        private readonly Document _doc;
        private readonly bool _refresh;
        private double _labelSize;
        private readonly double _minLineLength;
        private readonly ITransactionFactory _transactionFactory;
        private readonly double _arrowLabelAngle = 30.DegToRad();

        /// <summary>
        /// Instantiate an object to show <see cref="Autodesk.Revit.DB.XYZ"/>.
        /// </summary>
        /// <param name="uiDoc"></param>
        /// <param name="labelSize"></param>
        /// <param name="transactionBuilder"></param>
        /// <param name="refresh"></param>
        public XYZVisualizator(UIDocument uiDoc, double labelSize = 0, ITransactionFactory transactionBuilder = null, bool refresh = false) 
        {            
            _uiDoc = uiDoc;
            _doc = uiDoc.Document;
            _refresh = refresh;
            _labelSize = labelSize == 0 ? 100.MMToFeet() : labelSize;
            _minLineLength = 3 * _labelSize;
            _transactionFactory ??= new ContextTransactionFactory(_doc);
        }

        /// <summary>
        /// Label size.
        /// </summary>
        public double LabelSize { get => _labelSize; set => _labelSize = value; }

        /// <summary>
        /// Show point
        /// </summary>
        /// <param name="point"></param>
        /// <exception cref="ArgumentException"></exception>
        public void Show(XYZ point)
        {
            XYZ xYZ = (XYZ)point;
            if(xYZ == null ) { throw new ArgumentException(); }

            Line line1 = Line.CreateBound(
                xYZ + XYZ.BasisX.Multiply(_labelSize / 2),
                xYZ - XYZ.BasisX.Multiply(_labelSize / 2));

            Line line2 = Line.CreateBound(
               xYZ + XYZ.BasisY.Multiply(_labelSize / 2),
               xYZ - XYZ.BasisY.Multiply(_labelSize / 2));

            Line line3 = Line.CreateBound(
               xYZ + XYZ.BasisZ.Multiply(_labelSize / 2),
               xYZ - XYZ.BasisZ.Multiply(_labelSize / 2));

            _transactionFactory.CreateAsync(() =>
            {
                var creator = new ModelCurveCreator(_doc);
                creator.Create(line1);
                creator.Create(line2);
                creator.Create(line3);
                if (_refresh) { _doc.Regenerate(); _uiDoc?.RefreshActiveView(); }
            }, "ShowPoint");
        }

        /// <summary>
        /// Show vector between <paramref name="p1"/> and <paramref name="p2"/>.   
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// </summary>
        public void ShowVector(XYZ p1, XYZ p2)
        {
            XYZ endPoint = new XYZ(p2.X, p2.Y, p2.Z);

            var vector = (p2- p1);
            var length = vector.GetLength();
            if(length < _minLineLength) 
            {
                var offsetVector = vector.Normalize().Multiply(_minLineLength);
                endPoint = p1 + offsetVector; 
            }

            Line mainLine = Line.CreateBound(p1, endPoint);
            var labelLines = GetArrows(mainLine, endPoint, _labelSize);

            var vectorLabel = new List<Line>
            {
                mainLine
            };
            vectorLabel.AddRange(labelLines);

            _transactionFactory.CreateAsync(() =>
            {
                var creator = new ModelCurveCreator(_doc);
                vectorLabel.ForEach(l => creator.Create(l));
                if (_refresh) { _doc.Regenerate(); _uiDoc?.RefreshActiveView(); }
            }, "ShowVector");
        }

        /// <summary>
        /// Show vector from <paramref name="origin"/> by <paramref name="direction"/>.
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="direction"></param>
        public void ShowVectorByDirection(XYZ origin, XYZ direction)
        {
            var p2 = origin + direction;
            ShowVector(origin, p2);
        }


        private List<Line> GetArrows(Line line, XYZ point, double labelSize)
        {
            var lines = new List<Line>();

            var c = line.GetCenter();
            var dir = (c - point).Normalize();
            var p1 = point;
            var p2 = point + dir.Multiply(labelSize);

            var arrowLabelLine = Line.CreateBound(p1, p2);
            XYZ norm = GetNormal(line, dir);

            var tr = Transform.CreateRotationAtPoint(norm, _arrowLabelAngle, point);
            var l1 = arrowLabelLine.CreateTransformed(tr) as Line;
            var l2 = arrowLabelLine.CreateTransformed(tr.Inverse) as Line;

            tr = Transform.CreateRotationAtPoint(dir, 90.DegToRad(), point);
            var l3 = l1.CreateTransformed(tr) as Line;
            var l4 = l2.CreateTransformed(tr) as Line;

            lines.Add(l1);
            lines.Add(l2);
            lines.Add(l3);
            lines.Add(l4);

            return lines;
        }

        private static XYZ GetNormal(Line line, XYZ dir)
        {
            if (Math.Round(XYZ.BasisX.AngleTo(dir).RadToDeg()) == 90)
            { return XYZ.BasisX; }

            if (Math.Round(XYZ.BasisY.AngleTo(dir).RadToDeg()) == 90)
            { return XYZ.BasisY; }

            if (Math.Round(XYZ.BasisX.AngleTo(dir).RadToDeg()) == 90)
            { return XYZ.BasisZ; }

            return line.GetNormal(); ;
        }
    }
}
