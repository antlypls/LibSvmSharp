using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibSvm;

namespace LibSvmExtras.Kernel
{
  public sealed class CustomKernel<T> : KernelBase<T>
  {
    private Func<T, T, double> _func = null;

    public CustomKernel(Func<T, T, double> func)
    {
      _func = func;
    }

    internal override void FillParameters(SvmParameter<T> param)
    {
      param.KernelFunc = _func;
    }
  }
}
