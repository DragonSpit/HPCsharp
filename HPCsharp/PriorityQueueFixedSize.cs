using System;
using System.Collections.Generic;
using System.Text;

namespace HPCsharpFuture
{
    // Based on November, 2012 article in https://visualstudiomagazine.com/articles/2012/11/01/priority-queues-with-c.aspx
    // but with all allocations eliminated for the case where the maximum size of the priority queue is known in advance.

    public struct ValueAndWhichSpan<T>
    {
        T Value;
        Int32 WhichSpan;

        public ValueAndWhichSpan(T value, Int32 whichSpan)
        {
            Value = value;
            WhichSpan = whichSpan;
        }
    }

    public class PriorityQueueFixedSize<T> where T : IComparable<T>
    {
        private T[] data;
        Int32 currentLength;

        public PriorityQueueFixedSize(Int32 maxLength)
        {
            this.data = new T[maxLength];
            currentLength = 0;
        }

        public void Enqueue(T item)
        {
            data[currentLength++] = item;
            int ci = currentLength - 1; // child index; start at end
            while (ci > 0)
            {
                int pi = (ci - 1) / 2; // parent index
                if (data[ci].CompareTo(data[pi]) >= 0) break; // child item is larger than (or equal) parent so we're done
                T tmp = data[ci]; data[ci] = data[pi]; data[pi] = tmp;
                ci = pi;
            }
        }

        public T Dequeue()
        {
            // assumes pq is not empty; up to calling code
            int li = currentLength - 1; // last index (before removal)
            T frontItem = data[0];   // fetch the front
            data[0] = data[li];
            currentLength--;

            --li; // last index (after removal)
            int pi = 0; // parent index. start at front of pq
            while (true)
            {
                int ci = pi * 2 + 1; // left child index of parent
                if (ci > li) break;  // no children so done
                int rc = ci + 1;     // right child
                if (rc <= li && data[rc].CompareTo(data[ci]) < 0) // if there is a rc (ci + 1), and it is smaller than left child, use the rc instead
                    ci = rc;
                if (data[pi].CompareTo(data[ci]) <= 0) break; // parent is smaller than (or equal to) smallest child so done
                T tmp = data[pi]; data[pi] = data[ci]; data[ci] = tmp; // swap parent and child
                pi = ci;
            }
            return frontItem;
        }

        public T Peek()
        {
            T frontItem = data[0];
            return frontItem;
        }

        public int Length()
        {
            return currentLength;
        }

        public override string ToString()
        {
            string s = "";
            for (int i = 0; i < currentLength; ++i)
                s += data[i].ToString() + " ";
            s += "length = " + currentLength;
            return s;
        }

        public bool IsConsistent()
        {
            // is the heap property true for all data?
            if (currentLength == 0) return true;
            int li = currentLength - 1; // last index
            for (int pi = 0; pi < currentLength; ++pi) // each parent index
            {
                int lci = 2 * pi + 1; // left child index
                int rci = 2 * pi + 2; // right child index

                if (lci <= li && data[pi].CompareTo(data[lci]) > 0) return false; // if lc exists and it's greater than parent then bad.
                if (rci <= li && data[pi].CompareTo(data[rci]) > 0) return false; // check the right child too.
            }
            return true; // passed all checks
        }
    }
}
