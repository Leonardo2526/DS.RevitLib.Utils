using Autodesk.Revit.DB;
using DS.RevitLib.Utils;
using System.Collections.Generic;
using Ivanov.RevitLib.Utils;
using System.Linq;
using DS.RevitLib.Utils.GPExtractor;
using OLMP.RevitLib.MEPAC.DocumentModels;
using DS.RevitLib.Utils.ModelCurveUtils;
using OLMP.RevitLib.MEPAC.Collisons.Resolvers.MEPOffset.Lines;
using OLMP.RevitLib.MEPAC.Collisons.CollisionModels;
using OLMP.RevitLib.MEPAC.Collisons;

namespace OLMP.RevitLib.MEPAC.CR2
{
    class AllLinesCreator : BaseMEPCollision
    {
        readonly XYZ Offset;

        public AllLinesCreator(MEPCollision collision, XYZ offset) : base(collision)
        {
            Offset = offset;
        }

        public List<Line> CreateAllElementLines(Element element, XYZ moveVector, bool show = false)
        {
            List<Line> allLines = new List<Line>();
            
            Line centerline = DS.RevitLib.Utils.LineUtils.CreateCenterLine(element, Offset, show);             
            List<Line> generalLines = new ElementGeneralLines(_collision, element).CreateGeneralLines(Offset, show);
            List<Line> clearenceLines = new ElementClearanceLines(_collision, element).CreateClearenceLines(Offset, show);

            allLines.Add(centerline);
            allLines.AddRange(generalLines);
            allLines.AddRange(clearenceLines);

            return allLines;
        }

        public List<Line> CreateAllReducibleLines(Element element, XYZ point, XYZ moveVector, bool show = false)
        {
            List<Line> allLines = new List<Line>();

            var generalLines = new ElementGeneralLines(_collision, element).CreateGeneralLinesByStaticPoint(Offset, point, show);

            var centerline = new ElementCenterLine(element).CreateCenterLineByStaticPoint(Offset, point, show);

            allLines.Add(centerline);
            allLines.AddRange(generalLines);
            //allLines.AddRange(clearenceLines);

            return allLines;
        }
    }
}
