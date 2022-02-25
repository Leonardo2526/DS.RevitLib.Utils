using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using iUtils.Comparable;
using iUtils.SelctionFilters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Constants.Utils
{
    
    public class ElementsSelection
    {
        private Document _mainDoc;
        private RevitLinkInstance _link;
        private IEnumerable<Element> _mainDocElements;
        private IEnumerable<Element> _linkDocElements;
        private UIDocument _uiDoc;
        private Selection _sel;
        private UIApplication _uiApp;

        public ElementsSelection(Document mainDoc, RevitLinkInstance link, IEnumerable<Element> mainDocElements, IEnumerable<Element> linkDocElements)
        {
            _mainDoc = mainDoc;
            _link = link;
            _mainDocElements = mainDocElements;
            _linkDocElements = linkDocElements;
            _uiDoc = new UIDocument(mainDoc);
            _sel = _uiDoc.Selection;
            _uiApp = _uiDoc.Application;
        }

        /// <summary>
        /// Выделяет и зуммирует текущий вид по BoundingBoxXYZ, который охватывает все элементы с учетом Transform от RevitLinkInstance
        /// </summary>
        public void SelectAndZoomElements()
        {
            var box = GetBoundingBoxToZoomFromElements(_link, _mainDocElements, _linkDocElements);
            SelectElements();
            ZoomToBoundingBoxXYZ(box);
        }
        /// <summary>
        /// Зумирует текущий вид по заданному BoundingBoxXYZ
        /// </summary>
        /// <param name="origin">тока для зуммирования</param>
        /// <param name="offset">смещение в футах</param>
        public void ZoomToBoundingBoxXYZ(BoundingBoxXYZ box)
        {
            UIView uiview = null;
            var actview = _mainDoc.ActiveView;
            IList<UIView> uiviews = _uiApp.ActiveUIDocument.GetOpenUIViews();
            foreach (UIView uv in uiviews)
            {
                if (uv.ViewId.Equals(actview.Id))
                {
                    uiview = uv;
                    break;
                }
            }
            
            uiview.ZoomAndCenterRectangle(box.Min, box.Max);
            using (var tr = new Transaction(_mainDoc))
            {
                tr.Start("ResizeViewMode");
                if (actview is View3D v3d)
                    v3d.SetSectionBox(box);
                tr.Commit();
            }

        }
        /// <summary>
        /// Зумирует текущий вид с заданным смещением во все стороны от точки 
        /// </summary>
        /// <param name="origin">тока для зуммирования</param>
        /// <param name="offset">смещение в футах</param>
        public void ZoomToPointWhithOffset(XYZ origin, double offset)
        {
            UIView uiview = null;
            var actview = _mainDoc.ActiveView; 
            IList<UIView> uiviews = _uiApp.ActiveUIDocument.GetOpenUIViews();
            foreach (UIView uv in uiviews)
            {
                if (uv.ViewId.Equals(actview.Id))
                {
                    uiview = uv;
                    break;
                }
            }
            var box = new BoundingBoxXYZ
            {
                Min = new XYZ(origin.X - offset, origin.Y - offset, origin.Z - offset),
                Max = new XYZ(origin.X + offset, origin.Y + offset, origin.Z + offset)
            };
            uiview.ZoomAndCenterRectangle(box.Min, box.Max);
        }
        /// <summary>
        /// Выделяет элементы, переданные в конструкторе, на текущем виде
        /// </summary>
        public void SelectElements()
        {
            if (null != _link & null!= _linkDocElements)
            {
                var elemIds = _linkDocElements?.Select(x => new ElementId(x.Id.IntegerValue));
                var linkReferences = GetReferencesFromLinkedDocument(_mainDoc, elemIds, _link);
                var mainReferences = _mainDocElements.Select(x => new Reference(x));
                linkReferences?.ToList().AddRange(mainReferences);
                _sel.PickObjects(ObjectType.LinkedElement, new NoSelectionFilter(), "Select linkedElements", linkReferences?.ToList());
            }
            else
            {
                var elemIDs = new List<ElementId>();
                _mainDocElements.ToList().ForEach(x => elemIDs.Add(x.Id));
                _sel.SetElementIds(elemIDs);
            }

           
        }
        /// <summary> 
        /// Получает  Reference элемента из связанного документа, который пригоден для выделения на текущем виде //https://adn-cis.org/forum/index.php?topic=2757.0
        /// </summary>
        /// <param name="thisdoc"></param>
        /// <param name="eid"></param>
        /// <param name="link"></param>
        /// <returns></returns>
        private Reference GetReferenceFromLinkedDocument(Document thisdoc, ElementId eid, RevitLinkInstance link)
        {
            var element = link?.GetLinkDocument().GetElement(eid);
            var referens = new Reference(element);
            var stableRepresentation = referens.CreateLinkReference(link).ConvertToStableRepresentation(thisdoc);
            string fixedStableRepresentation = stableRepresentation.Replace(":RVTLINK", ":0:RVTLINK");
            var result = Reference.ParseFromStableRepresentation(thisdoc, fixedStableRepresentation);
            return result;
        }
        /// <summary>
        /// Получает список Reference элементов из связанного документа, которые пригодны для выделения на текущем виде
        /// </summary>
        /// <param name="thisDoc"></param>
        /// <param name="elemIds"></param>
        /// <param name="link"></param>
        /// <returns></returns>
        private IEnumerable<Reference> GetReferencesFromLinkedDocument(Document thisDoc, IEnumerable<ElementId> elemIds, RevitLinkInstance link)
        {
            var result = new List<Reference>();
            elemIds?.ToList().ForEach(x =>result.Add(GetReferenceFromLinkedDocument(thisDoc, x, link)));
            return result;
        }
        /// <summary>
        /// Получает BoundingBoxXYZ ограничивающий все элементы с учетом Transform RevitLinkInstance
        /// </summary>
        /// <param name="link"></param>
        /// <param name="mainDocElements"></param>
        /// <param name="linkDocElements"></param>
        /// <returns></returns>
        private BoundingBoxXYZ GetBoundingBoxToZoomFromElements(RevitLinkInstance link, IEnumerable<Element> mainDocElements, IEnumerable<Element> linkDocElements)
        {
            var transform = link?.GetTotalTransform();
            var minPoints = new List<XYZComparable>();
            var maxPoints = new List<XYZComparable>();
            mainDocElements?.ToList().ForEach(x => minPoints.Add(new XYZComparable(x.get_BoundingBox(null)?.Min)));
            linkDocElements?.ToList().ForEach(x => minPoints.Add(new XYZComparable(transform?.OfPoint(x.get_BoundingBox(null)?.Min))));
            mainDocElements?.ToList().ForEach(x => maxPoints.Add(new XYZComparable(x.get_BoundingBox(null)?.Max)));
            linkDocElements?.ToList().ForEach(x => maxPoints.Add(new XYZComparable(transform?.OfPoint(x.get_BoundingBox(null)?.Max))));
            var min = minPoints.Min().Point;
            var max = maxPoints.Max().Point;
            var result = new BoundingBoxXYZ
            {
                Min = min,
                Max = max
            };
            return result; 
        }
    }
   
}
