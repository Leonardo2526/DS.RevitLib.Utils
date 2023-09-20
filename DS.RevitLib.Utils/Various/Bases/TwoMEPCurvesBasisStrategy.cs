using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.Various;
using System;

namespace DS.RevitLib.Utils.Bases
{
    /// <summary>
    /// Get basis vectors by selecting two elements in model.
    /// </summary>
    public class TwoMEPCurvesBasisStrategy : IBasisStrategy
    {
        private UIDocument _uiDoc;
        private Document _doc;


        /// <summary>
        /// Instansiate an object to get basis vectors by selecting two elements in model.
        /// </summary>
        /// <param name="uidoc"></param>
        public IBasisStrategy Build(UIDocument uidoc)
        {
            _uiDoc = uidoc;
            _doc = _uiDoc.Document;

            return this;
        }

        /// <summary>
        /// Selected <see cref="MEPCurve"/> 1.
        /// </summary>
        public MEPCurve MEPCurve1 { get; set; }

        /// <summary>
        /// Selected <see cref="MEPCurve"/> 2.
        /// </summary>
        public MEPCurve MEPCurve2 { get; set; }

        /// <inheritdoc/>
        public XYZ BasisX { get; private set; }

        /// <inheritdoc/>
        public XYZ BasisY { get; private set; }

        /// <inheritdoc/>
        public XYZ BasisZ { get; private set; }

        /// <inheritdoc/>
        public XYZ Point { get; private set; }

        /// <inheritdoc/>
        public (XYZ basisX, XYZ basisY, XYZ basisZ) GetBasis()
        {
            try
            {
                MEPCurve1 ??= new MEPCurveSelector(_uiDoc) { AllowLink = false }.Pick("Выберите элемент 1 для получения базиса.");
                MEPCurve2 ??= new MEPCurveSelector(_uiDoc) { AllowLink = true }.Pick("Выберите элемент 2 для получения базиса.");
            }
            catch (OperationCanceledException)
            { return (null, null, null); }

            if (MEPCurve1.Id == MEPCurve2.Id)
            {
                TaskDialog.Show("Ошибка", "Необходимо указать разные элементы.");
                return (null, null, null);
            }

            //get basis
            var line1 = MEPCurve1.GetCenterLine();
            var line2 = MEPCurve2.GetCenterLine();
            BasisX = line1.Direction;
            BasisZ = BasisX.CrossProduct(line2.Direction);
            BasisZ = BasisZ.IsZeroLength() ? BasisX.GetPerpendicular() : BasisZ;
            BasisY = BasisZ.CrossProduct(BasisX);

            return (BasisX, BasisY, BasisZ);
        }

        /// <inheritdoc/>
        public (XYZ basisX, XYZ basisY, XYZ basisZ) SetBasis(XYZ basisX, XYZ basisY, XYZ basisZ)
        {
           return (BasisX = basisX, BasisY = basisY, BasisZ = basisZ);
        }
    }
}
