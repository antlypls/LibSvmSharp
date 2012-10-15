using LibSvm;

namespace LibSvmExtras.Kernel
{
  public sealed class LinearKernel : KernelBase<double[]>
  {
    internal override void FillParameters(SvmParameter<double[]> param)
    {
      param.KernelFunc = Kernels.Linear();
    }
  }
}
