using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils;
using Serilog;
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
    public abstract class XYZElementSelectorBase
    {
        /// <summary>
        /// 
        /// </summary>
        protected readonly UIDocument _uiDoc;

        /// <summary>
        /// 
        /// </summary>
        protected readonly Document _doc;

        /// <summary>
        /// Instansiate an object to select 
        /// (<see cref="Autodesk.Revit.DB.Element"/>,<see cref="Autodesk.Revit.DB.XYZ"/>) in <see cref="Document"/>.
        /// </summary>
        /// <param name="uiDoc"></param>
        public XYZElementSelectorBase(UIDocument uiDoc)
        {
            _uiDoc = uiDoc;
            _doc = _uiDoc.Document;
        }

        /// <summary>
        /// The core Serilog, used for writing log events.
        /// </summary>
        public ILogger Logger { get; set; }

        /// <summary>
        /// Messenger to show errors.
        /// </summary>
        public IWindowMessenger Messenger { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        protected (Element element, XYZ point) SelectElement(int index)
        {
            var selector = new ElementSelector(_uiDoc) { AllowLink = false };
            var element = selector.Pick($"Укажите элемент для получения точки присоединения {index}. " +
                "Или нажмите 'ESC', чтобы выбрать точку на элементе.");
            var centerPoint = ElementUtils.GetLocationPoint(element);
            //centerPoint.Show(_doc);
            Logger?.Information($"Selected element is: {element?.Id}.");
            return (element, centerPoint);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        protected (Element element, XYZ point) SelectPointOnElement(int index)
        {
            var selector = new PointSelector(_uiDoc) { AllowLink = false };
            var element = selector.Pick($"Укажите точку присоединения {index} на элементе.");
            //selector.Point.Show(_doc);
            Logger?.Information($"Selected element is: {element?.Id}.");
            return (element, selector.Point);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="validators"></param>
        protected void ShowResults(IEnumerable<IValidator<(Element, XYZ)>> validators)
        {
            if (Messenger == null) { return; }

            var results = new List<ValidationResult>();
            validators.ToList().ForEach(v => results.AddRange(v.ValidationResults));
            var messageBuilder = new StringBuilder();
            if (results.Count == 1)
            { messageBuilder.AppendLine(results.First().ErrorMessage); }
            else if (results.Count > 1)
                for (int i = 0; i < results.Count; i++)
                {
                    var r = results[i];
                    messageBuilder.AppendLine($"Ошибка {i + 1}. {r.ErrorMessage}");
                    messageBuilder.AppendLine("---------");
                }

            if (messageBuilder.Length > 0) { Messenger.Show(messageBuilder.ToString(), "Ошибка"); }
        }
    }
}