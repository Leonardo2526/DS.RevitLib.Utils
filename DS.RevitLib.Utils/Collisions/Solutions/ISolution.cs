using DS.RevitLib.Utils.Transactions;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.Collisions.Solutions
{
    /// <summary>
    /// An object that represents collision solution.
    /// </summary>
    public interface ISolution
    {
        /// <summary>
        /// Show current solution.
        /// </summary>
        /// <param name="trb"></param>
        /// <returns></returns>
        public Task ShowAsync(AbstractTransactionBuilder trb);
    }
}
