using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.RevitLib.Utils.Creation.Transactions;
using DS.RevitLib.Utils.Transactions;

namespace DS.RevitLib.Utils
{
    /// <summary>
    ///  An object to set sectionBox on view
    /// </summary>
    public class SectionBox
    {
        private readonly UIApplication _app;
        private readonly ITransactionFactory _transactionBuilder;
        private readonly Document _doc;

        /// <summary>
        /// Instantiate an object to set sectionBox on view.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="transactionBuilder"></param>
        public SectionBox(UIApplication app, ITransactionFactory transactionBuilder = null)
        {
            _app = app;
            _doc = _app.ActiveUIDocument.Document;
            _transactionBuilder = transactionBuilder ?? new ContextTransactionFactory(_doc);
        }

        /// <summary>
        /// Set sectionBox on ActiveView if it's <see cref="View3D"/>.
        /// </summary>
        /// <param name="boxXYZ"></param>
        public void Set(BoundingBoxXYZ boxXYZ)
        {
            if (_app.ActiveUIDocument.ActiveGraphicalView is not View3D v3d) return;

            try
            {
                _transactionBuilder.CreateAsync(() => v3d.SetSectionBox(boxXYZ), "setSectionBox");
            }
            catch (System.Exception)
            { }
        }
    }
}
