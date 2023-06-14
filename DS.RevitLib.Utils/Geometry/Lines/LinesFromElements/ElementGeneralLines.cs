using Autodesk.Revit.DB;
using DS.RevitLib.Utils;
using System.Collections.Generic;
using Ivanov.RevitLib.Utils;
using System.Linq;
using DS.RevitLib.Utils.GPExtractor;
using OLMP.RevitLib.MEPAC.DocumentModels;
using DS.RevitLib.Utils.ModelCurveUtils;
using OLMP.RevitLib.MEPAC.Collisons.Resolvers.MEPOffset.Lines;
using OLMP.RevitLib.MEPAC.CR2;
using OLMP.RevitLib.MEPAC.Collisons.CollisionModels;

namespace OLMP.RevitLib.MEPAC.Collisons.Resolvers.MEPOffset.Lines
{
    class ElementGeneralLines : BaseMEPCollision
    {
        readonly Element Elem;

        public ElementGeneralLines(MEPCollision collision, Element elem) : base(collision)
        {
            Elem = elem;
        }

        public List<Line> CreateGeneralLines(XYZ offset, bool show)
        {

            IGeneralPointExtractor pointExtractor = new GeneralPointExtractor(Elem);
            var pointUtils = new OLMP.RevitLib.MEPAC.CR2.PointUtils();
            pointUtils.GetGeneralPoints(pointExtractor, out List<XYZ> glStartPoints, out List<XYZ> glEndPoints);

            LinesCreator linesCreator = new LinesCreator();

            return linesCreator.CreateLines(_collision, Elem, glStartPoints, glEndPoints, offset, show);
        }

        public List<Line> CreateGeneralLinesByStaticPoint(XYZ offset, XYZ staticPoint, bool show)
        {
            List<Solid> elemSolids = ElementUtils.GetSolids(Elem);
            Solid elemSolid = elemSolids.First();

            var locCurve = Elem.Location as LocationCurve;
            Line line = locCurve.Curve as Line;

            List<XYZ> gPoints = GPExtractor.GetGeneralPoints(elemSolid);
            (List<XYZ> glStaticPoints, List<XYZ> glMovablePoints) = new OLMP.RevitLib.MEPAC.CR2.PointUtils().SortPoints(gPoints, staticPoint, line);

            LinesCreator linesCreator = new LinesCreator();

            return linesCreator.CreateLinesByStaticPoint(_collision, Elem, glStaticPoints, glMovablePoints, offset, staticPoint, show);
        }

        public List<Line> CreateGeneralLinesByStaticPointOld(XYZ offset, XYZ staticPoint, bool show)
        {

            IGeneralPointExtractor pointExtractor = new GeneralPointExtractor(Elem);
            var pointsUtils = new OLMP.RevitLib.MEPAC.CR2.PointUtils();
            pointsUtils.GetGeneralPoints(pointExtractor, out List<XYZ> glStartPoints, out List<XYZ> glEndPoints);

            LinesCreator linesCreator = new LinesCreator();

            return linesCreator.CreateLinesByStaticPoint(_collision, Elem, glStartPoints, glEndPoints, offset, staticPoint, show);
        }
    }
}
