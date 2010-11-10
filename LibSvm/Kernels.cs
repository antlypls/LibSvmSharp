using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibSvm
{
  public static class Kernels
  {
    //    Linear,
    //Poly,
    //Rbf,
    //Sigmoid,

    private static double RbfInt(double gamma, double[] x, double[] y)
    {
      var res = x.Zip(y, (x_, y_) => x_ - y_).Select(z => z * z).Sum();
      return Math.Exp(-1.0 * gamma * res);
    }

    public static Func<double[], double[], double> Rbf(double gamma)
    {
      return (x,y) =>  RbfInt(gamma, x,y);
    }
  }
}
