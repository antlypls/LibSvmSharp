using System;
using LibSvm;

namespace LibSvmExtras.Svm
{
  public abstract class SvmBase<TIn, TOut> : ISvm<TIn, TOut> where TOut : struct 
  {
    internal abstract void FillParameters(SvmParameter param);
    internal abstract ITrainer<TIn, TOut> GetTrainer(SvmParameter param);
  }
}
