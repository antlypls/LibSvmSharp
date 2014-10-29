using System;

namespace LibSvm
{
  // An SMO algorithm in Fan et al., JMLR 6(2005), p. 1889--1918
  // Solves:
  //
  //    min 0.5(\alpha^T Q \alpha) + p^T \alpha
  //
  //        y^T \alpha = \delta
  //        y_i = +1 or -1
  //        0 <= alpha_i <= Cp for y_i = 1
  //        0 <= alpha_i <= Cn for y_i = -1
  //
  // Given:
  //
  //    Q, p, y, Cp, Cn, and an initial feasible point \alpha
  //    length is the size of vectors and matrices
  //    eps is the stopping tolerance
  //
  // solution will be put in \alpha, objective value will be put in obj
  //

  internal class Solver
  {
    protected int _activeSize;
    protected sbyte[] _y;
    protected double[] _g;              // gradient of objective function

    private BoundType[] _alphaStatus;   // LOWER_BOUND, UPPER_BOUND, FREE

    private double[] _alpha;
    protected QMatrix _q;
    protected double[] _qd;
    protected double _eps;
    private double _cp, _cn;
    private double[] _p;
    private int[] _activeSet;
    private double[] _gBar;     // gradient, if we treat free variables as 0
    protected int _length;
    protected bool _unshrink;   // XXX

    private double GetC(int i)
    {
      return (_y[i] > 0) ? _cp : _cn;
    }

    private void UpdateAlphaStatus(int i)
    {
      if (_alpha[i] >= GetC(i))
        _alphaStatus[i] = BoundType.UpperBound;
      else if (_alpha[i] <= 0)
        _alphaStatus[i] = BoundType.LowerBound;
      else _alphaStatus[i] = BoundType.Free;
    }

    // from is_upper_bound
    protected bool IsUpperBound(int i)
    {
      return _alphaStatus[i] == BoundType.UpperBound;
    }

    // from is_lower_bound
    protected bool IsLowerBound(int i)
    {
      return _alphaStatus[i] == BoundType.LowerBound;
    }

    // from is_free
    protected bool IsFree(int i)
    {
      return _alphaStatus[i] == BoundType.Free;
    }

    // from swap_index
    protected void SwapIndex(int i, int j)
    {
      _q.SwapIndex(i, j);
      Common.Swap(ref _y[i], ref _y[j]);
      Common.Swap(ref _g[i], ref _g[j]);
      Common.Swap(ref _alphaStatus[i], ref _alphaStatus[j]);
      Common.Swap(ref _alpha[i], ref _alpha[j]);
      Common.Swap(ref _p[i], ref _p[j]);
      Common.Swap(ref _activeSet[i], ref _activeSet[j]);
      Common.Swap(ref _gBar[i], ref _gBar[j]);
    }

    // from reconstruct_gradient
    protected void ReconstructGradient()
    {
      // reconstruct inactive elements of G from G_bar and free variables

      if (_activeSize == _length) return;
      int nr_free = 0;

      for (int j = _activeSize; j < _length; j++)
      {
        _g[j] = _gBar[j] + _p[j];
      }

      for (int j = 0; j < _activeSize; j++)
      {
        if (IsFree(j))
        {
          nr_free++;
        }
      }

      if (2 * nr_free < _activeSize)
      {
        Svm.info("\nWARNING: using -h 0 may be faster\n");
      }

      if (nr_free * _length > 2 * _activeSize * (_length - _activeSize))
      {
        for (int i = _activeSize; i < _length; i++)
        {
          double[] Q_i = _q.GetQ(i, _activeSize);
          for (int j = 0; j < _activeSize; j++)
          {
            if (IsFree(j))
            {
              _g[i] += _alpha[j] * Q_i[j];
            }
          }
        }
      }
      else
      {
        for (int i = 0; i < _activeSize; i++)
          if (IsFree(i))
          {
            double[] Q_i = _q.GetQ(i, _length);
            double alpha_i = _alpha[i];
            for (int j = _activeSize; j < _length; j++)
            {
              _g[j] += alpha_i * Q_i[j];
            }
          }
      }
    }

    public virtual void Solve(int length, QMatrix Q, double[] p_, sbyte[] y_,
         double[] alpha_, double Cp, double Cn, double eps, SolutionInfo si, bool shrinking)
    {
      _length = length;
      _q = Q;
      _qd = Q.GetQD();
      _p = (double[]) p_.Clone();
      _y = (sbyte[]) y_.Clone();
      _alpha = (double[]) alpha_.Clone();
      _cp = Cp;
      _cn = Cn;
      _eps = eps;
      _unshrink = false;

      // initialize alpha_status
      _alphaStatus = new BoundType[length];
      for (int i = 0; i < length; i++)
      {
        UpdateAlphaStatus(i);
      }

      // initialize active set (for shrinking)
      _activeSet = new int[length];
      for (int i = 0; i < length; i++)
      {
        _activeSet[i] = i;
      }
      _activeSize = length;

      // initialize gradient
      _g = new double[length];
      _gBar = new double[length];

      for (int i = 0; i < length; i++)
      {
        _g[i] = _p[i];
        _gBar[i] = 0;
      }

      for (int i = 0; i < length; i++)
      {
        if (!IsLowerBound(i))
        {
          double[] Q_i = Q.GetQ(i, length);
          double alpha_i = _alpha[i];

          for (int j = 0; j < length; j++)
          {
            _g[j] += alpha_i * Q_i[j];
          }

          if (IsUpperBound(i))
          {
            for (int j = 0; j < length; j++)
            {
              _gBar[j] += GetC(i) * Q_i[j];
            }
          }
        }
      }

      // optimization step

      int iter = 0;
      int max_iter = Math.Max(10000000, length > int.MaxValue / 100 ? int.MaxValue : 100 * length);
      int counter = Math.Min(length, 1000) + 1;
      int[] working_set = new int[2];

      while (iter < max_iter)
      {
        // show progress and do shrinking

        if (--counter == 0)
        {
          counter = Math.Min(length, 1000);
          if (shrinking) DoShrinking();
          Svm.info(".");
        }

        if (SelectWorkingSet(working_set) != 0)
        {
          // reconstruct the whole gradient
          ReconstructGradient();
          // reset active set size and check
          _activeSize = length;
          Svm.info("*");
          if (SelectWorkingSet(working_set) != 0)
            break;
          else
            counter = 1; // do shrinking next iteration
        }

        int i = working_set[0];
        int j = working_set[1];

        ++iter;

        // update alpha[i] and alpha[j], handle bounds carefully

        double[] Q_i = Q.GetQ(i, _activeSize);
        double[] Q_j = Q.GetQ(j, _activeSize);

        double C_i = GetC(i);
        double C_j = GetC(j);

        double old_alpha_i = _alpha[i];
        double old_alpha_j = _alpha[j];

        if (_y[i] != _y[j])
        {
          double quad_coef = _qd[i] + _qd[j] + 2*Q_i[j];
          if (quad_coef <= 0)
            quad_coef = 1e-12;
          double delta = (-_g[i] - _g[j])/quad_coef;
          double diff = _alpha[i] - _alpha[j];
          _alpha[i] += delta;
          _alpha[j] += delta;

          if (diff > 0)
          {
            if (_alpha[j] < 0)
            {
              _alpha[j] = 0;
              _alpha[i] = diff;
            }
          }
          else
          {
            if (_alpha[i] < 0)
            {
              _alpha[i] = 0;
              _alpha[j] = -diff;
            }
          }
          if (diff > C_i - C_j)
          {
            if (_alpha[i] > C_i)
            {
              _alpha[i] = C_i;
              _alpha[j] = C_i - diff;
            }
          }
          else
          {
            if (_alpha[j] > C_j)
            {
              _alpha[j] = C_j;
              _alpha[i] = C_j + diff;
            }
          }
        }
        else
        {
          double quad_coef = _qd[i] + _qd[j] - 2*Q_i[j];
          if (quad_coef <= 0)
            quad_coef = 1e-12;
          double delta = (_g[i] - _g[j])/quad_coef;
          double sum = _alpha[i] + _alpha[j];
          _alpha[i] -= delta;
          _alpha[j] += delta;

          if (sum > C_i)
          {
            if (_alpha[i] > C_i)
            {
              _alpha[i] = C_i;
              _alpha[j] = sum - C_i;
            }
          }
          else
          {
            if (_alpha[j] < 0)
            {
              _alpha[j] = 0;
              _alpha[i] = sum;
            }
          }
          if (sum > C_j)
          {
            if (_alpha[j] > C_j)
            {
              _alpha[j] = C_j;
              _alpha[i] = sum - C_j;
            }
          }
          else
          {
            if (_alpha[i] < 0)
            {
              _alpha[i] = 0;
              _alpha[j] = sum;
            }
          }
        }

        // update G
        double delta_alpha_i = _alpha[i] - old_alpha_i;
        double delta_alpha_j = _alpha[j] - old_alpha_j;

        for (int k = 0; k < _activeSize; k++)
        {
          _g[k] += Q_i[k]*delta_alpha_i + Q_j[k]*delta_alpha_j;
        }

        // update alpha_status and G_bar
        bool ui = IsUpperBound(i);
        bool uj = IsUpperBound(j);
        UpdateAlphaStatus(i);
        UpdateAlphaStatus(j);
        //int k;
        if (ui != IsUpperBound(i))
        {
          Q_i = Q.GetQ(i, length);
          if (ui)
          {
            for (int k = 0; k < length; k++)
            {
              _gBar[k] -= C_i*Q_i[k];
            }
          }
          else
          {
            for (int k = 0; k < length; k++)
            {
              _gBar[k] += C_i*Q_i[k];
            }
          }
        }

        if (uj != IsUpperBound(j))
        {
          Q_j = Q.GetQ(j, length);
          if (uj)
          {
            for (int k = 0; k < length; k++)
            {
              _gBar[k] -= C_j*Q_j[k];
            }
          }
          else
          {
            for (int k = 0; k < length; k++)
            {
              _gBar[k] += C_j*Q_j[k];
            }
          }
        }
      }

      if (iter >= max_iter)
      {
        if (_activeSize < length)
        {
          // reconstruct the whole gradient to calculate objective value
          ReconstructGradient();
          _activeSize = length;
          Svm.info("*");
        }
        Console.Error.WriteLine("\nWARNING: reaching max number of iterations");
      }

      // calculate rho
      si.Rho = CalculateRho();

      // calculate objective value
      double v = 0;
      for (int i = 0; i < length; i++)
      {
        v += _alpha[i]*(_g[i] + _p[i]);
      }
      si.Obj = v/2;

      // put back the solution
      for (int i = 0; i < length; i++)
      {
        alpha_[_activeSet[i]] = _alpha[i];
      }

      si.UpperBoundP = Cp;
      si.UpperBoundN = Cn;

      Svm.info("\noptimization finished, #iter = " + iter + "\n");
    }

    // from select_working_set
    // return 1 if already optimal, return 0 otherwise
    protected virtual int SelectWorkingSet(int[] workingSet)
    {
      // return i,j such that
      // i: maximizes -y_i * grad(f)_i, i in I_up(\alpha)
      // j: mimimizes the decrease of obj value
      //    (if quadratic coefficeint <= 0, replace it with tau)
      //    -y_j*grad(f)_j < -y_i*grad(f)_i, j in I_low(\alpha)

      double Gmax = double.NegativeInfinity;
      double Gmax2 = double.NegativeInfinity;
      int Gmax_idx = -1;
      int Gmin_idx = -1;
      double obj_diff_min = Double.PositiveInfinity;

      for (int t = 0; t < _activeSize; t++)
        if (_y[t] == +1)
        {
          if (!IsUpperBound(t))
            if (-_g[t] >= Gmax)
            {
              Gmax = -_g[t];
              Gmax_idx = t;
            }
        }
        else
        {
          if (!IsLowerBound(t))
            if (_g[t] >= Gmax)
            {
              Gmax = _g[t];
              Gmax_idx = t;
            }
        }

      int i = Gmax_idx;
      double[] Q_i = null;
      if (i != -1) // null Q_i not accessed: Gmax=-INF if i=-1
        Q_i = _q.GetQ(i, _activeSize);

      for (int j = 0; j < _activeSize; j++)
      {
        if (_y[j] == +1)
        {
          if (!IsLowerBound(j))
          {
            double grad_diff = Gmax + _g[j];
            if (_g[j] >= Gmax2)
              Gmax2 = _g[j];
            if (grad_diff > 0)
            {
              double obj_diff;
              double quad_coef = _qd[i] + _qd[j] - 2.0 * _y[i] * Q_i[j];
              if (quad_coef > 0)
                obj_diff = -(grad_diff * grad_diff) / quad_coef;
              else
                obj_diff = -(grad_diff * grad_diff) / 1e-12;

              if (obj_diff <= obj_diff_min)
              {
                Gmin_idx = j;
                obj_diff_min = obj_diff;
              }
            }
          }
        }
        else
        {
          if (!IsUpperBound(j))
          {
            double grad_diff = Gmax - _g[j];
            if (-_g[j] >= Gmax2)
              Gmax2 = -_g[j];
            if (grad_diff > 0)
            {
              double obj_diff;
              double quad_coef = _qd[i] + _qd[j] + 2.0 * _y[i] * Q_i[j];
              if (quad_coef > 0)
                obj_diff = -(grad_diff * grad_diff) / quad_coef;
              else
                obj_diff = -(grad_diff * grad_diff) / 1e-12;

              if (obj_diff <= obj_diff_min)
              {
                Gmin_idx = j;
                obj_diff_min = obj_diff;
              }
            }
          }
        }
      }

      if (Gmax + Gmax2 < _eps)
        return 1;

      workingSet[0] = Gmax_idx;
      workingSet[1] = Gmin_idx;
      return 0;
    }

    // from be_shrunk
    private bool BeShrunk(int i, double Gmax1, double Gmax2)
    {
      if (IsUpperBound(i))
      {
        if (_y[i] == +1)
          return (-_g[i] > Gmax1);
        else
          return (-_g[i] > Gmax2);
      }
      else if (IsLowerBound(i))
      {
        if (_y[i] == +1)
          return (_g[i] > Gmax2);
        else
          return (_g[i] > Gmax1);
      }
      else
        return (false);
    }

    // from do_shrinking
    protected virtual void DoShrinking()
    {
      double gmax1 = double.NegativeInfinity;       // max { -y_i * grad(f)_i | i in I_up(\alpha) }
      double gmax2 = double.NegativeInfinity;       // max { y_i * grad(f)_i | i in I_low(\alpha) }

      // find maximal violating pair first
      for (int i = 0; i < _activeSize; i++)
      {
        if (_y[i] == +1)
        {
          if (!IsUpperBound(i))
          {
            if (-_g[i] >= gmax1)
              gmax1 = -_g[i];
          }
          if (!IsLowerBound(i))
          {
            if (_g[i] >= gmax2)
              gmax2 = _g[i];
          }
        }
        else
        {
          if (!IsUpperBound(i))
          {
            if (-_g[i] >= gmax2)
              gmax2 = -_g[i];
          }
          if (!IsLowerBound(i))
          {
            if (_g[i] >= gmax1)
              gmax1 = _g[i];
          }
        }
      }

      if (_unshrink == false && gmax1 + gmax2 <= _eps * 10)
      {
        _unshrink = true;
        ReconstructGradient();
        _activeSize = _length;
      }

      for (int i = 0; i < _activeSize; i++)
        if (BeShrunk(i, gmax1, gmax2))
        {
          _activeSize--;
          while (_activeSize > i)
          {
            if (!BeShrunk(_activeSize, gmax1, gmax2))
            {
              SwapIndex(i, _activeSize);
              break;
            }
            _activeSize--;
          }
        }
    }

    // from calculate_rho
    protected virtual double CalculateRho()
    {
      double r;
      int nr_free = 0;
      double ub = Double.PositiveInfinity, lb = double.NegativeInfinity, sum_free = 0;
      for (int i = 0; i < _activeSize; i++)
      {
        double yG = _y[i] * _g[i];

        if (IsLowerBound(i))
        {
          if (_y[i] > 0)
            ub = Math.Min(ub, yG);
          else
            lb = Math.Max(lb, yG);
        }
        else if (IsUpperBound(i))
        {
          if (_y[i] < 0)
            ub = Math.Min(ub, yG);
          else
            lb = Math.Max(lb, yG);
        }
        else
        {
          ++nr_free;
          sum_free += yG;
        }
      }

      if (nr_free > 0)
        r = sum_free / nr_free;
      else
        r = (ub + lb) / 2;

      return r;
    }

  }
}
