using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibSvm
{
  class OneClassQ<TPattern> : Kernel<TPattern>
  {
    private readonly Cache _cache;
    private readonly double[] _qd;

    public OneClassQ(SvmProblem<TPattern> prob, SvmParameter<TPattern> param)
      : base(prob.Lenght, prob.X, param)
    {
      _cache = new Cache(prob.Lenght, (long)(param.CacheSize * (1 << 20)));
      _qd = new double[prob.Lenght];
      for (int i = 0; i < prob.Lenght; i++)
        _qd[i] = kernel_function(i, i);
    }

    public override double[] GetQ(int i, int len)
    {
      double[] data;
      int start;
      if ((start = _cache.get_data(i, out data, len)) < len)
      {
        for (int j = start; j < len; j++)
          data[j] = (double)kernel_function(i, j);
      }
      return data;
    }

    public override double[] GetQD()
    {
      return _qd;
    }

    public override void SwapIndex(int i, int j)
    {
      _cache.swap_index(i, j);
      base.SwapIndex(i, j);

      Common.Swap(ref _qd[i], ref _qd[j]);
    }
  }
}
