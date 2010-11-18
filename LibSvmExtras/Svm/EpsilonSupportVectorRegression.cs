using System;
using LibSvm;
using LibSvmExtras.Trainers;

namespace LibSvmExtras.Svm
{
  public sealed class EpsilonSupportVectorRegression<TPattern> : SvmBase<Tuple<TPattern, double>, double, TPattern>
  {
    public double C
    {
      get;
      private set;
    }

    public double Eps
    {
      get;
      private set;
    }

    public EpsilonSupportVectorRegression(double c, double eps)
    {
      C = c;
      Eps = eps;
    }

    internal override void FillParameters(SvmParameter<TPattern> param)
    {
      param.SvmType = SvmType.EPSILON_SVR;
      param.C = C;
      param.P = Eps;
    }

    internal override ITrainer<Tuple<TPattern, double>, double, TPattern> GetTrainer(SvmParameter<TPattern> param)
    {
      return new SvrTrainer<TPattern>(param);
    }
  }
}
