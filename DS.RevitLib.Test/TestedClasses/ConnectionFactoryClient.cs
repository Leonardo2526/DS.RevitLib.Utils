using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using DS.RevitLib.Utils;
using DS.RevitLib.Utils.Connection;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.MEP;
using DS.RevitLib.Utils.MEP.Creator;
using DS.RevitLib.Utils.MEP.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
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
        private MEPCurve _baseMEPCurve;

        public ConnectionFactoryClient(UIDocument uidoc)
        {
            _uidoc = uidoc;
            _doc = uidoc.Document;
            InitiateSelection();
            Run();
        }

        private void Run()
        {
            var action = () => _element1.Connect(_element2, _element3);
            new TransactionBuilder(_doc).Build(action.Invoke, "Connect");
        }

        public void Run3()
        {
            //Reference referenceMC = _uidoc.Selection.PickObject(ObjectType.Element, "Select baseMEPCurve");
            //_baseMEPCurve= _doc.GetElement(referenceMC) as MEPCurve;


            //var model = new MEPCurveGeometryModel(_baseMEPCurve);
            //var pf = model.ProfileType;

            var factory = GetFactory();
            //factory.Connect();
            new TransactionBuilder(_doc).Build(() => factory.Connect(), "Connect");
        }

        public void Run2()
        {
            Reference referenceMC = _uidoc.Selection.PickObject(ObjectType.Element, "Select baseMEPCurve");
            _baseMEPCurve = _doc.GetElement(referenceMC) as MEPCurve;

            //_baseMEPCurve.FixNotValidOrientation();

            var trb = new TransactionBuilder(_doc);
            //_baseMEPCurve.FixNotValidOrientation(trb);

            trb.Build(() =>
            {
                _baseMEPCurve.FixNotValidOrientation();
            },
            "Rotate");
        }

        private void InitiateSelection()
        {

            Reference reference = _uidoc.Selection.PickObject(ObjectType.Element, "Select element1");
            _element1 = _doc.GetElement(reference);
            _baseMEPCurve = _element1 as MEPCurve;

            reference = _uidoc.Selection.PickObject(ObjectType.Element, "Select element2");
            _element2 = _doc.GetElement(reference);

            try
            {
                reference = _uidoc.Selection.PickObject(ObjectType.Element, "Select element3");
                _element3 = reference is null ? null : _doc.GetElement(reference);
            }
            catch (Exception)
            { }
        }

        private IConnectionFactory GetFactory()
        {
            MEPCurve mEPCurve3 = _element3 is not null && _element3 is MEPCurve ? _element3 as MEPCurve : null;
            IConnectionFactory factory = _element1 is MEPCurve && _element2 is MEPCurve ?
                new MEPCurveConnectionFactory(_doc, _element1 as MEPCurve, _element2 as MEPCurve, mEPCurve3) :
                new ElementConnectionFactory(_doc, _baseMEPCurve, _element1, _element2);

            return factory;
            //return new ElementConnectionFactory(_doc, _baseMEPCurve, _element1, _element2, _element3); ;
        }
    }
}
