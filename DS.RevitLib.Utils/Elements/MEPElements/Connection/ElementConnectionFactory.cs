using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Connection.Strategies;
using DS.RevitLib.Utils.MEP;
using System;
using System.Diagnostics;

namespace DS.RevitLib.Utils.Connection
{
    public class ElementConnectionFactory : IConnectionFactory
    {
        private readonly Document _doc;
        private readonly Element _element1;
        private readonly Element _element2;
        private readonly Element _element3;

        /// <summary>
        /// Initiate factory object to connect Elements
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="element1"></param>
        /// <param name="element2"></param>
        /// <param name="element3">Second parent element for tee. Optional parameter.</param>
        public ElementConnectionFactory(Document doc, Element element1, Element element2, Element element3 = null)
        {
            _doc = doc;
            _element1 = element1;
            _element2 = element2;
            _element3 = element3;
        }

        /// <inheritdoc/>
        public void Connect()
        {
            var strategy = GetStrategy();
            if (strategy == null)
            {
                var errorMessage = "Connection error! Unable to get connection strategy.";
                Debug.WriteLine(errorMessage, TraceLevel.Error.ToString());
                return;
                //throw new ArgumentNullException(errorMessage);
            }

            try
            {
                strategy.Connect();
            }
            catch (System.Exception)
            {
                var errorMessage = "Connection error! Unable to connect element.";
                Debug.WriteLine(errorMessage, TraceLevel.Error.ToString());
                //throw new Exception(errorMessage);
            }
        }

        private ElementConnectionStrategy GetStrategy()
        {
            var cons1 = ConnectorUtils.GetConnectors(_element1);
            var cons2 = ConnectorUtils.GetConnectors(_element2);

            var (commonCon1, commonCon2) = ConnectorUtils.GetNeighbourConnectors(cons1, cons2);
            var dir1 = ElementUtils.GetMainDirection(_element1);
            var dir2 = ElementUtils.GetMainDirection(_element2);

            if (commonCon1 is not null && commonCon2 is not null)
            {
                return new ConnectorElementStrategy(_doc, commonCon1, commonCon2);
            }
            return null;
            //return _element3 is null ? 
            //    new ElbowElementStrategy(_doc, cons1, cons2) : 
            //    new TeeElementStrategy(_doc, cons1, cons2, ConnectorUtils.GetConnectors(_element3));
        }
    }
}
