using System;
using LibSvm;
using LibSvmExtras.Trainers;

namespace LibSvmExtras.Svm
{
  //Nu-SVR
  public sealed class NuSupportVectorRegression : SvmBase<Tuple<double[], double>, double>
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

    internal override void FillParameters(SvmParameter param)
    {
      param.SvmType = SvmType.NU_SVR;
      param.C = C;
      param.Nu = Nu;
    }

    internal override ITrainer<Tuple<double[], double>, double> GetTrainer(SvmParameter param)
    {
      return new SvrTrainer(param);
    }
  }
}
