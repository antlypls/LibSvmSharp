using System;
using LibSvm;
using LibSvmExtras.Trainers;

namespace LibSvmExtras.Svm
{
  public sealed class CSupportVectorClassification<TPattern> : SvmBase<Tuple<TPattern, int>, int, TPattern>
  {
    public double C
    {
      get;
      private set;
    }

    public CSupportVectorClassification(double c)
    {
      C = c;
    }

    internal override void FillParameters(SvmParameter<TPattern> param)
    {
      param.SvmType = SvmType.C_SVC;
      param.C = C;
    }

    internal override ITrainer<Tuple<TPattern, int>, int, TPattern> GetTrainer(SvmParameter<TPattern> param)
    {
      return new SvcTrainer<TPattern>(param);
    }
  }
}
