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

namespace DS.RevitLib.Utils.Graphs
{
    public class VertexPairVisualizator : IItemVisualisator<(IVertex, IVertex)>
    {
        private readonly Document _doc;

        public VertexPairVisualizator(Document doc)
        {
            _doc = doc;
        }

        public double LabelSize { get; set; }
        public ITransactionFactory TransactionFactory  { get; set; }

        public void Show((IVertex, IVertex) vertexPair)
        {
            XYZ xYZPoint1 = vertexPair.Item1.GetLocation(_doc);
            XYZ xYZPoint2 = vertexPair.Item2.GetLocation(_doc);
            if (TransactionFactory is null) 
            { 
                xYZPoint1?.Show(_doc, LabelSize); 
                xYZPoint2?.Show(_doc, LabelSize); 
            }
            else 
            { 
                xYZPoint1?.Show(_doc, LabelSize, TransactionFactory); 
                xYZPoint2?.Show(_doc, LabelSize, TransactionFactory); 
            }
        }
    }
}
