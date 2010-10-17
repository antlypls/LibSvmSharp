using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibSvm
{
  public enum BoundType : byte
  {
    LOWER_BOUND = 0,
    UPPER_BOUND = 1,
    FREE = 2
  }
}
