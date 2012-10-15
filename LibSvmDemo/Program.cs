using System;
using LibSvmDemo.Demo;

namespace LibSvmDemo
{
  class Program
  {
    static void Main(string[] args)
    {
      CSVMDemo.Run();
      NuSVMDemo.Run();
      EpsSVRDemo.Run();
      NuSVRDemo.Run();
      OneClassDemo.Run();

      Console.ReadKey();
    }
  }
}
