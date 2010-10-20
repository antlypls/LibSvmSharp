using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibSvm
{
  // java: information about solution except alpha,
  // because we cannot return multiple values otherwise...
  // class some times used as reference
  internal class SolutionInfo
  {
    public double obj;
    public double rho;
    public double upper_bound_p;
    public double upper_bound_n;
    public double r;	// for Solver_NU
  }
}
