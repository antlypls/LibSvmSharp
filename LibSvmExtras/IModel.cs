using System;

namespace LibSvmExtras
{
  public interface IModel<TPattern, TOut> where TOut : struct
  {
    TOut Predict(TPattern point);
  }
}
