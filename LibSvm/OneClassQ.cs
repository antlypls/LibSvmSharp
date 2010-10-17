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
      : base(prob.l, prob.x, param)
    {
      //super(prob.l, prob.x, param);
      cache = new Cache(prob.l, (long)(param.cache_size * (1 << 20)));
      QD = new double[prob.l];
      for (int i = 0; i < prob.l; i++)
        QD[i] = kernel_function(i, i);
    }

    public override float[] get_Q(int i, int len)
    {
      float[][] data = new float[1][];
      int start, j;
      if ((start = cache.get_data(i, data, len)) < len)
      {
        for (j = start; j < len; j++)
          data[0][j] = (float)kernel_function(i, j);
      }
      return data[0];
    }

    public override double[] get_QD()
    {
      return QD;
    }

    public override void swap_index(int i, int j)
    {
      cache.swap_index(i, j);
      base.swap_index(i, j);
      do { double _ = QD[i]; QD[i] = QD[j]; QD[j] = _; } while (false);
    }
  }
}
