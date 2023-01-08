using Autodesk.Revit.DB;
using OLMP.RevitLib.MEPAC.Collisons.CollisionModels;
using System.Collections.Generic;

namespace OLMP.RevitLib.MEPAC.Collisons.Resolvers.MEPOffset.Lines
{
    class ElementClearanceLines : BaseMEPCollision
    {
        readonly Element Elem;

        public ElementClearanceLines(MEPCollision collision, Element elem) : base(collision)
        {
            Elem = elem;
        }

        public List<Line> CreateClearenceLines(XYZ offset, bool show)
        {
            var pointsUtils = new OLMP.RevitLib.MEPAC.CR2.PointUtils();

            pointsUtils.GetClearancePoints(Elem, out List<XYZ> clStartPoints, out List<XYZ> clEndPoints);

            LinesCreator linesCreator = new LinesCreator();

            return linesCreator.CreateLines(_collision, Elem, clStartPoints, clEndPoints, offset, show);
        }
    }
}
