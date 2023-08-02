using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using DS.RevitLib.Utils.Connections.PointModels;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.MEP;
using DS.RevitLib.Utils.Various.Selections;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DS.RevitLib.Test.TestedClasses
{
    internal class ConnectedElementFinderTest
    {
        private readonly UIDocument _uiDoc;
        private readonly Document _doc;

        public ConnectedElementFinderTest(UIDocument uidoc)
        {
            _uiDoc = uidoc;
            _doc = uidoc.Document;
            Run();
        }

        public void Run()
        {
            Reference reference = _uiDoc.Selection.PickObject(ObjectType.Element, "Select element");
            var element = _doc.GetElement(reference);

            Reference referenceToFind = _uiDoc.Selection.PickObject(ObjectType.Element, "Select element to find");
            var elementToFind = _doc.GetElement(referenceToFind);

            //var selector = new PointSelector(_uiDoc) { AllowLink = false };
            //var element = selector.Pick($"Укажите точку присоединения 1 на элементе.");
            //ConnectionPoint connectionPoint = new ConnectionPoint(element, selector.Point);

            Connector freeCon = ConnectorUtils.GetFreeConnector(element).FirstOrDefault();
            Connector connector = ConnectorUtils.GetConnectors(element).
                //FirstOrDefault();
                FirstOrDefault(c => c.Origin.DistanceTo(freeCon.Origin) > 0.001);
            //FirstOrDefault(c => c.Origin.RoundVector(3) != freeCon.Origin.RoundVector(3));
            //Where(c => c.Origin.RoundVector(3) == selector.Point.RoundVector(3)).

            //var foundElement = connector.Find(elementToFind);
            //var ids = new List<ElementId>();
            //if(foundElement != null) { ids.Add(foundElement.Id); }

            var elements = connector.Find();
            var ids = elements.Select(c => c.Id).ToList();

            _uiDoc.Selection.SetElementIds(ids);
            Debug.WriteLine("Selected elements count: " + ids.Count);
        }
    }
}
