
namespace IronText.Runtime
{
    public interface IStackLookback<T>
    {
        /// <summary>
        /// Get state before a reduction path
        /// </summary>
        int GetParentState();

        /// <summary>
        /// Get states before a reduction path
        /// </summary>
        /// <param name="backOffset"></param>
        /// <returns></returns>
        int GetState(int backOffset);

        /// <summary>
        /// Gets syntax nodes before a reduction path.
        /// </summary>
        /// <param name="backOffset">Lookback index starting from the 1 (1 stands for prior state transition)</param>
        /// <returns></returns>
        T GetNodeAt(int backOffset);
    }
}
