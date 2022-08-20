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
using System.Windows.Forms;

namespace DS.RevitLib.Test
{
    public class SymbolPlacerClient
    {
        private readonly List<FamilyInstance> _familyInstances;
        private readonly List<MEPCurve> _targerMEPCurves;
        private readonly Document _doc;

        public SymbolPlacerClient(List<FamilyInstance> familyInstances, List<MEPCurve> targerMEPCurves)
        {
            _familyInstances = familyInstances;
            _targerMEPCurves = targerMEPCurves;
            _doc = targerMEPCurves.First().Document;
        }

        public void Run()
        {
            int i = 0;
            XYZ point = null;
            MEPCurve mEPCurve = _targerMEPCurves[i];

            foreach (var family in _familyInstances)
            {
                if (i > _targerMEPCurves.Count)
                {
                    MessageBox.Show("No available MEPCurves exist for family insatance placement.");
                    break;
                }
                FamilySymbol familySymbol = family.GetFamilySymbol();

                double familyLength = new FamilySymbolUtils().GetLength(familySymbol, _doc, family);

                point ??= new PlacementPoint(mEPCurve, familyLength).GetStartPoint(PlacementOption.Edge);
                var symbolPlacer = new SymbolPlacer(familySymbol, mEPCurve, point, familyLength, family,
                    new RollBackCommitter(), "autoMEP");
                symbolPlacer.Place();


                mEPCurve = symbolPlacer.SplittedMEPCurve;
                Connector baseConnector = symbolPlacer.BaseConnector;

                point = new PlacementPoint(mEPCurve, familyLength).GetPoint(baseConnector);

                if (point is null)
                {
                    i++;
                    mEPCurve = _targerMEPCurves[i];
                }
            }
        }
    }
}
