using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace IronText.Tests.UseCases
{
    [TestFixture]
    public class ImplicitAstUseCase
    {
        static Action<MyTree> SampleTree =
                m => m.Branch(
                    1,
                    m1 => m1.Branch(
                                2,
                                m2 => m2.Leaf(3),
                                m2 => m2.Leaf(4)),
                    m1 => m1.Branch(
                                5,
                                m2 => m2.Leaf(6),
                                m2 => m2.Leaf(7)));

        [Test]
        public void NodeCountTest()
        {
            Assert.AreEqual(4, SampleTree.Match<LeafCounter>().Result);

            Assert.AreEqual(4, CountLeafs(SampleTree));
        }

        static int CountLeafs(Action<MyTree> tree)
        {
            int result = 0;
            TreeMatch match = default(TreeMatch);
            match = new TreeMatch
                {
                    Leaf = value => ++result,
                    Branch = (value, l, r) => { l(match); r(match); }
                };

            tree(match);
            return result;
        }

        static int ValueOf(Action<MyTree> tree)
        {
            int result = int.MinValue;
            tree(
                new TreeMatch
                {
                    Leaf=(value) => result = value,
                    Branch=(value, l, r) => { result = value; }
                });

            return result;
        }

        [Test]
        public void TestCollectDepthFirst()
        {
            Assert.AreEqual(new [] { 3, 4, 2, 6, 7, 5, 1 }, SampleTree.Match<CollectDepthFirst>().Result);
        }

        [Test]
        public void TestCollectLeftToRigth()
        {
            Assert.AreEqual(new [] { 1, 2, 3, 4, 5, 6, 7 }, SampleTree.Match<CollectLeftToRigth>().Result);
        }

        [Test]
        public void TestCollectTopDownDepthFirstTest()
        {
            Assert.AreEqual(new [] { 1, 2, 5, 3, 4, 6, 7 }, SampleTree.Match<CollectBreadthFirst>().Result);
        }

        public interface MyTree
        {
            void Leaf(int value);
            void Branch(int value, Action<MyTree> left, Action<MyTree> rigth);
        }

        public struct TreeMatch : MyTree
        {
            public Action<int> Leaf;
            public Action<int, Action<MyTree>, Action<MyTree>> Branch;

            void MyTree.Leaf(int value)
            {
                if (Leaf != null)
                    Leaf(value);
            }

            void MyTree.Branch(int value, Action<MyTree> left, Action<MyTree> right)
            {
                if (Branch != null)
                    Branch(value, left, right);
            }
        }

        class LeafCounter : MyTree
        {
            public int Result = 0;

            public void Branch(int value, Action<MyTree> left, Action<MyTree> right)
            {
                left.Match(this);
                right.Match(this);
            }

            public void Leaf(int value) { Result += 1; }
        }

        class CollectDepthFirst : MyTree
        {
            public readonly List<int> Result = new List<int>();

            public void Branch(int value, Action<MyTree> left, Action<MyTree> right)
            {
                left.Match(this);
                right.Match(this);
                Result.Add(value);
            }

            public void Leaf(int value) { Result.Add(value); }
        }

        class CollectLeftToRigth : MyTree
        {
            public readonly List<int> Result = new List<int>();

            public void Branch(int value, Action<MyTree> left, Action<MyTree> right)
            {
                Result.Add(value);
                left.Match(this);
                right.Match(this);
            }

            public void Leaf(int value) { Result.Add(value); }
        }

        class CollectBreadthFirst : MyTree
        {
            public readonly List<int> Result = new List<int>();

            public void Branch(int value, Action<MyTree> left, Action<MyTree> right)
            {
                Result.Add(value);

                var queueVisitor = new QueueVisitor { Result = Result };
                queueVisitor.Enqueue(left);
                queueVisitor.Enqueue(right);

                do
                {
                    queueVisitor.Dequeue()(queueVisitor);
                }
                while (queueVisitor.Count != 0);
            }

            public void Leaf(int value) { Result.Add(value); }

            class QueueVisitor : Queue<Action<MyTree>>, MyTree
            {
                public List<int> Result;

                public void Leaf(int value)
                {
                    Result.Add(value);
                }

                public void Branch(int value, Action<MyTree> left, Action<MyTree> right)
                {
                    Result.Add(value);
                    Enqueue(left);
                    Enqueue(right);
                }
            }
        }

        class Reify : MyTree
        {
            public object Result;

            public void Branch(int value, Action<MyTree> left, Action<MyTree> right)
            {
                left(this);
                object l = Result;
                right(this);
                Result = Tuple.Create(l, Result);
            }

            public void Leaf(int value) { Result = value; }
        }
    }
}
