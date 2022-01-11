using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HPCsharp;

namespace HPCsharpExamples
{
    public class UserDefinedClass
    {
        public uint Key;
        public int Index;

        public UserDefinedClass(uint key, int index)
        {
            Key   = key;
            Index = index;
        }

        public override string ToString()
        {
            return String.Format("({0,2} : {1,2})", Key, Index);
        }
    }

    public class UserDefinedClassComparer : IComparer<UserDefinedClass>
    {
        public int Compare(UserDefinedClass first, UserDefinedClass second)
        {
            return (int)first.Key - (int)second.Key;
        }
    }

    public class UserDefinedClassEquality : IEqualityComparer<UserDefinedClass>
    {
        public bool Equals(UserDefinedClass x, UserDefinedClass y)
        {
            return x.Key == y.Key;
        }
        public int GetHashCode(UserDefinedClass obj)    // Do not use. Just a placeholder to make IEqualityComparer happy
        {
            return (int)obj.Key;
        }
    }

    class SortOfUserDefinedClass
    {
        public static void SimpleInPlaceExample()
        {
            var comparer = new UserDefinedClassComparer();

            UserDefinedClass[] userArray = new UserDefinedClass[]
            {
                new UserDefinedClass(16, 0),
                new UserDefinedClass(12, 1),
                new UserDefinedClass(18, 2),
                new UserDefinedClass(18, 3),
                new UserDefinedClass(10, 4),
                new UserDefinedClass( 2, 5)
            };
            Console.Write("Unsorted array of user defined class: ");
            foreach (UserDefinedClass item in userArray)
                Console.Write(item);
            Console.WriteLine();

            Array.Sort(userArray, comparer);                        // Serial   Array.Sort (serial, not stable)
            Algorithm.SortMergeInPlace(userArray, comparer);        // Serial   Merge Sort. Direct function call syntax (serial, stable)

            userArray.SortMergeInPlace(comparer);                   // Serial   Merge Sort. Extension method syntax (serial, stable)
            userArray.SortMergeInPlaceAdaptivePar(comparer);        // Parallel Merge Sort (parallel, not stable)
            userArray.SortMergePseudoInPlaceStablePar(comparer);    // Parallel Merge Sort (parallel, stable)

            Console.Write("Sorted   array of user defined class: ");
            foreach (UserDefinedClass item in userArray)
                Console.Write(item);
            Console.WriteLine();
        }

        public static void SimpleNotInPlaceExample()
        {
            var comparer = new UserDefinedClassComparer();

            UserDefinedClass[] userArray = new UserDefinedClass[]
            {
                new UserDefinedClass(16, 0),
                new UserDefinedClass(12, 1),
                new UserDefinedClass(18, 2),
                new UserDefinedClass(18, 3),
                new UserDefinedClass(10, 4),
                new UserDefinedClass( 2, 5)
            };
            UserDefinedClass[] sortedUserArray;

            Console.Write("Unsorted array of user defined class: ");
            foreach (UserDefinedClass item in userArray)
                Console.Write(item);
            Console.WriteLine();

            sortedUserArray = userArray.OrderBy(element => element.Key).ToArray();              // Serial   Linq  Sort (C# standard sort, stable)
            sortedUserArray = userArray.SortRadix(element => element.Key);                      // Serial   Radix Sort (stable)

            sortedUserArray = Algorithm.SortMerge(userArray, comparer);                         // Serial   Merge Sort (stable)

            sortedUserArray = userArray.AsParallel().OrderBy(element => element.Key).ToArray(); // Parallel Linq  Sort (C# standard sort, stable)
            sortedUserArray = ParallelAlgorithm.SortMergePar(userArray, comparer);              // Parallel Merge Sort
            sortedUserArray = ParallelAlgorithm.SortMergeStablePar(userArray, comparer);        // Parallel Merge Sort (stable)

            Console.Write("Sorted   array of user defined class: ");
            foreach (UserDefinedClass item in sortedUserArray)
                Console.Write(item);
            Console.WriteLine();
        }
    }
}
