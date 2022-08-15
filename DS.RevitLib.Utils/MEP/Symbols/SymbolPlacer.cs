using Autodesk.Revit.DB;
using DS.RevitLib.Utils.MEP.Creator;
using DS.RevitLib.Utils.TransactionCommitter;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.MEP.Symbols
{
    internal class SymbolPlacer
    {
        private readonly Document _doc;
        private readonly SymbolModel _symbolModel;
        private readonly MEPCurve _targerMEPCurve;
        private readonly XYZ _placementPoint;
        private readonly Committer _committer;
        private readonly string _transactionPrefix;

        public SymbolPlacer(SymbolModel model, MEPCurve targerMEPCurve, XYZ placementPoint, 
            Committer committer = null, string transactionPrefix = "")
        {
            _doc = targerMEPCurve.Document;
            _symbolModel = model;
            _targerMEPCurve = targerMEPCurve;
            _placementPoint = placementPoint;
            _committer = committer;
            _transactionPrefix = transactionPrefix;
        }

        public FamilyInstance Place()
        {
            var famInstCreator = new FamInstCreator(_doc, _committer, _transactionPrefix);
            FamilyInstance famInst = famInstCreator.CreateFamilyInstane(_symbolModel.FamilySymbol, _placementPoint, _targerMEPCurve.ReferenceLevel);
        }
    }
}
