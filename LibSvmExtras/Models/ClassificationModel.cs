using System;
using LibSvm;

namespace LibSvmExtras.Models
{
  internal class ClassificationModel<TPattern> : ModelBase<TPattern>, IModel<TPattern, int>
  {
    public ClassificationModel(SvmModel<TPattern> model)
      : base(model)
    { 
    
    }

    public int Predict(TPattern point)
    {
      return (int)PredictInternal(point);
    }
  }
}
