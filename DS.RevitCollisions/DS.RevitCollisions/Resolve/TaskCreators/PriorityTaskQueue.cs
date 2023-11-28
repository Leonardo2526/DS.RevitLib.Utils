using Autodesk.Revit.DB;
using DS.GraphUtils.Entities;
using QuickGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS.RevitLib.Utils.MEP;
using DS.RevitLib.Utils.Graphs;

namespace DS.RevitCollisions.Resolve.TaskCreators
{
    internal class PriorityTaskQueue : Queue<(IVertex, IVertex)>
    {
        public PriorityTaskQueue(
            Document doc, 
            IEnumerator<(IVertex, IVertex)> enumerator, 
            IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> graph,
            MEPCurve baseMEPCurve
            )
        {
            var tasks = GetTasks(doc, enumerator, graph,baseMEPCurve).ToList();
            tasks.ForEach(Enqueue);
        }

        private IEnumerable<(IVertex, IVertex)> GetTasks(
            Document doc, 
            IEnumerator<(IVertex, IVertex)> enumerator,
            IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> graph, 
            MEPCurve baseMEPCurve)
        {
            List<(IVertex, IVertex)> list = new();

            while (list.Count != 4 && enumerator.MoveNext())
            { list.Add(enumerator.Current); }

            double sizeFactor = baseMEPCurve.GetMaxSize();
            return list.SortByTaggedLength(graph, doc, 25, sizeFactor).ToList();
        }
    }
}
