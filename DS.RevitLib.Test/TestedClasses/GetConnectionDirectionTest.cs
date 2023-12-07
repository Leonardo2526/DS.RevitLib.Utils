using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.RevitLib.Utils.Connections.PointModels;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.Geometry.Points;
using DS.RevitLib.Utils.Various.Selections;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Test.TestedClasses
{
    internal class GetConnectionDirectionTest
    {
        private readonly UIDocument _uiDoc;
        private readonly Document _doc;
        private readonly XYZVisualizator _visualisator;

        public GetConnectionDirectionTest(UIDocument uidoc)
        {
            _uiDoc = uidoc;
            _doc = uidoc.Document;
            _visualisator = new XYZVisualizator(_uiDoc, 100.MMToFeet());
            Run();
        }

        private void Run()
        {
            var selector = new PointSelector(_uiDoc) { AllowLink = false };

            var element = selector.Pick($"Укажите точку присоединения на элементе.");
            ConnectionPoint connectionPoint = new ConnectionPoint(element, selector.Point);

            if (connectionPoint == null) { return; }

            var refElement = selector.Pick($"Укажите относительную точку для получения направления.");
            var refPoint = selector.Point;
            ConnectionPoint refConnectionPoint = new ConnectionPoint(refElement, refPoint);
            if (refConnectionPoint == null) { return; }

            var dir = connectionPoint.GetDirection(refPoint, refElement, out var isManual, null, _uiDoc);
            _visualisator.ShowVectorByDirection(connectionPoint.Point, dir);

            Debug.WriteLineIf(dir is not null, "Connection direction is: " + dir);

        }
    }
}
