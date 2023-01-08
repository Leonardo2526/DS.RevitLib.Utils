using Autodesk.Revit.DB;
using DS.RevitLib.Utils;
using System.Collections.Generic;
using Ivanov.RevitLib.Utils;
using System.Linq;
using DS.RevitLib.Utils.GPExtractor;
using OLMP.RevitLib.MEPAC.DocumentModels;
using DS.RevitLib.Utils.ModelCurveUtils;


namespace OLMP.RevitLib.MEPAC.Collisons.Resolvers.MEPOffset.Lines
{
    class LinesCreator
    {
        ModelCurveCreator _modelCurveCreator = new ModelCurveCreator(DocModel.Doc);

        public List<Line> CreateLines(MEPCollision collision, Element Elem, List<XYZ> glStartPoints, List<XYZ> glEndPoints, XYZ offset, bool show)
        {

            List<Line> generalLines = new List<Line>();

            if (offset == null)
                offset = new XYZ();

            int j;
            for (j = 0; j < glStartPoints.Count; j++)
            {
                XYZ glPoint1 = new XYZ(glStartPoints[j].X + offset.X, glStartPoints[j].Y + offset.Y, glStartPoints[j].Z + offset.Z);
                XYZ glPoint2 = new XYZ(glEndPoints[j].X + offset.X, glEndPoints[j].Y + offset.Y, glEndPoints[j].Z + offset.Z);

                Line generalLine;

                if (collision.ResolvingMEPCurveModel.ProfileType == ConnectorProfileType.Rectangular && j < glStartPoints.Count - 1)
                {
                    generalLine = Line.CreateBound(glStartPoints[j] + offset, glEndPoints[j + 1] + offset);
                    generalLines.Add(generalLine);

                    if (show)
                        _modelCurveCreator.Create(glStartPoints[j] + offset, glEndPoints[j + 1] + offset);

                }
                else if (collision.ResolvingMEPCurveModel.ProfileType != ConnectorProfileType.Rectangular)
                {
                    generalLine = Line.CreateBound(glPoint1, glPoint2);
                    generalLines.Add(generalLine);

                    if (show)
                        _modelCurveCreator.Create(glPoint1, glPoint2);
                }

            }

            return generalLines;
        }

        public List<Line> CreateLinesByStaticPoint(MEPCollision collision, Element Elem,
            List<XYZ> glStaticPoints, List<XYZ> glMovablePoints, XYZ offset, XYZ staticPoint, bool show)
        {
            var pointUtils = new OLMP.RevitLib.MEPAC.CR2.PointUtils();

            ElementUtils.GetPoints(Elem, out XYZ startPoint, out XYZ endPoint, out XYZ centerPointElement);

            List<Line> generalLines = new List<Line>();

            if (offset == null)
                offset = new XYZ();

            int j;
            for (j = 0; j < glStaticPoints.Count; j++)
            {
                XYZ glPoint1 = glStaticPoints[j];
                XYZ glPoint2 = new XYZ(glMovablePoints[j].X + offset.X, glMovablePoints[j].Y + offset.Y, glMovablePoints[j].Z + offset.Z);

                Line generalLine;

                if (collision.ResolvingMEPCurveModel.ProfileType == ConnectorProfileType.Rectangular && j < glStaticPoints.Count - 1)
                {
                    generalLine = Line.CreateBound(glStaticPoints[j], glMovablePoints[j + 1] + offset);
                    generalLines.Add(generalLine);

                    if (show)
                        _modelCurveCreator.Create(glStaticPoints[j], glMovablePoints[j + 1] + offset);

                }
                else if (collision.ResolvingMEPCurveModel.ProfileType != ConnectorProfileType.Rectangular)
                {
                    generalLine = Line.CreateBound(glPoint1, glPoint2);
                    generalLines.Add(generalLine);

                    if (show)
                        _modelCurveCreator.Create(glPoint1, glPoint2);
                }

            }

            return generalLines;
        }
    }
}
