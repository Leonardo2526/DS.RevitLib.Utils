using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;

namespace DS.RevitUtils.MEP
{
    public class PipeSystemCreator
    {
        private List<Pipe> PipesList = new List<Pipe>();

        static Document Doc;

        public void CreatePypeSystem(Document doc, List<XYZ> points)
        {
            Doc = doc;

            for (int i = 0; i < points.Count - 1; i++)
            {
                XYZ p1 = points[i];
                XYZ p2 = points[i + 1];

                CreatePipeByPoints(p1, p2);

                if (PipesList.Count > 1)
                    CreateFittingByPipes(PipesList[i - 1], PipesList[i]);
            }

        }

        /// <summary>
        /// Create pipe between 2 points
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        private void CreatePipeByPoints(XYZ p1, XYZ p2)
        {
            using (Transaction transNew = new Transaction(Doc, "AddPipe"))
            {
                try
                {
                    transNew.Start();
                    Pipe pipe = Pipe.Create(Doc, PypeSystem.MEPTypeElementId,
                PypeSystem.PipeTypeElementId, PypeSystem.level.Id, p1, p2);
                    PipesList.Add(pipe);
                }

                catch (Exception e)
                {
                    transNew.RollBack();
                    TaskDialog.Show("Revit", e.ToString());
                }
                transNew.Commit();
            }
        }


        /// <summary>
        /// Create fitting between two pipes
        /// </summary>
        /// <param name="pipe1"></param>
        /// <param name="pipe2"></param>
        private void CreateFittingByPipes(Pipe pipe1, Pipe pipe2)
        {
            using (Transaction transNew = new Transaction(Doc, "AddFitting"))
            {
                try
                {
                    transNew.Start();

                    List<Connector> connectors1 = MEPConnector.GetMEPConnectors(pipe1);
                    List<Connector> connectors2 = MEPConnector.GetMEPConnectors(pipe2);

                    MEPConnector.GetCommonConnector(out Connector con1, out Connector con2,
                    connectors1, connectors2);

                    Doc.Create.NewElbowFitting(con1, con2);
                }

                catch (Exception e)
                {
                    transNew.RollBack();
                    TaskDialog.Show("Revit", e.ToString());
                }
                transNew.Commit();
            }

        }
    }
}

