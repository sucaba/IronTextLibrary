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

        [SetUp]
        public void SetUp()
        {
            this.collectionScope = new TestScope();
            this.target = new TestIndexedCollection(collectionScope);            
            this.x = new TestIndexableObject("x");
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

        class TestScope
        {
        }

        class TestIndexableObject : IIndexable<TestScope>, IHasIdentity
        {
            private readonly string Name;

            public  TestScope AttachedScope;
            public  TestScope DetachingScope;
            public  bool      IsAttached;

            public TestIndexableObject(string name) { this.Name = name; }

            public override string ToString() { return Name; }

            public object Identity { get { return Name; } }

            public bool IsDetached
            {
                get { throw new NotImplementedException(); }
            }

            public bool IsHidden
            {
                get { throw new NotImplementedException(); }
            }

            public void Attached(TestScope scope)
            {
                this.IsAttached = true;
                this.AttachedScope = scope;
            }

            public void AssignIndex(int index)
            {
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
