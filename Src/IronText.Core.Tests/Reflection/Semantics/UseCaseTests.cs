using IronText.Reflection;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace IronText.Tests.Reflection.Semantics
{
    class UseCaseTests
    {
        class SemanticVariable
        {
        }

        class SemanticReference
        {
        }

        interface IProductionSemantics
        {
            SemanticVariable  Variable(int pos, string name, Type type);

            SemanticReference Reference(int pos, string name);

            void AddFormula(
                SemanticVariable  lhe,
                SemanticReference rhe);

            void AddFormula<T>(
                SemanticVariable      lhe,
                SemanticReference[]   formalParams,
                Expression<Func<T,T>> runtimeAction);

            void AddFormula<T1,T2,T3,T4,T>(
                SemanticVariable      lhe,
                SemanticReference[]   formalParams,
                Expression<Func<T1,T2,T3,T4,T>> runtimeAction);
        }

        /// <summary>
        /// Inherited attribute semantics (shift semantics).
        /// </summary>
        /// <remarks>
        /// A = B B C C D  { C2.z = A.x + B2.y - B1.y + D.t /* unknown dependency delays evaluation of C2.z */ }
        /// </remarks>
        void InheritedCalculationUsingInhSyntUnknownAttributes(IProductionSemantics sem)
        {
            var C2_z = sem.Variable(4, "z", typeof(int));
            var A_x  = sem.Reference(0, "x");
            var B1_y = sem.Reference(1, "y");
            var B2_y = sem.Reference(2, "y");
            var D_t  = sem.Reference(5, "x");
            sem.AddFormula(
                C2_z,
                new[] { A_x, B2_y, B1_y, D_t },
                // Following is System.Linq.Expressions.Expression<...> and should be simple
                // enough to be compiled to JavaScript target platform (future functionality).
                (int a_x, int b2_y, int b1_y, int d_t) =>  a_x + b2_y - b1_y + d_t);
        }

        /// <summary>
        /// Main-value synthesized atribute semantics (main reduction semantics).
        /// A = B B C { A.$main = B2.$main - B1.$main + C.$main  + A.inh1 }
        /// </summary>
        /// <remarks>
        /// </remarks>
        void MainValueSemantics(IProductionSemantics sem)
        {
            var A       = sem.Variable(0, "$main", typeof(int));
            var B1      = sem.Reference(1, "$main");
            var B2      = sem.Reference(2, "$main");
            var C       = sem.Reference(3, "$main");
            var A_inh1  = sem.Reference(0, "inh1");

            sem.AddFormula(
                A,
                new[] { B2, B1, C, A_inh1 },
                (int b2, int b1, int c, int inh1) => b2 - b1 + c + inh1);
        }

        /// <summary>
        /// Inherited attribute semantics: attribute copy rule to 
        /// cause attributes belong to the same Equivalence Class.
        /// </summary>
        /// <remarks>
        /// A = B C D  { C.z = A.x }
        /// </remarks>
        void CopyInheritedCausesAttributesBelongToTheSameEC(IProductionSemantics sem)
        {
            var C_z = sem.Variable(1, "z", typeof(int));
            var A_x = sem.Reference(0, "x");
            sem.AddFormula(
                C_z,
                new[] { A_x },
                // Should be strightforward identity lambda. Otherwise 
                // compiler will not detect identity.
                (int a_x) =>  a_x);

            // Or using special overload:

            sem.AddFormula(C_z, A_x);
        }
    }
}
