using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Extensions;
using Revit.Async;
using System;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.Creation.Transactions
{
    /// <inheritdoc/>
    public class MessageTransactionFactory : ContextTransactionFactory
    {
        private readonly bool _handleMessage;

        /// <summary>
        /// Instantiate an object to create and perform transactions depending on <paramref name="contextOption"/>
        /// with handling transaction messages.
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="contextOption"></param>
        /// <param name="transactionPrefix"></param>
        /// <param name="handleMessage">Specifies if save errors or warning messages on transactions commit.</param>
        public MessageTransactionFactory(Document doc, RevitContextOption contextOption,
            string transactionPrefix = null, bool handleMessage = false) : base(doc, contextOption) 
        {         
            _handleMessage = handleMessage;
            Prefix = string.IsNullOrEmpty(transactionPrefix) ? null : transactionPrefix + "_";
        }

        #region Properties

        /// <summary>
        /// Prefis used for transactions names.
        /// </summary>
        public string Prefix { get; }

        /// <summary>
        /// Messages with errors prevented to commit transaction.
        /// </summary>
        public string ErrorMessages { get; private set; }

        /// <summary>
        /// Messages with warnings after committing transaction.
        /// </summary>
        public string WarningMessages { get; private set; }

        #endregion

        /// <summary>
        /// Clear <see cref="ErrorMessages"/> and <see cref="WarningMessages"/>.
        /// </summary>
        public void ClearMessages()
        {
            ErrorMessages = null;
            WarningMessages = null;
        }

        /// <inheritdoc/>
        protected override T CreateSync<T>(Func<T> operation, string transactionName, bool commitTransaction)
        {
            T result = default;

            using (Transaction transaction = new(Doc, Prefix + transactionName))
            {
                transaction.Start();

                result = operation.Invoke();

                if (transaction.HasStarted())
                {
                    if (commitTransaction)
                    {
                        transaction.Commit();
                        handleMessages(transaction);
                    }
                    else
                    { transaction.RollBack(); }
                }
            }

            return result;

            void handleMessages(Transaction transaction)
            {
                if (_handleMessage)
                {
                    var name = transaction.GetName();
                    if (name.Contains("RolledBack"))
                    {
                        ErrorMessages += name + "\n";
                    }
                    if (name.Contains("Warning"))
                    {
                        WarningMessages += name + "\n";
                    }
                }
            }
        }
    }
}
