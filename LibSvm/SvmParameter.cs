﻿using System;
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
      // svm_type

      var svm_type = this.svm_type;
      //if(svm_type != svm_parameter.C_SVC &&
      //   svm_type != svm_parameter.NU_SVC &&
      //   svm_type != svm_parameter.ONE_CLASS &&
      //   svm_type != svm_parameter.EPSILON_SVR &&
      //   svm_type != svm_parameter.NU_SVR)
      //return "unknown svm type";

      // kernel_type, degree

      var kernel_type = this.kernel_type;
      //if(kernel_type != svm_parameter.LINEAR &&
      //   kernel_type != svm_parameter.POLY &&
      //   kernel_type != svm_parameter.RBF &&
      //   kernel_type != svm_parameter.SIGMOID &&
      //   kernel_type != svm_parameter.PRECOMPUTED)
      //  return "unknown kernel type";

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

      if (this.shrinking != 0 &&
         this.shrinking != 1)
        throw new ApplicationException("shrinking != 0 and shrinking != 1");

      if (this.probability != 0 &&
         this.probability != 1)
        throw new ApplicationException("probability != 0 and probability != 1");

      if (this.probability == 1 &&
         svm_type == SvmType.ONE_CLASS)
        throw new ApplicationException("one-class SVM probability output not supported yet");

      // check whether nu-svc is feasible
      IsNuFeasible(prob);



    }

  }
}
