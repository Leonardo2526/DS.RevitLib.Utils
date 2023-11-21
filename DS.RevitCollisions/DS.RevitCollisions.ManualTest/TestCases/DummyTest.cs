using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.RevitCollisions.Impl;
using DS.RevitCollisions.Models;
using DS.RevitLib.Utils;
using DS.RevitLib.Utils.Creation.Transactions;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitCollisions.ManualTest.TestCases
{

    internal class DummyTest
    {
        private readonly UIDocument _uIDoc;
        private readonly Document _doc;
        private readonly ContextTransactionFactory _trfIn;
        private readonly ContextTransactionFactory _trfOut;
        private readonly ContextTransactionFactory _trfAuto;
        private ILogger _logger;

        public DummyTest(UIDocument uIDoc)
        {
            _uIDoc = uIDoc;
            _doc = uIDoc.Document;
            _trfIn = new ContextTransactionFactory(_doc, RevitContextOption.Inside);
            _trfOut = new ContextTransactionFactory(_doc, RevitContextOption.Outside);
            _trfAuto = new ContextTransactionFactory(_doc);


            _logger = new LoggerConfiguration()
                 .MinimumLevel.Information()
                 .WriteTo.Debug()
                 .CreateLogger();
        }

       public void CreateResolveProcessor()
        {
            var builder = new DummyResolveProcessorBuilder()
            {
                Logger = _logger
            };
            var p = builder.GetProcessor();

            var dummyCollision = new DummyCollision();

            var result = p.TryResolve(dummyCollision);

            _logger.Information(result.ToString()); 
        }
    }
}
