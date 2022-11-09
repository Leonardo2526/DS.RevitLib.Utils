using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils.Events;
using Revit.Async;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.Transactions
{
    /// <summary>
    /// An object to wrap transactions actions into transaction group with awaiting event.
    /// </summary>
    public class TrgEventBuilder
    {
        private readonly Document _doc;

        /// <summary>
        /// Create a new instance of object to wrap transactions actions into transaction group with awaiting event.
        /// </summary>
        /// <param name="doc"></param>
        public TrgEventBuilder(Document doc)
        {
            _doc = doc;
        }

        /// <summary>
        /// Create a new transaction group and build a new task inside it with <paramref name="operation"/>. 
        /// </summary>
        /// <param name="operation">Transactions to perform.</param>
        /// <param name="taskEvent">Task event object to complete.</param>
        /// <param name="revitAsync">Optional parameter to perform <paramref name="operation"/> outside of Revit API context.</param>
        /// <returns>Returns a new async Task to perform transaction group operations.</returns>
        public async Task BuildAsync(Action operation, TaskComplition taskEvent, bool revitAsync = false)
        {
            Task completionEventTask = taskEvent.Create();

            Debug.Print($"task {completionEventTask.Id} to wait event created.");

            using (var trg = new TransactionGroup(_doc, $"Trg {taskEvent.EventType}"))
            {
                trg.Start();
                Debug.Print($"\nTransactionGroup {trg.GetName()} started");

                Task operationTask = CreateOperationTask(operation, completionEventTask, revitAsync);
                await operationTask;

                TrgCommitter(trg, taskEvent);
            }

            Debug.Print($"task {completionEventTask.Id} to wait event complete status: {completionEventTask.IsCompleted}.");
            Debug.Print($"BuildAsync executed.");
        }

        private Task CreateOperationTask(Action operation, Task complitionTask, bool wrapRevitAsync = false)
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

                    complitionTask.Wait();
                    break;
                }
            });
            return task;
        }

        /// <summary>
        /// Perform action to close transaction group.
        /// </summary>
        /// <param name="trg">Current opened transaction group.</param>
        private void TrgCommitter(TransactionGroup trg, TaskComplition taskEvent)
        {
            if (trg.HasStarted() && taskEvent.EventType == EventType.Onward)
            {
                trg.RollBack();
                Debug.Print($"TransactionGroup {trg.GetName()} rolled");
            }
            else if (trg.HasStarted() && taskEvent.EventType == EventType.Backward)
            {
                trg.RollBack();
                Debug.Print($"TransactionGroup {trg.GetName()} rolled");
            }
            else if (trg.HasStarted() && taskEvent.EventType == EventType.Close)
            {
                trg.Commit();
                Debug.Print($"TransactionGroup {trg.GetName()} committed");
            }
            else
            {
                TaskDialog.Show($"{GetType().Name}", "trg is not closed due to it hasn't been started.");
            }
        }
    }
}
