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

    public class UserClassComparer : IComparer<UserDefinedClass>
    {
        public int Compare(UserDefinedClass first, UserDefinedClass second)
        {
            return (int)first.Key - (int)second.Key;
        }
    }

    class MergeSortOfUserDefinedClass
    {
        public static void SimpleInPlaceExample()
        {
            var comparer = new UserClassComparer();

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

            Algorithm.SortMergeInPlace(userArray, comparer);                        // Serial   Merge Sort
            //ParallelAlgorithm.SortMergeInPlacePar(userArray, comparer);           // Parallel Merge Sort
            //ParallelAlgorithm.SortMergeInPlaceStablePar(userArray, comparer);     // Parallel Merge Sort (stable)
            //Array.Sort(userArray, comparer);                                      // Serial   Array.Sort (C# standard sort)

            Console.Write("Sorted   array of user defined class: ");
            foreach (UserDefinedClass item in userArray)
                Console.Write(item);
            Console.WriteLine();
        }

        public static void SimpleNotInPlaceExample()
        {
            var comparer = new UserClassComparer();

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

            UserDefinedClass[] sortedUserArray = Algorithm.SortMerge(userArray, comparer);                         // Serial   Merge Sort
            //UserDefinedClass[] sortedUserArray = ParallelAlgorithm.SortMergePar(userArray, comparer);              // Parallel Merge Sort
            //UserDefinedClass[] sortedUserArray = ParallelAlgorithm.SortMergeStablePar(userArray, comparer);        // Parallel Merge Sort (stable)
            //UserDefinedClass[] sortedUserArray = userArray.OrderBy(element => element.Key).ToArray();              // Serial   Linq  Sort (C# standard sort, stable)
            //UserDefinedClass[] sortedUserArray = userArray.AsParallel().OrderBy(element => element.Key).ToArray(); // Parallel Linq  Sort (C# standard sort, stable)

            Console.Write("Sorted   array of user defined class: ");
            foreach (UserDefinedClass item in sortedUserArray)
                Console.Write(item);
            Console.WriteLine();
        }
    }
}
