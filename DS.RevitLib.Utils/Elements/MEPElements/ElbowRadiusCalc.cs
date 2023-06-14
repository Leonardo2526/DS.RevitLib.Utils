using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.MEP.Creator;
using DS.RevitLib.Utils.MEP.Models;
using DS.RevitLib.Utils.Transactions;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.MEP
{
    /// <summary>
    /// An object to get elbow radius
    /// </summary>
    public class ElbowRadiusCalc
    {
        private MEPCurveModel _mEPCurveModel;
        private readonly TransactionBuilder _transactionBuilder;
        private MEPCurve _MEPCurve;
        private readonly Document _doc;

        /// <summary>
        /// Instantiate an object get elbow radius of <paramref name="mEPCurve"/>.
        /// </summary>
        /// <param name="mEPCurve"></param>
        /// <param name="transactionBuilder"></param>
        public ElbowRadiusCalc(MEPCurveModel mEPCurve, TransactionBuilder transactionBuilder = null)
        {
            _mEPCurveModel = mEPCurve;
            _MEPCurve = mEPCurve.MEPCurve;
            _doc = _MEPCurve.Document;
            _transactionBuilder = transactionBuilder ?? new TransactionBuilder(_doc);
        }

        /// <summary>
        /// Get radius by given <paramref name="angle"/>.
        /// </summary>
        /// <param name="angle">Elbow angle</param>
        /// <returns></returns>
        public async Task<double> GetRadius(double angle)
        {
            FamilySymbol elbow = GetFamilySymbol() as FamilySymbol;
            if (elbow == null) { return 0; }

            ElementId id = elbow.GetTypeId();
            FamilySymbol familySymbol = _doc.GetElement(id) as FamilySymbol;

            double getElbowLength()
            {
                FamilyInstance elbowInst = FamInstCreator.Create(elbow, new XYZ(0, 0, 0), _MEPCurve.ReferenceLevel);
                CopyConnectorsParameters(elbowInst, angle);
                _doc.Regenerate();
                return GetLength(elbowInst);
            }

            return await _transactionBuilder.BuilAsync(getElbowLength, "GetRadius_CreateElbow.", false);
        }

        private Element GetFamilySymbol()
        {
            Document doc = _MEPCurve.Document;
            var elementType = _MEPCurve.GetElementType2();
            MEPCurveType mEPCurveType = elementType as MEPCurveType;
            CableTrayType cableTrayType = elementType as CableTrayType;
            if (cableTrayType != null)
            {
                var mEPPartId = cableTrayType.Elbow.Id;
                return doc.GetElement(mEPPartId) as Element;
            }
            else
            {
                RoutingPreferenceManager rpm = mEPCurveType.RoutingPreferenceManager;
                if (rpm.GetNumberOfRules(RoutingPreferenceRuleGroupType.Elbows) == 0)
                {
                    Debug.WriteLine("Не загружено семейство отводов для данной системы.");
                    return null;
                }

                var rule = rpm.GetRule(RoutingPreferenceRuleGroupType.Elbows, 0);

                return doc.GetElement(rule.MEPPartId) as Element;
            }

        }

        private double GetLength(FamilyInstance elbowInst)
        {
            var connectors = ConnectorUtils.GetConnectorsXYZ(elbowInst);
            XYZ elbowCenter = MEPElementUtils.GetElbowCenterPoint(elbowInst);

            return connectors.First().DistanceTo(elbowCenter);
        }

        private void CopyConnectorsParameters(FamilyInstance elbowInst, double angle)
        {
            Parameter a = MEPElementUtils.GetAssociatedParameter(elbowInst, BuiltInParameter.CONNECTOR_ANGLE);
            a.Set(angle);

            switch (_mEPCurveModel.ProfileType)
            {
                case ConnectorProfileType.Invalid:
                    break;
                case ConnectorProfileType.Round:
                    {
                        Parameter d = MEPElementUtils.GetAssociatedParameter(elbowInst, BuiltInParameter.CONNECTOR_DIAMETER);
                        Parameter r = MEPElementUtils.GetAssociatedParameter(elbowInst, BuiltInParameter.CONNECTOR_RADIUS);
                        bool set = d is not null ? d.Set(_mEPCurveModel.MEPCurve.Diameter) : r.Set(_mEPCurveModel.MEPCurve.Diameter / 2);
                    }
                    break;
                case ConnectorProfileType.Rectangular:
                    {
                        Parameter height = MEPElementUtils.GetAssociatedParameter(elbowInst, BuiltInParameter.CONNECTOR_HEIGHT);
                        Parameter width = MEPElementUtils.GetAssociatedParameter(elbowInst, BuiltInParameter.CONNECTOR_WIDTH);
                        (double size1, double size2) = MEPCurveUtils.GetWidthHeight(_MEPCurve);
                        double maxSize = Math.Max(size1, size2);
                        double minSize = Math.Min(size1, size2);
                        width.Set(maxSize);
                        height.Set(minSize);
                    }
                    break;
                case ConnectorProfileType.Oval:
                    break;
                default:
                    break;
            }
        }
    }
}
