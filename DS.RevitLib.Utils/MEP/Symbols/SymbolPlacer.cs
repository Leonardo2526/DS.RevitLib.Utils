using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Elements.Alignments;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.MEP.Creator;
using DS.RevitLib.Utils.TransactionCommitter;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Xml.Linq;

namespace DS.RevitLib.Utils.MEP.Symbols
{
    public class SymbolPlacer
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

            var angleAlignment = new AngleAlignment(famInst, _targerMEPCurve);
            angleAlignment.Align();

            List<MEPCurve> splittedMEPCurves = GetSplittedElements(_targerMEPCurve, _targetDirection, _placementPoint, cutWidth);

            angleAlignment = new AngleAlignment(splittedMEPCurves.First(), _targerMEPCurve);
            angleAlignment.AlignNormOrths();

            angleAlignment = new AngleAlignment(splittedMEPCurves.Last(), _targerMEPCurve);
            angleAlignment.AlignNormOrths();

            //connect connectors
            Connect(famInstCon1, famInstCon2, splittedMEPCurves.First(), splittedMEPCurves.Last());

            return famInst;
        }

        private MEPCurve GetMEPCurveToSplit(MEPCurve mEPCurve1, MEPCurve mEPCurve2, XYZ pointToSplit)
        {
            var line1 = MEPCurveUtils.GetLine(mEPCurve1);
            //var l1p0 = line1.GetEndPoint(0);
            //var l1p2 = line1.GetEndPoint(1);
            var line2 = MEPCurveUtils.GetLine(mEPCurve2);
            //var l2p0 = line2.GetEndPoint(0);
            //var l2p2 = line2.GetEndPoint(1);
            var p1 = line1.Project(pointToSplit);
            var p2 = line2.Project(pointToSplit);

            if (p1.Distance==0)
            {
                return mEPCurve1;
            }

            return mEPCurve2;
        }

        private void SetConnectorParameters(FamilyInstance famInst, Dictionary<Parameter, double> parameters)
        {
            var famInstParameters = MEPElementUtils.GetSizeParameters(famInst);

            var parameterSetter = new ParameterSetter(famInst, new RollBackCommitter(), _transactionPrefix);

            foreach (var param in parameters)
            {
               var keyValuePair = famInstParameters.Where(obj => obj.Key.Id == param.Key.Id).FirstOrDefault();
                parameterSetter.SetValue(keyValuePair.Key, param.Value);
            }
        }

        private void Connect(Connector famInstCon1, Connector famInstCon2, MEPCurve mEPCurve1, MEPCurve mEPCurve2)
        {
            List<Connector> cons = new List<Connector>();
            cons.AddRange(ConnectorUtils.GetConnectors(mEPCurve1));
            cons.AddRange(ConnectorUtils.GetConnectors(mEPCurve2));

            var selectedCon = ConnectorUtils.GetClosest(famInstCon1, cons);
            ConnectorUtils.ConnectConnectors(_doc, famInstCon1, selectedCon);

            selectedCon = ConnectorUtils.GetClosest(famInstCon2, cons);
            ConnectorUtils.ConnectConnectors(_doc, famInstCon2, selectedCon);
        }

       private List<MEPCurve> GetSplittedElements(MEPCurve mEPCurve, XYZ mEPCurveDir, XYZ placementPoint, double cutWidth)
        {
            var creator = new MEPCurveCreator(mEPCurve);
            MEPCurve splittedMEPCurve1 = creator.SplitElement(placementPoint + mEPCurveDir.Multiply(cutWidth)) as MEPCurve;


            XYZ pointToSplit = placementPoint - mEPCurveDir.Multiply(cutWidth);
            MEPCurve mEPCurveToSplit = GetMEPCurveToSplit(mEPCurve, splittedMEPCurve1, pointToSplit);
            var splitCreator = new MEPCurveCreator(mEPCurveToSplit);
            MEPCurve splittedMEPCurve2 = splitCreator.SplitElement(pointToSplit) as MEPCurve;

            var mepCurves = new List<MEPCurve>()
            { mEPCurve, splittedMEPCurve1, splittedMEPCurve2};
            var orderedElements = mepCurves.Cast<Element>().ToList().Order();

            //get elements to delete
            var toDelete = orderedElements.Where(obj => obj.Id != orderedElements.First().Id && obj.Id != orderedElements.Last().Id).ToList();
            foreach (var del in toDelete)
            {
                ElementUtils.DeleteElement(_doc, del);
                orderedElements.Remove(del);
            }

            return orderedElements.Cast<MEPCurve>().ToList();
        }

    }
}
