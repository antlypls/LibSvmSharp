using System;
using System.Linq;

namespace LibSvm
{
  public static class Kernels
  {
    private static double powi(double base_, int times)
    {
      double tmp = base_, ret = 1.0;

      for (int t = times; t > 0; t /= 2)
      {
        if (t % 2 == 1) ret *= tmp;
        tmp = tmp * tmp;
      }
      return ret;
    }

    private static double RbfInt(double gamma, double[] x, double[] y)
    {
      var res = x.Zip(y, (x_, y_) => x_ - y_).Select(z => z * z).Sum();
      return Math.Exp(-1.0 * gamma * res);
    }

    private static double LinearInt(double[] x, double[] y)
    {
      return x.Zip(y, (x_, y_) => x_ * y_).Sum();
    }

    private static double PolynomialInt(double gamma, int degree, double r, double[] x, double[] y)
    {
      var dot = x.Zip(y, (x_, y_) => x_ * y_).Sum();
      return powi(gamma * dot + r, degree);
    }

    private static double SigmoidInt(double gamma, double r, double[] x, double[] y)
    {
      var dot = x.Zip(y, (x_, y_) => x_ * y_).Sum();
      return Math.Tanh(gamma * dot + r);
    }

    public static Func<double[], double[], double> Rbf(double gamma)
    {
      return (x, y) => RbfInt(gamma, x, y);
    }

    public static Func<double[], double[], double> Polynomial(double gamma, int degree, double r)
    {
      return (x, y) => PolynomialInt(gamma, degree, r, x, y);
    }

    public static Func<double[], double[], double> Linear()
    {
      return (x, y) => LinearInt(x, y);
    }

    public static Func<double[], double[], double> Sigmoid(double gamma, double r)
    {
      return (x, y) => SigmoidInt(gamma, r, x, y);
    }
  }
}
