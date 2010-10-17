using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibSvm
{
  public class SvmParameter : ICloneable
  {
    /* svm_type */
    //public static int C_SVC = 0;
    //public static int NU_SVC = 1;
    //public static int ONE_CLASS = 2;
    //public static int EPSILON_SVR = 3;
    //public static int NU_SVR = 4;

    /* kernel_type */
    //public static int LINEAR = 0;
    //public static int POLY = 1;
    //public static int RBF = 2;
    //public static int SIGMOID = 3;
    //public static int PRECOMPUTED = 4;

    public SvmType svm_type;
    public KernelType kernel_type;
    public int degree;	// for poly
    public double gamma;	// for poly/rbf/sigmoid
    public double coef0;	// for poly/sigmoid

    // these are for training only
    public double cache_size; // in MB
    public double eps;	// stopping criteria
    public double C;	// for C_SVC, EPSILON_SVR and NU_SVR
    public int nr_weight;		// for C_SVC
    public int[] weight_label;	// for C_SVC
    public double[] weight;		// for C_SVC
    public double nu;	// for NU_SVC, ONE_CLASS, and NU_SVR
    public double p;	// for EPSILON_SVR
    public int shrinking;	// use the shrinking heuristics
    public int probability; // do probability estimates

    public object Clone()
    {
      throw new NotImplementedException();
    }
  }
}
