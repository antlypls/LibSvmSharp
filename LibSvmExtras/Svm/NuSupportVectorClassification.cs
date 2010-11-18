using System;
using LibSvm;
using LibSvmExtras.Trainers;

namespace LibSvmExtras.Svm
{
  public sealed class NuSupportVectorClassification<TPattern> : SvmBase<Tuple<TPattern, int>, int, TPattern>
  {
    public double Nu
    {
      get;
      private set;
    }

    public NuSupportVectorClassification(double nu)
    {
      Nu = nu;
    }

    internal override void FillParameters(SvmParameter<TPattern> param)
    {
      param.SvmType = SvmType.NU_SVC;
      param.Nu = Nu;
    }

    internal override ITrainer<Tuple<TPattern, int>, int, TPattern> GetTrainer(SvmParameter<TPattern> param)
    {
      return new SvcTrainer<TPattern>(param);
    }
  }
}
