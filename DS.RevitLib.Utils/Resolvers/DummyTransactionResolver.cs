using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils;
using DS.ClassLib.VarUtils.Resolvers;
using DS.RevitLib.Utils.Creation.Transactions;
using DS.RevitLib.Utils.Extensions;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.Resolvers
{
    public class DummyTransactionResolver : ITaskResolver<IResolveTask>
    {
        private readonly List<ISolution> _solutions = new();
        private readonly UIDocument _uIDoc;
        private readonly Document _doc;

        public DummyTransactionResolver(UIDocument uIDoc)
        {
            _uIDoc = uIDoc;
            _doc = uIDoc.Document;
        }

        public IEnumerable<ISolution> Solutions => _solutions;

        public ITransactionFactory TransactionFactory { get; set; }

        /// <summary>
        /// Operations logger.
        /// </summary>
        public ILogger Logger { get; set; }

        public ISolution TryResolve(IResolveTask task)
            => ResolveAsync(false).Result;


        public async Task<ISolution> TryResolveAsync(IResolveTask task)
            => await ResolveAsync(true);


        public async Task<ISolution> ResolveAsync(bool runParallel)
        {
            var solution = new DummySolution();

            var point = new XYZ(10, 0, 0);
            await TransactionFactory.CreateAsync(() =>
            { point.Show(_doc); _doc.Regenerate(); },
            "Regen");
            _uIDoc.RefreshActiveView();
            Logger?.Information($"Regenerated");

            //  await TransactionFactory.CreateAsync(() =>
            //  { TaskDialog.Show("Test", "Regenerated"); },
            //"ShowTaskDialog");

            TaskDialog.Show("Test", "Regenerated");

            var time1 = DateTime.Now;
            var result = runParallel ?
                 await Task.Run(LongTaskAsync) :
                await LongTaskAsync();
                //LongTask();

            var time2 = DateTime.Now;
            TimeSpan totalInterval = time2 - time1;
            Logger?.Information($"Task resoved in {(int)totalInterval.TotalMilliseconds} ms");

            _solutions.Add(solution);
            return solution;
        }


        private string LongTask()
        {
            Task.Delay(1500).Wait();
            return "Task resolving complete!";
        }

        private async Task<string> LongTaskAsync()
        {
            await Task.Delay(1500);
            return "Task resolving complete!";
        }
    }
}
