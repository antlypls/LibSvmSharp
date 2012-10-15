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

    public override void SwapIndex(int i, int j)
    {
      Common.Swap(ref _x[i], ref _x[j]);
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
    }

    protected Kernel(int l, TPattern[] x_, SvmParameter<TPattern> param)
    {
      _kernelFunc = param.KernelFunc;

      _x = (TPattern[])x_.Clone();
    }

    public static double k_function(TPattern x, TPattern y, SvmParameter<TPattern> param)
    {
      return param.KernelFunc(x, y);
    }
  }
}
