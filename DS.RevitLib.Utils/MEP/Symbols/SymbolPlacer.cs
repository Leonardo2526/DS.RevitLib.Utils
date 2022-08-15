using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.MEP.Creator;
using DS.RevitLib.Utils.TransactionCommitter;
using System.Collections.Generic;
using System.Linq;

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
        private readonly XYZ _targetDirection;

        public SymbolPlacer(SymbolModel symbolModel, MEPCurve targerMEPCurve, XYZ placementPoint,
            Committer committer = null, string transactionPrefix = "")
        {
            _doc = targerMEPCurve.Document;
            _symbolModel = symbolModel;
            _targerMEPCurve = targerMEPCurve;
            _targetDirection = MEPCurveUtils.GetDirection(targerMEPCurve);
            _placementPoint = placementPoint;
            _committer = committer;
            _transactionPrefix = transactionPrefix;
        }

        public FamilyInstance Place()
        {
            var famInstCreator = new FamInstCreator(_doc, _committer, _transactionPrefix);
            FamilyInstance famInst = famInstCreator.CreateFamilyInstane(_symbolModel.FamilySymbol, _placementPoint, _targerMEPCurve.ReferenceLevel);

            //here should set parameters to connectors!
            SetConnectorParameters(famInst, _symbolModel.Parameters);

            var (famInstCon1, famInstCon2) = ConnectorUtils.GetMainConnectors(famInst);
            double cutWidth = famInstCon1.Origin.DistanceTo(famInstCon2.Origin) / 2;

            //Set rotation
            //....

            var creator = new MEPCurveCreator(_targerMEPCurve);
            MEPCurve splittedMEPCurve1 = creator.SplitElement(_placementPoint + _targetDirection.Multiply(cutWidth)) as MEPCurve;
            MEPCurve splittedMEPCurve2 = creator.SplitElement(_placementPoint - _targetDirection.Multiply(cutWidth)) as MEPCurve;

            var mepCurves = new List<MEPCurve>()
            { _targerMEPCurve, splittedMEPCurve1, splittedMEPCurve2};
            var orderedElements = mepCurves.Cast<Element>().ToList().Order();


            //get elements to delete
            var toDelete = orderedElements.Where(obj => obj.Id != orderedElements.First().Id && obj.Id != orderedElements.Last().Id).ToList();
            foreach (var del in toDelete)
            {
                ElementUtils.DeleteElement(_doc, del);
            }

            //connect connectors
            Connect(famInstCon1, famInstCon2, orderedElements.First() as MEPCurve, orderedElements.Last() as MEPCurve);

            return famInst;
        }


        private void SetConnectorParameters(FamilyInstance famInst, Dictionary<Parameter, double> parameters)
        {
            var parameterSetter = new ParameterSetter(famInst, new RollBackCommitter(), _transactionPrefix);

            foreach (var param in parameters)
            {
                parameterSetter.SetValue(param.Key, param.Value);
            }
        }

        private void Connect(Connector famInstCon1, Connector famInstCon2, MEPCurve mEPCurve1, MEPCurve mEPCurve2)
        {
            var cons = ConnectorUtils.GetConnectors(mEPCurve1);
            var selectedCon = ConnectorUtils.GetClosest(famInstCon1, cons);
            ConnectorUtils.ConnectConnectors(_doc, famInstCon1, selectedCon);

            cons = ConnectorUtils.GetConnectors(mEPCurve2);
            selectedCon = ConnectorUtils.GetClosest(famInstCon2, cons);
            ConnectorUtils.ConnectConnectors(_doc, famInstCon2, selectedCon);
        }

    }
}
