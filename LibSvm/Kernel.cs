using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibSvm
{
  abstract class Kernel<TPattern> : QMatrix 
  {
    private Func<TPattern, TPattern, double> _kernelFunc;

    private readonly TPattern[] _x;
    //private readonly double[] _xSquare;

    // svm_parameter
    //private readonly KernelType kernel_type;
    //private readonly int degree;
    //private readonly double gamma;
    //private readonly double coef0;

    public override void SwapIndex(int i, int j)
    {
      Common.Swap(ref _x[i], ref _x[j]);
      //if (_xSquare != null)
      //{
      //  Common.Swap(ref _xSquare[i], ref _xSquare[j]);
      //}
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
      return _kernelFunc(_x[i], _x[j]);
      //switch (kernel_type)
      //{
      //  case KernelType.Linear:
      //    return dot(_x[i], _x[j]);
      //  case KernelType.Poly:
      //    return powi(gamma * dot(_x[i], _x[j]) + coef0, degree);
      //  case KernelType.Rbf:
      //    return Math.Exp(-gamma * (_xSquare[i] + _xSquare[j] - 2 * dot(_x[i], _x[j])));
      //  case KernelType.Sigmoid:
      //    return Math.Tanh(gamma * dot(_x[i], _x[j]) + coef0);
      //  case KernelType.Precomputed:
      //    return _x[i][(int)(_x[j][0].Value)].Value;
      //  default:
      //    throw new ApplicationException("Bad kernel_type");
      //}
    }

    protected Kernel(int l, TPattern[] x_, SvmParameter<TPattern> param)
    {
      //this.kernel_type = param.KernelType;
      //this.degree = param.Degree;
      //this.gamma = param.Gamma;
      //this.coef0 = param.Coef0;
      _kernelFunc = param.KernelFunc;

      _x = (TPattern[])x_.Clone();

      //if (kernel_type == KernelType.Rbf)
      //{
      //  _xSquare = new double[l];
      //  for (int i = 0; i < l; i++)
      //    _xSquare[i] = dot(_x[i], _x[i]);
      //}
      //else _xSquare = null;
    }

    //static double dot(TPattern x, TPattern y)
    //{
    //  double sum = 0;
    //  int xlen = x.Length;
    //  int ylen = y.Length;
    //  int i = 0;
    //  int j = 0;
    //  while (i < xlen && j < ylen)
    //  {
    //    if (x[i].Index == y[j].Index)
    //    {
    //      sum += x[i++].Value * y[j++].Value;
    //    }
    //    else
    //    {
    //      if (x[i].Index > y[j].Index)
    //      {
    //        ++j;
    //      }
    //      else
    //      {
    //        ++i;
    //      }
    //    }
    //  }
    //  return sum;
    //}

    public static double k_function(TPattern x, TPattern y, SvmParameter<TPattern> param)
    {
      return param.KernelFunc(x, y);
      //switch (param.KernelType)
      //{
      //  case KernelType.Linear:
      //    return dot(x, y);
      //  case KernelType.Poly:
      //    return powi(param.Gamma*dot(x, y) + param.Coef0, param.Degree);
      //  case KernelType.Rbf:
      //    double sum = 0;
      //    int xlen = x.Length;
      //    int ylen = y.Length;
      //    int i = 0;
      //    int j = 0;
      //    while (i < xlen && j < ylen)
      //    {
      //      if (x[i].Index == y[j].Index)
      //      {
      //        double d = x[i++].Value - y[j++].Value;
      //        sum += d * d;
      //      }
      //      else
      //      {
      //        if (x[i].Index > y[j].Index)
      //        {
      //          sum += y[j].Value*y[j].Value;
      //          ++j;
      //        }
      //        else
      //        {
      //          sum += x[i].Value*x[i].Value;
      //          ++i;
      //        }
      //      }
      //    }

      //    while (i < xlen)
      //    {
      //      sum += x[i].Value*x[i].Value;
      //      ++i;
      //    }

      //    while (j < ylen)
      //    {
      //      sum += y[j].Value*y[j].Value;
      //      ++j;
      //    }

      //    return Math.Exp(-param.Gamma*sum);
      //  case KernelType.Sigmoid:
      //    return Math.Tanh(param.Gamma*dot(x, y) + param.Coef0);
      //  case KernelType.Precomputed:
      //    return x[(int) (y[0].Value)].Value;
      //  default:
      //    throw new ApplicationException("Bad kernel_type");
      //}
    }
  }

}
