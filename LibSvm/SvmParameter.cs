using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace LibSvm
{
  public class SvmParameter : ICloneable
  {
    public SvmType SvmType;
    public KernelType KernelType;
    public int Degree;            // for poly
    public double Gamma;          // for poly/rbf/sigmoid
    public double Coef0;          // for poly/sigmoid

    // these are for training only
    public double CacheSize;    // in MB
    public double Eps;          // stopping criteria
    public double C;            // for C_SVC, EPSILON_SVR and NU_SVR
    public int WeightsCount     // for C_SVC
    {
      get
      {
        if (WeightLabel == null || Weight == null)
        {
          return 0;
        }
        Debug.Assert(WeightLabel.Length == Weight.Length, "WeightLabel.Length == Weight.Length");
        return WeightLabel.Length;
      }
    }
    public int[] WeightLabel;  // for C_SVC
    public double[] Weight;    // for C_SVC
    public double Nu;          // for NU_SVC, ONE_CLASS, and NU_SVR
    public double P;           // for EPSILON_SVR
    public bool Shrinking;     // use the shrinking heuristics
    public bool Probability;    // do probability estimates

    public object Clone()
    {
      var clone = (SvmParameter)MemberwiseClone();

      if (WeightLabel != null)
      {
        clone.WeightLabel = (int[])WeightLabel.Clone();
      }

      if (Weight != null)
      {
        clone.Weight = (double[])Weight.Clone();
      }

      return clone;
    }

    // check whether nu-svc is feasible
    private void IsNuFeasible(SvmProblem prob)
    {
      if (!SvmType.IsNuSVC())
      {
        return;
      }

      int l = prob.Lenght;
      int max_nr_class = 16;
      int nr_class = 0;
      int[] label = new int[max_nr_class];
      int[] count = new int[max_nr_class];

      int i;
      for (i = 0; i < l; i++)
      {
        int this_label = (int) prob.Y[i];
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
          if (this.Nu*(n1 + n2)/2 > Math.Min(n1, n2))
            throw new ApplicationException("specified nu is infeasible");
        }
      }
    }

    //from Svm.svm_check_parameter
    public void Check(SvmProblem prob)
    {
      if (Gamma < 0)
      {
        throw new ApplicationException("gamma < 0");
      }

      if (Degree < 0)
      {
        throw new ApplicationException("degree of polynomial kernel < 0");
      }

      // cache_size,eps,C,nu,p,shrinking

      if (CacheSize <= 0)
      {
        throw new ApplicationException("cache_size <= 0");
      }

      if (Eps <= 0)
      {
        throw new ApplicationException("eps <= 0");
      }

      if (SvmType.UseCParameter() && C <= 0)
      {
        throw new ApplicationException("C <= 0");
      }

      if (SvmType.UseNuParameter() && (Nu <= 0 || Nu > 1))
      {
        throw new ApplicationException("nu <= 0 or nu > 1");
      }

      if (SvmType.UsePParameter() && P < 0)
      {
        throw new ApplicationException("p < 0");
      }

      if (Probability && SvmType.IsOneClass())
      {
        throw new ApplicationException("one-class SVM probability output not supported yet");
      }

      // check whether nu-svc is feasible
      IsNuFeasible(prob);
    }
  }
}
