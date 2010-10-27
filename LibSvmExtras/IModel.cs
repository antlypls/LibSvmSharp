using System;

namespace LibSvmExtras
{
  public interface IModel<TOut> where TOut : struct
  {
    TOut Predict(double[] point);
  }
}
