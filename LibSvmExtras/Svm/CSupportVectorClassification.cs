using System;
using LibSvm;
using LibSvmExtras.Trainers;

namespace LibSvmExtras.Svm
{
  public sealed class CSupportVectorClassification : SvmBase<Tuple<double[], int>, int>
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

    internal override void FillParameters(SvmParameter param)
    {
      param.SvmType = SvmType.C_SVC;
      param.C = C;
    }

    internal override ITrainer<Tuple<double[], int>, int> GetTrainer(SvmParameter param)
    {
      return new SvcTrainer(param);
    }
  }
}
