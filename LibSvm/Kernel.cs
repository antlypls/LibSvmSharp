using System;

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

    protected double kernel_function(int i, int j)
    {
      return _kernelFunc(_x[i], _x[j]);
    }

    protected Kernel(TPattern[] x_, SvmParameter<TPattern> param)
    {
      _kernelFunc = param.KernelFunc;

      _x = (TPattern[])x_.Clone();
    }

    //public static double k_function(TPattern x, TPattern y, SvmParameter<TPattern> param)
    //{
    //  return param.KernelFunc(x, y);
    //}
  }
}
