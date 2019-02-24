using System;
using System.Collections;
using System.Collections.Generic;

using NUnit.Framework;
using Shouldly;

using static IngameScript.Program;

namespace Mixins.Test
{
    [TestFixture]
    public class DequeTests
    {
        [Test]
        public void Capacity_SetTo0_ActsLikeList()
        {
            var list = new List<int>()
            {
                Capacity = 0
            };
            list.Capacity.ShouldBe(0);

            var deque = new Deque<int>()
            {
                Capacity = 0
            };
            deque.Capacity.ShouldBe(0);
        }

        [Test]
        public void Capacity_SetNegative_ActsLikeList()
        {
            var list = new List<int>();
            Should.Throw<ArgumentException>(() => { list.Capacity = -1; });

            var deque = new Deque<int>();
            Should.Throw<ArgumentException>(() => { deque.Capacity = -1; });
        }

        [Test]
        public void Capacity_SetLarger_UsesSpecifiedCapacity()
        {
            var deque = new Deque<int>(1);
            deque.Capacity.ShouldBe(1);
            deque.Capacity = 17;
            deque.Capacity.ShouldBe(17);
        }

        [Test]
        public void Capacity_SetSmaller_UsesSpecifiedCapacity()
        {
            var deque = new Deque<int>(13);
            deque.Capacity.ShouldBe(13);
            deque.Capacity = 7;
            deque.Capacity.ShouldBe(7);
        }

        [Test]
        public void Capacity_Set_PreservesData()
        {
            var deque = new Deque<int>(new int[] { 1, 2, 3 });
            deque.Capacity.ShouldBe(3);
            deque.Capacity = 7;
            deque.Capacity.ShouldBe(7);
            deque.ShouldBe(new[] { 1, 2, 3 });
        }

        [Test]
        public void Capacity_Set_WhenSplit_PreservesData()
        {
            var deque = new Deque<int>(new int[] { 1, 2, 3 });
            deque.RemoveFromFront();
            deque.AddToBack(4);
            deque.Capacity.ShouldBe(3);
            deque.Capacity = 7;
            deque.Capacity.ShouldBe(7);
            deque.ShouldBe(new[] { 2, 3, 4 });
        }

        [Test]
        public void Capacity_Set_SmallerThanCount_ActsLikeList()
        {
            var list = new List<int>(new int[] { 1, 2, 3 });
            list.Capacity.ShouldBe(3);
            Should.Throw<ArgumentException>(() => { list.Capacity = 2; });

            var deque = new Deque<int>(new int[] { 1, 2, 3 });
            deque.Capacity.ShouldBe(3);
            Should.Throw<ArgumentException>(() => { deque.Capacity = 2; });
        }

        [Test]
        public void Capacity_Set_ToItself_DoesNothing()
        {
            var deque = new Deque<int>(13);
            deque.Capacity.ShouldBe(13);
            deque.Capacity = 13;
            deque.Capacity.ShouldBe(13);
        }

        // Implementation detail: the default capacity.
        const int DefaultCapacity = 8;

        [Test]
        public void Constructor_WithoutExplicitCapacity_UsesDefaultCapacity()
        {
            var deque = new Deque<int>();
            deque.Capacity.ShouldBe(DefaultCapacity);
        }

        [Test]
        public void Constructor_CapacityOf0_ActsLikeList()
        {
            var list = new List<int>(0);
            list.Capacity.ShouldBe(0);

            var deque = new Deque<int>(0);
            deque.Capacity.ShouldBe(0);
        }

        [Test]
        public void Constructor_CapacityOf0_PermitsAdd()
        {
            var deque = new Deque<int>(0);
            deque.AddToBack(13);
            deque.ShouldBe(new[] { 13 });
        }

        [Test]
        public void Constructor_NegativeCapacity_ActsLikeList()
        {
            Should.Throw<ArgumentException>(() => new List<int>(-1));

            Should.Throw<ArgumentException>(() => new Deque<int>(-1));
        }

        [Test]
        public void Constructor_CapacityOf1_UsesSpecifiedCapacity()
        {
            var deque = new Deque<int>(1);
            deque.Capacity.ShouldBe(1);
        }

        [Test]
        public void Constructor_FromEmptySequence_UsesDefaultCapacity()
        {
            var deque = new Deque<int>(new int[] { });
            deque.Capacity.ShouldBe(DefaultCapacity);
        }

        [Test]
        public void Constructor_FromSequence_InitializesFromSequence()
        {
            var deque = new Deque<int>(new int[] { 1, 2, 3 });
            deque.Capacity.ShouldBe(3);
            deque.Count.ShouldBe(3);
            deque.ShouldBe(new int[] { 1, 2, 3 });
        }

        [Test]
        public void IndexOf_ItemPresent_ReturnsItemIndex()
        {
            var deque = new Deque<int>(new[] { 1, 2 });
            var result = deque.IndexOf(2);
            result.ShouldBe(1);
        }

        [Test]
        public void IndexOf_ItemNotPresent_ReturnsNegativeOne()
        {
            var deque = new Deque<int>(new[] { 1, 2 });
            var result = deque.IndexOf(3);
            result.ShouldBe(-1);
        }

        [Test]
        public void IndexOf_ItemPresentAndSplit_ReturnsItemIndex()
        {
            var deque = new Deque<int>(new[] { 1, 2, 3 });
            deque.RemoveFromBack();
            deque.AddToFront(0);
            deque.IndexOf(0).ShouldBe(0);
            deque.IndexOf(1).ShouldBe(1);
            deque.IndexOf(2).ShouldBe(2);
        }

        [Test]
        public void Contains_ItemPresent_ReturnsTrue()
        {
            var deque = new Deque<int>(new[] { 1, 2 }) as ICollection<int>;
            deque.Contains(2).ShouldBeTrue();
        }

        [Test]
        public void Contains_ItemNotPresent_ReturnsFalse()
        {
            var deque = new Deque<int>(new[] { 1, 2 }) as ICollection<int>;
            deque.Contains(3).ShouldBeFalse();
        }

        [Test]
        public void Contains_ItemPresentAndSplit_ReturnsTrue()
        {
            var deque = new Deque<int>(new[] { 1, 2, 3 });
            deque.RemoveFromBack();
            deque.AddToFront(0);
            var deq = deque as ICollection<int>;
            deq.Contains(0).ShouldBeTrue();
            deq.Contains(1).ShouldBeTrue();
            deq.Contains(2).ShouldBeTrue();
            deq.Contains(3).ShouldBeFalse();
        }

        [Test]
        public void Add_IsAddToBack()
        {
            var deque1 = new Deque<int>(new[] { 1, 2 });
            var deque2 = new Deque<int>(new[] { 1, 2 });
            ((ICollection<int>)deque1).Add(3);
            deque2.AddToBack(3);
            deque2.ShouldBe(deque1);
        }

        [Test]
        public void NonGenericEnumerator_EnumeratesItems()
        {
            var deque = new Deque<int>(new[] { 1, 2 });
            var results = new List<int>();
            var objEnum = ((System.Collections.IEnumerable)deque).GetEnumerator();
            while (objEnum.MoveNext())
            {
                results.Add((int)objEnum.Current);
            }
            deque.ShouldBe(results);
        }

        [Test]
        public void IsReadOnly_ReturnsFalse()
        {
            var deque = new Deque<int>();
            ((ICollection<int>)deque).IsReadOnly.ShouldBeFalse();
        }

        [Test]
        public void CopyTo_CopiesItems()
        {
            var deque = new Deque<int>(new[] { 1, 2, 3 });
            var results = new int[3];
            ((ICollection<int>)deque).CopyTo(results, 0);
        }

        [Test]
        public void CopyTo_NullArray_ActsLikeList()
        {
            var list = new List<int>(new[] { 1, 2, 3 });
            Should.Throw<ArgumentNullException>(() => ((ICollection<int>)list).CopyTo(null, 0));

            var deque = new Deque<int>(new[] { 1, 2, 3 });
            Should.Throw<ArgumentNullException>(() => ((ICollection<int>)deque).CopyTo(null, 0));
        }

        [Test]
        public void CopyTo_NegativeOffset_ActsLikeList()
        {
            var destination = new int[3];
            var list = new List<int>(new[] { 1, 2, 3 });
            Should.Throw<ArgumentException>(() => ((ICollection<int>)list).CopyTo(destination, -1));

            var deque = new Deque<int>(new[] { 1, 2, 3 });
            Should.Throw<ArgumentException>(() => ((ICollection<int>)deque).CopyTo(destination, -1));
        }

        [Test]
        public void CopyTo_InsufficientSpace_ActsLikeList()
        {
            var destination = new int[3];
            var list = new List<int>(new[] { 1, 2, 3 });
            Should.Throw<ArgumentException>(() => ((ICollection<int>)list).CopyTo(destination, 1));

            var deque = new Deque<int>(new[] { 1, 2, 3 });
            Should.Throw<ArgumentException>(() => ((ICollection<int>)deque).CopyTo(destination, 1));
        }

        [Test]
        public void Clear_EmptiesAllItems()
        {
            var deque = new Deque<int>(new[] { 1, 2, 3 });
            deque.Clear();
            deque.Count.ShouldBe(0);
            deque.ShouldBe(new int[] { });
        }

        [Test]
        public void Clear_DoesNotChangeCapacity()
        {
            var deque = new Deque<int>(new[] { 1, 2, 3 });
            deque.Capacity.ShouldBe(3);
            deque.Clear();
            deque.Capacity.ShouldBe(3);
        }

        [Test]
        public void RemoveFromFront_Empty_ActsLikeStack()
        {
            var stack = new Stack<int>();
            Should.Throw<InvalidOperationException>(() => stack.Pop());

            var deque = new Deque<int>();
            Should.Throw<InvalidOperationException>(() => deque.RemoveFromFront());
        }

        [Test]
        public void RemoveFromBack_Empty_ActsLikeQueue()
        {
            var queue = new Queue<int>();
            Should.Throw<InvalidOperationException>(() => queue.Dequeue());

            var deque = new Deque<int>();
            Should.Throw<InvalidOperationException>(() => deque.RemoveFromBack());
        }

        [Test]
        public void Remove_ItemPresent_RemovesItemAndReturnsTrue()
        {
            var deque = new Deque<int>(new[] { 1, 2, 3, 4 });
            var result = deque.Remove(3);
            result.ShouldBeTrue();
            deque.ShouldBe(new[] { 1, 2, 4 });
        }

        [Test]
        public void Remove_ItemNotPresent_KeepsItemsReturnsFalse()
        {
            var deque = new Deque<int>(new[] { 1, 2, 3, 4 });
            var result = deque.Remove(5);
            result.ShouldBeFalse();
            deque.ShouldBe(new[] { 1, 2, 3, 4 });
        }

        [Test]
        public void Insert_InsertsElementAtIndex()
        {
            var deque = new Deque<int>(new[] { 1, 2 });
            deque.Insert(1, 13);
            deque.ShouldBe(new[] { 1, 13, 2 });
        }

        [Test]
        public void Insert_AtIndex0_IsSameAsAddToFront()
        {
            var deque1 = new Deque<int>(new[] { 1, 2 });
            var deque2 = new Deque<int>(new[] { 1, 2 });
            deque1.Insert(0, 0);
            deque2.AddToFront(0);
            deque2.ShouldBe(deque1);
        }

        [Test]
        public void Insert_AtCount_IsSameAsAddToBack()
        {
            var deque1 = new Deque<int>(new[] { 1, 2 });
            var deque2 = new Deque<int>(new[] { 1, 2 });
            deque1.Insert(deque1.Count, 0);
            deque2.AddToBack(0);
            deque2.ShouldBe(deque1);
        }

        [Test]
        public void Insert_NegativeIndex_ActsLikeList()
        {
            var list = new List<int>(new[] { 1, 2, 3 });
            Should.Throw<ArgumentException>(() => list.Insert(-1, 0));

            var deque = new Deque<int>(new[] { 1, 2, 3 });
            Should.Throw<ArgumentException>(() => deque.Insert(-1, 0));
        }

        [Test]
        public void Insert_IndexTooLarge_ActsLikeList()
        {
            var list = new List<int>(new[] { 1, 2, 3 });
            Should.Throw<ArgumentException>(() => list.Insert(list.Count + 1, 0));

            var deque = new Deque<int>(new[] { 1, 2, 3 });
            Should.Throw<ArgumentException>(() => deque.Insert(deque.Count + 1, 0));
        }

        [Test]
        public void RemoveAt_RemovesElementAtIndex()
        {
            var deque = new Deque<int>(new[] { 1, 2, 3 });
            deque.RemoveFromBack();
            deque.AddToFront(0);
            deque.RemoveAt(1);
            deque.ShouldBe(new[] { 0, 2 });
        }

        [Test]
        public void RemoveAt_Index0_IsSameAsRemoveFromFront()
        {
            var deque1 = new Deque<int>(new[] { 1, 2 });
            var deque2 = new Deque<int>(new[] { 1, 2 });
            deque1.RemoveAt(0);
            deque2.RemoveFromFront();
            deque2.ShouldBe(deque1);
        }

        [Test]
        public void RemoveAt_LastIndex_IsSameAsRemoveFromBack()
        {
            var deque1 = new Deque<int>(new[] { 1, 2 });
            var deque2 = new Deque<int>(new[] { 1, 2 });
            deque1.RemoveAt(1);
            deque2.RemoveFromBack();
            deque2.ShouldBe(deque1);
        }

        [Test]
        public void RemoveAt_NegativeIndex_ActsLikeList()
        {
            var list = new List<int>(new[] { 1, 2, 3 });
            Should.Throw<ArgumentException>(() => list.RemoveAt(-1));

            var deque = new Deque<int>(new[] { 1, 2, 3 });
            Should.Throw<ArgumentException>(() => deque.RemoveAt(-1));
        }

        [Test]
        public void RemoveAt_IndexTooLarge_ActsLikeList()
        {
            var list = new List<int>(new[] { 1, 2, 3 });
            Should.Throw<ArgumentException>(() => list.RemoveAt(list.Count));

            var deque = new Deque<int>(new[] { 1, 2, 3 });
            Should.Throw<ArgumentException>(() => deque.RemoveAt(deque.Count));
        }

        [Test]
        public void InsertMultiple()
        {
            InsertTest(new[] { 1, 2, 3 }, new[] { 7, 13 });
            InsertTest(new[] { 1, 2, 3, 4 }, new[] { 7, 13 });
        }

        private void InsertTest(IReadOnlyCollection<int> initial, IReadOnlyCollection<int> items)
        {
            var totalCapacity = initial.Count + items.Count;
            for (int rotated = 0; rotated <= totalCapacity; ++rotated)
            {
                for (int index = 0; index <= initial.Count; ++index)
                {
                    // Calculate the expected result using the slower List<int>.
                    var result = new List<int>(initial);
                    for (int i = 0; i != rotated; ++i)
                    {
                        var item = result[0];
                        result.RemoveAt(0);
                        result.Add(item);
                    }
                    result.InsertRange(index, items);

                    // First, start off the deque with the initial items.
                    var deque = new Deque<int>(initial);

                    // Ensure there's enough room for the inserted items.
                    deque.Capacity += items.Count;

                    // Rotate the existing items.
                    for (int i = 0; i != rotated; ++i)
                    {
                        var item = deque[0];
                        deque.RemoveFromFront();
                        deque.AddToBack(item);
                    }

                    // Do the insert.
                    deque.InsertRange(index, items);

                    // Ensure the results are as expected.
                    deque.ShouldBe(result);
                }
            }
        }

        [Test]
        public void Insert_RangeOfZeroElements_HasNoEffect()
        {
            var deque = new Deque<int>(new[] { 1, 2, 3 });
            deque.InsertRange(1, new int[] { });
            deque.ShouldBe(new[] { 1, 2, 3 });
        }

        [Test]
        public void InsertMultiple_MakesRoomForNewElements()
        {
            var deque = new Deque<int>(new[] { 1, 2, 3 });
            deque.InsertRange(1, new[] { 7, 13 });
            deque.ShouldBe(new[] { 1, 7, 13, 2, 3 });
            deque.Capacity.ShouldBe(5);
        }

        [Test]
        public void RemoveMultiple()
        {
            RemoveTest(new[] { 1, 2, 3 });
            RemoveTest(new[] { 1, 2, 3, 4 });
        }

        private void RemoveTest(IReadOnlyCollection<int> initial)
        {
            for (int count = 0; count <= initial.Count; ++count)
            {
                for (int rotated = 0; rotated <= initial.Count; ++rotated)
                {
                    for (int index = 0; index <= initial.Count - count; ++index)
                    {
                        // Calculated the expected result using the slower List<int>.
                        var result = new List<int>(initial);
                        for (int i = 0; i != rotated; ++i)
                        {
                            var item = result[0];
                            result.RemoveAt(0);
                            result.Add(item);
                        }
                        result.RemoveRange(index, count);

                        // First, start off the deque with the initial items.
                        var deque = new Deque<int>(initial);

                        // Rotate the existing items.
                        for (int i = 0; i != rotated; ++i)
                        {
                            var item = deque[0];
                            deque.RemoveFromFront();
                            deque.AddToBack(item);
                        }

                        // Do the remove.
                        deque.RemoveRange(index, count);

                        // Ensure the results are as expected.
                        deque.ShouldBe(result);
                    }
                }
            }
        }

        [Test]
        public void Remove_RangeOfZeroElements_HasNoEffect()
        {
            var deque = new Deque<int>(new[] { 1, 2, 3 });
            deque.RemoveRange(1, 0);
            deque.ShouldBe(new[] { 1, 2, 3 });
        }

        [Test]
        public void Remove_NegativeCount_ActsLikeList()
        {
            var list = new List<int>(new[] { 1, 2, 3 });
            Should.Throw<ArgumentException>(() => list.RemoveRange(1, -1));

            var deque = new Deque<int>(new[] { 1, 2, 3 });
            Should.Throw<ArgumentException>(() => deque.RemoveRange(1, -1));
        }

        [Test]
        public void GetItem_ReadsElements()
        {
            var deque = new Deque<int>(new[] { 1, 2, 3 });
            deque[0].ShouldBe(1);
            deque[1].ShouldBe(2);
            deque[2].ShouldBe(3);
        }

        [Test]
        public void GetItem_Split_ReadsElements()
        {
            var deque = new Deque<int>(new[] { 1, 2, 3 });
            deque.RemoveFromBack();
            deque.AddToFront(0);
            deque[0].ShouldBe(0);
            deque[1].ShouldBe(1);
            deque[2].ShouldBe(2);
        }

        [Test]
        public void GetItem_IndexTooLarge_ActsLikeList()
        {
            var list = new List<int>(new[] { 1, 2, 3 });
            Should.Throw<ArgumentException>(() => list[3]);

            var deque = new Deque<int>(new[] { 1, 2, 3 });
            Should.Throw<ArgumentException>(() => deque[3]);
        }

        [Test]
        public void GetItem_NegativeIndex_ActsLikeList()
        {
            var list = new List<int>(new[] { 1, 2, 3 });
            Should.Throw<ArgumentException>(() => list[-1]);

            var deque = new Deque<int>(new[] { 1, 2, 3 });
            Should.Throw<ArgumentException>(() => deque[-1]);
        }

        [Test]
        public void SetItem_WritesElements()
        {
            var deque = new Deque<int>(new[] { 1, 2, 3 });
            deque[0] = 7;
            deque[1] = 11;
            deque[2] = 13;
            deque.ShouldBe(new[] { 7, 11, 13 });
        }

        [Test]
        public void SetItem_Split_WritesElements()
        {
            var deque = new Deque<int>(new[] { 1, 2, 3 });
            deque.RemoveFromBack();
            deque.AddToFront(0);
            deque[0] = 7;
            deque[1] = 11;
            deque[2] = 13;
            deque.ShouldBe(new[] { 7, 11, 13 });
        }

        [Test]
        public void SetItem_IndexTooLarge_ActsLikeList()
        {
            var list = new List<int>(new[] { 1, 2, 3 });
            Should.Throw<ArgumentException>(() => { list[3] = 13; });

            var deque = new Deque<int>(new[] { 1, 2, 3 });
            Should.Throw<ArgumentException>(() => { deque[3] = 13; });
        }

        [Test]
        public void SetItem_NegativeIndex_ActsLikeList()
        {
            var list = new List<int>(new[] { 1, 2, 3 });
            Should.Throw<ArgumentException>(() => { list[-1] = 13; });

            var deque = new Deque<int>(new[] { 1, 2, 3 });
            Should.Throw<ArgumentException>(() => { deque[-1] = 13; });
        }

        [Test]
        public void ToArray_CopiesToNewArray()
        {
            var deque = new Deque<int>(new[] { 0, 1 });
            deque.AddToBack(13);
            var result = deque.ToArray();
            result.ShouldBe(new[] { 0, 1, 13 });
        }
    }
}