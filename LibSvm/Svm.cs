using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibSvm
{
  using svm_print_interface = Action<string>;

  //
  // construct and solve various formulations
  //
  public static class Svm
  {
    public const int LIBSVM_VERSION = 300;

    #region private_members


    private static readonly svm_print_interface svm_print_stdout = str => Console.WriteLine(str);

    private static svm_print_interface svm_print_string = svm_print_stdout;

    public static void info(String s)
    {
      svm_print_string(s);
    }

    private static void solve_c_svc(SvmProblem prob, SvmParameter param, double[] alpha, SolutionInfo si, double Cp, double Cn)
    {
      int l = prob.Lenght;
      double[] minus_ones = new double[l];
      sbyte[] y = new sbyte[l];

      for (int i = 0; i < l; i++)
      {
        alpha[i] = 0;
        minus_ones[i] = -1;

        if (prob.Y[i] > 0)
        {
          y[i] = +1;
        }
        else
        {
          y[i] = -1;
        }
      }

      Solver s = new Solver();
      s.Solve(l, new SvcQ(prob, param, y), minus_ones, y,
        alpha, Cp, Cn, param.eps, si, param.shrinking);

      double sum_alpha = 0;
      for (int i = 0; i < l; i++)
      {
        sum_alpha += alpha[i];
      }

      if (Cp == Cn)
      {
        Svm.info("nu = " + sum_alpha / (Cp * prob.Lenght) + "\n");
      }

      for (int i = 0; i < l; i++)
      {
        alpha[i] *= y[i];
      }
    }

    private static void solve_nu_svc(SvmProblem prob, SvmParameter param, double[] alpha, SolutionInfo si)
    {
      int i;
      int l = prob.Lenght;
      double nu = param.nu;

      sbyte[] y = new sbyte[l];

      for (i = 0; i < l; i++)
        if (prob.Y[i] > 0)
          y[i] = +1;
        else
          y[i] = -1;

      double sum_pos = nu * l / 2;
      double sum_neg = nu * l / 2;

      for (i = 0; i < l; i++)
        if (y[i] == +1)
        {
          alpha[i] = Math.Min(1.0, sum_pos);
          sum_pos -= alpha[i];
        }
        else
        {
          alpha[i] = Math.Min(1.0, sum_neg);
          sum_neg -= alpha[i];
        }

      double[] zeros = new double[l];

      for (i = 0; i < l; i++)
        zeros[i] = 0;

      var s = new SolverNu();
      s.Solve(l, new SvcQ(prob, param, y), zeros, y,
        alpha, 1.0, 1.0, param.eps, si, param.shrinking);
      double r = si.r;

      Svm.info("C = " + 1 / r + "\n");

      for (i = 0; i < l; i++)
        alpha[i] *= y[i] / r;

      si.rho /= r;
      si.obj /= (r * r);
      si.upper_bound_p = 1 / r;
      si.upper_bound_n = 1 / r;
    }

    private static void solve_one_class(SvmProblem prob, SvmParameter param, double[] alpha, SolutionInfo si)
    {
      int l = prob.Lenght;
      double[] zeros = new double[l];
      sbyte[] ones = new sbyte[l];
      int i;

      int n = (int)(param.nu * prob.Lenght);	// # of alpha's at upper bound

      for (i = 0; i < n; i++)
        alpha[i] = 1;
      if (n < prob.Lenght)
        alpha[n] = param.nu * prob.Lenght - n;
      for (i = n + 1; i < l; i++)
        alpha[i] = 0;

      for (i = 0; i < l; i++)
      {
        zeros[i] = 0;
        ones[i] = 1;
      }

      var s = new Solver();
      s.Solve(l, new OneClassQ(prob, param), zeros, ones,
        alpha, 1.0, 1.0, param.eps, si, param.shrinking);
    }

    private static void solve_epsilon_svr(SvmProblem prob, SvmParameter param, double[] alpha, SolutionInfo si)
    {
      int l = prob.Lenght;
      double[] alpha2 = new double[2 * l];
      double[] linear_term = new double[2 * l];
      sbyte[] y = new sbyte[2 * l];
      int i;

      for (i = 0; i < l; i++)
      {
        alpha2[i] = 0;
        linear_term[i] = param.p - prob.Y[i];
        y[i] = 1;

        alpha2[i + l] = 0;
        linear_term[i + l] = param.p + prob.Y[i];
        y[i + l] = -1;
      }

      Solver s = new Solver();
      s.Solve(2 * l, new SvrQ(prob, param), linear_term, y,
        alpha2, param.C, param.C, param.eps, si, param.shrinking);

      double sum_alpha = 0;
      for (i = 0; i < l; i++)
      {
        alpha[i] = alpha2[i] - alpha2[i + l];
        sum_alpha += Math.Abs(alpha[i]);
      }
      Svm.info("nu = " + sum_alpha / (param.C * l) + "\n");
    }

    private static void solve_nu_svr(SvmProblem prob, SvmParameter param, double[] alpha, SolutionInfo si)
    {
      int l = prob.Lenght;
      double C = param.C;
      double[] alpha2 = new double[2 * l];
      double[] linear_term = new double[2 * l];
      sbyte[] y = new sbyte[2 * l];
      int i;

      double sum = C * param.nu * l / 2;
      for (i = 0; i < l; i++)
      {
        alpha2[i] = alpha2[i + l] = Math.Min(sum, C);
        sum -= alpha2[i];

        linear_term[i] = -prob.Y[i];
        y[i] = 1;

        linear_term[i + l] = prob.Y[i];
        y[i + l] = -1;
      }

      var s = new SolverNu();
      s.Solve(2 * l, new SvrQ(prob, param), linear_term, y,
        alpha2, C, C, param.eps, si, param.shrinking);

      Svm.info("epsilon = " + (-si.r) + "\n");

      for (i = 0; i < l; i++)
        alpha[i] = alpha2[i] - alpha2[i + l];
    }

    private static DecisionFunction svm_train_one(SvmProblem prob, SvmParameter param, double Cp, double Cn)
    {
      double[] alpha = new double[prob.Lenght];
      var si = new SolutionInfo();
      switch (param.svm_type)
      {
        case SvmType.C_SVC:
          solve_c_svc(prob, param, alpha, si, Cp, Cn);
          break;
        case SvmType.NU_SVC:
          solve_nu_svc(prob, param, alpha, si);
          break;
        case SvmType.ONE_CLASS:
          solve_one_class(prob, param, alpha, si);
          break;
        case SvmType.EPSILON_SVR:
          solve_epsilon_svr(prob, param, alpha, si);
          break;
        case SvmType.NU_SVR:
          solve_nu_svr(prob, param, alpha, si);
          break;
      }

      Svm.info("obj = " + si.obj + ", rho = " + si.rho + "\n");

      // output SVs

      int nSV = 0;
      int nBSV = 0;
      for (int i = 0; i < prob.Lenght; i++)
      {
        if (Math.Abs(alpha[i]) > 0)
        {
          ++nSV;
          if (prob.Y[i] > 0)
          {
            if (Math.Abs(alpha[i]) >= si.upper_bound_p)
              ++nBSV;
          }
          else
          {
            if (Math.Abs(alpha[i]) >= si.upper_bound_n)
              ++nBSV;
          }
        }
      }

      Svm.info("nSV = " + nSV + ", nBSV = " + nBSV + "\n");

      var f = new DecisionFunction(alpha, si.rho);
      return f;
    }

    // Platt's binary SVM Probablistic Output: an improvement from Lin et al.
    private static void sigmoid_train(int l, double[] dec_values, double[] labels, double[] probAB)
    {
      //double A, B;
      double prior1 = 0, prior0 = 0;
      //int i;

      for (int i = 0; i < l; i++)
      {
        if (labels[i] > 0) prior1 += 1;
        else prior0 += 1;
      }

      const int max_iter = 100;	// Maximal number of iterations
      const double min_step = 1e-10;	// Minimal step taken in line search
      const double sigma = 1e-12;	// For numerically strict PD of Hessian
      const double eps = 1e-5;

      double hiTarget = (prior1 + 1.0) / (prior1 + 2.0);
      double loTarget = 1 / (prior0 + 2.0);
      double[] t = new double[l];
      double fApB, p, q, h11, h22, h21, g1, g2, det, dA, dB, gd, stepsize;
      double newA, newB, newf, d1, d2;
      int iter;

      // Initial Point and Initial Fun Value
      double A = 0.0; 
      double B = Math.Log((prior0 + 1.0) / (prior1 + 1.0));

      double fval = 0.0;

      for (int i = 0; i < l; i++)
      {
        if (labels[i] > 0) t[i] = hiTarget;
        else t[i] = loTarget;
        fApB = dec_values[i] * A + B;
        if (fApB >= 0)
          fval += t[i] * fApB + Math.Log(1 + Math.Exp(-fApB));
        else
          fval += (t[i] - 1) * fApB + Math.Log(1 + Math.Exp(fApB));
      }

      for (iter = 0; iter < max_iter; iter++)
      {
        // Update Gradient and Hessian (use H' = H + sigma I)
        h11 = sigma; // numerically ensures strict PD
        h22 = sigma;
        h21 = 0.0; g1 = 0.0; g2 = 0.0;
        for (int i = 0; i < l; i++)
        {
          fApB = dec_values[i] * A + B;
          if (fApB >= 0)
          {
            p = Math.Exp(-fApB) / (1.0 + Math.Exp(-fApB));
            q = 1.0 / (1.0 + Math.Exp(-fApB));
          }
          else
          {
            p = 1.0 / (1.0 + Math.Exp(fApB));
            q = Math.Exp(fApB) / (1.0 + Math.Exp(fApB));
          }
          d2 = p * q;
          h11 += dec_values[i] * dec_values[i] * d2;
          h22 += d2;
          h21 += dec_values[i] * d2;
          d1 = t[i] - p;
          g1 += dec_values[i] * d1;
          g2 += d1;
        }

        // Stopping Criteria
        if (Math.Abs(g1) < eps && Math.Abs(g2) < eps)
          break;

        // Finding Newton direction: -inv(H') * g
        det = h11 * h22 - h21 * h21;
        dA = -(h22 * g1 - h21 * g2) / det;
        dB = -(-h21 * g1 + h11 * g2) / det;
        gd = g1 * dA + g2 * dB;


        stepsize = 1;		// Line Search
        while (stepsize >= min_step)
        {
          newA = A + stepsize * dA;
          newB = B + stepsize * dB;

          // New function value
          newf = 0.0;
          for (int i = 0; i < l; i++)
          {
            fApB = dec_values[i] * newA + newB;
            if (fApB >= 0)
              newf += t[i] * fApB + Math.Log(1 + Math.Exp(-fApB));
            else
              newf += (t[i] - 1) * fApB + Math.Log(1 + Math.Exp(fApB));
          }
          // Check sufficient decrease
          if (newf < fval + 0.0001 * stepsize * gd)
          {
            A = newA; B = newB; fval = newf;
            break;
          }
          else
            stepsize = stepsize / 2.0;
        }

        if (stepsize < min_step)
        {
          Svm.info("Line search fails in two-class probability estimates\n");
          break;
        }
      }

      if (iter >= max_iter)
        Svm.info("Reaching maximal iterations in two-class probability estimates\n");
      probAB[0] = A; probAB[1] = B;
    }

    internal static double sigmoid_predict(double decision_value, double A, double B)
    {
      double fApB = decision_value * A + B;
      if (fApB >= 0)
        return Math.Exp(-fApB) / (1.0 + Math.Exp(-fApB));
      else
        return 1.0 / (1 + Math.Exp(fApB));
    }

    // Method 2 from the multiclass_prob paper by Wu, Lin, and Weng
    internal static void multiclass_probability(int k, double[][] r, double[] p)
    {
      int t, j;
      int iter = 0, max_iter = Math.Max(100, k);

      //double[][] Q = new double[k][k];
      double[][] Q = new double[k][];
      for (int i = 0; i < k; i++)
      {
        Q[i] = new double[k];
      }

      double[] Qp = new double[k];
      double pQp, eps = 0.005 / k;

      for (t = 0; t < k; t++)
      {
        p[t] = 1.0 / k;  // Valid if k = 1
        Q[t][t] = 0;
        for (j = 0; j < t; j++)
        {
          Q[t][t] += r[j][t] * r[j][t];
          Q[t][j] = Q[j][t];
        }
        for (j = t + 1; j < k; j++)
        {
          Q[t][t] += r[j][t] * r[j][t];
          Q[t][j] = -r[j][t] * r[t][j];
        }
      }
      for (iter = 0; iter < max_iter; iter++)
      {
        // stopping condition, recalculate QP,pQP for numerical accuracy
        pQp = 0;
        for (t = 0; t < k; t++)
        {
          Qp[t] = 0;
          for (j = 0; j < k; j++)
            Qp[t] += Q[t][j] * p[j];
          pQp += p[t] * Qp[t];
        }
        double max_error = 0;
        for (t = 0; t < k; t++)
        {
          double error = Math.Abs(Qp[t] - pQp);
          if (error > max_error)
            max_error = error;
        }
        if (max_error < eps) break;

        for (t = 0; t < k; t++)
        {
          double diff = (-Qp[t] + pQp) / Q[t][t];
          p[t] += diff;
          pQp = (pQp + diff * (diff * Q[t][t] + 2 * Qp[t])) / (1 + diff) / (1 + diff);
          for (j = 0; j < k; j++)
          {
            Qp[j] = (Qp[j] + diff * Q[t][j]) / (1 + diff);
            p[j] /= (1 + diff);
          }
        }
      }
      if (iter >= max_iter)
        Svm.info("Exceeds max_iter in multiclass_prob\n");
    }

    // Cross-validation decision values for probability estimates
    private static void svm_binary_svc_probability(SvmProblem prob, SvmParameter param, double Cp, double Cn, double[] probAB)
    {
      //int i;
      int nr_fold = 5;
      int[] perm = new int[prob.Lenght];
      double[] dec_values = new double[prob.Lenght];

      // random shuffle
      var rnd = new Random();
      for (int i = 0; i < prob.Lenght; i++) perm[i] = i;
      
      for (int i = 0; i < prob.Lenght; i++)
      {
        int j = i + (int)(rnd.NextDouble() * (prob.Lenght - i));
        //do { int _ = perm[i]; perm[i] = perm[j]; perm[j] = _; } while (false);
        Common.Swap(ref perm[i], ref perm[j]);
      }

      for (int i = 0; i < nr_fold; i++)
      {
        int begin = i * prob.Lenght / nr_fold;
        int end = (i + 1) * prob.Lenght / nr_fold;
        //int j;

        var subprobLenght = prob.Lenght - (end - begin);
        var subprob = new SvmProblem
        {
          X = new SvmNode[subprobLenght][],
          Y = new double[subprobLenght]
        };

        int k = 0;
        for (int j = 0; j < begin; j++)
        {
          subprob.X[k] = prob.X[perm[j]];
          subprob.Y[k] = prob.Y[perm[j]];
          ++k;
        }

        for (int j = end; j < prob.Lenght; j++)
        {
          subprob.X[k] = prob.X[perm[j]];
          subprob.Y[k] = prob.Y[perm[j]];
          ++k;
        }

        int p_count = 0, n_count = 0;
        
        for (int j = 0; j < k; j++)
          if (subprob.Y[j] > 0)
            p_count++;
          else
            n_count++;

        if (p_count == 0 && n_count == 0)
          for (int j = begin; j < end; j++)
            dec_values[perm[j]] = 0;
        else if (p_count > 0 && n_count == 0)
          for (int j = begin; j < end; j++)
            dec_values[perm[j]] = 1;
        else if (p_count == 0 && n_count > 0)
          for (int j = begin; j < end; j++)
            dec_values[perm[j]] = -1;
        else
        {
          var subparam = (SvmParameter)param.Clone();
          subparam.probability = false;
          subparam.C = 1.0;
          subparam.nr_weight = 2;
          subparam.weight_label = new int[2];
          subparam.weight = new double[2];
          subparam.weight_label[0] = +1;
          subparam.weight_label[1] = -1;
          subparam.weight[0] = Cp;
          subparam.weight[1] = Cn;
          var submodel = svm_train(subprob, subparam);
          for (int j = begin; j < end; j++)
          {
            double[] dec_value = new double[1];
            submodel.PredictValues(prob.X[perm[j]], dec_value);
            dec_values[perm[j]] = dec_value[0];
            // ensure +1 -1 order; reason not using CV subroutine
            dec_values[perm[j]] *= submodel.label[0];
          }
        }
      }
      sigmoid_train(prob.Lenght, dec_values, prob.Y, probAB);
    }

    // Return parameter of a Laplace distribution 
    private static double svm_svr_probability(SvmProblem prob, SvmParameter param)
    {
      int i;
      int nr_fold = 5;
      double[] ymv = new double[prob.Lenght];
      double mae = 0;

      var newparam = (SvmParameter)param.Clone();
      newparam.probability = false;
      svm_cross_validation(prob, newparam, nr_fold, ymv);
      for (i = 0; i < prob.Lenght; i++)
      {
        ymv[i] = prob.Y[i] - ymv[i];
        mae += Math.Abs(ymv[i]);
      }
      mae /= prob.Lenght;
      double std = Math.Sqrt(2 * mae * mae);
      int count = 0;
      mae = 0;
      for (i = 0; i < prob.Lenght; i++)
        if (Math.Abs(ymv[i]) > 5 * std)
          count = count + 1;
        else
          mae += Math.Abs(ymv[i]);
      mae /= (prob.Lenght - count);
      Svm.info("Prob. model for test data: target value = predicted value + z,\nz: Laplace distribution e^(-|z|/sigma)/(2sigma),sigma=" + mae + "\n");
      return mae;
    }

    // label: label name, start: begin of each class, count: #data of classes, perm: indices to the original data
    // perm, length l, must be allocated before calling this subroutine
    private static void svm_group_classes(SvmProblem prob, int[] nr_class_ret, int[][] label_ret, int[][] start_ret, int[][] count_ret, int[] perm)
    {
      int l = prob.Lenght;
      int max_nr_class = 16;
      int nr_class = 0;
      int[] label = new int[max_nr_class];
      int[] count = new int[max_nr_class];
      int[] data_label = new int[l];
      int i;

      for (i = 0; i < l; i++)
      {
        int this_label = (int)(prob.Y[i]);
        int j;
        for (j = 0; j < nr_class; j++)
        {
          if (this_label == label[j])
          {
            ++count[j];
            break;
          }
        }
        data_label[i] = j;
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

      int[] start = new int[nr_class];
      start[0] = 0;
      for (i = 1; i < nr_class; i++)
        start[i] = start[i - 1] + count[i - 1];
      for (i = 0; i < l; i++)
      {
        perm[start[data_label[i]]] = i;
        ++start[data_label[i]];
      }
      start[0] = 0;
      for (i = 1; i < nr_class; i++)
        start[i] = start[i - 1] + count[i - 1];

      nr_class_ret[0] = nr_class;
      label_ret[0] = label;
      start_ret[0] = start;
      count_ret[0] = count;
    }

    #endregion


    //
    // Interface functions
    //
    public static SvmModel svm_train(SvmProblem prob, SvmParameter param)
    {
      var model = new SvmModel();
      model.param = param;

      if (param.svm_type.IsSVROrOneClass())
      {
        // regression or one-class-svm
        model.nr_class = 2;
        model.label = null;
        model.nSV = null;
        model.probA = null; model.probB = null;
        model.sv_coef = new double[1][];

        if (param.probability && param.svm_type.IsSVR())
        {
          model.probA = new double[1];
          model.probA[0] = svm_svr_probability(prob, param);
        }

        DecisionFunction f = svm_train_one(prob, param, 0, 0);
        model.rho = new double[1];
        model.rho[0] = f.Rho;

        int nSV = 0;
        int i;
        for (i = 0; i < prob.Lenght; i++)
          if (Math.Abs(f.Alpha[i]) > 0) ++nSV;
        model.l = nSV;
        model.SV = new SvmNode[nSV][];
        model.sv_coef[0] = new double[nSV];
        int j = 0;
        for (i = 0; i < prob.Lenght; i++)
          if (Math.Abs(f.Alpha[i]) > 0)
          {
            model.SV[j] = prob.X[i];
            model.sv_coef[0][j] = f.Alpha[i];
            ++j;
          }
      }
      else
      {
        // classification
        int l = prob.Lenght;
        int[] tmp_nr_class = new int[1];
        int[][] tmp_label = new int[1][];
        int[][] tmp_start = new int[1][];
        int[][] tmp_count = new int[1][];
        int[] perm = new int[l];

        // group training data of the same class
        svm_group_classes(prob, tmp_nr_class, tmp_label, tmp_start, tmp_count, perm);
        int nr_class = tmp_nr_class[0];
        int[] label = tmp_label[0];
        int[] start = tmp_start[0];
        int[] count = tmp_count[0];
        SvmNode[][] x = new SvmNode[l][];
        int i;
        for (i = 0; i < l; i++)
          x[i] = prob.X[perm[i]];

        // calculate weighted C

        double[] weighted_C = new double[nr_class];
        for (i = 0; i < nr_class; i++)
          weighted_C[i] = param.C;
        for (i = 0; i < param.nr_weight; i++)
        {
          int j;
          for (j = 0; j < nr_class; j++)
            if (param.weight_label[i] == label[j])
              break;
          if (j == nr_class)
            Console.Error.WriteLine("warning: class label " + param.weight_label[i] + " specified in weight is not found\n");
          else
            weighted_C[j] *= param.weight[i];
        }

        // train k*(k-1)/2 models

        bool[] nonzero = new bool[l];
        for (i = 0; i < l; i++)
          nonzero[i] = false;
        DecisionFunction[] f = new DecisionFunction[nr_class * (nr_class - 1) / 2];

        double[] probA = null, probB = null;
        if (param.probability)
        {
          probA = new double[nr_class * (nr_class - 1) / 2];
          probB = new double[nr_class * (nr_class - 1) / 2];
        }

        int p = 0;
        for (i = 0; i < nr_class; i++)
          for (int j = i + 1; j < nr_class; j++)
          {
            
            int si = start[i], sj = start[j];
            int ci = count[i], cj = count[j];
            var subprobLenght = ci + cj;
            var sub_prob = new SvmProblem
            {
              X = new SvmNode[subprobLenght][],
              Y = new double[subprobLenght]
            };

            int k;
            for (k = 0; k < ci; k++)
            {
              sub_prob.X[k] = x[si + k];
              sub_prob.Y[k] = +1;
            }
            for (k = 0; k < cj; k++)
            {
              sub_prob.X[ci + k] = x[sj + k];
              sub_prob.Y[ci + k] = -1;
            }

            if (param.probability)
            {
              double[] probAB = new double[2];
              svm_binary_svc_probability(sub_prob, param, weighted_C[i], weighted_C[j], probAB);
              probA[p] = probAB[0];
              probB[p] = probAB[1];
            }

            f[p] = svm_train_one(sub_prob, param, weighted_C[i], weighted_C[j]);
            for (k = 0; k < ci; k++)
              if (!nonzero[si + k] && Math.Abs(f[p].Alpha[k]) > 0)
                nonzero[si + k] = true;
            for (k = 0; k < cj; k++)
              if (!nonzero[sj + k] && Math.Abs(f[p].Alpha[ci + k]) > 0)
                nonzero[sj + k] = true;
            ++p;
          }

        // build output

        model.nr_class = nr_class;

        model.label = new int[nr_class];
        for (i = 0; i < nr_class; i++)
          model.label[i] = label[i];

        model.rho = new double[nr_class * (nr_class - 1) / 2];
        for (i = 0; i < nr_class * (nr_class - 1) / 2; i++)
          model.rho[i] = f[i].Rho;

        if (param.probability)
        {
          model.probA = new double[nr_class * (nr_class - 1) / 2];
          model.probB = new double[nr_class * (nr_class - 1) / 2];
          for (i = 0; i < nr_class * (nr_class - 1) / 2; i++)
          {
            model.probA[i] = probA[i];
            model.probB[i] = probB[i];
          }
        }
        else
        {
          model.probA = null;
          model.probB = null;
        }

        int nnz = 0;
        int[] nz_count = new int[nr_class];
        model.nSV = new int[nr_class];
        for (i = 0; i < nr_class; i++)
        {
          int nSV = 0;
          for (int j = 0; j < count[i]; j++)
            if (nonzero[start[i] + j])
            {
              ++nSV;
              ++nnz;
            }
          model.nSV[i] = nSV;
          nz_count[i] = nSV;
        }

        Svm.info("Total nSV = " + nnz + "\n");

        model.l = nnz;
        model.SV = new SvmNode[nnz][];
        p = 0;
        for (i = 0; i < l; i++)
          if (nonzero[i]) model.SV[p++] = x[i];

        int[] nz_start = new int[nr_class];
        nz_start[0] = 0;
        for (i = 1; i < nr_class; i++)
          nz_start[i] = nz_start[i - 1] + nz_count[i - 1];

        model.sv_coef = new double[nr_class - 1][];
        for (i = 0; i < nr_class - 1; i++)
          model.sv_coef[i] = new double[nnz];

        p = 0;
        for (i = 0; i < nr_class; i++)
          for (int j = i + 1; j < nr_class; j++)
          {
            // classifier (i,j): coefficients with
            // i are in sv_coef[j-1][nz_start[i]...],
            // j are in sv_coef[i][nz_start[j]...]

            int si = start[i];
            int sj = start[j];
            int ci = count[i];
            int cj = count[j];

            int q = nz_start[i];
            int k;
            for (k = 0; k < ci; k++)
              if (nonzero[si + k])
                model.sv_coef[j - 1][q++] = f[p].Alpha[k];
            q = nz_start[j];
            for (k = 0; k < cj; k++)
              if (nonzero[sj + k])
                model.sv_coef[i][q++] = f[p].Alpha[ci + k];
            ++p;
          }
      }
      return model;
    }

    // Stratified cross validation
    public static void svm_cross_validation(SvmProblem prob, SvmParameter param, int nr_fold, double[] target)
    {
      int i;
      int[] fold_start = new int[nr_fold + 1];
      int l = prob.Lenght;
      int[] perm = new int[l];

      // stratified cv may not give leave-one-out rate
      // Each class to l folds -> some folds may have zero elements
      if (param.svm_type.IsSVC() && nr_fold < l)
      {
        int[] tmp_nr_class = new int[1];
        int[][] tmp_label = new int[1][];
        int[][] tmp_start = new int[1][];
        int[][] tmp_count = new int[1][];

        svm_group_classes(prob, tmp_nr_class, tmp_label, tmp_start, tmp_count, perm);

        int nr_class = tmp_nr_class[0];
        int[] start = tmp_start[0];
        int[] count = tmp_count[0];

        // random shuffle and then data grouped by fold using the array perm
        int[] fold_count = new int[nr_fold];
        int c;
        int[] index = new int[l];
        for (i = 0; i < l; i++)
          index[i] = perm[i];

        var rnd = new Random();

        for (c = 0; c < nr_class; c++)
          for (i = 0; i < count[c]; i++)
          {
            int j = i + (int)(rnd.NextDouble() * (count[c] - i));
            //do { int _ = index[start[c] + j]; index[start[c] + j] = index[start[c] + i]; index[start[c] + i] = _; } while (false);
            Common.Swap(ref index[start[c] + j], ref index[start[c] + j]);
          }
        for (i = 0; i < nr_fold; i++)
        {
          fold_count[i] = 0;
          for (c = 0; c < nr_class; c++)
            fold_count[i] += (i + 1) * count[c] / nr_fold - i * count[c] / nr_fold;
        }
        fold_start[0] = 0;
        for (i = 1; i <= nr_fold; i++)
          fold_start[i] = fold_start[i - 1] + fold_count[i - 1];
        for (c = 0; c < nr_class; c++)
          for (i = 0; i < nr_fold; i++)
          {
            int begin = start[c] + i * count[c] / nr_fold;
            int end = start[c] + (i + 1) * count[c] / nr_fold;
            for (int j = begin; j < end; j++)
            {
              perm[fold_start[i]] = index[j];
              fold_start[i]++;
            }
          }
        fold_start[0] = 0;
        for (i = 1; i <= nr_fold; i++)
          fold_start[i] = fold_start[i - 1] + fold_count[i - 1];
      }
      else
      {
        var rnd = new Random();

        for (i = 0; i < l; i++)
        {
          perm[i] = i;
        }

        for (i = 0; i < l; i++)
        {
          int j = i + (int)(rnd.NextDouble() * (l - i));
          //do { int _ = perm[i]; perm[i] = perm[j]; perm[j] = _; } while (false);
          Common.Swap(ref perm[i], ref perm[j]);
        }

        for (i = 0; i <= nr_fold; i++)
        {
          fold_start[i] = i * l / nr_fold;
        }
      }

      for (i = 0; i < nr_fold; i++)
      {
        int begin = fold_start[i];
        int end = fold_start[i + 1];
        int j, k;
        
        var subprobLenght = l - (end - begin);
        var subprob = new SvmProblem
        {
          X = new SvmNode[subprobLenght][],
          Y = new double[subprobLenght]
        };

        k = 0;
        for (j = 0; j < begin; j++)
        {
          subprob.X[k] = prob.X[perm[j]];
          subprob.Y[k] = prob.Y[perm[j]];
          ++k;
        }
        for (j = end; j < l; j++)
        {
          subprob.X[k] = prob.X[perm[j]];
          subprob.Y[k] = prob.Y[perm[j]];
          ++k;
        }
        var submodel = svm_train(subprob, param);
        if (param.probability && param.svm_type.IsSVC())
        {
          double[] prob_estimates = new double[submodel.NrClass];
          for (j = begin; j < end; j++)
            target[perm[j]] = submodel.PredictProbability(prob.X[perm[j]], prob_estimates);
        }
        else
          for (j = begin; j < end; j++)
            target[perm[j]] = submodel.Predict(prob.X[perm[j]]);
      }
    }

    //static readonly string[] svm_type_table = { "c_svc", "nu_svc", "one_class", "epsilon_svr", "nu_svr", };
    //static readonly string[] kernel_type_table = { "linear", "polynomial", "rbf", "sigmoid", "precomputed" };

    //implement later
    //public static void svm_save_model(String model_file_name, svm_model model) 
    //{
    //  DataOutputStream fp = new DataOutputStream(new BufferedOutputStream(new FileOutputStream(model_file_name)));

    //  svm_parameter param = model.param;

    //  fp.writeBytes("svm_type "+svm_type_table[param.svm_type]+"\n");
    //  fp.writeBytes("kernel_type "+kernel_type_table[param.kernel_type]+"\n");

    //  if(param.kernel_type == svm_parameter.POLY)
    //    fp.writeBytes("degree "+param.degree+"\n");

    //  if(param.kernel_type == svm_parameter.POLY ||
    //     param.kernel_type == svm_parameter.RBF ||
    //     param.kernel_type == svm_parameter.SIGMOID)
    //    fp.writeBytes("gamma "+param.gamma+"\n");

    //  if(param.kernel_type == svm_parameter.POLY ||
    //     param.kernel_type == svm_parameter.SIGMOID)
    //    fp.writeBytes("coef0 "+param.coef0+"\n");

    //  int nr_class = model.nr_class;
    //  int l = model.l;
    //  fp.writeBytes("nr_class "+nr_class+"\n");
    //  fp.writeBytes("total_sv "+l+"\n");

    //  {
    //    fp.writeBytes("rho");
    //    for(int i=0;i<nr_class*(nr_class-1)/2;i++)
    //      fp.writeBytes(" "+model.rho[i]);
    //    fp.writeBytes("\n");
    //  }

    //  if(model.label != null)
    //  {
    //    fp.writeBytes("label");
    //    for(int i=0;i<nr_class;i++)
    //      fp.writeBytes(" "+model.label[i]);
    //    fp.writeBytes("\n");
    //  }

    //  if(model.probA != null) // regression has probA only
    //  {
    //    fp.writeBytes("probA");
    //    for(int i=0;i<nr_class*(nr_class-1)/2;i++)
    //      fp.writeBytes(" "+model.probA[i]);
    //    fp.writeBytes("\n");
    //  }
    //  if(model.probB != null) 
    //  {
    //    fp.writeBytes("probB");
    //    for(int i=0;i<nr_class*(nr_class-1)/2;i++)
    //      fp.writeBytes(" "+model.probB[i]);
    //    fp.writeBytes("\n");
    //  }

    //  if(model.nSV != null)
    //  {
    //    fp.writeBytes("nr_sv");
    //    for(int i=0;i<nr_class;i++)
    //      fp.writeBytes(" "+model.nSV[i]);
    //    fp.writeBytes("\n");
    //  }

    //  fp.writeBytes("SV\n");
    //  double[][] sv_coef = model.sv_coef;
    //  svm_node[][] SV = model.SV;

    //  for(int i=0;i<l;i++)
    //  {
    //    for(int j=0;j<nr_class-1;j++)
    //      fp.writeBytes(sv_coef[j][i]+" ");

    //    svm_node[] p = SV[i];
    //    if(param.kernel_type == svm_parameter.PRECOMPUTED)
    //      fp.writeBytes("0:"+(int)(p[0].value));
    //    else	
    //      for(int j=0;j<p.length;j++)
    //        fp.writeBytes(p[j].index+":"+p[j].value+" ");
    //    fp.writeBytes("\n");
    //  }

    //  fp.close();
    //}

    private static double atof(string s)
    {
      return Double.Parse(s);
    }

    private static int atoi(string s)
    {
      return int.Parse(s);
    }

    //implement later
    //public static svm_model svm_load_model(String model_file_name)
    //{
    //  return svm_load_model(new BufferedReader(new FileReader(model_file_name)));
    //}

    //implement later
    //public static svm_model svm_load_model(BufferedReader fp)
    //{
    //  // read parameters

    //  svm_model model = new svm_model();
    //  svm_parameter param = new svm_parameter();
    //  model.param = param;
    //  model.rho = null;
    //  model.probA = null;
    //  model.probB = null;
    //  model.label = null;
    //  model.nSV = null;

    //  while(true)
    //  {
    //    String cmd = fp.readLine();
    //    String arg = cmd.substring(cmd.indexOf(' ')+1);

    //    if(cmd.startsWith("svm_type"))
    //    {
    //      int i;
    //      for(i=0;i<svm_type_table.length;i++)
    //      {
    //        if(arg.indexOf(svm_type_table[i])!=-1)
    //        {
    //          param.svm_type=i;
    //          break;
    //        }
    //      }
    //      if(i == svm_type_table.length)
    //      {
    //        System.err.print("unknown svm type.\n");
    //        return null;
    //      }
    //    }
    //    else if(cmd.startsWith("kernel_type"))
    //    {
    //      int i;
    //      for(i=0;i<kernel_type_table.length;i++)
    //      {
    //        if(arg.indexOf(kernel_type_table[i])!=-1)
    //        {
    //          param.kernel_type=i;
    //          break;
    //        }
    //      }
    //      if(i == kernel_type_table.length)
    //      {
    //        System.err.print("unknown kernel function.\n");
    //        return null;
    //      }
    //    }
    //    else if(cmd.startsWith("degree"))
    //      param.degree = atoi(arg);
    //    else if(cmd.startsWith("gamma"))
    //      param.gamma = atof(arg);
    //    else if(cmd.startsWith("coef0"))
    //      param.coef0 = atof(arg);
    //    else if(cmd.startsWith("nr_class"))
    //      model.nr_class = atoi(arg);
    //    else if(cmd.startsWith("total_sv"))
    //      model.l = atoi(arg);
    //    else if(cmd.startsWith("rho"))
    //    {
    //      int n = model.nr_class * (model.nr_class-1)/2;
    //      model.rho = new double[n];
    //      StringTokenizer st = new StringTokenizer(arg);
    //      for(int i=0;i<n;i++)
    //        model.rho[i] = atof(st.nextToken());
    //    }
    //    else if(cmd.startsWith("label"))
    //    {
    //      int n = model.nr_class;
    //      model.label = new int[n];
    //      StringTokenizer st = new StringTokenizer(arg);
    //      for(int i=0;i<n;i++)
    //        model.label[i] = atoi(st.nextToken());					
    //    }
    //    else if(cmd.startsWith("probA"))
    //    {
    //      int n = model.nr_class*(model.nr_class-1)/2;
    //      model.probA = new double[n];
    //      StringTokenizer st = new StringTokenizer(arg);
    //      for(int i=0;i<n;i++)
    //        model.probA[i] = atof(st.nextToken());					
    //    }
    //    else if(cmd.startsWith("probB"))
    //    {
    //      int n = model.nr_class*(model.nr_class-1)/2;
    //      model.probB = new double[n];
    //      StringTokenizer st = new StringTokenizer(arg);
    //      for(int i=0;i<n;i++)
    //        model.probB[i] = atof(st.nextToken());					
    //    }
    //    else if(cmd.startsWith("nr_sv"))
    //    {
    //      int n = model.nr_class;
    //      model.nSV = new int[n];
    //      StringTokenizer st = new StringTokenizer(arg);
    //      for(int i=0;i<n;i++)
    //        model.nSV[i] = atoi(st.nextToken());
    //    }
    //    else if(cmd.startsWith("SV"))
    //    {
    //      break;
    //    }
    //    else
    //    {
    //      System.err.print("unknown text in model file: ["+cmd+"]\n");
    //      return null;
    //    }
    //  }

    //  // read sv_coef and SV

    //  int m = model.nr_class - 1;
    //  int l = model.l;
    //  model.sv_coef = new double[m][l];
    //  model.SV = new svm_node[l][];

    //  for(int i=0;i<l;i++)
    //  {
    //    String line = fp.readLine();
    //    StringTokenizer st = new StringTokenizer(line," \t\n\r\f:");

    //    for(int k=0;k<m;k++)
    //      model.sv_coef[k][i] = atof(st.nextToken());
    //    int n = st.countTokens()/2;
    //    model.SV[i] = new svm_node[n];
    //    for(int j=0;j<n;j++)
    //    {
    //      model.SV[i][j] = new svm_node();
    //      model.SV[i][j].index = atoi(st.nextToken());
    //      model.SV[i][j].value = atof(st.nextToken());
    //    }
    //  }

    //  fp.close();
    //  return model;
    //}


    public static void svm_set_print_string_function(svm_print_interface print_func)
    {
      if (print_func == null)
        svm_print_string = svm_print_stdout;
      else
        svm_print_string = print_func;
    }
  }
}
