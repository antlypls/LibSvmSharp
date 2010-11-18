using System;

namespace LibSvmExtras
{
  using Svm;
  using Kernel;
  using LibSvm;

  public static class SVM
  {
    public static ITrainer<TIn, TOut, TPattern> Create<TIn, TOut, TPattern>(ISvm<TIn, TOut> svm, IKernel<TPattern> kernel, 
      double cacheSize = 128, double tolerance = 0.001, bool shrinking = true, bool probability = false)
      where TOut : struct
    {
      var svmBase = svm as SvmBase<TIn, TOut, TPattern>;
      var kernelBase = kernel as KernelBase<TPattern>;

      if (svmBase == null || kernelBase == null)
      {
        throw new ApplicationException("Bad svm or/and kernel parameters");
      }

      var parameters = new SvmParameter<TPattern>
      {
        CacheSize = cacheSize,
        Eps = tolerance,
        Shrinking = shrinking,
        Probability = probability
      };

      svmBase.FillParameters(parameters);
      kernelBase.FillParameters(parameters);

      return svmBase.GetTrainer(parameters);
    }
  }
}
