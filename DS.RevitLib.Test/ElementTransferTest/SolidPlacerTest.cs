using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using DS.RevitLib.Test.ElementTransferTest;
using DS.RevitLib.Utils;
using DS.RevitLib.Utils.Collisions.Checkers;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.MEP;
using DS.RevitLib.Utils.ModelCurveUtils;
using DS.RevitLib.Utils.Models;
using DS.RevitLib.Utils.Solids.Models;
using DS.RevitLib.Utils.Visualisators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DS.RevitLib.Test
{
    internal class SolidPlacerTest
    {
        readonly UIDocument Uidoc;
        readonly Document Doc;
        readonly UIApplication Uiapp;
        private readonly double _minFamInstLength = 50.mmToFyt2();
        private readonly double _minMEPCurveLength = 50.mmToFyt2();
        private double _minPlacementLength;


        public SolidPlacerTest(UIDocument uidoc, Document doc, UIApplication uiapp)
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
            var sorceModel = new SolidModelExt(operationElement);
            var operationModel = new SolidModelExt(operationElement);

            reference = Uidoc.Selection.PickObject(ObjectType.Element, "Select target MEPCurve");
            MEPCurve targetElement = (MEPCurve)Doc.GetElement(reference);

            double placementLength = operationModel.Length + 2 * _minMEPCurveLength;
            var (con1, con2) = ConnectorUtils.GetMainConnectors(targetElement);
            Connector baseConnector = con1;
            XYZ startPoint = baseConnector is null
                  ? new PlacementPoint(targetElement, placementLength).GetPoint(PlacementOption.Edge)
                  : new PlacementPoint(targetElement, placementLength).GetPoint(baseConnector);


            XYZ vector = (con2.Origin - con1.Origin).RoundVector().Normalize();
            XYZ endPoint = con2.Origin - vector.Multiply(placementLength / 2);
            TargetMEPCuve targetMEPCuve = new TargetMEPCuve(targetElement, startPoint, endPoint, con1, con2);


            //Collisions search
            var checkedObjects2 = GetGeometryElements(Doc);
            var excludedObjects = new List<Element> { targetElement };
            var colChecker = new SolidCollisionChecker(checkedObjects2, excludedObjects);

            TransformBuilder transformBuilder = new TransformBuilder(targetMEPCuve, operationModel, colChecker);
            transformBuilder.Build();

            var transformModel = GetTransform(sorceModel, operationModel);

            Disconnect(operationElement);
            TransformElement(operationElement, transformModel);
        }

        private void Show(SolidModelExt model)
        {
            BoundingBoxXYZ box = model.Solid.GetBoundingBox();
            IVisualisator vs = new BoundingBoxVisualisator(box, Doc);
            new Visualisator(vs);

            var lineCreator = new ModelCurveCreator(Doc);
            lineCreator.Create(model.CentralLine);

            var normLine = Line.CreateBound(model.CentralPoint, model.CentralPoint + model.MaxOrthLine.Direction);
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
                    if (transformModel.MaxOrthLineRotation.Angle != 0)
                    {
                        ElementTransformUtils.RotateElement(Doc, element.Id,
                   transformModel.MaxOrthLineRotation.Axis, transformModel.MaxOrthLineRotation.Angle);
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

        private TransformModel GetTransform(SolidModelExt sorceModel, SolidModelExt operationModel)
        {
            var transformModel = new TransformModel();

            XYZ moveVector = operationModel.CentralPoint - sorceModel.CentralPoint;
            if (!moveVector.IsZeroLength())
            {
                transformModel.MoveVector = moveVector;
            }

            //get centerline rotation model
            XYZ sourceDir = sorceModel.CentralLine.Direction;
            XYZ opDir = operationModel.CentralLine.Direction;
            if (!XYZUtils.Collinearity(sourceDir, opDir))
            {
                double angle = Math.Round(sourceDir.AngleTo(opDir), 3);

                XYZ axisDir = sourceDir.CrossProduct(opDir).RoundVector().Normalize();
                Line axis = Line.CreateBound(operationModel.CentralPoint, operationModel.CentralPoint + axisDir);

                if (!XYZUtils.BasisEqualToOrigin(sourceDir, opDir, axis.Direction))
                {
                    angle = -angle;
                }
                transformModel.CenterLineRotation = new RotationModel(axis, angle);
            }

            //get maxOrth rotation model
            sourceDir = sorceModel.MaxOrthLine.Direction;
            opDir = operationModel.MaxOrthLine.Direction;
            if (!XYZUtils.Collinearity(sourceDir, opDir))
            {
                double angle = Math.Round(opDir.AngleTo(sourceDir), 3);

                Line axis = operationModel.CentralLine;
                if (!XYZUtils.BasisEqualToOrigin(sourceDir, opDir, axis.Direction))
                {
                    angle = -angle;
                }
                transformModel.MaxOrthLineRotation = new RotationModel(axis, angle);
            }



            return transformModel;
        }


        private RotationModel GetModel(XYZ sourceDir, XYZ opDir, Line axis, SolidModelExt operationModel)
        {
            RotationModel? model = null;
            if (XYZUtils.Collinearity(sourceDir, opDir))
            {
                return (RotationModel)model;
            }

            double angle = Math.Round(sourceDir.AngleTo(opDir), 3);
            if (!XYZUtils.BasisEqualToOrigin(sourceDir, opDir, axis.Direction))
            {
                angle = -angle;
            }
            return new RotationModel(axis, angle);

        }
    }
}
