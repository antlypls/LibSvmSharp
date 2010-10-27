using System;
using System.Collections.Generic;

namespace LibSvmExtras
{
  public interface ITrainer<TIn, TOut> where TOut:struct
  {
    IModel<TOut> Train(IEnumerable<TIn> data);
  }
}
