using System;
using LibSvm;

namespace LibSvmExtras.Kernel
{
  public sealed class LinearKernel : KernelBase
  {
    internal override void FillParameters(SvmParameter param)
    {
      param.KernelType = KernelType.Linear;
    }
  }
}
