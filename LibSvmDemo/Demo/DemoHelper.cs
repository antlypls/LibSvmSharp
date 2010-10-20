using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibSvm;

namespace LibSvmDemo.Demo
{
  internal static class DemoHelper
  {
    public static SvmNode[] ToSvmNodes(this double x)
    {
      return new[] { new SvmNode(1, x) };
    }

    public static double Sinc(double x)
    {
      return x == 0 ? 1 : Math.Sin(x) / x;
    }

    public static List<Point> GenerateClass(int label, double x, double y, int count)
    {
      var rndx = new Random();
      var data = Enumerable.Range(0, count).Select(_ => new Point
      {
        X = x + (rndx.NextDouble() - 0.5) / 4,
        Y = y + (rndx.NextDouble() - 0.5) / 4,
        Label = label
      });

      return data.ToList();
    }
  }
}
