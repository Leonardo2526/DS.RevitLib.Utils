using Autodesk.Revit.DB.DirectContext3D;
using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils;
using DS.GraphUtils.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.Creation.Transactions;
using Autodesk.Revit.UI;

namespace DS.RevitLib.Utils.Graphs
{
    public class ElementPointPairVisualizator : IItemVisualisator<((Element, XYZ), (Element, XYZ))>
    {
        private readonly Document _doc;
        private readonly UIDocument _uiDoc;

        public ElementPointPairVisualizator(UIDocument uiDoc)
        {
            _uiDoc = uiDoc;
            _doc = uiDoc.Document;
        }

        public double LabelSize { get; set; }
        public ITransactionFactory TransactionFactory { get; set; }

        public bool RefreshView { get; set; }

        /// <inheritdoc/>
        public void Show(((Element, XYZ), (Element, XYZ)) xYZPair)
        {
            if (TransactionFactory is null)
            {
                ShowPair(xYZPair);
            }
            else
            { TransactionFactory.CreateAsync(() => ShowPair(xYZPair), "showXYZPair"); }

            if (RefreshView) { _uiDoc.RefreshActiveView(); }
        }

        /// <inheritdoc/>
        public async Task ShowAsync(((Element, XYZ), (Element, XYZ)) item)
        {
            await TransactionFactory?.CreateAsync(() => ShowPair(item), "showXYZPair");
            if (RefreshView) { _uiDoc.RefreshActiveView(); }
        }


        private void ShowPair(((Element, XYZ), (Element, XYZ)) vertexPair)
        {
            XYZ xYZPoint1 = vertexPair.Item1.Item2;
            XYZ xYZPoint2 = vertexPair.Item2.Item2;

            xYZPoint1?.Show(_doc, LabelSize);
            xYZPoint2?.Show(_doc, LabelSize);
        }
    }
}
