using System;
using System.Collections.Generic;

namespace LibSvmExtras
{
  public interface ITrainer<TIn, TOut, TPattern> where TOut : struct
  {
    IModel<TPattern, TOut> Train(IEnumerable<TIn> data);
  }
}
