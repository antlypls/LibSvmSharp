using LibSvm;
using LibSvmExtras.Trainers;

namespace LibSvmExtras.Svm
{
  public sealed class OneClass<TPattern> : SvmBase<TPattern, bool, TPattern>
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

    internal override void FillParameters(SvmParameter<TPattern> param)
    {
      param.SvmType = SvmType.ONE_CLASS;
      param.Nu = Nu;
    }

    internal override ITrainer<TPattern, bool, TPattern> GetTrainer(SvmParameter<TPattern> param)
    {
      return new OneClassTrainer<TPattern>(param);
    }
  }
}
