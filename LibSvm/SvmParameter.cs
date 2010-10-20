using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibSvm
{
  public class SvmParameter : ICloneable
  {
    public SvmType svm_type;
    public KernelType kernel_type;
    public int degree;	            // for poly
    public double gamma;	          // for poly/rbf/sigmoid
    public double coef0;	          // for poly/sigmoid

    // these are for training only
    public double cache_size; // in MB
    public double eps;	        // stopping criteria
    public double C;	          // for C_SVC, EPSILON_SVR and NU_SVR
    public int nr_weight;		    // for C_SVC
    public int[] weight_label;	// for C_SVC
    public double[] weight;		  // for C_SVC
    public double nu;	          // for NU_SVC, ONE_CLASS, and NU_SVR
    public double p;	          // for EPSILON_SVR
    public bool shrinking;	      // use the shrinking heuristics
    public bool probability;     // do probability estimates

    public object Clone()
    {
      var clone = (SvmParameter)this.MemberwiseClone();

      //check for null
      clone.weight_label = (int[])weight_label.Clone();
      clone.weight = (double[])weight.Clone();

      return clone;
    }

    // check whether nu-svc is feasible
    private void IsNuFeasible(SvmProblem prob)
    {
      if (svm_type == SvmType.NU_SVC)
      {
        int l = prob.l;
        int max_nr_class = 16;
        int nr_class = 0;
        int[] label = new int[max_nr_class];
        int[] count = new int[max_nr_class];

        int i;
        for (i = 0; i < l; i++)
        {
          int this_label = (int)prob.y[i];
          int j;
          for (j = 0; j < nr_class; j++)
            if (this_label == label[j])
            {
              ++count[j];
              break;
            }

          if (j == nr_class)
          {
            if (nr_class == max_nr_class)
            {
              max_nr_class *= 2;
              int[] new_data = new int[max_nr_class];
              Array.Copy(label, 0, new_data, 0, label.Length);
              label = new_data;

              new_data = new int[max_nr_class];
              Array.Copy(count, 0, new_data, 0, count.Length);
              count = new_data;
            }
            label[nr_class] = this_label;
            count[nr_class] = 1;
            ++nr_class;
          }
        }

        for (i = 0; i < nr_class; i++)
        {
          int n1 = count[i];
          for (int j = i + 1; j < nr_class; j++)
          {
            int n2 = count[j];
            if (this.nu * (n1 + n2) / 2 > Math.Min(n1, n2))
              throw new ApplicationException("specified nu is infeasible");
          }
        }
      }
    }

    //from Svm.svm_check_parameter
    public void Check(SvmProblem prob)
    {
      var svm_type = this.svm_type;
      //var kernel_type = this.kernel_type;

      if (this.gamma < 0)
        throw new ApplicationException("gamma < 0");

      if (this.degree < 0)
        throw new ApplicationException("degree of polynomial kernel < 0");

      // cache_size,eps,C,nu,p,shrinking

      if (this.cache_size <= 0)
        throw new ApplicationException("cache_size <= 0");

      if (this.eps <= 0)
        throw new ApplicationException("eps <= 0");

      if (svm_type == SvmType.C_SVC ||
         svm_type == SvmType.EPSILON_SVR ||
         svm_type == SvmType.NU_SVR)
      {
        if (this.C <= 0)
          throw new ApplicationException("C <= 0");
      }

      if (svm_type == SvmType.NU_SVC ||
         svm_type == SvmType.ONE_CLASS ||
         svm_type == SvmType.NU_SVR)
      {
        if (this.nu <= 0 || this.nu > 1)
          throw new ApplicationException("nu <= 0 or nu > 1");
      }

      if (svm_type == SvmType.EPSILON_SVR)
        if (this.p < 0)
          throw new ApplicationException("p < 0");

      if (this.probability &&
         svm_type == SvmType.ONE_CLASS)
        throw new ApplicationException("one-class SVM probability output not supported yet");

      // check whether nu-svc is feasible
      IsNuFeasible(prob);
    }
  }
}
