namespace LibSvm
{
  // java: information about solution except alpha,
  // because we cannot return multiple values otherwise...
  // class some times used as reference
  internal class SolutionInfo
  {
    public double Obj;
    public double Rho;
    public double UpperBoundP;
    public double UpperBoundN;
    public double R; // for Solver_NU
  }
}
