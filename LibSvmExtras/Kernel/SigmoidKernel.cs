using System;
using LibSvm;

namespace LibSvmExtras.Kernel
{
  public sealed class SigmoidKernel : KernelBase
  {
    public double Gamma
    {
      get;
      private set;
    }

    public double R
    {
      get;
      private set;
    }

    public SigmoidKernel(double gamma, double r)
    {
      Gamma = gamma;
      R = r;
    }

    internal override void FillParameters(SvmParameter param)
    {
      param.KernelType = KernelType.Sigmoid;
      param.Gamma = Gamma;
      param.Coef0 = R;
    }
  }
}
