using System;
using LibSvm;

namespace LibSvmExtras.Models
{
  internal class ModelBase
  {
    private readonly SvmModel _model;

    protected ModelBase(SvmModel model)
    {
      _model = model;
    }

    protected double PredictInternal(double[] point)
    {
      return _model.Predict(point.ToSvmNodes());
    }
  }
}
