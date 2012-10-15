using LibSvm;

namespace LibSvmExtras.Kernel
{
  public abstract class KernelBase<TPattern> : IKernel<TPattern>
  {
    internal abstract void FillParameters(SvmParameter<TPattern> param);
  }
}
