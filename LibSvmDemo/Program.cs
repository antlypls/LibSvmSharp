using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibSvm;
using LibSvmDemo.Demo;

namespace LibSvmDemo
{
  class Program
  {
    static void Main(string[] args)
    {
      CSVMDemo.Run();
      OneClassDemo.Run();

      Console.ReadKey();
    }
  }
}
