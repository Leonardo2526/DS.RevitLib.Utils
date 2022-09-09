using Autodesk.Revit.DB;
using DS.RevitLib.Utils.TransactionCommitter;
using System;
using System.Linq;

namespace DS.RevitLib.Utils
{
    public class FailedTransaction
    {
        private readonly Document _doc;
        private readonly Level _level;

        public FailedTransaction(Document doc)
        {
            _doc = doc;
            _level = new FilteredElementCollector(_doc)
                .OfClass(typeof(Level))
                .Cast<Level>()
                .FirstOrDefault();
        }

        public void CreateTwoWalls()
        {
            Line line = Line.CreateBound(XYZ.Zero, XYZ.BasisX);
            using (Transaction transNew = new Transaction(_doc, "CreateTwoWalls"))
            {
                try
                {
                    transNew.Start();

                    Wall.Create(_doc, line, _level.Id, false);
                    Wall.Create(_doc, line, _level.Id, false);
                }

                catch (Exception e)
                { }

                transNew.Commit();
            }
        }

        public void CreateFittingByConnectors(Connector con1 = null, Connector con2 = null)
        {
            using (Transaction transNew = new Transaction(_doc, "CreateFittingByConnectors"))
            {
                transNew.Start();

                _doc.Create.NewElbowFitting(con1, con2);

                transNew.Commit();
            }

        }
    }
}
