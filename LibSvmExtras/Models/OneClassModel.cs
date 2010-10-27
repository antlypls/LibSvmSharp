using System;
using LibSvm;

namespace LibSvmExtras.Models
{
  internal class OneClassModel : ModelBase, IModel<bool>
  {
    public OneClassModel(SvmModel model)
      : base(model)
    {

    }

    public bool Predict(double[] point)
    {
      var res = PredictInternal(point);
      return  res > 0;
    }
  }
}
