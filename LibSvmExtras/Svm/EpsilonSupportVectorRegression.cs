using System;
using LibSvm;
using LibSvmExtras.Trainers;

namespace LibSvmExtras.Svm
{
  public sealed class EpsilonSupportVectorRegression : SvmBase<Tuple<double[], double>, double>
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

    internal override void FillParameters(SvmParameter param)
    {
      param.SvmType = SvmType.EPSILON_SVR;
      param.C = C;
      param.P = Eps;
    }

    internal override ITrainer<Tuple<double[], double>, double> GetTrainer(SvmParameter param)
    {
      return new SvrTrainer(param);
    }
  }
}
