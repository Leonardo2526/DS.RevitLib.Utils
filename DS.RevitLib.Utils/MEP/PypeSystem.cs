using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DS.RevitLib.Utils.MEP
{
    public class PypeSystem
    {
        readonly UIDocument Uidoc;
        readonly Document Doc;
        readonly UIApplication Uiapp;
        public readonly Element _element;

        public PypeSystem(UIApplication uiapp, UIDocument uidoc, Document doc, Element element)
        {
            Uiapp = uiapp;
            Uidoc = uidoc;
            Doc = doc;
            _element = element;
        }


        public static ElementId MEPTypeElementId;
        public static ElementId PipeTypeElementId;
        public static Level level;

        public void CreatePipeSystem(List<XYZ> points)
        {
            GetPipeSystemTypes();
            PipeSystemCreator pipeSystemCreator = new PipeSystemCreator();
            pipeSystemCreator.CreatePypeSystem(Doc, _element ,points);
        }

        void GetPipeSystemTypes()
        {
            //Get pipes sizes
            Pipe pipeA = _element as Pipe;

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
