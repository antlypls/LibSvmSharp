using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibSvm
{
  class OneClassQ : Kernel
  {
    private readonly Cache _cache;
    private readonly double[] _qd;

    public OneClassQ(SvmProblem prob, SvmParameter param)
      : base(prob.Lenght, prob.X, param)
    {
      //super(prob.l, prob.x, param);
      _cache = new Cache(prob.Lenght, (long)(param.cache_size * (1 << 20)));
      _qd = new double[prob.Lenght];
      for (int i = 0; i < prob.Lenght; i++)
        _qd[i] = kernel_function(i, i);
    }

    public override float[] GetQ(int i, int len)
    {
      float[] data;
      int start, j;
      if ((start = _cache.get_data(i, out data, len)) < len)
      {
        for (j = start; j < len; j++)
          data[j] = (float)kernel_function(i, j);
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

      //do { double _ = QD[i]; QD[i] = QD[j]; QD[j] = _; } while (false);
      Common.Swap(ref _qd[i], ref _qd[j]);
    }
  }
}
