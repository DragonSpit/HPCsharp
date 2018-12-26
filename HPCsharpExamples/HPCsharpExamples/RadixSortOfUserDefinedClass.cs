using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HPCsharp;

namespace HPCsharpExamples
{
    class RadixSortOfUserDefinedClass
    {
        public static void SimpleExample()
        {
            UserDefinedClass[] userArray = new UserDefinedClass[]   // Same UserDefinedClass as is defined in MergeSortOfUserDefinedClass.cs
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

            UserDefinedClass[] sortedUserArray = userArray.SortRadix(element => element.Key);                        // Serial   Radix Sort (stable)
            //UserDefinedClass[] sortedUserArray = userArray.OrderBy(element => element.Key).ToArray();              // Serial   Linq  Sort (C# standard sort, stable)
            //UserDefinedClass[] sortedUserArray = userArray.AsParallel().OrderBy(element => element.Key).ToArray(); // Parallel Linq  Sort (C# standard sort, stable)

            Console.Write("Sorted   array of user defined class: ");
            foreach (UserDefinedClass item in sortedUserArray)
                Console.Write(item);
            Console.WriteLine();
        }
    }
}
