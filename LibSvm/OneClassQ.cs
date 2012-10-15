namespace LibSvm
{
  class OneClassQ<TPattern> : Kernel<TPattern>
  {
    private readonly Cache _cache;
    private readonly double[] _qd;

    public OneClassQ(SvmProblem<TPattern> prob, SvmParameter<TPattern> param)
      : base(prob.X, param)
    {
      _cache = new Cache(prob.Length, (long)(param.CacheSize * (1 << 20)));
      _qd = new double[prob.Length];
      for (int i = 0; i < prob.Length; i++)
        _qd[i] = kernel_function(i, i);
    }

    public override double[] GetQ(int i, int len)
    {
      double[] data;
      int start;
      if ((start = _cache.get_data(i, out data, len)) < len)
      {
        for (int j = start; j < len; j++)
          data[j] = (double)kernel_function(i, j);
      }
      return data;
    }

    public override double[] GetQD()
    {
      return _qd;
    }

    public override void SwapIndex(int i, int j)
    {
      _cache.swap_index(i, j);
      base.SwapIndex(i, j);

      Common.Swap(ref _qd[i], ref _qd[j]);
    }
  }
}
