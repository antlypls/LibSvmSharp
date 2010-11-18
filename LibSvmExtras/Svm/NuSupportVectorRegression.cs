using System;
using LibSvm;
using LibSvmExtras.Trainers;

namespace LibSvmExtras.Svm
{
  //Nu-SVR
  public sealed class NuSupportVectorRegression<TPattern> : SvmBase<Tuple<TPattern, double>, double, TPattern>
  {
    public double C
    {
      get;
      private set;
    }

    public double Nu
    {
      get;
      private set;
    }

    public NuSupportVectorRegression(double c, double nu)
    {
      C = c;
      Nu = nu;
    }

    internal override void FillParameters(SvmParameter<TPattern> param)
    {
      param.SvmType = SvmType.NU_SVR;
      param.C = C;
      param.Nu = Nu;
    }

    internal override ITrainer<Tuple<TPattern, double>, double, TPattern> GetTrainer(SvmParameter<TPattern> param)
    {
      return new SvrTrainer<TPattern>(param);
    }
  }
}
