using System;
using System.Collections.Generic;

namespace IronText.Algorithm
{
    internal static class Sorting
    {
        /// <summary>
        /// Sorts .Net types by inheritance.
        /// Base classes will always precede children.
        /// </summary>
        /// <param name="input"></param>
        public static void InheritanceSort(Type[] input)
        {
            InheritanceSort(input, (x, y) => y.IsAssignableFrom(x));
        }

        /// <summary>
        /// Sorts elements in array by inheriance i.e. the "leaf" 
        /// objects in single-inheriance hierarhy tree will allways 
        /// come after "base" objects.
        /// </summary>
        /// <typeparam name="T">Element type</typeparam>
        /// <param name="input">Element array being sorted in-place</param>
        /// <param name="isA">IS-A relation for determining hierarhy</param>
        public static void InheritanceSort<T>(T[] input, Func<T, T, bool> isA)
        {
            new InheritanceSortHelper<T>(input, isA).Sort();
        }

        /// <summary>
        /// Sorts .Net types by specialization.
        /// Base classes will always follow children.
        /// </summary>
        /// <param name="input"></param>
        public static void SpecializationSort(Type[] input)
        {
            SpecializationSort(input, (x, y) => y.IsAssignableFrom(x));
        }

        /// <summary>
        /// Sorts elements in array by specialization i.e. "derived" 
        /// objects in single-inheriance hierarhy tree will allways 
        /// precede "base" objects.
        /// </summary>
        /// <typeparam name="T">Element type</typeparam>
        /// <param name="input">Element array being sorted in-place</param>
        /// <param name="isA">IS-A relation for determining hierarhy</param>
        public static void SpecializationSort<T>(T[] input, Func<T, T, bool> isA)
        {
            new InheritanceSortHelper<T>(input, isA, reversed:true).Sort();
        }

        /// <summary>
        /// Builds a forest with each tree representing independant 
        /// single-inheritance hierarhy.
        /// </summary>
        /// <typeparam name="T">Element type</typeparam>
        /// <param name="input">Element array being sorted in-place</param>
        /// <param name="isA">IS-A relation for determining hierarhy</param>
        public static List<TreeNode<T>> GetHierarhies<T>(T[] input, Func<T, T, bool> isA)
        {
            return new InheritanceSortHelper<T>(input, isA, reversed:true).BuildForest();
        }

        /// <summary>
        /// Enumerates all X is-a Y pairs.
        /// </summary>
        /// <typeparam name="T">Element type</typeparam>
        /// <param name="input">Element array being sorted in-place</param>
        /// <param name="isA">IS-A relation for determining hierarhy</param>
        public static IEnumerable<Tuple<T, T>> GetInheritancePairs<T>(T[] input, Func<T, T, bool> isA)
        {
            return new InheritanceSortHelper<T>(input, isA, reversed:true).GetInheritancePairs();
        }
    }
}
