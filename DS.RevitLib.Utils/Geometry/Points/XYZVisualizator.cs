using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils;
using DS.RevitLib.Utils.Creation.Transactions;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.ModelCurveUtils;
using DS.RevitLib.Utils.Visualisators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.Geometry.Points
{
    public class XYZVisualizator : IPointVisualisator<XYZ>
    {
        private readonly UIDocument _uiDoc;
        private readonly Document _doc;
        private readonly bool _refresh;
        private readonly double _labelSize;
        private readonly ITransactionFactory _transactionFactory;

        public XYZVisualizator(UIDocument uiDoc, double labelSize = 0, ITransactionFactory transactionBuilder = null, bool refresh = false) 
        {            
            _uiDoc = uiDoc;
            _doc = uiDoc.Document;
            _refresh = refresh;
            _labelSize = labelSize == 0 ? 100.MMToFeet() : labelSize;
            _transactionFactory ??= new ContextTransactionFactory(_doc);
        }


        public void Show(XYZ point)
        {
            XYZ xYZ = (XYZ)point;
            if(xYZ == null ) { throw new ArgumentException(); }

            Line line1 = Line.CreateBound(
                xYZ + XYZ.BasisX.Multiply(_labelSize / 2),
                xYZ - XYZ.BasisX.Multiply(_labelSize / 2));

            Line line2 = Line.CreateBound(
               xYZ + XYZ.BasisY.Multiply(_labelSize / 2),
               xYZ - XYZ.BasisY.Multiply(_labelSize / 2));

            Line line3 = Line.CreateBound(
               xYZ + XYZ.BasisZ.Multiply(_labelSize / 2),
               xYZ - XYZ.BasisZ.Multiply(_labelSize / 2));

            _transactionFactory.CreateAsync(() =>
            {
                var creator = new ModelCurveCreator(_doc);
                creator.Create(line1);
                creator.Create(line2);
                creator.Create(line3);
                if (_refresh) { _doc.Regenerate(); _uiDoc.RefreshActiveView(); }
            }, "ShowPoint");
        }
    }
}
