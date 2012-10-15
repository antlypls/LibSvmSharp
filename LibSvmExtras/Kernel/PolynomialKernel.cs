using LibSvm;

namespace LibSvmExtras.Kernel
{
  public sealed class PolynomialKernel : KernelBase<double[]>
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

    internal override void FillParameters(SvmParameter<double[]> param)
    {
      param.KernelFunc = Kernels.Polynomial(Gamma, Degree, R);
    }
  }
}
