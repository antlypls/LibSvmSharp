using System;
using LibSvm;

namespace LibSvmExtras.Kernel
{
  public sealed class PolynomialKernel : KernelBase
  {
    public double Gamma
    {
      get;
      private set;
    }

    public int Degree
    {
      get;
      private set;
    }

    public double R
    {
      get;
      private set;
    }

    public PolynomialKernel(double gamma, int degree, double r)
    {
      Gamma = gamma;
      Degree = degree;
      R = r;
    }

    internal override void FillParameters(SvmParameter param)
    {
      param.KernelType = KernelType.Poly;
      param.Gamma = Gamma;
      param.Degree = Degree;
      param.Coef0 = R;
    }
  }
}
