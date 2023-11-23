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
    public class VertexPairVisualizator : IItemVisualisator<(IVertex, IVertex)>
    {
        private readonly Document _doc;
        private readonly UIDocument _uiDoc;

        public VertexPairVisualizator(UIDocument uiDoc)
        {
            _uiDoc = uiDoc;
            _doc = uiDoc.Document;
        }

        public double LabelSize { get; set; }
        public ITransactionFactory TransactionFactory { get; set; }

        public bool RefreshView { get; set; }

        /// <inheritdoc/>
        public void Show((IVertex, IVertex) vertexPair)
        {
            if (TransactionFactory is null)
            {
                ShowPair(vertexPair);
            }
            else
            { TransactionFactory.CreateAsync(() => ShowPair(vertexPair), "showVertexPair"); }

            if (RefreshView) { _uiDoc.RefreshActiveView(); }
        }

        /// <inheritdoc/>
        public async Task ShowAsync((IVertex, IVertex) item)
        {
            await TransactionFactory?.CreateAsync(() => ShowPair(item), "showVertexPair");
            if (RefreshView) { _uiDoc.RefreshActiveView(); }
        }


        private void ShowPair((IVertex, IVertex) vertexPair)
        {
            XYZ xYZPoint1 = vertexPair.Item1.GetLocation(_doc);
            XYZ xYZPoint2 = vertexPair.Item2.GetLocation(_doc);

            xYZPoint1?.Show(_doc, LabelSize);
            xYZPoint2?.Show(_doc, LabelSize);
        }
    }
}
