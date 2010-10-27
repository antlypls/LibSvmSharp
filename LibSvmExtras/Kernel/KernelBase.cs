using System;
using LibSvm;

namespace LibSvmExtras.Kernel
{
  public abstract class KernelBase : IKernel
  {
    internal abstract void FillParameters(SvmParameter param);
  }
}
