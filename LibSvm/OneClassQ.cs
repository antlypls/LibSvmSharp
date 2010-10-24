using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibSvm
{
  class OneClassQ : Kernel
  {
    private readonly Cache cache;
    private readonly double[] QD;

    public OneClassQ(SvmProblem prob, SvmParameter param)
      : base(prob.Lenght, prob.X, param)
    {
      //super(prob.l, prob.x, param);
      cache = new Cache(prob.Lenght, (long)(param.cache_size * (1 << 20)));
      QD = new double[prob.Lenght];
      for (int i = 0; i < prob.Lenght; i++)
        QD[i] = kernel_function(i, i);
    }

    public override float[] get_Q(int i, int len)
    {
      float[] data;
      int start, j;
      if ((start = cache.get_data(i, out data, len)) < len)
      {
        for (j = start; j < len; j++)
          data[j] = (float)kernel_function(i, j);
      }
      return data;
    }

    public override double[] get_QD()
    {
      return QD;
    }

    public override void swap_index(int i, int j)
    {
      cache.swap_index(i, j);
      base.swap_index(i, j);

      //do { double _ = QD[i]; QD[i] = QD[j]; QD[j] = _; } while (false);
      Common.Swap(ref QD[i], ref QD[j]);
    }
  }
}
