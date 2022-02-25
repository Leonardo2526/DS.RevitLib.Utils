using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DS.RVT.PipeTest
{
    class DSPipe
    {
        readonly UIDocument Uidoc;
        readonly Document Doc;
        readonly UIApplication Uiapp;

        public DSPipe(UIApplication uiapp, UIDocument uidoc, Document doc)
        {
            Uiapp = uiapp;
            Uidoc = uidoc;
            Doc = doc;
        }


        MEPSystemType mepSystemType;
        PipeType pipeType;
        Level level;

        ElementId MEPTypeElementId;
        ElementId PipeTypeElementId;

        public void CreatePipeSystem()
        {
            GetPipeSystemTypes();
            CreateTransaction();
        }

        public void DeleteElement()
        {
            // Find collisions between elements and a selected element
            Reference reference = Uidoc.Selection.PickObject(ObjectType.Element, "Select element that will be checked for intersection with all elements");
            Element elementA = Doc.GetElement(reference);

            ICollection<ElementId> selectedIds = new List<ElementId>
            {
                elementA.Id
            };

            using (Transaction transNew = new Transaction(Doc, "newTransaction"))
            {
                try
                {
                    transNew.Start();
                    Doc.Delete(selectedIds);
                }

                catch (Exception e)
                {
                    transNew.RollBack();
                    TaskDialog.Show("Revit", e.ToString());
                }
                transNew.Commit();
            }


        }


       public void SplitElement()
        {
            // Find collisions between elements and a selected element
            Reference reference = Uidoc.Selection.PickObject(ObjectType.Element, "Select element that will be checked for intersection with all elements");
            Element elementA = Doc.GetElement(reference);

            //Get pipes sizes
            Pipe pipeA = elementA as Pipe;
            GetPoints(elementA, out XYZ startPoint, out XYZ endPoint, out XYZ centerPoint);

            using (Transaction transNew = new Transaction(Doc, "newTransaction"))
            {
                try
                {
                    transNew.Start();
                    ElementId newPipeId = PlumbingUtils.BreakCurve(Doc, pipeA.Id, centerPoint);
                    Element newPipe = Doc.GetElement(newPipeId);
                }

                catch (Exception e)
                {
                    transNew.RollBack();
                    TaskDialog.Show("Revit", e.ToString());
                }
                transNew.Commit();
            }
        }


        void GetPipeSystemTypes()
        {
            // Find collisions between elements and a selected element
            Reference reference = Uidoc.Selection.PickObject(ObjectType.Element, "Select element that will be checked for intersection with all elements");
            Element elementA = Doc.GetElement(reference);

            //Get pipes sizes
            Pipe pipeA = elementA as Pipe;

            MEPTypeElementId = pipeA.MEPSystem.GetTypeId();
            PipeTypeElementId = pipeA.GetTypeId();          

            //Level
            level = new FilteredElementCollector(Doc)
                .OfClass(typeof(Level))
                .Cast<Level>()
                .FirstOrDefault();

        }

        void CreateTransaction()
        {
            using (Transaction transNew = new Transaction(Doc, "newTransaction"))
            {
                try
                {
                    transNew.Start();
                    CreatePipe();
                }

                catch (Exception e)
                {
                    transNew.RollBack();
                    TaskDialog.Show("Revit", e.ToString());
                }
                transNew.Commit();
            }
        }


        void CreatePipe()
        {
            double pipeLength = 1000;
            double pipeLengthF = UnitUtils.Convert(pipeLength / 1000,
                                        DisplayUnitType.DUT_METERS,
                                        DisplayUnitType.DUT_DECIMAL_FEET);

            // create pipe between 2 points
            XYZ p1 = new XYZ(0, 0, 0);
            XYZ p2 = new XYZ(pipeLengthF, 0, 0);

            Pipe pipe = Pipe.Create(Doc, MEPTypeElementId, PipeTypeElementId, level.Id, p1, p2);
            ChangePipeSize(pipe);
           


            List<Connector> connectors = GetConnectors(pipe);
            var connectorList = connectors.OrderBy(x => x.Origin.X).ToList();

            List<XYZ> connectorlocations = GetConnectorsXYZ(pipe);
            var connectorLocationsList = connectorlocations.OrderBy(x => x.X).ToList();

            XYZ p3 = new XYZ(connectorLocationsList[1].X, connectorLocationsList[1].Y, connectorLocationsList[1].Z);
            XYZ p4 = new XYZ(connectorLocationsList[1].X + pipeLengthF, connectorLocationsList[1].Y + pipeLengthF, connectorLocationsList[1].Z + pipeLengthF);

            Pipe pipe1 = null;
            pipe1 = Pipe.Create(Doc, MEPTypeElementId, PipeTypeElementId, level.Id, p3, p4);
            ChangePipeSize(pipe1);

            List<Connector> connectors1 = GetConnectors(pipe1);
            var connector1List = connectors1.OrderBy(x => x.Origin.Y).ToList();

            Doc.Create.NewElbowFitting(connectorList[1], connector1List[0]);

        }

        public List<Connector> GetConnectors(Element element)
        {
            //1. Cast Element to FamilyInstance
            FamilyInstance inst = element as FamilyInstance;

            //2. Get MEPModel Property
            MEPModel mepModel = inst.MEPModel;

            //3. Get connector set of MEPModel
            ConnectorSet connectorSet = mepModel.ConnectorManager.Connectors;

            //4. Initialise empty list of connectors
            List<Connector> connectorList = new List<Connector>();

            //5. Loop through connector set and add to list
            foreach (Connector connector in connectorSet)
            {
                connectorList.Add(connector);
            }
            return connectorList;
        }

        public List<XYZ> GetConnectorsXYZ(MEPCurve mepCurve)
        {
            ConnectorSet connectorSet = mepCurve.ConnectorManager.Connectors;
            List<XYZ> connectorPointList = new List<XYZ>();
            foreach (Connector connector in connectorSet)
            {
                XYZ connectorPoint = connector.Origin;
                connectorPointList.Add(connectorPoint);
            }

            return connectorPointList;
        }

        public List<Connector> GetConnectors(MEPCurve mepCurve)
        {
            //1. Get connector set of MEPCurve
            ConnectorSet connectorSet = mepCurve.ConnectorManager.Connectors;

            //2. Initialise empty list of connectors
            List<Connector> connectorList = new List<Connector>();

            //3. Loop through connector set and add to list
            foreach (Connector connector in connectorSet)
            {
                connectorList.Add(connector);
            }
            return connectorList;
        }

        public void CreateModelLine(XYZ startPoint, XYZ endPoint)
        {
            Line geomLine = Line.CreateBound(startPoint, endPoint);

            // Create a geometry plane in Revit application
            XYZ p1 = startPoint;
            XYZ p2 = endPoint;
            XYZ p3 = p2 + XYZ.BasisZ;
            Plane geomPlane = Plane.CreateByThreePoints(p1, p2, p3);

            using (Transaction transNew = new Transaction(Doc, "CreateModelLine"))
            {
                try
                {
                    transNew.Start();

                    // Create a sketch plane in current document
                    SketchPlane sketch = SketchPlane.Create(Doc, geomPlane);

                    // Create a ModelLine element using the created geometry line and sketch plane
                    ModelLine line = Doc.Create.NewModelCurve(geomLine, sketch) as ModelLine;
                }

                catch (Exception e)
                {
                    transNew.RollBack();
                    TaskDialog.Show("Revit", e.ToString());
                }

                transNew.Commit();
            }
        }

        void ChangePipeSize(Pipe pipe)
        {
            Parameter parameter = pipe.get_Parameter(BuiltInParameter.RBS_PIPE_DIAMETER_PARAM);

            double diam = 100;
            double diamF = UnitUtils.Convert(diam / 1000,
                                        DisplayUnitType.DUT_METERS,
                                        DisplayUnitType.DUT_DECIMAL_FEET);
            parameter.Set(diamF);

            // Regenerate the docucment before trying to read a parameter that has been edited
            pipe.Document.Regenerate();
        }

        void GetPoints(Element element, out XYZ startPoint, out XYZ endPoint, out XYZ centerPoint)
        {
            //get the current location           
            LocationCurve lc = element.Location as LocationCurve;
            Curve c = lc.Curve;
            c.GetEndPoint(0);
            c.GetEndPoint(1);

            startPoint = c.GetEndPoint(0);
            endPoint = c.GetEndPoint(1);
            centerPoint = new XYZ((startPoint.X + endPoint.X) / 2,
                (startPoint.Y + endPoint.Y) / 2,
                (startPoint.Z + endPoint.Z) / 2);

        }

    }
}
