using System;

namespace ConsoleDemo
{
  class Program
  {
    static void Main(string[] args)
    {
      Console.WriteLine("Hello Basic Maths!");
      var maths = new BasicMaths();

      Console.WriteLine($"1 + 1 = {maths.Add(1, 1)}");
    }
  }
}
