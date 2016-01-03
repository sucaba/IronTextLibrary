using IronText.Reflection;

namespace IronText.Tests.Reflection.Semantics
{
    class UseCaseTests
    {
        /// <summary>
        /// Inherited attribute semantics (shift semantics).
        /// </summary>
        /// <remarks>
        /// A = B B C C D  { C2.z = A.x + B2.y - B1.y + D.t /* unknown dependency delays evaluation of C2.z */ }
        /// </remarks>
        void InheritedCalculationUsingInhSyntUnknownAttributes(Production prod)
        {
            var C2_z = new SemanticVariable(4, "z", typeof(int));
            var A_x  = new SemanticReference(0, "x");
            var B1_y = new SemanticReference(1, "y");
            var B2_y = new SemanticReference(2, "y");
            var D_t  = new SemanticReference(5, "x");

            new Production(new Symbol(""), new Symbol(""))
            {
                Semantics =
                {
                    {
                        C2_z, new[] { A_x, B2_y, B1_y, D_t },
                        // Following is System.Linq.Expressions.Expression<...> and should be 
                        // compilable to a JavaScript target platform (future functionality).
                        (int a_x, int b2_y, int b1_y, int d_t) =>  a_x + b2_y - b1_y + d_t
                    }
                }
            };
        }

        /// <summary>
        /// Main-value synthesized atribute semantics (main reduction semantics).
        /// A = B B C { A.$main = B2.$main - B1.$main + C.$main  + A.inh1 }
        /// </summary>
        /// <remarks>
        /// </remarks>
        void MainValueSemantics()
        {
            var A       = new SemanticVariable(typeof(int));
            var B1      = new SemanticReference(1);
            var B2      = new SemanticReference(2);
            var C       = new SemanticReference(3);
            var A_inh1  = new SemanticReference(0, "inh1");

            new Production(new Symbol(""), new Symbol(""))
            {
                Semantics =
                {
                    {
                        A, new[] { B2, B1, C, A_inh1 },
                        (int b2, int b1, int c, int inh1) => b2 - b1 + c + inh1
                    }
                }
            };
        }

        /// <summary>
        /// Inherited attribute semantics: attribute copy rule to 
        /// cause attributes belong to the same Equivalence Class.
        /// </summary>
        /// <remarks>
        /// A = B C D  { C.z = A.x }
        /// </remarks>
        void CopyInheritedCausesAttributesBelongToTheSameEC()
        {
            var C_z = new SemanticVariable(2, "z", typeof(int));
            var A_x = new SemanticReference(0, "x");

            new Production(new Symbol(""), new Symbol(""))
            {
                Semantics =
                {
                    {
                        C_z, new[] { A_x },
                        // Should be a strightforward identity lambda. Otherwise 
                        // compiler will not detect identity.
                        (int a_x) =>  a_x
                    },
                    // Or using special typeless copy-attribute overload:
                    { C_z, A_x }
                }
            };
        }
    }
}
