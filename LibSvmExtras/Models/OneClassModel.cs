using System;
using LibSvm;

namespace LibSvmExtras.Models
{
  internal class OneClassModel<TPattern> : ModelBase<TPattern>, IModel<TPattern, bool>
  {
    public OneClassModel(SvmModel<TPattern> model)
      : base(model)
    {

    }

    public bool Predict(TPattern point)
    {
      var res = PredictInternal(point);
      return  res > 0;
    }
  }
}
