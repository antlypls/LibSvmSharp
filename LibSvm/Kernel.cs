using System;

namespace LibSvm
{
  abstract class Kernel<TPattern> : QMatrix
  {
    private readonly Func<TPattern, TPattern, double> _kernelFunc;

    private readonly TPattern[] _x;

    public override void SwapIndex(int i, int j)
    {
      Common.Swap(ref _x[i], ref _x[j]);
    }

    protected double kernel_function(int i, int j)
    {
      return _kernelFunc(_x[i], _x[j]);
    }

    protected Kernel(TPattern[] x, SvmParameter<TPattern> param)
    {
      _kernelFunc = param.KernelFunc;

      _x = (TPattern[])x.Clone();
    }
  }
}
