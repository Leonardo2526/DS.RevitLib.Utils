using Autodesk.Revit.DB;
using DS.RevitLib.Utils;
using System.Collections.Generic;
using Ivanov.RevitLib.Utils;
using System.Linq;
using DS.RevitLib.Utils.GPExtractor;
using OLMP.RevitLib.MEPAC.DocumentModels;
using DS.RevitLib.Utils.ModelCurveUtils;
using OLMP.RevitLib.MEPAC.Collisons.Resolvers.MEPOffset.Lines;


namespace OLMP.RevitLib.MEPAC.Collisons.Resolvers.MEPOffset.Lines
{
    class ElementCenterLine
    {
        readonly Element Elem;

        public ElementCenterLine(Element elem)
        {
            Elem = elem;
        }
       
        public Line CreateCenterLineByStaticPoint(XYZ offset, XYZ staticPoint, bool show)
        {
            ModelCurveCreator _modelCurveCreator = new ModelCurveCreator(DocModel.Doc);
            ElementUtils.GetPoints(Elem, out XYZ startPoint, out XYZ endPoint, out XYZ centerPointElement);

            List<XYZ> glPoints = new List<XYZ>();
            glPoints.Add(startPoint);
            glPoints.Add(endPoint);

            var locCurve = Elem.Location as LocationCurve;
            Line line = locCurve.Curve as Line;
            (List<XYZ> glStaticPoints, List<XYZ> glMovablePoints) = 
                new OLMP.RevitLib.MEPAC.CR2.PointUtils().SortPoints(glPoints, staticPoint, line);

            if (offset == null)
                offset = new XYZ();

            if (show)
                _modelCurveCreator.Create(glStaticPoints[0], glMovablePoints[0] + offset);

            return Line.CreateBound(glStaticPoints[0], glMovablePoints[0] + offset);
        }
    }
}
