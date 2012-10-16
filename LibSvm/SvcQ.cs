namespace LibSvm
{
  //SvcQ
  class SvcQ<TPattern> : Kernel<TPattern>
  {
    private readonly sbyte[] _y;
    private readonly Cache _cache;
    private readonly double[] _QD;

    public SvcQ(SvmProblem<TPattern> prob, SvmParameter<TPattern> param, sbyte[] y)
      : base(prob.X, param)
    {
      //super(prob.l, prob.x, param);
      _y = (sbyte[])y.Clone();
      _cache = new Cache(prob.Length, (long)(param.CacheSize * (1 << 20)));
      _QD = new double[prob.Length];
      for (int i = 0; i < prob.Length; i++)
        _QD[i] = kernel_function(i, i);
    }

    public override double[] GetQ(int i, int len)
    {
      double[] data;
      int start = _cache.get_data(i, out data, len);
      if (start < len)
      {
        for (int j = start; j < len; j++)
        {
          data[j] = _y[i] * _y[j] * kernel_function(i, j);
        }
      }
      return data;
    }

    public override double[] GetQD()
    {
      return _QD;
    }

    public override void SwapIndex(int i, int j)
    {
      _cache.swap_index(i, j);
      base.SwapIndex(i, j);

      Common.Swap(ref _y[i], ref _y[j]);
      Common.Swap(ref _QD[i], ref _QD[j]);
    }
  }

}
