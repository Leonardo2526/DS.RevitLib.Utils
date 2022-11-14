using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils.Events;
using DS.RevitLib.Utils.Transactions.Committers;
using Revit.Async;
using System;
using System.Diagnostics;
using System.Threading;
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
        public async Task BuildAsync(Action operation, ITransactionGroupCommitter committer, 
            TaskComplition taskEvent, bool revitAsync = false, string trgName = "")
        {
            Task completionEventTask = taskEvent.Create();

            Debug.Print($"task {completionEventTask.Id} to wait event created in thread {Thread.CurrentThread.ManagedThreadId}.");
            trgName ??= $"Trg {taskEvent.EventType}";
            using (var trg = new TransactionGroup(_doc, trgName))
            {
                trg.Start();
                Debug.Print($"\nTransactionGroup {trg.GetName()} started in thread {Thread.CurrentThread.ManagedThreadId}");

                Task operationTask = CreateOperationTask(operation, completionEventTask, revitAsync);
                await operationTask;

                committer.Close(trg, taskEvent);
            }

            Debug.Print($"task {completionEventTask.Id} to wait event complete status: {completionEventTask.IsCompleted}.");
            Debug.Print($"BuildAsync executed in thread {Thread.CurrentThread.ManagedThreadId}.");
        }

        private Task CreateOperationTask(Action operation, Task complitionTask, bool wrapRevitAsync = false)
        {
           Debug.Print($"{nameof(CreateOperationTask)} started in thread {Thread.CurrentThread.ManagedThreadId}");
            Task task = Task.Run(() =>
            {
                Debug.Print($"task event started in thread {Thread.CurrentThread.ManagedThreadId}");

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

                    Debug.Print($"{nameof(complitionTask)} start waiting in thread {Thread.CurrentThread.ManagedThreadId}");
                    complitionTask.Wait();
                    Debug.Print($"\n{nameof(complitionTask)} was waited in thread {Thread.CurrentThread.ManagedThreadId}");
                    break;
                }
            });
            return task;
        }
    }
}
