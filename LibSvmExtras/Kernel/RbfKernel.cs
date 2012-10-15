using LibSvm;

namespace LibSvmExtras.Kernel
{
  public sealed class RbfKernel : KernelBase<double[]>
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

    internal override void FillParameters(SvmParameter<double[]> param)
    {
      param.KernelFunc = Kernels.Rbf(Gamma);
    }
  }
}
