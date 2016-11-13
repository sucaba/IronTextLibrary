using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronText.Automata.TreeNfa
{
    class TreeItem
    {
        public TreeItem(TreeNode root)
        {
            this.Root = root;
        }

        public TreeNode Root { get; }

        public IEnumerable<TreeTransition> AllTransitions()
        {
            throw new NotImplementedException();
        }
    }
}
