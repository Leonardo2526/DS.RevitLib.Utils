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
        private readonly FamilySymbol _familySymbol;
        private readonly MEPCurve _targerMEPCurve;
        private readonly double _familyLength;
        private readonly XYZ _placementPoint;
        private readonly Committer _committer;
        private readonly string _transactionPrefix;
        private readonly XYZ _targetDirection;
        private readonly FamilyInstance _sourceFamInst;

        public SymbolPlacer(FamilySymbol familySymbol, MEPCurve targerMEPCurve, XYZ placementPoint, double familyLength, FamilyInstance sourceFamInst,
            Committer committer = null, string transactionPrefix = "")
        {
            _doc = targerMEPCurve.Document;
            _familySymbol = familySymbol;
            _targerMEPCurve = targerMEPCurve;
            _placementPoint = placementPoint;
            _familyLength = familyLength;
            _sourceFamInst = sourceFamInst;
            _targetDirection = MEPCurveUtils.GetDirection(targerMEPCurve);
            _committer = committer;
            _transactionPrefix = transactionPrefix;
        }

        public MEPCurve SplittedMEPCurve { get; private set; }
        public Connector BaseConnector { get; private set; }

        public FamilyInstance Place()
        {           
            var famInstCreator = new FamInstTransactions(_doc, _committer, _transactionPrefix);
            FamilyInstance famInst = famInstCreator.
                CreateFamilyInstane(_familySymbol, _placementPoint, _targerMEPCurve.ReferenceLevel, _sourceFamInst);

            var (famInstCon1, famInstCon2) = ConnectorUtils.GetMainConnectors(famInst);

            var angleAlignment = new AngleAlignment(famInst, _targerMEPCurve);
            angleAlignment.Align();
            List<MEPCurve> splittedMEPCurves = GetSplittedElements(_targerMEPCurve, _targetDirection, _placementPoint, _familyLength);

            angleAlignment = new AngleAlignment(splittedMEPCurves.First(), _targerMEPCurve);
            angleAlignment.AlignNormOrths();

            angleAlignment = new AngleAlignment(splittedMEPCurves.Last(), _targerMEPCurve);
            angleAlignment.AlignNormOrths();

            //connect connectors
            Connect(famInstCon1, famInstCon2, splittedMEPCurves.First(), splittedMEPCurves.Last());

            SplittedMEPCurve = GetMaxLengthMEPCurve(splittedMEPCurves.First(), splittedMEPCurves.Last());
            BaseConnector = ConnectorUtils.GetCommonConnectors(famInst, SplittedMEPCurve).elem2Con;

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

       private List<MEPCurve> GetSplittedElements(MEPCurve mEPCurve, XYZ mEPCurveDir, XYZ placementPoint, double familyLength)
        {
            var creator = new MEPCurveTransactions(mEPCurve);
            MEPCurve splittedMEPCurve1 = creator.SplitElementTransaction(placementPoint + mEPCurveDir.Multiply(familyLength/2)) as MEPCurve;


            XYZ pointToSplit = placementPoint - mEPCurveDir.Multiply(familyLength/2);
            MEPCurve mEPCurveToSplit = GetMEPCurveToSplit(mEPCurve, splittedMEPCurve1, pointToSplit);
            var splitCreator = new MEPCurveTransactions(mEPCurveToSplit);
            MEPCurve splittedMEPCurve2 = splitCreator.SplitElementTransaction(pointToSplit) as MEPCurve;

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

        private MEPCurve GetMaxLengthMEPCurve(MEPCurve mEPCurve1, MEPCurve mEPCurve2)
        {
            double l1 = MEPCurveUtils.GetLength(mEPCurve1);
            double l2 = MEPCurveUtils.GetLength(mEPCurve2);

            if (l1>l2)
            {
                return mEPCurve1;
            }

            return mEPCurve2;
        }
    }
}
