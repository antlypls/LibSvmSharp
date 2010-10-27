using System;
using LibSvm;

namespace LibSvmExtras.Models
{
  internal class RegressionModel : ModelBase, IModel<double>
  {
    public RegressionModel(SvmModel model)
      : base(model)
    { 
    
    }

    public double Predict(double[] point)
    {
      return PredictInternal(point);
    }
  }
}
