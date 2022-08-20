using Autodesk.Revit.DB;
using DS.RevitLib.Utils.MEP.Symbols;
using DS.RevitLib.Utils.MEP;
using DS.RevitLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.TransactionCommitter;
using System.Security.Cryptography;

namespace DS.RevitLib.Test
{
    internal class SymbolPlacerTest
    {
        private readonly List<FamilyInstance> _familyInstances;
        private readonly MEPCurve _targerMEPCurve;
        private readonly Document _doc;

        public SymbolPlacerTest(List<FamilyInstance> familyInstances, MEPCurve targerMEPCurve)
        {
            _familyInstances = familyInstances;
            _targerMEPCurve = targerMEPCurve;
            _doc = targerMEPCurve.Document;
        }

        public void Run()
        {
            XYZ dir = MEPCurveUtils.GetDirection(_targerMEPCurve);
            MEPCurve mEPCurve = _targerMEPCurve;
            XYZ point = null;

            foreach (var family in _familyInstances)
            {
                var parameters = MEPElementUtils.GetSizeParameters(family);

                FamilySymbol familySymbol = family.GetFamilySymbol();
                SymbolModel symbolModel = new SymbolModel(familySymbol, parameters);

                FamilySymbolUtils familySymbolUtils = new FamilySymbolUtils();
                double familyLength = familySymbolUtils.GetLength(familySymbol, _doc, family);

                if (point is null)
                {
                    point = new PlacementPoint(mEPCurve, familyLength).GetStartPoint(PlacementOption.Edge);
                }
                SymbolPlacer symbolPlacer = new SymbolPlacer(symbolModel, mEPCurve, point, familyLength, family,
                    new RollBackCommitter(), "autoMEP");
                symbolPlacer.Place();


                mEPCurve = symbolPlacer.SplittedMEPCurve;
                Connector baseConnector = symbolPlacer.BaseConnector;

                point = new PlacementPoint(mEPCurve, familyLength).GetPoint(baseConnector);
            }
        }     
    }
}
