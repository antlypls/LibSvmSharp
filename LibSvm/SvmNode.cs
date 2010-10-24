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
      Index = index;
      Value = value;
    }

    public readonly int Index;
    public readonly double Value;
  }
}
