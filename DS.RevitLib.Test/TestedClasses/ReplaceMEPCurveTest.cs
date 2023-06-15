using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using DS.RevitLib.Utils;
using DS.RevitLib.Utils.MEP;
using System;
using System.Diagnostics;
using System.Linq;

namespace DS.RevitLib.Test.TestedClasses
{
    internal class ReplaceMEPCurveTest
    {
        private readonly Document _doc;
        private readonly TransactionBuilder _trb;
        private readonly UIDocument _uidoc;

        public ReplaceMEPCurveTest(UIDocument uidoc)
        {
            _uidoc = uidoc;
            _doc = uidoc.Document;
            _trb = new TransactionBuilder(_doc);
            Run();
        }

        private async void Run()
        {
            Reference reference1 = _uidoc.Selection.PickObject(ObjectType.Element, "Select mEPCurve");
            var sourceMEPCurve = _doc.GetElement(reference1) as MEPCurve;
            if (sourceMEPCurve == null) { return; }

            ElementId sourceMEPCurveId = sourceMEPCurve.Id;

            MEPCurve replacedMEPCurve = null;
            using (var trg = new TransactionGroup(_doc, "trg"))
            {
                try
                {
                    trg.Start();

                    //initiate transaction actions
                    replacedMEPCurve = await _trb.BuilAsync(() => sourceMEPCurve.Replace(), "replace");

                    //check errors
                    string errors = _trb.ErrorMessages;
                    if (errors is not null && errors.Any())
                    {
                        Debug.WriteLine($"Exception occured. TransactionGroup {trg.GetName()} was rolled.");
                        trg.RollBack();
                    }
                    else
                    {
                        trg.Commit();
                        Debug.WriteLine("Replaced!");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Exception occured. TransactionGroup {trg.GetName()} was rolled.");
                    if (trg.HasStarted())
                    { trg.RollBack(); }
                }
            }

            Debug.Assert(replacedMEPCurve is not null && replacedMEPCurve.Id != sourceMEPCurveId, "Replacing was failed");
        }
    }
}
