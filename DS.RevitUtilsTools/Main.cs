using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using System.Collections.Generic;
using System.Linq;

namespace DS.RevitUtilsTools
{
    public class Main
    {
        readonly Application App;
        readonly UIDocument Uidoc;
        readonly Document Doc;
        readonly UIApplication Uiapp;

        public Main(Application app, UIApplication uiapp, UIDocument uidoc, Document doc)
        {
            App = app;
            Uiapp = uiapp;
            Uidoc = uidoc;
            Doc = doc;
        }

        public void ExtractPoints()
        {
            ElementUtils elementUtils = new ElementUtils();
            Element element = elementUtils.GetCurrent(new PickedElement(Uidoc, Doc));

            GeneralPointExtractor generalPointExtractor = new GeneralPointExtractor(element);

            generalPointExtractor.GetGeneralPoints(out List<XYZ> points);

            VisiblePointsCreator linesCreator = new VisiblePointsCreator();
            linesCreator.Create(Doc, points);
        }
    }
}