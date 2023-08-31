﻿using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.MEP;
using DS.RevitLib.Utils.ModelCurveUtils;
using DS.RevitLib.Utils.PathCreators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Test
{
    internal class SimplePathFinderTest
    {
        private readonly UIDocument _uidoc;
        private readonly Document _doc;

        public SimplePathFinderTest(UIDocument uidoc, Document doc)
        {
            _uidoc = uidoc;
            _doc = doc;
        }

        public List<XYZ> RunTest1()
        {
            Reference reference = _uidoc.Selection.PickObject(ObjectType.Element, "Select element1");
            var mc1 = _doc.GetElement(reference) as MEPCurve;
            var (con1, con2) = ConnectorUtils.GetMainConnectors(mc1);

            //var finder = new PathCreator().Create(mc1.GetCenterLine(), null, 500.mmToFyt2(), 500.mmToFyt2(), 45);
            //return finder.CreateAsync(con1.Origin, con2.Origin).Result;
            return null;
        }

        public List<XYZ> RunTest2()
        {
            Reference reference = _uidoc.Selection.PickObject(ObjectType.Element, "Select element1");
            var mc1 = _doc.GetElement(reference) as MEPCurve;
            var (con11, con12) = ConnectorUtils.GetMainConnectors(mc1);

            reference = _uidoc.Selection.PickObject(ObjectType.Element, "Select element2");
            var mc2 = _doc.GetElement(reference) as MEPCurve;
            var (con21, con22) = ConnectorUtils.GetMainConnectors(mc2);
            double midDistPoints = 500.mmToFyt2();
            //var finder = new PathCreator().Create(mc1.GetCenterLine(), mc2.GetCenterLine(), midDistPoints, midDistPoints * 5, 90, midDistPoints * 3);
            //return finder.CreateAsync(con11.Origin, con21.Origin).Result;
            return null;
        }

        public void ShowPath(List<XYZ> path)
        {
            using (var tr = new Transaction(_doc,"show"))
            {
                tr.Start();

                var mcreator = new ModelCurveCreator(_doc);
                for (int i = 0; i < path.Count - 1; i++)
                {
                    mcreator.Create(path[i], path[i + 1]);
                    var line = Line.CreateBound(path[i], path[i + 1]);
                }

                tr.Commit();
            }
        }
    }
}
