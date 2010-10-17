using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibSvm
{
  abstract class QMatrix
  {
    public abstract float[] get_Q(int column, int len);
    public abstract double[] get_QD();
    public abstract void swap_index(int i, int j);
  }
}
