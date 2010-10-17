using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibSvm
{
  public class SvmModel
  {
    public SvmParameter param;	// parameter
    public int nr_class;		// number of classes, = 2 in regression/one class svm
    public int l;			// total #SV
    public SvmNode[][] SV;	// SVs (SV[l])
    public double[][] sv_coef;	// coefficients for SVs in decision functions (sv_coef[k-1][l])
    public double[] rho;		// constants in decision functions (rho[k*(k-1)/2])
    public double[] probA;         // pariwise probability information
    public double[] probB;

    // for classification only

    public int[] label;		// label of each class (label[k])
    public int[] nSV;		// number of SVs for each class (nSV[k])
    // nSV[0] + nSV[1] + ... + nSV[k-1] = l

    //from Svm.svm_get_svm_type
    public SvmType SvmType
    {
      get
      {
        return param.svm_type;
      }
    }

    //from Svm.svm_get_nr_class
    public int NrClass
    {
      get
      {
        return nr_class;
      }
    }

    //from Svm.svm_get_labels
    public void svm_get_labels(int[] label)
    {
      if (this.label != null)
      {
        for (int i = 0; i < this.nr_class; i++)
        {
          label[i] = this.label[i];
        }
      }
    }

    //from Svm.svm_get_svr_probability
    public double GetSvrProbability()
    {
      if ((param.svm_type == SvmType.EPSILON_SVR || param.svm_type == SvmType.NU_SVR) && probA != null)
      {
        return probA[0];
      }
      else
      {
        throw new ApplicationException("Model doesn't contain information for SVR probability inference\n");
      }
    }

    //from Svm.svm_predict_values
    public double PredictValues(SvmNode[] x, double[] dec_values)
    {
      if (this.param.svm_type == SvmType.ONE_CLASS ||
         this.param.svm_type == SvmType.EPSILON_SVR ||
         this.param.svm_type == SvmType.NU_SVR)
      {
        double[] sv_coef = this.sv_coef[0];
        double sum = 0;
        for (int i = 0; i < this.l; i++)
          sum += sv_coef[i] * Kernel.k_function(x, this.SV[i], this.param);
        sum -= this.rho[0];
        dec_values[0] = sum;

        if (this.param.svm_type == SvmType.ONE_CLASS)
          return (sum > 0) ? 1 : -1;
        else
          return sum;
      }
      else
      {
        int i;
        int nr_class = this.nr_class;
        int l = this.l;

        double[] kvalue = new double[l];
        for (i = 0; i < l; i++)
          kvalue[i] = Kernel.k_function(x, this.SV[i], this.param);

        int[] start = new int[nr_class];
        start[0] = 0;
        for (i = 1; i < nr_class; i++)
          start[i] = start[i - 1] + this.nSV[i - 1];

        int[] vote = new int[nr_class];
        for (i = 0; i < nr_class; i++)
          vote[i] = 0;

        int p = 0;
        for (i = 0; i < nr_class; i++)
          for (int j = i + 1; j < nr_class; j++)
          {
            double sum = 0;
            int si = start[i];
            int sj = start[j];
            int ci = this.nSV[i];
            int cj = this.nSV[j];

            int k;
            double[] coef1 = this.sv_coef[j - 1];
            double[] coef2 = this.sv_coef[i];
            for (k = 0; k < ci; k++)
              sum += coef1[si + k] * kvalue[si + k];
            for (k = 0; k < cj; k++)
              sum += coef2[sj + k] * kvalue[sj + k];
            sum -= this.rho[p];
            dec_values[p] = sum;

            if (dec_values[p] > 0)
              ++vote[i];
            else
              ++vote[j];
            p++;
          }

        int vote_max_idx = 0;
        for (i = 1; i < nr_class; i++)
          if (vote[i] > vote[vote_max_idx])
            vote_max_idx = i;

        return this.label[vote_max_idx];
      }
    }

    //from Svm.svm_predict
    public double Predict(SvmNode[] x)
    {
      int nr_class = this.nr_class;
      double[] dec_values;
      if (param.svm_type == SvmType.ONE_CLASS ||
          param.svm_type == SvmType.EPSILON_SVR ||
          param.svm_type == SvmType.NU_SVR)
        dec_values = new double[1];
      else
        dec_values = new double[nr_class * (nr_class - 1) / 2];
      double pred_result = PredictValues(x, dec_values);
      return pred_result;
    }

    //from Svm.svm_predict_probability
    public double PredictProbability(SvmNode[] x, double[] prob_estimates)
    {
      if ((this.param.svm_type == SvmType.C_SVC || this.param.svm_type == SvmType.NU_SVC) &&
          this.probA != null && this.probB != null)
      {
        int i;
        int nr_class = this.nr_class;
        double[] dec_values = new double[nr_class * (nr_class - 1) / 2];
        this.PredictValues(x, dec_values);

        double min_prob = 1e-7;
        //double[][] pairwise_prob=new double[nr_class][nr_class];
        double[][] pairwise_prob = new double[nr_class][];
        for (i = 0; i < nr_class; i++)
        {
          pairwise_prob[i] = new double[nr_class];
        }

        int k = 0;
        for (i = 0; i < nr_class; i++)
          for (int j = i + 1; j < nr_class; j++)
          {
            pairwise_prob[i][j] = Math.Min(Math.Max(Svm.sigmoid_predict(dec_values[k], this.probA[k], this.probB[k]), min_prob), 1 - min_prob);
            pairwise_prob[j][i] = 1 - pairwise_prob[i][j];
            k++;
          }
        Svm.multiclass_probability(nr_class, pairwise_prob, prob_estimates);

        int prob_max_idx = 0;
        for (i = 1; i < nr_class; i++)
          if (prob_estimates[i] > prob_estimates[prob_max_idx])
            prob_max_idx = i;
        return this.label[prob_max_idx];
      }
      else
        return this.Predict(x);
    }

    //from Svm.svm_check_probability_model
    public int CheckProbabilityModel()
    {
      if (((param.svm_type == SvmType.C_SVC || param.svm_type == SvmType.NU_SVC) && probA != null && probB != null) ||
      ((param.svm_type == SvmType.EPSILON_SVR || param.svm_type == SvmType.NU_SVR) && probA != null))
        return 1;
      else
        return 0;
    }


  }
}
