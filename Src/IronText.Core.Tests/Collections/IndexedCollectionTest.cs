using IronText.Collections;
using IronText.Misc;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.Tests.Collections
{
    [TestFixture]
    public class IndexedCollectionTest
    {
        const string DefaultStr = "<default>";

        private TestScope collectionScope;
        private TestIndexedCollection target;
        private TestIndexableObject x;
        private TestIndexableObject y;
        private TestIndexableObject z;
        private const int StartIndex = 0;

        [SetUp]
        public void SetUp()
        {
            this.collectionScope = new TestScope();
            this.target = new TestIndexedCollection(collectionScope);            
            this.x = new TestIndexableObject("x");
            this.y = new TestIndexableObject("y");
            this.z = new TestIndexableObject("z");
        }

        [Test]
        public void AddedElementIsContainedTest()
        {
            // When
            target.Add(x);

            // Then
            Assert.That(target.Contains(x));
        }

        [Test]
        public void AttachedElementHasCorrectScope()
        {
            // When
            target.Add(x);

            // Then
            Assert.That(x.AttachedScope, Is.SameAs(collectionScope));
        }

        [Test]
        public void RemovedElementIsNotContainedTest()
        {
            // Given
            target.Add(x);
            Assume.That(target.Contains(x));

            // When
            bool outcome = target.Remove(x);

            // Then
            Assert.IsTrue(outcome);
            Assert.That(!target.Contains(x));
        }

        [Test]
        public void RemovedElementDetachingWithCorrectScope()
        {
            // Given
            target.Add(x);
            Assume.That(target.Contains(x));

            // When
            target.Remove(x);

            // Then
            Assert.That(x.DetachingScope, Is.SameAs(collectionScope));
        }

        [Test]
        public void AddedInstanceIsNotMarkedAsSoftRemove()
        {
            target.Add(x);

            Assert.IsFalse(x.IsSoftRemoved);
        }

        [Test]
        public void SoftRemoveMarksInstance()
        {
            target.Add(x);

            target.SoftRemove(x);

            Assert.IsTrue(x.IsSoftRemoved);
        }

        [Test]
        public void SoftRemoveDetachesInstance()
        {
            target.Add(x);

            target.SoftRemove(x);

            Assert.IsTrue(x.IsDetached);
        }


        [Test]
        public void SoftRemovedIsIndexed()
        {
            target.Add(x);
            target.SoftRemove(x);

            target.BuildIndexes();

            Assert.AreEqual(0, x.AssignedIndex);
        }

        [Test]
        public void SoftRemoveMakesInstanceNonEnumerable()
        {
            target.Add(x);

            target.SoftRemove(x);

            Assert.IsEmpty(AsEnumerable(target));
        }

        [Test]
        public void BuildIndexAssignesIndexesToObjects()
        {
            target.Add(x);
            target.BuildIndexes();
            Assert.IsTrue(x.AssignedIndex.HasValue);
        }

        [Test]
        public void BuildIndexAssignesIndexesEntriesStartingFromZero()
        {
            target.Add(x);
            target.BuildIndexes();
            Assert.AreEqual(0, x.AssignedIndex);
        }

        [Test]
        public void StartIndexPropertyIsAlwaysZero()
        {
            target.BuildIndexes();
            Assert.AreEqual(0, target.StartIndex);
        }

        [Test]
        public void AddedItemsHaveSubsequentFreeIndexes()
        {
            target.Add(x);
            target.Add(y);
            target.BuildIndexes();
            Assert.AreEqual(2, target.Count);
        }

        [Test]
        public void IndexedItemsAreAccessibleByIndex()
        {
            target.Add(x);
            target.Add(y);
            target.BuildIndexes();
            Assert.AreSame(x, target[x.AssignedIndex.Value]);
            Assert.AreSame(y, target[y.AssignedIndex.Value]);
        }

        [Test]
        public void CreateCompatibleArrayReturnsArrayOfSizeEqualLastIndex()
        {
            GivenIndexedCollectionOfSize2();

            var array = target.CreateCompatibleArray<string>(DefaultStr);

            Assert.AreEqual(2, array.Length);
        }

        [Test]
        public void CreateCompatibleArrayReturnsArrayWithFilledEmptySlots()
        {
            GivenIndexedCollectionOfSize2();

            var array = target.CreateCompatibleArray<string>(DefaultStr);

            Assert.That(array.Take(2), Is.All.EqualTo(DefaultStr));
        }

        [Test]
        public void ForcedIndexAreUsed()
        {
            const int xForcedIndex = 1;
            const int yForcedIndex = 0;

            target.Add(x, xForcedIndex);
            target.Add(y, yForcedIndex);

            target.BuildIndexes();

            Assert.AreEqual(xForcedIndex, x.AssignedIndex);
            Assert.AreEqual(yForcedIndex, y.AssignedIndex);
        }

        [Test]
        public void ForcedIndexIsNotUsedForSubsequentAutoindexing()
        {
            target.Add(x, 1);
            target.Add(y);
            target.Add(z);
            target.BuildIndexes();

            Assert.AreEqual(1, x.AssignedIndex);
            Assert.AreEqual(0, y.AssignedIndex);
            Assert.AreEqual(2, z.AssignedIndex);
        }

        [Test]
        public void IndexGapsCauseBuildIndexToFail()
        {
            target.Add(x, 0);
            target.Add(y, 2);
            Assert.Throws<InvalidOperationException>(target.BuildIndexes);
        }

        private void GivenIndexedCollectionOfSize2()
        {
            target.Add(x);
            target.Add(y);
            target.BuildIndexes();
        }

        class TestScope
        {
        }

        private static IEnumerable<T> AsEnumerable<T>(IEnumerable<T> result)
        {
            return result;
        }

        class TestIndexableObject
            : IIndexable<TestScope>
            , IIndexableImpl<TestScope>
            , IHasIdentity
        {
            private readonly string Name;

            public  TestScope AttachedScope;
            public  TestScope DetachingScope;
            public  bool      IsAttached;
            public  int?      AssignedIndex;

            public TestIndexableObject(string name) { this.Name = name; }

            public override string ToString() { return Name; }

            public object Identity { get { return Name; } }

            public bool IsDetached { get; private set; }

            public bool IsSoftRemoved { get; private set; }

            public void MarkSoftRemoved()
            {
                IsSoftRemoved = true;
            }

            public void Attached(TestScope scope)
            {
                this.IsAttached = true;
                this.AttachedScope = scope;
            }

            public void AssignIndex(int index)
            {
                this.AssignedIndex = index;
            }

            public void Detaching(TestScope scope)
            {
                this.DetachingScope = scope;
                this.IsDetached = true;
            }

            public override bool Equals(object obj)
            {
                throw new AssertionException("item.Equals() shot not be used in indexed collection.");
            }

            // Prevent warning
            public override int GetHashCode() { return base.GetHashCode(); }
        }

        class TestIndexedCollection : IndexedCollection<TestIndexableObject,TestScope>
        {
            public TestIndexedCollection(TestScope scope)
                : base(scope)
            {
            }
        }
    }
}
