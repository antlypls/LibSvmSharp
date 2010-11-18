using System;
using LibSvm;

namespace LibSvmExtras.Models
{
  internal class RegressionModel<TPattern> : ModelBase<TPattern>, IModel<TPattern, double>
  {
    public RegressionModel(SvmModel<TPattern> model)
      : base(model)
    { 
    
    }

    public double Predict(TPattern point)
    {
      return PredictInternal(point);
    }
  }
}
