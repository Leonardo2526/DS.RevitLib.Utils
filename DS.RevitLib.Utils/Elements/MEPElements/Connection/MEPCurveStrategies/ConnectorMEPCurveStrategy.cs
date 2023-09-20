using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.MEP;
using DS.RevitLib.Utils.MEP.Creator;
using DS.RevitLib.Utils.MEP.Models;
using MoreLinq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DS.RevitLib.Utils.Connection.Strategies
{
    internal class ConnectorMEPCurveStrategy : MEPCurveConnectionStrategy
    {
        private readonly Connector _elem1Con;
        private readonly Connector _elem2Con;

        /// <summary>
        /// Initiate object to connect two neighbour connectors.
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="mEPCurve1"></param>
        /// <param name="mEPCurve2"></param>
        /// <param name="minCurveLength"></param>
        /// <param name="elem1Con"></param>
        /// <param name="elem2Con"></param>
        public ConnectorMEPCurveStrategy(Document doc,
            MEPCurveGeometryModel mEPCurve1, MEPCurveGeometryModel mEPCurve2, double minCurveLength,
            Connector elem1Con, Connector elem2Con) :
            base(doc, mEPCurve1, mEPCurve2, minCurveLength)
        {
            _elem1Con = elem1Con;
            _elem2Con = elem2Con;
        }

        public override void Connect()
        {
            if (_elem1Con.Origin.DistanceTo(_elem2Con.Origin) > 0.001)
            {
                ConnectWithCut();
                //var line = _mEPCurve1.MEPCurve.GetCenterLine();
                //_mEPCurve1.MEPCurve.Location.Rotate(line, 2 * Math.PI);
            }
            else
            {
                _elem1Con.ConnectTo(_elem2Con);
            }
        }

        public override bool IsConnectionAvailable()
        {
            throw new NotImplementedException();
        }

        private void ConnectWithCut()
        {
            var connectors1 = ConnectorUtils.GetConnectors(_mEPCurve1.MEPCurve).
                OrderBy(c => c.Origin.DistanceTo(_elem1Con.Origin)).ToList();
            var connectors2 = ConnectorUtils.GetConnectors(_mEPCurve2.MEPCurve).
                OrderBy(c => c.Origin.DistanceTo(_elem2Con.Origin)).ToList();

            var farhestCon1 = connectors1.Last();
            var farhestCon2 = connectors2.Last();          
            var line = _mEPCurve2.MEPCurve.GetCenterLine();

            var mEPSystem = new BuilderByPoints(_mEPCurve2.MEPCurve, new List<XYZ>() { farhestCon1.Origin, farhestCon2.Origin }).
                BuildMEPCurves();
            _doc.Regenerate();
            mEPSystem.RefineDucts(_mEPCurve2.MEPCurve);
            var newMEPCurve = mEPSystem.MEPCurves.FirstOrDefault();

            var elems1 = ConnectorUtils.GetConnectedElements(_mEPCurve1.MEPCurve);
            var elems2 = ConnectorUtils.GetConnectedElements(_mEPCurve2.MEPCurve);

            //get connected to spuds 
            var spuds1 = elems1.Where(e => e.IsSpud()
             && line.Project(ConnectorUtils.GetConnectors(e).FirstOrDefault().Origin).XYZPoint.OnLine(line, false));
            var spuds2 = elems2.Where(e => e.IsSpud()
            && line.Project(ConnectorUtils.GetConnectors(e).FirstOrDefault().Origin).XYZPoint.OnLine(line, false));
           

            var elemsToSpud1 = new List<Element>();
            spuds1.ForEach(s => elemsToSpud1.
            AddRange(
                ConnectorUtils.GetConnectedElements(s).
                Where(e => e.Id != _mEPCurve1.MEPCurve.Id)
                ));
            var elemsToSpud2 = new List<Element>();
            spuds2.ForEach(s => elemsToSpud2.
            AddRange(
                ConnectorUtils.GetConnectedElements(s).
                Where(e => e.Id != _mEPCurve2.MEPCurve.Id)
                ));

            //delete
            DeleteOdds(spuds1, spuds2);

            //resotre connection
            elems1 = elems1.Where(e => e.IsValidObject && !e.IsSpud()).ToList();
            elems1.AddRange(elemsToSpud1);
            elems2 = elems2.Where(e => e.IsValidObject && !e.IsSpud()).ToList();
            elems2.AddRange(elemsToSpud2);

            var elementsToResotore = elems1.Concat(elems2);
            RestoreConnection(newMEPCurve, elementsToResotore);
        }

        private void DeleteOdds(IEnumerable<Element> spuds1, IEnumerable<Element> spuds2)
        {
            spuds1.ForEach(_doc.Delete2);
            spuds2.ForEach(_doc.Delete2);

            _doc.Delete2(_mEPCurve1.MEPCurve);
            _doc.Delete2(_mEPCurve2.MEPCurve);
        }


        private void RestoreConnection(MEPCurve newMEPCurve, IEnumerable<Element> elementsToResotore)
        {
            elementsToResotore.ForEach(e => e.Connect(newMEPCurve));
        }
    }
}
