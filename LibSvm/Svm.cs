using System;

namespace LibSvm
{
  using svm_print_interface = Action<string>;

  //
  // construct and solve various formulations
  //
  public static class Svm
  {
    public const int LIBSVM_VERSION = 312;

    #region private_members

    private static readonly svm_print_interface svm_print_stdout = str => Console.WriteLine(str);

    private static svm_print_interface svm_print_string = svm_print_stdout;

    internal static void info(String s)
    {
      svm_print_string(s);
    }

    private static void solve_c_svc<TPattern>(SvmProblem<TPattern> prob, SvmParameter<TPattern> param, double[] alpha, SolutionInfo si, double Cp, double Cn)
    {
      var length = prob.Length;
      var minus_ones = new double[length];
      var y = new sbyte[length];

      for (int i = 0; i < length; i++)
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

      var solver = new Solver();
      solver.Solve(length, new SvcQ<TPattern>(prob, param, y), minus_ones, y,
        alpha, Cp, Cn, param.Eps, si, param.Shrinking);

      double sum_alpha = 0;
      for (int i = 0; i < length; i++)
      {
        sum_alpha += alpha[i];
      }

      if (Cp == Cn)
      {
        Svm.info("nu = " + sum_alpha / (Cp * prob.Length) + "\n");
      }

      for (int i = 0; i < length; i++)
      {
        alpha[i] *= y[i];
      }
    }

    private static void solve_nu_svc<TPattern>(SvmProblem<TPattern> prob, SvmParameter<TPattern> param, double[] alpha, SolutionInfo si)
    {
      var length = prob.Length;
      var nu = param.Nu;
      var y = new sbyte[length];

      for (int i = 0; i < length; i++)
        if (prob.Y[i] > 0)
          y[i] = +1;
        else
          y[i] = -1;

      var sum_pos = nu * length / 2;
      var sum_neg = nu * length / 2;

      for (int i = 0; i < length; i++)
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

      double[] zeros = new double[length];

      for (int i = 0; i < length; i++)
        zeros[i] = 0;

      var solver = new SolverNu();
      solver.Solve(length, new SvcQ<TPattern>(prob, param, y), zeros, y,
        alpha, 1.0, 1.0, param.Eps, si, param.Shrinking);
      double r = si.R;

      Svm.info("C = " + 1 / r + "\n");

      for (int i = 0; i < length; i++)
        alpha[i] *= y[i] / r;

      si.Rho /= r;
      si.Obj /= (r * r);
      si.UpperBoundP = 1 / r;
      si.UpperBoundN = 1 / r;
    }

    private static void solve_one_class<TPattern>(SvmProblem<TPattern> prob, SvmParameter<TPattern> param, double[] alpha, SolutionInfo si)
    {
      var length = prob.Length;
      var zeros = new double[length];
      var ones = new sbyte[length];

      int n = (int)(param.Nu * prob.Length);    // # of alpha's at upper bound

      for (int i = 0; i < n; i++)
        alpha[i] = 1;
      if (n < prob.Length)
        alpha[n] = param.Nu * prob.Length - n;
      for (int i = n + 1; i < length; i++)
        alpha[i] = 0;

      for (int i = 0; i < length; i++)
      {
        zeros[i] = 0;
        ones[i] = 1;
      }

      var solver = new Solver();
      solver.Solve(length, new OneClassQ<TPattern>(prob, param), zeros, ones,
        alpha, 1.0, 1.0, param.Eps, si, param.Shrinking);
    }

    private static void solve_epsilon_svr<TPattern>(SvmProblem<TPattern> prob, SvmParameter<TPattern> param, double[] alpha, SolutionInfo si)
    {
      var length = prob.Length;
      var alpha2 = new double[2 * length];
      var linear_term = new double[2 * length];
      var y = new sbyte[2 * length];

      for (int i = 0; i < length; i++)
      {
        alpha2[i] = 0;
        linear_term[i] = param.P - prob.Y[i];
        y[i] = 1;

        alpha2[i + length] = 0;
        linear_term[i + length] = param.P + prob.Y[i];
        y[i + length] = -1;
      }

      var solver = new Solver();
      solver.Solve(2 * length, new SvrQ<TPattern>(prob, param), linear_term, y,
        alpha2, param.C, param.C, param.Eps, si, param.Shrinking);

      double sum_alpha = 0;
      for (int i = 0; i < length; i++)
      {
        alpha[i] = alpha2[i] - alpha2[i + length];
        sum_alpha += Math.Abs(alpha[i]);
      }
      Svm.info("nu = " + sum_alpha / (param.C * length) + "\n");
    }

    private static void solve_nu_svr<TPattern>(SvmProblem<TPattern> prob, SvmParameter<TPattern> param, double[] alpha, SolutionInfo si)
    {
      var length = prob.Length;
      var C = param.C;
      var alpha2 = new double[2 * length];
      var linear_term = new double[2 * length];
      var y = new sbyte[2 * length];

      var sum = C * param.Nu * length / 2;
      for (int i = 0; i < length; i++)
      {
        alpha2[i] = alpha2[i + length] = Math.Min(sum, C);
        sum -= alpha2[i];

        linear_term[i] = -prob.Y[i];
        y[i] = 1;

        linear_term[i + length] = prob.Y[i];
        y[i + length] = -1;
      }

      var solver = new SolverNu();
      solver.Solve(2 * length, new SvrQ<TPattern>(prob, param), linear_term, y,
        alpha2, C, C, param.Eps, si, param.Shrinking);

      Svm.info("epsilon = " + (-si.R) + "\n");

      for (int i = 0; i < length; i++)
        alpha[i] = alpha2[i] - alpha2[i + length];
    }

    private static DecisionFunction svm_train_one<TPattern>(SvmProblem<TPattern> prob, SvmParameter<TPattern> param, double Cp, double Cn)
    {
      var alpha = new double[prob.Length];
      var si = new SolutionInfo();
      switch (param.SvmType)
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

      Svm.info("obj = " + si.Obj + ", rho = " + si.Rho + "\n");

      // output SVs

      int nSV = 0;
      int nBSV = 0;
      for (int i = 0; i < prob.Length; i++)
      {
        if (Math.Abs(alpha[i]) > 0)
        {
          ++nSV;
          if (prob.Y[i] > 0)
          {
            if (Math.Abs(alpha[i]) >= si.UpperBoundP)
              ++nBSV;
          }
          else
          {
            if (Math.Abs(alpha[i]) >= si.UpperBoundN)
              ++nBSV;
          }
        }
      }

      Svm.info("nSV = " + nSV + ", nBSV = " + nBSV + "\n");

      var f = new DecisionFunction(alpha, si.Rho);
      return f;
    }

    // Platt's binary SVM Probablistic Output: an improvement from Lin et al.
    private static void sigmoid_train(int length, double[] dec_values, double[] labels, double[] probAB)
    {
      double prior1 = 0, prior0 = 0;

      for (int i = 0; i < length; i++)
      {
        if (labels[i] > 0) prior1 += 1;
        else prior0 += 1;
      }

      const int max_iter = 100;         // Maximal number of iterations
      const double min_step = 1e-10;    // Minimal step taken in line search
      const double sigma = 1e-12;       // For numerically strict PD of Hessian
      const double eps = 1e-5;

      double hiTarget = (prior1 + 1.0) / (prior1 + 2.0);
      double loTarget = 1 / (prior0 + 2.0);
      double[] t = new double[length];
      double fApB;
      int iter;

      // Initial Point and Initial Fun Value
      double A = 0.0;
      double B = Math.Log((prior0 + 1.0) / (prior1 + 1.0));

      double fval = 0.0;

      for (int i = 0; i < length; i++)
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
        double h11 = sigma; // numerically ensures strict PD
        double h22 = sigma;
        double h21 = 0.0;

        double g1 = 0.0;
        double g2 = 0.0;

        for (int i = 0; i < length; i++)
        {
          double p, q;
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
          double d2 = p * q;
          h11 += dec_values[i] * dec_values[i] * d2;
          h22 += d2;
          h21 += dec_values[i] * d2;
          double d1 = t[i] - p;
          g1 += dec_values[i] * d1;
          g2 += d1;
        }

        // Stopping Criteria
        if (Math.Abs(g1) < eps && Math.Abs(g2) < eps)
          break;

        // Finding Newton direction: -inv(H') * g
        double det = h11 * h22 - h21 * h21;
        double dA = -(h22 * g1 - h21 * g2) / det;
        double dB = -(-h21 * g1 + h11 * g2) / det;
        double gd = g1 * dA + g2 * dB;

        double stepsize = 1;  // Line Search
        while (stepsize >= min_step)
        {
          double newA = A + stepsize * dA;
          double newB = B + stepsize * dB;

          // New function value
          double newf = 0.0;
          for (int i = 0; i < length; i++)
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
    private static void svm_binary_svc_probability<TPattern>(SvmProblem<TPattern> prob, SvmParameter<TPattern> param, double Cp, double Cn, double[] probAB)
    {
      int nr_fold = 5;
      int[] perm = new int[prob.Length];
      double[] dec_values = new double[prob.Length];

      // random shuffle
      var rnd = new Random();
      for (int i = 0; i < prob.Length; i++) perm[i] = i;

      for (int i = 0; i < prob.Length; i++)
      {
        int j = i + (int)(rnd.NextDouble() * (prob.Length - i));
        //do { int _ = perm[i]; perm[i] = perm[j]; perm[j] = _; } while (false);
        Common.Swap(ref perm[i], ref perm[j]);
      }

      for (int i = 0; i < nr_fold; i++)
      {
        int begin = i * prob.Length / nr_fold;
        int end = (i + 1) * prob.Length / nr_fold;
        //int j;

        var subprobLenght = prob.Length - (end - begin);
        var subprob = new SvmProblem<TPattern>
        {
          X = new TPattern[subprobLenght],
          Y = new double[subprobLenght]
        };

        int k = 0;
        for (int j = 0; j < begin; j++)
        {
          subprob.X[k] = prob.X[perm[j]];
          subprob.Y[k] = prob.Y[perm[j]];
          ++k;
        }

        for (int j = end; j < prob.Length; j++)
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
          var subparam = (SvmParameter<TPattern>)param.Clone();
          subparam.Probability = false;
          subparam.C = 1.0;
          subparam.WeightLabel = new int[2];
          subparam.Weight = new double[2];
          subparam.WeightLabel[0] = +1;
          subparam.WeightLabel[1] = -1;
          subparam.Weight[0] = Cp;
          subparam.Weight[1] = Cn;
          var submodel = Train(subprob, subparam);
          for (int j = begin; j < end; j++)
          {
            double[] dec_value = new double[1];
            submodel.PredictValues(prob.X[perm[j]], dec_value);
            dec_values[perm[j]] = dec_value[0];
            // ensure +1 -1 order; reason not using CV subroutine
            dec_values[perm[j]] *= submodel.Label[0];
          }
        }
      }
      sigmoid_train(prob.Length, dec_values, prob.Y, probAB);
    }

    // Return parameter of a Laplace distribution
    private static double svm_svr_probability<TPattern>(SvmProblem<TPattern> prob, SvmParameter<TPattern> param)
    {
      int nr_fold = 5;
      double[] ymv = new double[prob.Length];
      double mae = 0;

      var newparam = (SvmParameter<TPattern>)param.Clone();
      newparam.Probability = false;
      CrossValidation(prob, newparam, nr_fold, ymv);
      for (int i = 0; i < prob.Length; i++)
      {
        ymv[i] = prob.Y[i] - ymv[i];
        mae += Math.Abs(ymv[i]);
      }
      mae /= prob.Length;
      double std = Math.Sqrt(2 * mae * mae);
      int count = 0;
      mae = 0;
      for (int i = 0; i < prob.Length; i++)
        if (Math.Abs(ymv[i]) > 5 * std)
          count = count + 1;
        else
          mae += Math.Abs(ymv[i]);
      mae /= (prob.Length - count);
      Svm.info("Prob. model for test data: target value = predicted value + z,\nz: Laplace distribution e^(-|z|/sigma)/(2sigma),sigma=" + mae + "\n");
      return mae;
    }

    // label: label name, start: begin of each class, count: #data of classes, perm: indices to the original data
    // perm, length l, must be allocated before calling this subroutine
    private static void svm_group_classes<TPattern>(SvmProblem<TPattern> prob, out int nr_class_ret, out int[] label_ret, out int[] start_ret, out int[] count_ret, int[] perm)
    {
      var length = prob.Length;
      var max_nr_class = 16;
      int nr_class = 0;
      var label = new int[max_nr_class];
      var count = new int[max_nr_class];
      var data_label = new int[length];

      for (int i = 0; i < length; i++)
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
      for (int i = 1; i < nr_class; i++)
      {
        start[i] = start[i - 1] + count[i - 1];
      }

      for (int i = 0; i < length; i++)
      {
        perm[start[data_label[i]]] = i;
        ++start[data_label[i]];
      }

      start[0] = 0;
      for (int i = 1; i < nr_class; i++)
      {
        start[i] = start[i - 1] + count[i - 1];
      }

      nr_class_ret = nr_class;
      label_ret = label;
      start_ret = start;
      count_ret = count;
    }

    #endregion


    //
    // Interface functions
    //
    public static SvmModel<TPattern> Train<TPattern>(SvmProblem<TPattern> prob, SvmParameter<TPattern> param)
    {
      var model = new SvmModel<TPattern>
      {
        Param = param
      };

      if (param.SvmType.IsSVROrOneClass())
      {
        // regression or one-class-svm
        model.NrClass = 2;
        model.Label = null;
        model.SupportVectorsNumbers = null;
        model.ProbA = null; model.ProbB = null;
        model.SupportVectorsCoefficients = new double[1][];

        if (param.Probability && param.SvmType.IsSVR())
        {
          model.ProbA = new double[1];
          model.ProbA[0] = svm_svr_probability(prob, param);
        }

        DecisionFunction f = svm_train_one(prob, param, 0, 0);
        model.Rho = new double[1];
        model.Rho[0] = f.Rho;

        int nSV = 0;
        for (int i = 0; i < prob.Length; i++)
          if (Math.Abs(f.Alpha[i]) > 0) ++nSV;
        model.TotalSupportVectorsNumber = nSV;
        model.SupportVectors = new TPattern[nSV];
        model.SupportVectorsCoefficients[0] = new double[nSV];
        int j = 0;
        for (int i = 0; i < prob.Length; i++)
          if (Math.Abs(f.Alpha[i]) > 0)
          {
            model.SupportVectors[j] = prob.X[i];
            model.SupportVectorsCoefficients[0][j] = f.Alpha[i];
            ++j;
          }
      }
      else
      {
        // classification
        var length = prob.Length;
        int[] perm = new int[length];

        int nr_class;
        int[] label;
        int[] start;
        int[] count;

        // group training data of the same class
        svm_group_classes(prob, out nr_class, out label, out start, out count, perm);

        if (nr_class == 1)
          Svm.info("WARNING: training data in only one class. See README for details.\n");

        TPattern[] x = new TPattern[length];
        for (int i = 0; i < length; i++)
          x[i] = prob.X[perm[i]];

        // calculate weighted C

        double[] weighted_C = new double[nr_class];
        for (int i = 0; i < nr_class; i++)
          weighted_C[i] = param.C;
        for (int i = 0; i < param.WeightsCount; i++)
        {
          int j;
          for (j = 0; j < nr_class; j++)
            if (param.WeightLabel[i] == label[j])
              break;
          if (j == nr_class)
            Console.Error.WriteLine("WARNING: class label " + param.WeightLabel[i] + " specified in weight is not found\n");
          else
            weighted_C[j] *= param.Weight[i];
        }

        // train k*(k-1)/2 models

        var nonzero = new bool[length];
        for (int i = 0; i < length; i++)
          nonzero[i] = false;
        var f = new DecisionFunction[nr_class * (nr_class - 1) / 2];

        double[] probA = null, probB = null;
        if (param.Probability)
        {
          probA = new double[nr_class * (nr_class - 1) / 2];
          probB = new double[nr_class * (nr_class - 1) / 2];
        }

        int p = 0;
        for (int i = 0; i < nr_class; i++)
          for (int j = i + 1; j < nr_class; j++)
          {

            int si = start[i], sj = start[j];
            int ci = count[i], cj = count[j];
            var subprobLenght = ci + cj;
            var sub_prob = new SvmProblem<TPattern>
            {
              X = new TPattern[subprobLenght],
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

            if (param.Probability)
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

        model.NrClass = nr_class;

        model.Label = new int[nr_class];
        for (int i = 0; i < nr_class; i++)
          model.Label[i] = label[i];

        model.Rho = new double[nr_class * (nr_class - 1) / 2];
        for (int i = 0; i < nr_class * (nr_class - 1) / 2; i++)
          model.Rho[i] = f[i].Rho;

        if (param.Probability)
        {
          model.ProbA = new double[nr_class * (nr_class - 1) / 2];
          model.ProbB = new double[nr_class * (nr_class - 1) / 2];
          for (int i = 0; i < nr_class * (nr_class - 1) / 2; i++)
          {
            model.ProbA[i] = probA[i];
            model.ProbB[i] = probB[i];
          }
        }
        else
        {
          model.ProbA = null;
          model.ProbB = null;
        }

        int nnz = 0;
        int[] nz_count = new int[nr_class];
        model.SupportVectorsNumbers = new int[nr_class];
        for (int i = 0; i < nr_class; i++)
        {
          int nSV = 0;
          for (int j = 0; j < count[i]; j++)
            if (nonzero[start[i] + j])
            {
              ++nSV;
              ++nnz;
            }
          model.SupportVectorsNumbers[i] = nSV;
          nz_count[i] = nSV;
        }

        Svm.info("Total nSV = " + nnz + "\n");

        model.TotalSupportVectorsNumber = nnz;
        model.SupportVectors = new TPattern[nnz];
        p = 0;
        for (int i = 0; i < length; i++)
          if (nonzero[i]) model.SupportVectors[p++] = x[i];

        int[] nz_start = new int[nr_class];
        nz_start[0] = 0;
        for (int i = 1; i < nr_class; i++)
          nz_start[i] = nz_start[i - 1] + nz_count[i - 1];

        model.SupportVectorsCoefficients = new double[nr_class - 1][];
        for (int i = 0; i < nr_class - 1; i++)
          model.SupportVectorsCoefficients[i] = new double[nnz];

        p = 0;
        for (int i = 0; i < nr_class; i++)
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
                model.SupportVectorsCoefficients[j - 1][q++] = f[p].Alpha[k];
            q = nz_start[j];
            for (k = 0; k < cj; k++)
              if (nonzero[sj + k])
                model.SupportVectorsCoefficients[i][q++] = f[p].Alpha[ci + k];
            ++p;
          }
      }
      return model;
    }

    // Stratified cross validation
    public static void CrossValidation<TPattern>(SvmProblem<TPattern> prob, SvmParameter<TPattern> param, int nr_fold, double[] target)
    {
      var fold_start = new int[nr_fold + 1];
      var length = prob.Length;
      var perm = new int[length];

      // stratified cv may not give leave-one-out rate
      // Each class to l folds -> some folds may have zero elements
      if (param.SvmType.IsSVC() && nr_fold < length)
      {
        int nr_class;
        int[] tmp_label;
        int[] start;
        int[] count;

        svm_group_classes(prob, out nr_class, out tmp_label, out start, out count, perm);

        // random shuffle and then data grouped by fold using the array perm
        int[] fold_count = new int[nr_fold];
        //int c;
        int[] index = new int[length];
        for (int i = 0; i < length; i++)
        {
          index[i] = perm[i];
        }
        var rnd = new Random();

        for (int c = 0; c < nr_class; c++)
        {
          for (int i = 0; i < count[c]; i++)
          {
            int j = i + (int)(rnd.NextDouble() * (count[c] - i));
            Common.Swap(ref index[start[c] + j], ref index[start[c] + j]);
          }
        }

        for (int i = 0; i < nr_fold; i++)
        {
          fold_count[i] = 0;
          for (int c = 0; c < nr_class; c++)
            fold_count[i] += (i + 1) * count[c] / nr_fold - i * count[c] / nr_fold;
        }

        fold_start[0] = 0;
        for (int i = 1; i <= nr_fold; i++)
        {
          fold_start[i] = fold_start[i - 1] + fold_count[i - 1];
        }

        for (int c = 0; c < nr_class; c++)
        {
          for (int i = 0; i < nr_fold; i++)
          {
            int begin = start[c] + i * count[c] / nr_fold;
            int end = start[c] + (i + 1) * count[c] / nr_fold;
            for (int j = begin; j < end; j++)
            {
              perm[fold_start[i]] = index[j];
              fold_start[i]++;
            }
          }
        }

        fold_start[0] = 0;
        for (int i = 1; i <= nr_fold; i++)
        {
          fold_start[i] = fold_start[i - 1] + fold_count[i - 1];
        }
      }
      else
      {
        var rnd = new Random();

        for (int i = 0; i < length; i++)
        {
          perm[i] = i;
        }

        for (int i = 0; i < length; i++)
        {
          int j = i + (int)(rnd.NextDouble() * (length - i));
          Common.Swap(ref perm[i], ref perm[j]);
        }

        for (int i = 0; i <= nr_fold; i++)
        {
          fold_start[i] = i * length / nr_fold;
        }
      }

      for (int i = 0; i < nr_fold; i++)
      {
        int begin = fold_start[i];
        int end = fold_start[i + 1];
        int j, k;

        var subprobLenght = length - (end - begin);
        var subprob = new SvmProblem<TPattern>
        {
          X = new TPattern[subprobLenght],
          Y = new double[subprobLenght]
        };

        k = 0;
        for (j = 0; j < begin; j++)
        {
          subprob.X[k] = prob.X[perm[j]];
          subprob.Y[k] = prob.Y[perm[j]];
          ++k;
        }
        for (j = end; j < length; j++)
        {
          subprob.X[k] = prob.X[perm[j]];
          subprob.Y[k] = prob.Y[perm[j]];
          ++k;
        }
        var submodel = Train(subprob, param);
        if (param.Probability && param.SvmType.IsSVC())
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

    public static void SetPrintStringFunction(svm_print_interface print_func)
    {
      svm_print_string = print_func ?? svm_print_stdout;
    }
  }
}
