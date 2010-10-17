using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibSvm
{
  public struct SvmNode
  {
    public SvmNode(int index, double value)
    { 
      this.index = index;
      this.value = value;
    }

    public int index;
    public double value;
  }
}
