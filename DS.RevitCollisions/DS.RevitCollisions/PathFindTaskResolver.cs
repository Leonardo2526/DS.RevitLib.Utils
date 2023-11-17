using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils;
using DS.ClassLib.VarUtils.Resolvers;
using DS.ClassLib.VarUtils.Resolvers.ResolveTasks;
using DS.ClassLib.VarUtils.Solutions;
using DS.GraphUtils.Entities;
using DS.PathFinder;
using DS.RevitLib.Utils.Connections.PointModels;
using DS.RevitLib.Utils.Creation.Transactions;
using Rhino.Geometry;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitCollisions
{
    internal class PathFindTaskResolver : ITaskResolver<PathFindVertexTask>
    {
        private readonly IPathFinder<IVertex, Point3d> _pathFinder;
        private List<ISolution> _solutions = new List<ISolution>();  

        public PathFindTaskResolver(IPathFinder<IVertex, Point3d> pathFinder)
        {
            _pathFinder = pathFinder;
        }

        public IEnumerable<ISolution> Solutions => _solutions;

        /// <summary>
        /// Operations logger.
        /// </summary>
        public ILogger Logger { get; set; }

        public ISolution TryResolve(PathFindVertexTask task)
          => ResolveAsync(task, false).Result;

        public async Task<ISolution> TryResolveAsync(PathFindVertexTask task)
        => await ResolveAsync(task, true);

        private async Task<ISolution> ResolveAsync(PathFindVertexTask task, bool runParallel)
        {                        
            
            //await TransactionFactory.CreateAsync(() =>
            //{ point.Show(_doc); _doc.Regenerate(); },
            //"Regen");
            //_uIDoc.RefreshActiveView();
            //Logger?.Information($"Regenerated");


            //  await TransactionFactory.CreateAsync(() =>
            //  { TaskDialog.Show("Test", "Regenerated"); },
            //"ShowTaskDialog");

            TaskDialog.Show("Test", "Regenerated");

            var time1 = DateTime.Now;
            var result = runParallel ?
                 await Task.Run(() => LongOperationAsync(task)) :
                await LongOperationAsync(task);
            //LongTask();
            var solution = new Solution<IEnumerable<Point3d>>(result);

            var time2 = DateTime.Now;
            TimeSpan totalInterval = time2 - time1;
            Logger?.Information($"Task resoved in {(int)totalInterval.TotalMilliseconds} ms");

            _solutions.Add(solution);

            return solution;
        }

        private async Task<IEnumerable<Point3d>> LongOperationAsync(PathFindVertexTask task)
            => await _pathFinder.FindPathAsync(task.Source, task.Target);        
    }
}
