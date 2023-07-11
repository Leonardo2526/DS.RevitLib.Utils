using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Extensions;
using Revit.Async;
using System;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.Creation.Transactions
{
    /// <inheritdoc/>
    public class ContextTransactionFactory : ITransactionFactory
    {
        private readonly RevitContextOption _contextOption;

        /// <summary>
        /// Instantiate an object to create and perform transactions depending on <paramref name="contextOption"/>.
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="contextOption"></param>
        public ContextTransactionFactory(Document doc, RevitContextOption contextOption = RevitContextOption.Inside)
        {
            Doc = doc;
            _contextOption = contextOption;
        }

        /// <inheritdoc/>
        public Document Doc { get; }

        /// <summary>
        /// Specifies current Revit context.
        /// </summary>
        public bool IsRevitContext => GetRevitContext(_contextOption);

        /// <inheritdoc/>
        public async Task CreateAsync(Action operation, string transactionName, bool commitTransactions = true)
        {
            await CreateAsync(opFunc, transactionName, commitTransactions);
            bool opFunc()
            {
                operation.Invoke();
                return true;
            }
        }

        /// <inheritdoc/>
        public async Task<T> CreateAsync<T>(Func<T> operation, string transactionName, bool commitTransaction = true)
        {
            T func() => CreateSync(operation, transactionName, commitTransaction);
            return IsRevitContext ? func() : await RevitTask.RunAsync(func);
        }

        /// <inheritdoc/>
        protected virtual T CreateSync<T>(Func<T> operation, string transactionName, bool commitTransaction)
        {
            T result = default;

            using (Transaction transaction = new(Doc, transactionName))
            {
                transaction.Start();

                result = operation.Invoke();

                if (transaction.HasStarted())
                {
                    if (commitTransaction)
                    {transaction.Commit();}
                    else
                    { transaction.RollBack(); }
                }
            }

            return result;
         
        }

        private bool GetRevitContext(RevitContextOption contextOption)
        {
            switch (contextOption)
            {
                case RevitContextOption.Inside:
                    return true;
                case RevitContextOption.Outside:
                    return false;
                case RevitContextOption.Auto:
                    return Doc.IsRevitContext();
                default:
                    return false;
            }
        }
    }
}