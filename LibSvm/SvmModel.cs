using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace LibSvm
{
  public class SvmModel
  {
    // parameterS
    public SvmParameter Param
    {
      get;
      internal set;
    }

    // number of classes, = 2 in regression/one class svm
    public int NrClass
    {
      get;
      internal set;
    }		

    // total #SV
    public int TotalSupportVectorsNumber
    {
      get;
      internal set;
    }			

    // SVs (SV[TotalSupportVectorsNumber])
    public SvmNode[][] SupportVectors
    {
      get;
      internal set;
    }	

    // coefficients for SVs in decision functions (sv_coef[k-1][l])
    public double[][] SupportVectorsCoefficients
    {
      get;
      internal set;
    }	

    // constants in decision functions (rho[k*(k-1)/2])
    public double[] Rho
    {
      get;
      internal set;
    }		

    // pariwise probability information
    public double[] ProbA
    {
      get;
      internal set;
    }
    public double[] ProbB
    {
      get;
      internal set;
    }

    // for classification only

    // label of each class (label[k])
    public int[] Label
    {
      get;
      internal set;
    }

    // number of SVs for each class (SupportVectorsNumbers[k])
    public int[] SupportVectorsNumbers
    {
      get;
      internal set;
    }		
    // SupportVectorsNumbers[0] + SupportVectorsNumbers[1] + ... + SupportVectorsNumbers[k-1] = TotalSupportVectorsNumber

    //from Svm.svm_get_svm_type
    public SvmType SvmType
    {
      get
      {
        return Param.SvmType;
      }
    }

    //from Svm.svm_get_labels
    public void GetLabels(int[] label)
    {
      if (Label == null) return;

      for (int i = 0; i < NrClass; i++)
      {
        label[i] = Label[i];
      }
    }

    //from Svm.svm_get_svr_probability
    public double GetSvrProbability()
    {
      if ((SvmType.IsSVR()) && ProbA != null)
      {
        return ProbA[0];
      }

      throw new ApplicationException("Model doesn't contain information for SVR probability inference\n");
    }

    private double PredictValuesSvrOrOneClass(SvmNode[] x, double[] dec_values)
    {
      Debug.Assert(SvmType.IsSVROrOneClass(), "SvmType.IsSVROrOneClass()");

      double[] sv_coef = SupportVectorsCoefficients[0];
      double sum = 0;
      for (int i = 0; i < TotalSupportVectorsNumber; i++)
        sum += sv_coef[i] * Kernel.k_function(x, SupportVectors[i], Param);
      sum -= Rho[0];
      dec_values[0] = sum;

      return SvmType.IsOneClass() ? ((sum > 0) ? 1 : -1) : sum;
    }

    private double PredictValuesNonSvrOrOneClass(SvmNode[] x, double[] dec_values)
    {
      Debug.Assert(!SvmType.IsSVROrOneClass(), "!SvmType.IsSVROrOneClass()");

      int nr_class = NrClass;
      int l = TotalSupportVectorsNumber;

      double[] kvalue = new double[l];
      for (int i = 0; i < l; i++)
        kvalue[i] = Kernel.k_function(x, SupportVectors[i], Param);

      int[] start = new int[nr_class];
      start[0] = 0;
      for (int i = 1; i < nr_class; i++)
        start[i] = start[i - 1] + SupportVectorsNumbers[i - 1];

      int[] vote = new int[nr_class];
      for (int i = 0; i < nr_class; i++)
        vote[i] = 0;

      int p = 0;
      for (int i = 0; i < nr_class; i++)
        for (int j = i + 1; j < nr_class; j++)
        {
          double sum = 0;
          int si = start[i];
          int sj = start[j];
          int ci = SupportVectorsNumbers[i];
          int cj = SupportVectorsNumbers[j];

          int k;
          double[] coef1 = SupportVectorsCoefficients[j - 1];
          double[] coef2 = SupportVectorsCoefficients[i];
          for (k = 0; k < ci; k++)
            sum += coef1[si + k] * kvalue[si + k];
          for (k = 0; k < cj; k++)
            sum += coef2[sj + k] * kvalue[sj + k];
          sum -= Rho[p];
          dec_values[p] = sum;

          if (dec_values[p] > 0)
            ++vote[i];
          else
            ++vote[j];
          p++;
        }

      int vote_max_idx = 0;
      for (int i = 1; i < nr_class; i++)
        if (vote[i] > vote[vote_max_idx])
          vote_max_idx = i;

      return Label[vote_max_idx];
    }

    //from Svm.svm_predict_values
    public double PredictValues(SvmNode[] x, double[] dec_values)
    {
      return SvmType.IsSVROrOneClass() ? PredictValuesSvrOrOneClass(x ,dec_values) : PredictValuesNonSvrOrOneClass(x, dec_values);
    }

    //from Svm.svm_predict
    public double Predict(SvmNode[] x)
    {
      int nr_class = NrClass;
      int length = SvmType.IsSVROrOneClass() ? 1 : nr_class*(nr_class - 1)/2;
      var dec_values = new double[length];
      return PredictValues(x, dec_values);
    }

    //from Svm.svm_predict_probability
    public double PredictProbability(SvmNode[] x, double[] prob_estimates)
    {
      if (!SvmType.IsSVC() || ProbA == null || ProbB == null)
      {
        return Predict(x);
      }

      int nr_class = NrClass;
      double[] dec_values = new double[nr_class*(nr_class - 1)/2];
      PredictValues(x, dec_values);

      double min_prob = 1e-7;
      double[][] pairwise_prob = new double[nr_class][];
      for (int i = 0; i < nr_class; i++)
      {
        pairwise_prob[i] = new double[nr_class];
      }

      int k = 0;
      for (int i = 0; i < nr_class; i++)
      {
        for (int j = i + 1; j < nr_class; j++)
        {
          pairwise_prob[i][j] = Math.Min(Math.Max(Svm.sigmoid_predict(dec_values[k], ProbA[k], ProbB[k]), min_prob), 1 - min_prob);
          pairwise_prob[j][i] = 1 - pairwise_prob[i][j];
          k++;
        }
      }
      Svm.multiclass_probability(nr_class, pairwise_prob, prob_estimates);

      int prob_max_idx = 0;
      for (int i = 1; i < nr_class; i++)
        if (prob_estimates[i] > prob_estimates[prob_max_idx])
          prob_max_idx = i;
      return Label[prob_max_idx];
    }

    //from Svm.svm_check_probability_model
    public bool CheckProbabilityModel()
    {
      return (SvmType.IsSVC() && ProbA != null && ProbB != null) ||
             (SvmType.IsSVR() && ProbA != null);
    }
  }
}
