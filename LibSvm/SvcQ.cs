using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibSvm
{
  //SvcQ
  class SvcQ : Kernel
  {
    private readonly sbyte[] y;
    private readonly Cache cache;
    private readonly double[] QD;

    public SvcQ(SvmProblem prob, SvmParameter param, sbyte[] y_)
      : base(prob.Lenght, prob.X, param)
    {
      //super(prob.l, prob.x, param);
      y = (sbyte[])y_.Clone();
      cache = new Cache(prob.Lenght, (long)(param.cache_size * (1 << 20)));
      QD = new double[prob.Lenght];
      for (int i = 0; i < prob.Lenght; i++)
        QD[i] = kernel_function(i, i);
    }

    public override float[] GetQ(int i, int len)
    {
      float[] data;
      int start, j;
      if ((start = cache.get_data(i, out data, len)) < len)
      {
        for (j = start; j < len; j++)
          data[j] = (float)(y[i] * y[j] * kernel_function(i, j));
      }
      return data;
    }

    public override double[] GetQD()
    {
      return QD;
    }

    public override void SwapIndex(int i, int j)
    {
      cache.swap_index(i, j);
      base.SwapIndex(i, j);
      
      //do { sbyte _ = y[i]; y[i] = y[j]; y[j] = _; } while (false);
      Common.Swap(ref y[i], ref y[j]);

      //do { double _ = QD[i]; QD[i] = QD[j]; QD[j] = _; } while (false);
      Common.Swap(ref QD[i], ref QD[j]);
    }
  }

}
