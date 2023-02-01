using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using DS.RevitLib.Utils.Connection.Strategies;
using DS.RevitLib.Utils.MEP;
using DS.RevitLib.Utils.MEP.Creator;
using DS.RevitLib.Utils.MEP.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Linq;

namespace DS.RevitLib.Utils.Connection
{
    /// <summary>
    /// An object that represents connection factory for Elements.
    /// </summary>
    public class ElementConnectionFactory : IConnectionFactory
    {
        private readonly Document _doc;
        private readonly MEPCurve _baseMEPCurve;
        private readonly Element _element1;
        private readonly Element _element2;
        private readonly Connector _elem1Con;
        private readonly Connector _elem2Con;
        private readonly XYZ _dir1;
        private readonly XYZ _dir2;

        /// <summary>
        /// Initiate factory object to connect Elements
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="baseMEPCurve"></param>
        /// <param name="element1"></param>
        /// <param name="element2"></param>
        public ElementConnectionFactory(Document doc, MEPCurve baseMEPCurve, Element element1, Element element2)
        {
            _doc = doc;
            _baseMEPCurve = baseMEPCurve;
            _element1 = element1;
            _element2 = element2;

            var cons1 = ConnectorUtils.GetConnectors(_element1);
            var cons2 = ConnectorUtils.GetConnectors(_element2);

            (_elem1Con, _elem2Con) = ConnectorUtils.GetClosest(cons1, cons2);
            _dir1 = ElementUtils.GetMainDirection(_element1);
            _dir2 = ElementUtils.GetMainDirection(_element2);
        }


        public TransactionBuilder Trb { get; set; }

        /// <inheritdoc/>
        public void Connect()
        {
            var strategy = GetStrategy(XYZUtils.Collinearity(_dir1, _dir2), 
                (_elem1Con.Origin - _elem2Con.Origin).IsZeroLength());

            if (strategy == null)
            {
                var errorMessage = "Connection error! Unable to get connection strategy.";
                Debug.WriteLine(errorMessage, TraceLevel.Error.ToString());
                return;
            }
            try
            {
                strategy.Connect();
            }
            catch (System.Exception)
            {
                var errorMessage = "Connection error! Unable to connect element.";
                Debug.WriteLine(errorMessage, TraceLevel.Error.ToString());
            }
        }
        private ElementConnectionStrategy GetStrategy(bool dirParallel, bool consCoincidense) =>
            (dirParallel, consCoincidense) switch
            {
                (true, true) => new ConnectWithMEPCurve(_doc, _elem1Con, _elem2Con, _baseMEPCurve, 
                    Trb ?? new TransactionBuilder(_doc)),
                (false, _) => new ElbowElementStrategy(_doc, _elem1Con, _elem2Con, _baseMEPCurve),
                _ => null
            };

        //private ElementConnectionStrategy GetTwoElementsStrategy()
        //{

        //    if (XYZUtils.Collinearity(_dir1, _dir2))
        //    {
        //        if ((_elem1Con.Origin - _elem2Con.Origin).IsZeroLength())
        //        {
        //            _elem1Con.ConnectTo(_elem2Con);
        //        }
        //        else
        //        {
        //            return new ConnectWithMEPCurve(_doc, _elem1Con, _elem2Con, _baseMEPCurve, Trb ?? new TransactionBuilder(_doc));
        //        }
        //    }
        //    else
        //    {
        //        return new ElbowElementStrategy(_doc, _elem1Con, _elem2Con, _baseMEPCurve);
        //    }

        //    return null;
        //}


    }
}
