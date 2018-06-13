
using System;

namespace DragonFruit.SubCommands
{
    class Program
    {
        /*

            DragonFruit.SubCommands.exe Add 10 20
            Adding 10 and 20
            Result is: 30

            DragonFruit.SubCommands.exe Subtract 10 20
            Subtracting 20 from 10
            Result is: -10

         */
        public static void Add(long firstNumber, long secondNumber)
        {

            Console.WriteLine($"Adding {firstNumber} and {secondNumber}");
            DisplayResult(firstNumber + secondNumber);
        }

        public static void Subtract(long firstNumber, long secondNumber)
        {
            Console.WriteLine($"Subtracting {secondNumber} from {firstNumber}");
            DisplayResult(secondNumber - firstNumber);
        }

        private static void DisplayResult(long result)
        {
            Console.WriteLine($"Result is: {result}");
        }
    }
}
