using System.Diagnostics;

namespace LibSvm
{
  public class SvmProblem<TPattern>
  {
    public int Length
    {
      get
      {
        Debug.Assert(Y.Length == X.Length, "Y.Length == X.Length");
        return Y.Length;
      }
    }
    public double[] Y;
    public TPattern[] X;
  }
}
