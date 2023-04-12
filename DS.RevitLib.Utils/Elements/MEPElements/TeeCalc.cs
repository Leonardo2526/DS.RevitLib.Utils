using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.MEP.Creator;
using DS.RevitLib.Utils.MEP.Models;
using DS.RevitLib.Utils.TransactionCommitter;
using DS.RevitLib.Utils.Transactions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DS.RevitLib.Utils.MEP
{
    /// <summary>
    /// An object to get tee sizes
    /// </summary>
    public class TeeCalc
    {
        private readonly MEPCurveModel _parentMEPCurveModel;
        private readonly MEPCurveModel _childMEPCurveModel;
        private readonly AbstractTransactionBuilder _transactionBuilder;
        private readonly MEPCurve _parentMEPCurve;
        private readonly MEPCurve _childMEPCurve;
        private readonly Document _doc;

        private double _lenth;
        private double _height;

        /// <summary>
        /// Instantiate an object get tee sizes of <paramref name="parentMEPCurveModel"/>.
        /// </summary>
        /// <param name="parentMEPCurveModel"></param>
        /// <param name="childMEPCurveModel"></param>
        /// <param name="transactionBuilder"></param>
        public TeeCalc(MEPCurveModel parentMEPCurveModel, MEPCurveModel childMEPCurveModel = null, AbstractTransactionBuilder transactionBuilder = null)
        {
            _parentMEPCurveModel = parentMEPCurveModel;
            _childMEPCurveModel = childMEPCurveModel;
            _parentMEPCurve = parentMEPCurveModel.MEPCurve;
            _childMEPCurve = childMEPCurveModel.MEPCurve;
            _doc = _parentMEPCurve.Document;
            _transactionBuilder = transactionBuilder ?? new TransactionBuilder(_doc, new BaseCommitter());
            GetSizes();
        }

        /// <summary>
        /// Tee length. Specifies distance between main connectors.
        /// </summary>
        public double Length => _lenth;

        /// <summary>
        /// Tee height. 
        /// </summary>
        public double Heigth => _height;


        /// <summary>
        /// Get tee sizes.
        /// </summary>
        /// <returns></returns>
        public void GetSizes()
        {
            FamilySymbol familySymbol = GetFamilySymbol() as FamilySymbol;
            if (familySymbol == null) { return; }

            FamilyInstance junctionInst = null;

            _transactionBuilder.Build(() =>
            {
                junctionInst = FamInstCreator.Create(familySymbol, new XYZ(0, 0, 0), _parentMEPCurve.ReferenceLevel);
                CopyConnectorsParameters(junctionInst);
                _doc.Regenerate();
                _lenth = GetLength(junctionInst);
                _height = GetHeight(junctionInst);
            }, "GetSizes", false);

        }

        private double GetHeight(FamilyInstance instance)
        {
            var (con1, con2) = ConnectorUtils.GetMainConnectors(instance);
            var line = Line.CreateBound(con1.Origin, con2.Origin);
            var connectors = ConnectorUtils.GetConnectors(instance);

            var childConnector = connectors.FirstOrDefault(c =>
                       c.Origin.DistanceTo(con1.Origin) > 0 &&
                       c.Origin.DistanceTo(con2.Origin) > 0);
            var projPoint = line.Project(childConnector.Origin).XYZPoint;

            return childConnector.Origin.DistanceTo(projPoint);
        }

        private Element GetFamilySymbol()
        {
            Document doc = _parentMEPCurve.Document;
            var elementType = _parentMEPCurve.GetElementType2();
            MEPCurveType mEPCurveType = elementType as MEPCurveType;

            RoutingPreferenceManager rpm = mEPCurveType.RoutingPreferenceManager;
            if (rpm.PreferredJunctionType == PreferredJunctionType.Tap) { return null; }

            CableTrayType cableTrayType = elementType as CableTrayType;
            if (cableTrayType != null)
            {
                var mEPPartId = cableTrayType.Tee.Id;
                return doc.GetElement(mEPPartId) as Element;
            }
            else
            {
                var rulesCount = rpm.GetNumberOfRules(RoutingPreferenceRuleGroupType.Junctions);
                if (rulesCount == 0)
                {
                    Debug.WriteLine("Не загружено семейство соединителей для данной системы.");
                    return null;
                }
                var rule = rpm.GetRule(RoutingPreferenceRuleGroupType.Junctions, 0);
                return rule is null ? null : doc.GetElement(rule.MEPPartId) as Element; ;
            }

        }

        private double GetLength(FamilyInstance instance)
        {
            var (con1, con2) = ConnectorUtils.GetMainConnectors(instance);

            return con1.Origin.DistanceTo(con2.Origin);
        }

        private void CopyConnectorsParameters(FamilyInstance junctionInst)
        {
            switch (_parentMEPCurveModel.ProfileType)
            {
                case ConnectorProfileType.Invalid:
                    break;
                case ConnectorProfileType.Round:
                    {
                        Parameter dParam = MEPElementUtils.GetAssociatedParameter(junctionInst, BuiltInParameter.CONNECTOR_DIAMETER);
                        Parameter rParam = MEPElementUtils.GetAssociatedParameter(junctionInst, BuiltInParameter.CONNECTOR_RADIUS);
                        if (dParam is not null) { dParam.Set(_parentMEPCurve.Diameter); }
                        else { rParam.Set(_parentMEPCurve.Diameter / 2); }
                    }
                    break;
                case ConnectorProfileType.Rectangular:
                    {
                        var (con1, con2) = ConnectorUtils.GetMainConnectors(junctionInst);
                        var parentConnectors = new List<Connector>() { con1, con2 };
                        var connectors = ConnectorUtils.GetConnectors(junctionInst);

                        var childConnector = connectors.FirstOrDefault(c =>
                        c.Origin.DistanceTo(con1.Origin) > 0 &&
                        c.Origin.DistanceTo(con2.Origin) > 0);
                        SetConnectorParameter(junctionInst, childConnector, _childMEPCurveModel);

                        foreach (var con in parentConnectors)
                        {
                            SetConnectorParameter(junctionInst, con, _parentMEPCurveModel);
                        }

                    }
                    break;
                case ConnectorProfileType.Oval:
                    break;
                default:
                    break;
            }

        }

        private void SetConnectorParameter(FamilyInstance instance, Connector con, MEPCurveModel mEPCurveModel)
        {
            Parameter heightParam = MEPElementUtils.GetAssociatedParameter(instance, con, BuiltInParameter.CONNECTOR_HEIGHT);
            Parameter widthParam = MEPElementUtils.GetAssociatedParameter(instance, con, BuiltInParameter.CONNECTOR_WIDTH);

            double maxSize = Math.Max(mEPCurveModel.Width, mEPCurveModel.Height);
            double minSize = Math.Min(mEPCurveModel.Width, mEPCurveModel.Height);
            widthParam.Set(mEPCurveModel.Width);
            heightParam.Set(mEPCurveModel.Height);
        }
    }
}
