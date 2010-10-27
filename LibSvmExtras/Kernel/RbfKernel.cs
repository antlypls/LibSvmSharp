using System;
using LibSvm;

namespace LibSvmExtras.Kernel
{
  public sealed class RbfKernel : KernelBase
  {
    public double Gamma 
    {
      get; 
      private set; 
    }

    public RbfKernel(double gamma)
    {
      Gamma = gamma;
    }

    internal override void FillParameters(SvmParameter param)
    {
      param.KernelType = KernelType.Rbf;
      param.Gamma = Gamma;
    }
  }
}
