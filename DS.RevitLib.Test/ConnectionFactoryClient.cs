using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using DS.RevitLib.Utils.Connection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Test
{
    internal class ConnectionFactoryClient
    {
        private readonly UIDocument _uidoc;
        private readonly Document _doc;
        private readonly double _minCurveLength = 100;
        private Element _element1;
        private Element _element2;

        public ConnectionFactoryClient(UIDocument uidoc)
        {
            _uidoc = uidoc;
            _doc = uidoc.Document;
        }

        public void Run()
        {
            Reference reference = _uidoc.Selection.PickObject(ObjectType.Element, "Select element1");
            _element1 = _doc.GetElement(reference) as MEPCurve;

            reference = _uidoc.Selection.PickObject(ObjectType.Element, "Select element2");
            _element2 = _doc.GetElement(reference) as MEPCurve;

            var factory = GetFactory();
            factory.Connect();
        }

        private IConnectionFactory GetFactory()
        {
            IConnectionFactory factory = null;
            if (_element1 is MEPCurve && _element2 is MEPCurve)
            {
                factory =  new MEPCurveConnectionFactory(_doc, 
                    _element1 as MEPCurve, _element2 as MEPCurve, _minCurveLength);
            }

            return factory;
        }
    }
}
