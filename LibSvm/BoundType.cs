using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibSvm
{
  public enum BoundType : byte
  {
    LowerBound = 0,
    UpperBound = 1,
    Free = 2
  }
}
