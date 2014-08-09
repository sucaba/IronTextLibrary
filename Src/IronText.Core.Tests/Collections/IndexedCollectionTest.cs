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
        private const int StartIndex = 5;

        [SetUp]
        public void SetUp()
        {
            this.collectionScope = new TestScope();
            this.target = new TestIndexedCollection(collectionScope);            
            this.x = new TestIndexableObject("x");
            this.y = new TestIndexableObject("y");
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
        public void BuildIndexAssignesIndexesToObjects()
        {
            target.Add(x);
            target.BuildIndexes(StartIndex);
            Assert.IsTrue(x.AssignedIndex.HasValue);
        }

        [Test]
        public void BuildIndexAssignesIndexesEntriesStartingFromLimit()
        {
            target.Add(x);
            target.BuildIndexes(StartIndex);
            Assert.AreEqual(StartIndex, x.AssignedIndex);
        }

        [Test]
        public void StartIndexPropertyHasProvidedValue()
        {
            target.BuildIndexes(StartIndex);
            Assert.AreEqual(StartIndex, target.StartIndex);
        }

        [Test]
        public void IndexCountContainsIndexCountPlusStartIndex()
        {
            target.Add(x);
            target.Add(y);
            target.BuildIndexes(StartIndex);
            Assert.AreEqual(StartIndex + 2, target.LastIndex);
        }

        [Test]
        public void IndexedItemsAreAccessibleByIndex()
        {
            target.Add(x);
            target.Add(y);
            target.BuildIndexes(StartIndex);
            Assert.AreSame(x, target[x.AssignedIndex.Value]);
            Assert.AreSame(y, target[y.AssignedIndex.Value]);
        }

        [Test]
        public void CreateCompatibleArrayReturnsArrayOfCorrectSize()
        {
            GivenIndexedCollectionOfSize2();

            var array = target.CreateCompatibleArray<string>(DefaultStr);

            Assert.AreEqual(StartIndex + 2, array.Length);
        }

        [Test]
        public void CreateCompatibleArrayReturnsArrayWithFilledEmptySlots()
        {
            GivenIndexedCollectionOfSize2();

            var array = target.CreateCompatibleArray<string>(DefaultStr);

            Assert.That(array.Take(StartIndex), Is.All.EqualTo(DefaultStr));
        }

        [Test]
        public void ForcedIndexOverlappingWithOtherIndexIsUsedForIndexing()
        {
            int xForcedIndex = StartIndex + 1;
            int yForcedIndex = StartIndex + 0;
            target.Add(x, xForcedIndex);
            target.Add(y, yForcedIndex);
            target.BuildIndexes(StartIndex);

            Assert.AreEqual(xForcedIndex, x.AssignedIndex);
            Assert.AreEqual(yForcedIndex, y.AssignedIndex);
        }

        [Test]
        public void ForcedIndexOutOfRangeIsUsedForIndexing()
        {
            int xForcedIndex = StartIndex + 10; // big enough
            int yForcedIndex = StartIndex + 0;
            target.Add(x, xForcedIndex);
            target.Add(y, yForcedIndex);
            target.BuildIndexes(StartIndex);

            Assert.AreEqual(xForcedIndex, x.AssignedIndex);
            Assert.AreEqual(yForcedIndex, y.AssignedIndex);
        }

        [Test]
        public void TooLowForcedIndexCannotBeUsed()
        {
            int xForcedIndex = StartIndex - 1; // too low 
            target.Add(x, xForcedIndex);
            Assert.Throws<InvalidOperationException>(() => target.BuildIndexes(StartIndex));
        }

        private void GivenIndexedCollectionOfSize2()
        {
            target.Add(x);
            target.Add(y);
            target.BuildIndexes(StartIndex);
        }

        class TestScope
        {
        }

        class TestIndexableObject : IIndexable<TestScope>, IHasIdentity
        {
            private readonly string Name;

            public  TestScope AttachedScope;
            public  TestScope DetachingScope;
            public  bool      IsAttached;
            public  int?      AssignedIndex;

            public TestIndexableObject(string name) { this.Name = name; }

            public override string ToString() { return Name; }

            public object Identity { get { return Name; } }

            public bool IsDetached
            {
                get { throw new NotImplementedException(); }
            }

            public bool IsHidden
            {
                get { return false; }
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
