using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.RevitLib.Utils.Connections.PointModels.PointModels;
using DS.RevitLib.Utils.Connections.PointModels;
using DS.RevitLib.Utils.MEP.SystemTree;
using DS.RevitLib.Utils.Various.Selections;
using DS.RevitLib.Utils.Various;
using DS.RevitLib.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RVT.ModelSpaceFragmentation
{ 
    internal class PointCreator 
    {
        private readonly UIDocument _uiDoc;

        public PointCreator(UIDocument uiDoc)
        {
            _uiDoc = uiDoc;
        }

        public IConnectionPoint GetPoint(int pointId)
        {
            XYZ point = null;
            Element element = null;

            (element, point) = PickPoint(pointId);
            //try
            //{
            //    (element, point) = CenterPoint(pointId);
            //}
            //catch (OperationCanceledException)
            //{ (element, point) = PickPoint(pointId); }

            var connectionPoint = new ConnectionPoint(element, point);

            return connectionPoint;
        }
        public (Element element, XYZ point) CenterPoint(int pointId)
        {
            var selector = new ElementSelector(_uiDoc) { AllowLink = false };
            var element = selector.Pick($"Укажите элемент для получения точки присоединения {pointId}. " +
                "Или нажмите 'ESC', чтобы выбрать точку на элементе.");
            var centerPoint = ElementUtils.GetLocationPoint(element);
            //centerPoint.Show(_doc);
            Debug.WriteLine($"Selected element is: {element.Id}.");
            return (element, centerPoint);
        }

        public (Element element, XYZ point) PickPoint(int pointId)
        {
            var selector = new PointSelector(_uiDoc) { AllowLink = false };
            var element = selector.Pick($"Укажите точку присоединения {pointId} на элементе.");
            //selector.Point.Show(_doc);
            Debug.WriteLine($"Selected element is: {element.Id}.");
            return (element, selector.Point);
        }

    }
}
