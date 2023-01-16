﻿using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
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
            PickedElement pickedElement = new PickedElement(Uidoc, Doc);
            Element element = pickedElement.GetElement();

            List<Solid> solids = ElementUtils.GetSolids(element);
            List<XYZ> points = GPExtractor.GetGeneralPoints(solids);

            VisiblePointsCreator linesCreator = new VisiblePointsCreator();
            linesCreator.Create(Doc, points);
        }
    }
}