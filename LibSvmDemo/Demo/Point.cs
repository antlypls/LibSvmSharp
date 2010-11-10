using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibSvm;

namespace LibSvmDemo.Demo
{
  internal struct Point
  {
    public Point(double x, double y)
      : this()
    {
      X = x;
      Y = y;
    }

    public double X { get; set; }
    public double Y { get; set; }
    public int Label { get; set; }

    public SvmNode[] ToSvmNodes()
    {
      return new[] { new SvmNode(1, X), new SvmNode(2, Y) };
    }

    public double[] ToArray()
    {
      return new[] { X, Y };
    }

  }
}
