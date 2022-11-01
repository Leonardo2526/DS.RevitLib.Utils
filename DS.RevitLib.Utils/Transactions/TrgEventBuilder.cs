using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils;
using Revit.Async;
using System;
using System.Threading.Tasks;

namespace DS.RevitApp.TransactionTest
{
    /// <summary>
    /// An object to wrap transactions actions into transaction group with awaiting event.
    /// </summary>
    public class TrgEventBuilder
    {
        private readonly Document _doc;
        private readonly WindowTaskEvent _windowsTaskEvent;
        private readonly Task _taskEvent;

        /// <summary>
        /// Create a new instance of object to wrap transactions actions into 
        /// transaction group with <paramref name="windowsTaskEvent"/>.
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="windowsTaskEvent"><see cref="WindowTaskEvent"/> to create a new event task.</param>
        public TrgEventBuilder(Document doc, WindowTaskEvent windowsTaskEvent)
        {
            _doc = doc;
            _taskEvent = windowsTaskEvent.Create();
            _windowsTaskEvent = windowsTaskEvent;
        }

        /// <summary>
        /// Create a new transaction group and build a new task inside it with <paramref name="operation"/>. 
        /// </summary>
        /// <param name="operation">Transactions to perform.</param>
        /// <param name="revitAsync">Optional parameter to perform <paramref name="operation"/> outside of Revit API context.</param>
        /// <returns>Returns a new async Task to perform transaction group operations.</returns>
        public async Task BuildAsync(Action operation, bool revitAsync = false)
        {
            using (var trg = new TransactionGroup(_doc))
            {
                trg.Start();

                Task transactionTask = CreateTask(operation, revitAsync);
                await transactionTask;

                TrgCommitter(trg);
            }
        }

        private Task CreateTask(Action operation, bool wrapRevitAsync = false)
        {
            Task task = Task.Run(() =>
            {
                while (true)
                {
                    if (wrapRevitAsync)
                    {
                        RevitTask.RunAsync(() => operation.Invoke());
                    }
                    else
                    {
                        operation.Invoke();
                    }

                    _taskEvent.Wait();
                    break;
                }
            });
            return task;
        }

        /// <summary>
        /// Perform action to close transaction group.
        /// </summary>
        /// <param name="trg">Current opened transaction group.</param>
        private void TrgCommitter(TransactionGroup trg)
        {
            if (trg.HasStarted() && !_windowsTaskEvent.WindowClosed)
            {
                trg.RollBack();
                TaskDialog.Show($"{GetType().Name}", "trg rolled");
            }
            else if (trg.HasStarted() && _windowsTaskEvent.WindowClosed)
            {
                trg.Commit();
                TaskDialog.Show($"{GetType().Name}", "trg committed");
            }
            else
            {
                TaskDialog.Show($"{GetType().Name}", "trg is not closed due to it hasn't been started.");
            }
        }
    }
}
