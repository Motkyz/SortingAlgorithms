using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SortingAlgorithms
{
    public class ArrayGenerator
    {   
        public static long[] CreateArray()
        {
            long[] array = new long[20];
            Random random = new Random();

            for (int i = 0; i < array.Length; i++)
            {
                array[i] = random.Next(1,20);
            }

            return array;
        }
    }
}
