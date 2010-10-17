using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibSvm
{
  abstract class Kernel : QMatrix
  {
    private SvmNode[][] x;
    private readonly double[] x_square;

    // svm_parameter
    private readonly KernelType kernel_type;
    private readonly int degree;
    private readonly double gamma;
    private readonly double coef0;

    //public abstract float[] get_Q(int column, int len);
    //public abstract double[] get_QD();

    public override void swap_index(int i, int j)
    {
      //do { SvmNode[] _ = x[i]; x[i] = x[j]; x[j] = _; } while (false);
      Common.Swap(ref x[i], ref x[j]);
      if (x_square != null)
      {
        Common.Swap(ref x_square[i], ref x_square[j]);
        //do { double _ = x_square[i]; x_square[i] = x_square[j]; x_square[j] = _; } while (false);
      }
    }

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

    protected double kernel_function(int i, int j)
    {
      switch (kernel_type)
      {
        case KernelType.LINEAR:
          return dot(x[i], x[j]);
        case KernelType.POLY:
          return powi(gamma * dot(x[i], x[j]) + coef0, degree);
        case KernelType.RBF:
          return Math.Exp(-gamma * (x_square[i] + x_square[j] - 2 * dot(x[i], x[j])));
        case KernelType.SIGMOID:
          return Math.Tanh(gamma * dot(x[i], x[j]) + coef0);
        case KernelType.PRECOMPUTED:
          return x[i][(int)(x[j][0].value)].value;
        default:
          return 0;	// java
      }
    }

    public Kernel(int l, SvmNode[][] x_, SvmParameter param)
    {
      this.kernel_type = param.kernel_type;
      this.degree = param.degree;
      this.gamma = param.gamma;
      this.coef0 = param.coef0;

      x = (SvmNode[][])x_.Clone();

      if (kernel_type == KernelType.RBF)
      {
        x_square = new double[l];
        for (int i = 0; i < l; i++)
          x_square[i] = dot(x[i], x[i]);
      }
      else x_square = null;
    }

    static double dot(SvmNode[] x, SvmNode[] y)
    {
      double sum = 0;
      int xlen = x.Length;
      int ylen = y.Length;
      int i = 0;
      int j = 0;
      while (i < xlen && j < ylen)
      {
        if (x[i].index == y[j].index)
          sum += x[i++].value * y[j++].value;
        else
        {
          if (x[i].index > y[j].index)
            ++j;
          else
            ++i;
        }
      }
      return sum;
    }

    public static double k_function(SvmNode[] x, SvmNode[] y, SvmParameter param)
    {
      switch (param.kernel_type)
      {
        case KernelType.LINEAR:
          return dot(x, y);
        case KernelType.POLY:
          return powi(param.gamma * dot(x, y) + param.coef0, param.degree);
        case KernelType.RBF:
          {
            double sum = 0;
            int xlen = x.Length;
            int ylen = y.Length;
            int i = 0;
            int j = 0;
            while (i < xlen && j < ylen)
            {
              if (x[i].index == y[j].index)
              {
                double d = x[i++].value - y[j++].value;
                sum += d * d;
              }
              else if (x[i].index > y[j].index)
              {
                sum += y[j].value * y[j].value;
                ++j;
              }
              else
              {
                sum += x[i].value * x[i].value;
                ++i;
              }
            }

            while (i < xlen)
            {
              sum += x[i].value * x[i].value;
              ++i;
            }

            while (j < ylen)
            {
              sum += y[j].value * y[j].value;
              ++j;
            }

            return Math.Exp(-param.gamma * sum);
          }
        case KernelType.SIGMOID:
          return Math.Tanh(param.gamma * dot(x, y) + param.coef0);
        case KernelType.PRECOMPUTED:
          return x[(int)(y[0].value)].value;
        default:
          return 0;	// java
      }
    }
  }

}
