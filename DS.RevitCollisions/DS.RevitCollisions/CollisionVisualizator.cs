using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils.Collisions;
using DS.RevitCollisions.Models;
using DS.RevitLib.Utils;
using DS.RevitLib.Utils.Elements;
using DS.RevitLib.Utils.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DS.RevitCollisions
{
    /// <summary>
    /// An object that used to show collisions.
    /// </summary>
    public class CollisionVisualizator : ICollisionVisualizator<Collision>
    {
        private readonly UIApplication _uiApp;
        private readonly UIDocument _uiDoc;
        private readonly Document _doc;

        public CollisionVisualizator(UIApplication uiApp)
        {
            _uiApp = uiApp;
            _uiDoc = uiApp.ActiveUIDocument;
            _doc = _uiDoc.Document;
        }

        /// <inheritdoc/>
        public void Show(Collision collision)
        {
            //Initiate zoom
            var zoomer = new Zoomer(_doc, _uiApp);

            var resolvingElem = collision.Item1;
            var stateElem = collision.Item2;

            Solid intersectionSolid = collision is not null ?
                collision.IntersectionSolid.Solid :
                GetIntersectionSolid(resolvingElem, stateElem);


            var boxXYZ = zoomer.Zoom(intersectionSolid, 5);
            //new SectionBox(DocModel.UIApplication, DocModel.TransactionFactory).Set(boxXYZ);

            //Initiate selection
            var selection = new List<ElementId>();
            if (resolvingElem.IsValidObject) selection.Add(resolvingElem.Id);
            if (stateElem.IsValidObject) selection.Add(stateElem.Id);

            var elementsSelector = new ElementsSelector(_doc, _uiDoc.Selection);
            var link = collision.Item2.GetLink(_doc);

            try
            {
                if (link is not null)
                    elementsSelector.SelectTwoElements(link, resolvingElem.Id, stateElem.Id);
                else
                {
                    _uiDoc.Selection.SetElementIds(selection);
                };
            }
            catch (Exception)
            {
                Debug.Indent();
                Debug.WriteLine($"{FailureSeverity.Warning.ToString().ToUpper()}: Unable to select items");
                Debug.Unindent();
            }
        }

        private Solid GetIntersectionSolid(Element element1, Element element2)
        {
            var stateSolids = ElementUtils.GetSolids(element1);
            var changeSolids = ElementUtils.GetSolids(element2);

            Solid unitStateSolid = DS.RevitLib.Utils.Solids.SolidUtils.UniteSolids(stateSolids);
            Solid unitChangeSolid = DS.RevitLib.Utils.Solids.SolidUtils.UniteSolids(changeSolids);

            //check solids intersection
            var intersectionSolid =
                BooleanOperationsUtils.ExecuteBooleanOperation(unitStateSolid, unitChangeSolid, BooleanOperationsType.Intersect);
            if (intersectionSolid is null || intersectionSolid.Volume == 0)
            {
                Debug.WriteLine("Выбранные элементы не пересекаются.");
                return null;
            }

            return intersectionSolid;
        }
    }
}
