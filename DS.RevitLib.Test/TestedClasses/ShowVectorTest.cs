using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.RevitLib.Utils;
using DS.RevitLib.Utils.Connections.PointModels;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.Geometry.Points;
using DS.RevitLib.Utils.Various.Selections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Test.TestedClasses
{
    internal class ShowVectorTest
    {
        private readonly UIDocument _uiDoc;
        private readonly Document _doc;
        private readonly PointSelector _selector;
        private readonly XYZVisualizator _visualisator;

        public ShowVectorTest(UIDocument uidoc)
        {
            _uiDoc = uidoc;
            _doc = uidoc.Document;
            _selector = new PointSelector(_uiDoc) { AllowLink = true };
            _visualisator = new XYZVisualizator(_uiDoc, 100.MMToFeet());
            Run();
            //ShowBases();
        }

        private void Run()
        {
            var element = _selector.Pick($"Укажите точку 1.");
            ConnectionPoint connectionPoint1 = new ConnectionPoint(element, _selector.Point);
            if (connectionPoint1 == null) { return; }

            element = _selector.Pick($"Укажите точку 2.");
            ConnectionPoint connectionPoint2 = new ConnectionPoint(element, _selector.Point);
            if (connectionPoint2 == null) { return; }

            var p1 = connectionPoint1.Point; var p2 = connectionPoint2.Point;


            _visualisator.ShowVector(p1, p2); 
        }

        private void ShowBases()
        {
            var element = _selector.Pick($"Укажите точку базиса.");
            ConnectionPoint connectionPoint1 = new ConnectionPoint(element, _selector.Point);
            if (connectionPoint1 == null) { return; }

            _visualisator.ShowVectorByDirection(connectionPoint1.Point, XYZ.BasisX);
            _visualisator.ShowVectorByDirection(connectionPoint1.Point, XYZ.BasisY);
            _visualisator.ShowVectorByDirection(connectionPoint1.Point, XYZ.BasisZ);
        }
    }
}
