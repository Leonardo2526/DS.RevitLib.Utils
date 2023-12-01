using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace DS.RevitLib.Utils.Various.Selections
{
    /// <summary>
    /// The base class to select 
    /// (<see cref="Autodesk.Revit.DB.Element"/>,<see cref="Autodesk.Revit.DB.XYZ"/>) in <see cref="Document"/>.
    /// </summary>
    public class XYZElementSelectors
    {
        private readonly UIDocument _uiDoc;
        private readonly Document _doc;
        private int _index1;
        private int _index2;
        private ElementSelector _selector1;
        private PointSelector _selector2;

        /// <summary>
        /// Instansiate an object to select 
        /// (<see cref="Autodesk.Revit.DB.Element"/>,<see cref="Autodesk.Revit.DB.XYZ"/>) in <see cref="Document"/>.
        /// </summary>
        /// <param name="uiDoc"></param>
        public XYZElementSelectors(UIDocument uiDoc)
        {
            _uiDoc = uiDoc;
            _doc = _uiDoc.Document;
        }

        /// <summary>
        /// Specify if it's allowed to select <see cref="RevitLinkInstance"/>.
        /// </summary>
        public bool AllowLink { get; set; } = false;

        /// <summary>
        /// The core Serilog, used for writing log events.
        /// </summary>
        public ILogger Logger { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public (Element element, XYZ point) SelectElement()
        {
            _index1++;
            _index1 = Math.Max(_index1, _index2);
            _selector1 ??= new ElementSelector(_uiDoc) { AllowLink = AllowLink };
            var element = _selector1.Pick($"Укажите элемент для получения точки присоединения {_index1}. " +
                "Или нажмите 'ESC', чтобы выбрать точку на элементе.");
            var centerPoint = ElementUtils.GetLocationPoint(element);
            //centerPoint.Show(_doc);
            Logger?.Information($"Selected element is: {element?.Id}.");
            return (element, centerPoint);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public (Element element, XYZ point) SelectPointOnElement()
        {
            _index2++;
            _index2 = Math.Max(_index1, _index2);
            _selector2 ??= new PointSelector(_uiDoc) { AllowLink = AllowLink };
            var element = _selector2.Pick($"Укажите точку присоединения {_index2} на элементе.");
            //selector.Point.Show(_doc);
            Logger?.Information($"Selected element is: {element?.Id}.");
            return (element, _selector2.Point);
        }
    }
}