using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.UI;
using DS.RevitLib.Utils.Solids.Models;
using DS.RevitLib.Utils.Solids;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.MEP;
using DS.RevitLib.Utils.ModelCurveUtils;
using DS.RevitLib.Utils.Models;
using DS.RevitLib.Utils.Visualisators;
using DS.RevitLib.Utils.Collisions.Resolve;
using DS.RevitLib.Utils.Collisions;
using DS.RevitLib.Utils;
using DS.RevitLib.Utils.Collisions.Checkers;
using DS.RevitLib.Utils.Collisions.Models;
using DS.RevitLib.Utils.Collisions.Resolvers;

namespace DS.RevitLib.Test
{
    internal class NewsSolidPlacerTest
    {
        readonly UIDocument Uidoc;
        readonly Document Doc;
        readonly UIApplication Uiapp;

        public NewsSolidPlacerTest(UIDocument uidoc, Document doc, UIApplication uiapp)
        {
            Uidoc = uidoc;
            Doc = doc;
            Uiapp = uiapp;
        }

        public void Run()
        {
            // Find collisions between elements and a selected element
            Reference reference = Uidoc.Selection.PickObject(ObjectType.Element, "Select operation element");
            Element operationElement = Doc.GetElement(reference);

            reference = Uidoc.Selection.PickObject(ObjectType.Element, "Select target MEPCurve");
            MEPCurve targetElement = (MEPCurve)Doc.GetElement(reference);

            XYZ point = ElementUtils.GetLocationPoint(targetElement);

            //Solid place
            var model = new SolidModelExt(operationElement);
            var solidPlacer = new SolidPlacer(model, targetElement, point);
            model = solidPlacer.Place();
            Show(model);

            //Collisions search
            var checkedObjects1 = new List<Solid>() { model.Solid };
            var checkedObjects2 = GetGeometryElements(Doc);
            var excludedObjects = new List<Element> { targetElement };
            var colChecker = new SolidCollisionChecker(checkedObjects1, checkedObjects2, excludedObjects);
            var collisions = colChecker.GetCollisions();

            if (collisions.Any())
            {
                List<SolidElemCollision> solidElemCollisions = collisions.Cast<SolidElemCollision>().ToList();
                SolidElemCollision currentCollision = solidElemCollisions.First();
                Solid intersectionSolid = currentCollision.GetIntersection() as Solid;

                BoundingBoxXYZ box = intersectionSolid.GetBoundingBox();
                IVisualisator vs = new BoundingBoxVisualisator(box, Doc);
                new Visualisator(vs);

                //resolve collision
                colChecker = new SolidCollisionChecker(new List<Solid>() { currentCollision.BaseObject }, currentCollision.CollisionObjects, excludedObjects);
                var resolver = new SolidCollisionResolver(colChecker, solidElemCollisions, solidPlacer.TransformModel, model, targetElement);
                resolver.Resolve();

            }
            else
            {
                Disconnect(operationElement);
                TransformElement(operationElement, solidPlacer.TransformModel);
            }





        }
        private void Show(SolidModelExt model)
        {
            BoundingBoxXYZ box = model.Solid.GetBoundingBox();
            IVisualisator vs = new BoundingBoxVisualisator(box, Doc);
            new Visualisator(vs);

            var lineCreator = new ModelCurveCreator(Doc);
            lineCreator.Create(model.CentralLine);

            var normLine = Line.CreateBound(model.CentralPoint, model.CentralPoint + model.MaxOrth);
            lineCreator.Create(normLine);
        }

        private List<Element> GetGeometryElements(Document doc, Transform tr = null)
        {
            if (doc == null || !doc.IsValidObject)
                return null;

            var categories = doc.Settings.Categories.Cast<Category>().Where(x => x.CategoryType == CategoryType.Model).Select(x => x.Id)
                .Where(x => !x.IntegerValue.Equals((int)BuiltInCategory.OST_Materials)).ToList();
            var filter = new ElementMulticategoryFilter(categories);
            var elems = new FilteredElementCollector(doc).WhereElementIsNotElementType()
                    .WherePasses(filter).Where(x => ContainsSolidChecker(x)).ToList();

            elems = elems.Where(x => x.Category.GetBuiltInCategory().ToString() !=
            BuiltInCategory.OST_TelephoneDevices.ToString()).ToList();

            return elems;
        }

        private bool ContainsSolidChecker(Element x)
        {
            var g = x.get_Geometry(new Options() { ComputeReferences = false, DetailLevel = ViewDetailLevel.Fine, IncludeNonVisibleObjects = false })
                ?.Cast<GeometryObject>().ToList();

            return CheckGeometry(g);
        }

        private bool CheckGeometry(List<GeometryObject> g)
        {
            if (g is null) return false;

            foreach (var elem in g)
            {
                if (elem is Solid s && s.Volume > 1e-6)
                    return true;
                else if (elem is GeometryInstance gi)
                {
                    var go = gi.GetInstanceGeometry();
                    return CheckGeometry(go?.Cast<GeometryObject>().ToList());
                }
            }
            return false;
        }


        private void CollisionsSearchOutput(List<Element> collisions)
        {
            string collisionsAccount = "Collisions count: " + collisions.Count.ToString();
            foreach (var item in collisions)
            {
                collisionsAccount += "\n" + item.Id;
            }

            TaskDialog.Show("Collisions", collisionsAccount);
        }

        private Dictionary<Element, Solid> GetIntersectionSolids(List<Element> elements, Solid solid)
        {
            var elementsIntersections = new Dictionary<Element, Solid>();

            foreach (var element in elements)
            {
                Solid elemSolid = ElementUtils.GetSolid(element);
                Solid intersectionSolid = DS.RevitLib.Utils.Solids.SolidUtils.GetIntersection(solid, elemSolid);
                elementsIntersections.Add(element, intersectionSolid);
            }

            return elementsIntersections;
        }

        private void Disconnect(Element element)
        {
            var cons = ConnectorUtils.GetConnectors(element);
            foreach (var con in cons)
            {
                var connectors = con.AllRefs;
                foreach (Connector c in connectors)
                {
                    if (con.IsConnectedTo(c))
                    {
                        ConnectorUtils.DisconnectConnectors(con, c);
                    }
                }
            }
        }


        private Element TransformElement(Element element, TransformModel transformModel)
        {
            Document Doc = element.Document;

            using (Transaction transNew = new Transaction(Doc, "MoveElement"))
            {
                try
                {
                    transNew.Start();
                    if (transformModel.MoveVector is not null)
                    {
                        ElementTransformUtils.MoveElement(Doc, element.Id, transformModel.MoveVector);
                    }
                    if (transformModel.CenterLineRotation.Angle != 0)
                    {
                        ElementTransformUtils.RotateElement(Doc, element.Id,
                                transformModel.CenterLineRotation.Axis, transformModel.CenterLineRotation.Angle);
                    }
                    if (transformModel.AroundCenterLineRotation.Angle != 0)
                    {
                        ElementTransformUtils.RotateElement(Doc, element.Id,
                   transformModel.AroundCenterLineRotation.Axis, transformModel.AroundCenterLineRotation.Angle);
                    }
                }

                catch (Exception e)
                { return null; }

                if (transNew.HasStarted())
                {
                    transNew.Commit();
                }
            }

            return element;
        }
    }
}
