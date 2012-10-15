using LibSvm;

namespace LibSvmExtras.Kernel
{
  public sealed class SigmoidKernel : KernelBase<double[]>
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

    internal override void FillParameters(SvmParameter<double[]> param)
    {
      param.KernelFunc = Kernels.Sigmoid(Gamma, R);
    }
  }
}
