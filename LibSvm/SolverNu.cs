using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibSvm
{
  //
  // Solver for nu-svm classification and regression
  //
  // additional constraint: e^T \alpha = constant
  //
  sealed class SolverNu : Solver
  {
    private SolutionInfo _si;

    public override void Solve(int length, QMatrix Q, double[] p, sbyte[] y,
         double[] alpha, double Cp, double Cn, double eps,
         SolutionInfo si, bool shrinking)
    {
      _si = si;
      base.Solve(length, Q, p, y, alpha, Cp, Cn, eps, si, shrinking);
    }

    // return 1 if already optimal, return 0 otherwise
    protected override int select_working_set(int[] working_set)
    {
      // return i,j such that y_i = y_j and
      // i: maximizes -y_i * grad(f)_i, i in I_up(\alpha)
      // j: minimizes the decrease of obj value
      //    (if quadratic coefficeint <= 0, replace it with tau)
      //    -y_j*grad(f)_j < -y_i*grad(f)_i, j in I_low(\alpha)

      double Gmaxp = double.NegativeInfinity;
      double Gmaxp2 = double.NegativeInfinity;
      int Gmaxp_idx = -1;

      double Gmaxn = double.NegativeInfinity;
      double Gmaxn2 = double.NegativeInfinity;
      int Gmaxn_idx = -1;

      int Gmin_idx = -1;
      double obj_diff_min = double.PositiveInfinity;

      for (int t = 0; t < _activeSize; t++)
        if (_y[t] == +1)
        {
          if (!is_upper_bound(t))
            if (-_g[t] >= Gmaxp)
            {
              Gmaxp = -_g[t];
              Gmaxp_idx = t;
            }
        }
        else
        {
          if (!is_lower_bound(t))
            if (_g[t] >= Gmaxn)
            {
              Gmaxn = _g[t];
              Gmaxn_idx = t;
            }
        }

      int ip = Gmaxp_idx;
      int @in = Gmaxn_idx;
      float[] Q_ip = null;
      float[] Q_in = null;
      if (ip != -1) // null Q_ip not accessed: Gmaxp=-INF if ip=-1
        Q_ip = _q.GetQ(ip, _activeSize);
      if (@in != -1)
        Q_in = _q.GetQ(@in, _activeSize);

      for (int j = 0; j < _activeSize; j++)
      {
        if (_y[j] == +1)
        {
          if (!is_lower_bound(j))
          {
            double grad_diff = Gmaxp + _g[j];
            if (_g[j] >= Gmaxp2)
              Gmaxp2 = _g[j];
            if (grad_diff > 0)
            {
              double obj_diff;
              double quad_coef = _qd[ip] + _qd[j] - 2 * Q_ip[j];
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
          if (!is_upper_bound(j))
          {
            double grad_diff = Gmaxn - _g[j];
            if (-_g[j] >= Gmaxn2)
              Gmaxn2 = -_g[j];
            if (grad_diff > 0)
            {
              double obj_diff;
              double quad_coef = _qd[@in] + _qd[j] - 2 * Q_in[j];
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

      if (Math.Max(Gmaxp + Gmaxp2, Gmaxn + Gmaxn2) < _eps)
        return 1;

      if (_y[Gmin_idx] == +1)
        working_set[0] = Gmaxp_idx;
      else
        working_set[0] = Gmaxn_idx;
      working_set[1] = Gmin_idx;

      return 0;
    }

    private bool be_shrunk(int i, double Gmax1, double Gmax2, double Gmax3, double Gmax4)
    {
      if (is_upper_bound(i))
      {
        if (_y[i] == +1)
          return (-_g[i] > Gmax1);
        else
          return (-_g[i] > Gmax4);
      }
      else if (is_lower_bound(i))
      {
        if (_y[i] == +1)
          return (_g[i] > Gmax2);
        else
          return (_g[i] > Gmax3);
      }
      else
        return (false);
    }

    protected override void do_shrinking()
    {
      double Gmax1 = double.NegativeInfinity;
      double Gmax2 = double.NegativeInfinity;
      double Gmax3 = double.NegativeInfinity;
      double Gmax4 = double.NegativeInfinity;

      // find maximal violating pair first
      int i;
      for (i = 0; i < _activeSize; i++)
      {
        if (!is_upper_bound(i))
        {
          if (_y[i] == +1)
          {
            if (-_g[i] > Gmax1) Gmax1 = -_g[i];
          }
          else if (-_g[i] > Gmax4) Gmax4 = -_g[i];
        }
        if (!is_lower_bound(i))
        {
          if (_y[i] == +1)
          {
            if (_g[i] > Gmax2) Gmax2 = _g[i];
          }
          else if (_g[i] > Gmax3) Gmax3 = _g[i];
        }
      }

      if (_unshrink == false && Math.Max(Gmax1 + Gmax2, Gmax3 + Gmax4) <= _eps * 10)
      {
        _unshrink = true;
        reconstruct_gradient();
        _activeSize = _length;
      }

      for (i = 0; i < _activeSize; i++)
        if (be_shrunk(i, Gmax1, Gmax2, Gmax3, Gmax4))
        {
          _activeSize--;
          while (_activeSize > i)
          {
            if (!be_shrunk(_activeSize, Gmax1, Gmax2, Gmax3, Gmax4))
            {
              swap_index(i, _activeSize);
              break;
            }
            _activeSize--;
          }
        }
    }

    protected override double calculate_rho()
    {
      int nr_free1 = 0, nr_free2 = 0;
      double ub1 = double.PositiveInfinity, ub2 = double.PositiveInfinity;
      double lb1 = double.NegativeInfinity, lb2 = double.NegativeInfinity;
      double sum_free1 = 0, sum_free2 = 0;

      for (int i = 0; i < _activeSize; i++)
      {
        if (_y[i] == +1)
        {
          if (is_lower_bound(i))
            ub1 = Math.Min(ub1, _g[i]);
          else if (is_upper_bound(i))
            lb1 = Math.Max(lb1, _g[i]);
          else
          {
            ++nr_free1;
            sum_free1 += _g[i];
          }
        }
        else
        {
          if (is_lower_bound(i))
            ub2 = Math.Min(ub2, _g[i]);
          else if (is_upper_bound(i))
            lb2 = Math.Max(lb2, _g[i]);
          else
          {
            ++nr_free2;
            sum_free2 += _g[i];
          }
        }
      }

      double r1, r2;
      if (nr_free1 > 0)
        r1 = sum_free1 / nr_free1;
      else
        r1 = (ub1 + lb1) / 2;

      if (nr_free2 > 0)
        r2 = sum_free2 / nr_free2;
      else
        r2 = (ub2 + lb2) / 2;

      _si.R = (r1 + r2) / 2;
      return (r1 - r2) / 2;
    }
  }

}
