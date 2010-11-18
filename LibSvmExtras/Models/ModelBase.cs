using System;
using LibSvm;

namespace LibSvmExtras.Models
{
  internal class ModelBase<TPattern>
  {
    private readonly SvmModel<TPattern> _model;

    protected ModelBase(SvmModel<TPattern> model)
    {
      _model = model;
    }

    protected double PredictInternal(TPattern point)
    {
      return _model.Predict(point);
    }
  }
}
