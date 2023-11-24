using Autodesk.Revit.DB;
using DS.GraphUtils.Entities;
using DS.RevitLib.Utils.Various.Selections;
using DS.RevitLib.Utils.Various;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils.Collisions;
using DS.ClassLib.VarUtils;
using DS.RevitLib.Utils.Collisions.Detectors.AbstractDetectors;
using DS.RevitLib.Utils.MEP.SystemTree;
using QuickGraph;
using DS.RevitLib.Utils.Extensions;
using Rhino.Geometry;
using DS.RevitLib.Utils.Connections.PointModels;
using System.ComponentModel.DataAnnotations;
using Serilog;
using System.Data;

namespace DS.RevitLib.Utils.Graphs
{
    public class PairVertexPointer : IVertexPointer
    {
        private readonly UIDocument _uiDoc;
        private readonly Document _doc;

        public PairVertexPointer(UIDocument uiDoc)
        {
            _uiDoc = uiDoc;
            _doc = _uiDoc.Document;
        }

        /// <summary>
        /// Validators to include to next only specified veritces.
        /// </summary>
        public IEnumerable<IValidator<IVertex>> Validators { get; set; } = new List<IValidator<IVertex>>();

        /// <summary>
        /// Messenger to show errors.
        /// </summary>
        public IWindowMessenger Messenger { get; set; }


        /// <summary>
        /// The core Serilog, used for writing log events.
        /// </summary>
        public ILogger Logger { get; set; }

        /// <inheritdoc/>
        public IVertex Point(string pointMessage = null)
        {
            XYZ point;
            Element element;
            try
            {
                (element, point) = CenterPoint(pointMessage);
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                (element, point) = PickPoint(pointMessage);
            }           

            if (element == null) { return null; }

            int.TryParse(pointMessage, out int id);
            IVertex vertex = CreateVertex(id, point, element);

            return IsValid(vertex) ? vertex : null;

            static IVertex CreateVertex(int id, XYZ point, Element element)
                => element is FamilyInstance ?
                    new TaggedGVertex<int>(id, element.Id.IntegerValue) :
                    new TaggedGVertex<Point3d>(id, point.ToPoint3d());
        }

        public (Element element, XYZ point) CenterPoint(string pointId)
        {
            var selector = new ElementSelector(_uiDoc) { AllowLink = false };
            var element = selector.Pick($"Укажите элемент для получения точки присоединения {pointId}. " +
                "Или нажмите 'ESC', чтобы выбрать точку на элементе.");
            var centerPoint = ElementUtils.GetLocationPoint(element);
            //centerPoint.Show(_doc);
            Logger?.Information($"Selected element is: {element?.Id}.");
            return (element, centerPoint);
        }

        public (Element element, XYZ point) PickPoint(string pointId)
        {
            var selector = new PointSelector(_uiDoc) { AllowLink = false };
            var element = selector.Pick($"Укажите точку присоединения {pointId} на элементе.");
            //selector.Point.Show(_doc);
            Logger?.Information($"Selected element is: {element?.Id}.");
            return (element, selector.Point);
        }

        private bool IsValid(IVertex vertex)
        {
            var b = Validators.ToList().TrueForAll(v => v.IsValid(vertex));
            if (Messenger != null) { ShowResults(Validators); }
            return b;

            void ShowResults(IEnumerable<IValidator<IVertex>> validators)
            {
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
}
