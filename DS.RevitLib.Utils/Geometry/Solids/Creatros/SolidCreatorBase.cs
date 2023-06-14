using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils;
using DS.RevitLib.Utils.Transactions;
using System;

namespace DS.RevitLib.Utils.Solids
{
    /// <summary>
    /// Base class to create <see cref="Autodesk.Revit.DB.Solid"/> object.
    /// </summary>
    public abstract class SolidCreatorBase
    {
        /// <summary>
        /// Create <see cref="Autodesk.Revit.DB.Solid"/> object.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public virtual Solid CreateSolid() => throw new NotImplementedException();

        /// <summary>
        /// Create <see cref="Autodesk.Revit.DB.Solid"/> object at given <paramref name="point"/>.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public virtual Solid CreateSolid(XYZ point) => throw new NotImplementedException();

        /// <summary>
        /// Created <see cref="Autodesk.Revit.DB.Solid"/>.
        /// </summary>
        public Solid Solid { get; protected set; }

        /// <summary>
        /// Show created <see cref="Autodesk.Revit.DB.Solid"/>.
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="transactionBuilder"></param>
        public virtual void ShowSolid(Document doc, AbstractTransactionBuilder transactionBuilder = null)
        {
            transactionBuilder ??= new TransactionBuilder(doc);
            transactionBuilder.Build(() =>
            {
                Solid.ShowShape(doc);
            }, "Show Solid");
        }
    }
}
