using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace DS.RVT.ModelSpaceFragmentation.Lines
{
    class CurvesByPointsCreator : ICurves
    {
        public List<XYZ> Points;

        public CurvesByPointsCreator(List<XYZ> points)
        {
            Points = points;
        }

        public void Create()
        {
            int i;
            for (i = 0; i < Points.Count - 1; i++)
            {
                TransactionCreator transactionCreator = new TransactionCreator(Main.Doc);
                transactionCreator.Create(new ModelCurveTransaction(Points[i], Points[i + 1]));
            }

        }
    }
}
