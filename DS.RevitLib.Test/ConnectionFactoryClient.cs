using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using DS.RevitLib.Utils.Connection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DS.RevitLib.Test
{
    public class ConnectionFactoryClient
    {
        private readonly UIDocument _uidoc;
        private readonly Document _doc;
        private readonly double _minCurveLength = 100;
        private Element _element1;
        private Element _element2;
        private Element _element3;

        public ConnectionFactoryClient(UIDocument uidoc)
        {
            _uidoc = uidoc;
            _doc = uidoc.Document;
        }

        public void Run()
        {
            Reference reference = _uidoc.Selection.PickObject(ObjectType.Element, "Select element1");
            _element1 = _doc.GetElement(reference);

            reference = _uidoc.Selection.PickObject(ObjectType.Element, "Select element2");
            _element2 = _doc.GetElement(reference);

            try
            {
                reference = _uidoc.Selection.PickObject(ObjectType.Element, "Select element3");
                _element3 = reference is null ? null : _doc.GetElement(reference);
            }
            catch (Exception)
            { }

            var factory = GetFactory();
            factory.Connect();
        }

        private IConnectionFactory GetFactory()
        {
            MEPCurve mEPCurve3 = _element3 is not null && _element3 is MEPCurve ? _element3 as MEPCurve : null;
            IConnectionFactory factory = _element1 is MEPCurve && _element2 is MEPCurve ?
                new MEPCurveConnectionFactory(_doc, _element1 as MEPCurve, _element2 as MEPCurve, mEPCurve3) :
                new ElementConnectionFactory(_doc, _element1, _element2, _element3);

            return factory;
        }
    }
}
