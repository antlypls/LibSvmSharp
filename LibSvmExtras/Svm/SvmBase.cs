using System;
using LibSvm;

namespace LibSvmExtras.Svm
{
  public abstract class SvmBase<TIn, TOut, TPattern> : ISvm<TIn, TOut> where TOut : struct 
  {
    internal abstract void FillParameters(SvmParameter<TPattern> param);
    internal abstract ITrainer<TIn, TOut, TPattern> GetTrainer(SvmParameter<TPattern> param);
  }
}
