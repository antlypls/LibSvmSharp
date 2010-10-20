using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibSvm
{
  //SvrQ
  class SvrQ : Kernel
  {
    private readonly int l;
    private readonly Cache cache;
    private readonly sbyte[] sign;
    private readonly int[] index;
    private int next_buffer;
    private float[][] buffer;
    private readonly double[] QD;

    public SvrQ(SvmProblem prob, SvmParameter param)
      : base(prob.l, prob.x, param)
    {
      //super(prob.l, prob.x, param);
      l = prob.l;
      cache = new Cache(l, (long)(param.cache_size * (1 << 20)));
      QD = new double[2 * l];
      sign = new sbyte[2 * l];
      index = new int[2 * l];
      for (int k = 0; k < l; k++)
      {
        sign[k] = 1;
        sign[k + l] = -1;
        index[k] = k;
        index[k + l] = k;
        QD[k] = kernel_function(k, k);
        QD[k + l] = QD[k];
      }
      //buffer = new float[2][2*l];
      buffer = new float[2][] { new float[2 * l], new float[2 * l] };

      next_buffer = 0;
    }

    public override void swap_index(int i, int j)
    {
      //do { sbyte _ = sign[i]; sign[i] = sign[j]; sign[j] = _; } while (false);
      Common.Swap(ref sign[i], ref sign[j]);

      //do { int _ = index[i]; index[i] = index[j]; index[j] = _; } while (false);
      Common.Swap(ref index[i], ref index[j]);

      //do { double _ = QD[i]; QD[i] = QD[j]; QD[j] = _; } while (false);
      Common.Swap(ref QD[i], ref QD[j]);

    }

    public override float[] get_Q(int i, int len)
    {
      float[] data;
      int j, real_i = index[i];
      if (cache.get_data(real_i, out data, l) < l)
      {
        for (j = 0; j < l; j++)
          data[j] = (float)kernel_function(real_i, j);
      }

      // reorder and copy
      float[] buf = buffer[next_buffer];
      next_buffer = 1 - next_buffer;
      sbyte si = sign[i];
      for (j = 0; j < len; j++)
        buf[j] = (float)si * sign[j] * data[index[j]];
      return buf;
    }

    public override double[] get_QD()
    {
      return QD;
    }
  }
}
