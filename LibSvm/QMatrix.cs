using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibSvm
{
  abstract class QMatrix
  {
    public abstract float[] GetQ(int column, int len);
    public abstract double[] GetQD();
    public abstract void SwapIndex(int i, int j);
  }
}
