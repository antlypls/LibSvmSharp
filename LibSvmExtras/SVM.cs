using System;

namespace LibSvmExtras
{
  using Svm;
  using Kernel;
  using LibSvm;

  public static class SVM
  {
    public static ITrainer<TIn, TOut> Create<TIn, TOut>(ISvm<TIn, TOut> svm, IKernel kernel, 
      double cacheSize = 128, double tolerance = 0.001, bool shrinking = true, bool probability = false)
      where TOut : struct
    {
      var svmBase = svm as SvmBase<TIn, TOut>;
      var kernelBase = kernel as KernelBase;

      if (svmBase == null || kernelBase == null)
      {
        throw new ApplicationException("Bad svm or/and kernel parameters");
      }

      var parameters = new SvmParameter
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
