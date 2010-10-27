using System;
using LibSvm;
using LibSvmExtras.Trainers;

namespace LibSvmExtras.Svm
{
  public sealed class NuSupportVectorClassification : SvmBase<Tuple<double[], int>, int>
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

    internal override void FillParameters(SvmParameter param)
    {
      param.SvmType = SvmType.NU_SVC;
      param.Nu = Nu;
    }

    internal override ITrainer<Tuple<double[], int>, int> GetTrainer(SvmParameter param)
    {
      return new SvcTrainer(param);
    }
  }
}
