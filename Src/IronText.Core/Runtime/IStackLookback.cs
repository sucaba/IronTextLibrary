
namespace IronText.Runtime
{
    public interface IStackLookback<T>
    {
        /// <summary>
        /// Get state before reduction path
        /// </summary>
        int GetParentState();

        /// <summary>
        /// Gets syntax node at the prior stack state.
        /// </summary>
        /// <param name="backOffset">Lookback index starting from the 1 (1 stands for prior state transition)</param>
        /// <returns></returns>
        T GetNodeAt(int backOffset);
    }
}
