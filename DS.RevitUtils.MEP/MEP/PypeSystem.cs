using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DS.RevitUtils.MEP
{
    class PypeSystem
    {
        readonly UIDocument Uidoc;
        readonly Document Doc;
        readonly UIApplication Uiapp;

        public PypeSystem(UIApplication uiapp, UIDocument uidoc, Document doc)
        {
            Uiapp = uiapp;
            Uidoc = uidoc;
            Doc = doc;
        }

        public static ElementId MEPTypeElementId;
        public static ElementId PipeTypeElementId;
        public static Level level;

        public void CreatePipeSystem(List<XYZ> points)
        {
            GetPipeSystemTypes();
            PipeSystemCreator pipeSystemCreator = new PipeSystemCreator();
            pipeSystemCreator.CreatePypeSystem(Doc, points);
        }

        void GetPipeSystemTypes()
        {
            // Find collisions between elements and a selected element
            Reference reference = Uidoc.Selection.PickObject(ObjectType.Element, "Select element that will be checked for intersection with all elements");
            Element elementA = Doc.GetElement(reference);

            //Get pipes sizes
            Pipe pipeA = elementA as Pipe;

            MEPTypeElementId = pipeA.MEPSystem.GetTypeId();
            PipeTypeElementId = pipeA.GetTypeId();

            //Level
            level = new FilteredElementCollector(Doc)
                .OfClass(typeof(Level))
                .Cast<Level>()
                .FirstOrDefault();

        }
    }
}
