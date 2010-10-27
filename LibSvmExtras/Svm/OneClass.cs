using System;
using LibSvm;
using LibSvmExtras.Trainers;

namespace LibSvmExtras.Svm
{
  public sealed class OneClass : SvmBase<double[], bool>
  {
    public double Nu
    {
      get;
      private set;
    }

    public OneClass(double nu)
    {
      Nu = nu;
    }

    internal override void FillParameters(SvmParameter param)
    {
      param.SvmType = SvmType.ONE_CLASS;
      param.Nu = Nu;
    }

    internal override ITrainer<double[], bool> GetTrainer(SvmParameter param)
    {
      return new OneClassTrainer(param);
    }
  }
}
