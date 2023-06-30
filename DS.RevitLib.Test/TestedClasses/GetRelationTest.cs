using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using DS.ClassLib.VarUtils;
using DS.RevitLib.Utils;
using DS.ClassLib.VarUtils.Collisions;
using DS.RevitLib.Utils.MEP;
using DS.RevitLib.Utils.MEP.Models;
using DS.RevitLib.Utils.MEP.SystemTree;
using DS.RevitLib.Utils.Solids.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Test
{
    internal class GetRelationTest
    {
        private readonly Document _doc;
        private readonly TransactionBuilder _trb;
        private readonly UIDocument _uidoc;

        public GetRelationTest(UIDocument uidoc)
        {
            _uidoc = uidoc;
            _doc = uidoc.Document;
            _trb = new TransactionBuilder(_doc);
            Run();
        }

        public void Run()
        {
            Reference reference1 = _uidoc.Selection.PickObject(ObjectType.Element, "Select element1");
            Reference reference2 = _uidoc.Selection.PickObject(ObjectType.Element, "Select element2");
            var mc1 = _doc.GetElement(reference1) as MEPCurve;
            var mc2 = _doc.GetElement(reference2) as MEPCurve;
            var (parentMC, childMC) = MEPCurveUtils.GetRelation(mc1, mc2, out bool inverted);

            if(parentMC is null && childMC is null)
            {Debug.WriteLine("Relation is null"); return; }


            var selectionList = inverted ? 
                new List<ElementId>() { mc1.Id } : 
                new List<ElementId>() { mc2.Id };

            Debug.WriteLine(selectionList.First());
            _trb.Build(() => _uidoc.Selection.SetElementIds(selectionList), "show child");
        }
    }
}
