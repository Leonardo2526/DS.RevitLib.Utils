using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using System.Collections.Generic;
using System.Linq;
using DS.Revit.Utils;

namespace DS.Revit.Utils.GPExtractor
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
            PickedElement pickedElement = new PickedElement(Uidoc, Doc);
            Element element = pickedElement.GetElement();

            GPExtractor generalPointExtractor = new GPExtractor(element);

            generalPointExtractor.GetGeneralPoints(out List<XYZ> points);

            VisiblePointsCreator linesCreator = new VisiblePointsCreator();
            linesCreator.Create(Doc, points);
        }
    }
}