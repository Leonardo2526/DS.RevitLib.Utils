using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.RevitLib.Utils.Various;
using System.Collections.Generic;

namespace DS.RevitLib.Utils.GPExtractor
{
    public class GPCreator
    {
        readonly Application App;
        readonly UIDocument Uidoc;
        readonly Document Doc;
        readonly UIApplication Uiapp;

        public GPCreator(Application app, UIApplication uiapp, UIDocument uidoc, Document doc)
        {
            App = app;
            Uiapp = uiapp;
            Uidoc = uidoc;
            Doc = doc;
        }

        public void Create()
        {
            Element element = new ElementSelector(Uidoc).Pick();

            List<Solid> solids = ElementUtils.GetSolids(element);
            List<XYZ> points = GPExtractor.GetGeneralPoints(solids);

            VisiblePointsCreator linesCreator = new VisiblePointsCreator();
            linesCreator.Create(Doc, points);
        }
    }
}