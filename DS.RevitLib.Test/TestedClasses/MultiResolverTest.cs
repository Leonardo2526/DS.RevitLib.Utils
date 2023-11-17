using DS.ClassLib.VarUtils.Resolvers;
using DS.ClassLib.VarUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Resolvers;
using DS.RevitLib.Utils.Creation.Transactions;

namespace DS.RevitLib.Test.TestedClasses
{
    internal class MultiResolverTest
    {
        private readonly UIDocument _uIDoc;
        private readonly Document _doc;
        private readonly ContextTransactionFactory _trfIn;
        private readonly ContextTransactionFactory _trfOut;
        private readonly ContextTransactionFactory _trfAuto;
        private MultiResolver _resolver;

        public MultiResolverTest(UIDocument uIDoc)
        {
            _uIDoc = uIDoc;
            _doc = uIDoc.Document;
            _trfIn = new ContextTransactionFactory(_doc, Utils.RevitContextOption.Inside);
            _trfOut = new ContextTransactionFactory(_doc, Utils.RevitContextOption.Outside);
            _trfAuto = new ContextTransactionFactory(_doc);


            Log.Logger = new LoggerConfiguration()
                 .MinimumLevel.Information()
                 .WriteTo.Debug()
                 .CreateLogger();
        }

        public MultiResolverTest CreateResolver()
        {
            var taskCreator = new DummyCreator();
            var taskResolver = new DummyTransactionResolver(_uIDoc)
            {
                TransactionFactory = _trfAuto,
                Logger = Log.Logger,
            };

            var resolvers = new List<ITaskResolver>()
            { taskResolver};
            _resolver = new MultiResolver(taskCreator, resolvers)
            {
                Logger = Log.Logger
            };

            return this;
        }

        public ISolution Resolve()
        {
            return _resolver.TryResolve();
        }

        public async Task<ISolution> ResolveAsync()
        {
            return await _resolver.TryResolveAsync();
        }
    }
}
