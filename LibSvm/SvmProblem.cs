using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace LibSvm
{
  public class SvmProblem
  {
    public int Lenght
    {
      get
      {
        Debug.Assert(Y.Length == X.Length, "Y.Length == X.Length");
        return Y.Length;
      }
    }
    public double[] Y;
    public SvmNode[][] X;
  }
}
