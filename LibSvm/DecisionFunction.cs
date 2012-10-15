namespace LibSvm
{
  //
  // decision_function
  //
  internal class DecisionFunction
  {
    public DecisionFunction(double[] alpha, double rho)
    {
      Alpha = alpha;
      Rho = rho;
    }

    public readonly double[] Alpha;
    public readonly double Rho;
  }
}
