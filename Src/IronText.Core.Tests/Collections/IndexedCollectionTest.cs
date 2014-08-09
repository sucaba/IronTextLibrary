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
