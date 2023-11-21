using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using DS.RevitLib.Utils.Extensions;
using iUtils.SelctionFilters;
using System.Linq;

namespace DS.RevitLib.Utils.Elements
{
    public class ElementsSelector
    {
        
        Document _doc;
        Selection _sel;

        public ElementsSelector(Document doc, Selection sel)
        {
            _doc = doc;
            _sel = sel;
        }

        public void SelectTwoElements(ElementId elem1Id, ElementId elem2Id, Selection sel)
        {
            try
            {
                sel.SetElementIds(new ElementId[] { elem1Id, elem2Id });
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
            }

        }

        /// <summary>
        /// Метод для выделения двух элементов, один из которых находится в связанной модели
        /// </summary>
        /// <param name="link">Связанная модель</param>
        /// <param name="thisDocId">Id элемента открытой модели</param>
        /// <param name="LinkDocId">Id элемента связаной модели</param>
        public void SelectTwoElements(RevitLinkInstance link, ElementId thisDocId, ElementId LinkDocId)
        {
            try
            {
                var e1 = link.GetLinkDocument().GetElement(LinkDocId);
                var ref1 = new Reference(e1);
                var e2 = _doc.GetElement(thisDocId);
                var stableRepresentation = ref1?.CreateLinkReference(link).ConvertToStableRepresentation(_doc); // здесь doc - ОСНОВНОЙ документ
                string fixedStableRepresentation = stableRepresentation.Replace(":RVTLINK", ":0:RVTLINK");
                var reference2 = Reference.ParseFromStableRepresentation(_doc, fixedStableRepresentation);

                if (e2 is not null)
                {
                    var ref2 = new Reference(e2);
                    _sel.PickObjects(ObjectType.LinkedElement, new NoSelectionFilter(), "select", new Reference[] { reference2, ref2 });
                }
                else
                {
                    _sel.PickObjects(ObjectType.LinkedElement, new NoSelectionFilter(), "select", new Reference[] { reference2 });
                }
            }

            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
            }

        }

        /// <summary>
        /// Метод для выделения одного элемента  в связанной модели
        /// </summary>
        /// <param name="elemntInLink">Элемент в связанной модели</param>
        /// <param name="sel">Объект Selection</param>
        /// <param name="doc">Докумен текущей\основной модели</param>
        public void SelectElementInLink(Element elemntInLink)
        {
            var link = elemntInLink.GetLink(_doc);
            //var link = GetLinkByElement(elemntInLink);
            try
            {
                var e1 = link.GetLinkDocument().GetElement(elemntInLink.Id);
                var ref1 = new Reference(e1);
                var stableRepresentation = ref1?.CreateLinkReference(link).ConvertToStableRepresentation(_doc); // здесь doc - ОСНОВНОЙ документ
                string fixedStableRepresentation = stableRepresentation.Replace(":RVTLINK", ":0:RVTLINK");
                var reference2 = Reference.ParseFromStableRepresentation(_doc, fixedStableRepresentation);
                _sel.PickObjects(ObjectType.LinkedElement, new NoSelectionFilter(), "select", new Reference[] { reference2 });
            }

            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
            }

        }
    }
}
