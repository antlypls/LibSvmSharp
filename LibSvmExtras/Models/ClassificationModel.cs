using System;
using LibSvm;

namespace LibSvmExtras.Models
{
  internal class ClassificationModel : ModelBase, IModel<int>
  {
    public ClassificationModel(SvmModel model)
      : base(model)
    {

    }

    public int Predict(double[] point)
    {
      return (int)PredictInternal(point);
    }
  }
}
