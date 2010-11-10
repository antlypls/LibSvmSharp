using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibSvm
{
  //SvcQ
  class SvcQ<TPattern> : Kernel<TPattern>
  {
    private readonly sbyte[] y;
    private readonly Cache cache;
    private readonly double[] QD;

    public SvcQ(SvmProblem<TPattern> prob, SvmParameter<TPattern> param, sbyte[] y_)
      : base(prob.Lenght, prob.X, param)
    {
      //super(prob.l, prob.x, param);
      y = (sbyte[])y_.Clone();
      cache = new Cache(prob.Lenght, (long)(param.CacheSize * (1 << 20)));
      QD = new double[prob.Lenght];
      for (int i = 0; i < prob.Lenght; i++)
        QD[i] = kernel_function(i, i);
    }

    public override double[] GetQ(int i, int len)
    {
      double[] data;
      int start = cache.get_data(i, out data, len);
      if (start < len)
      {
        for (int j = start; j < len; j++)
        {
          data[j] = (double)(y[i] * y[j] * kernel_function(i, j));
        }
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
      
      Common.Swap(ref y[i], ref y[j]);
      Common.Swap(ref QD[i], ref QD[j]);
    }
  }

}
