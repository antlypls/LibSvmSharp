namespace LibSvm
{
  //SvrQ
  class SvrQ<TPattern> : Kernel<TPattern>
  {
    private readonly int _length;
    private readonly Cache _cache;
    private readonly sbyte[] _sign;
    private readonly int[] _index;
    private int _nextBuffer;
    private readonly double[][] _buffer;
    private readonly double[] _QD;

    public SvrQ(SvmProblem<TPattern> prob, SvmParameter<TPattern> param)
      : base(prob.X, param)
    {
      _length = prob.Length;
      _cache = new Cache(_length, (long)(param.CacheSize * (1 << 20)));

      _QD = new double[2 * _length];
      _sign = new sbyte[2 * _length];
      _index = new int[2 * _length];

      for (int k = 0; k < _length; k++)
      {
        _sign[k] = 1;
        _sign[k + _length] = -1;
        _index[k] = k;
        _index[k + _length] = k;
        _QD[k] = kernel_function(k, k);
        _QD[k + _length] = _QD[k];
      }

      _buffer = new[] { new double[2 * _length], new double[2 * _length] };

      _nextBuffer = 0;
    }

    public override void SwapIndex(int i, int j)
    {
      Common.Swap(ref _sign[i], ref _sign[j]);
      Common.Swap(ref _index[i], ref _index[j]);
      Common.Swap(ref _QD[i], ref _QD[j]);
    }

    public override double[] GetQ(int i, int len)
    {
      double[] data;
      int real_i = _index[i];
      if (_cache.get_data(real_i, out data, _length) < _length)
      {
        for (int j = 0; j < _length; j++)
        {
          data[j] = kernel_function(real_i, j);
        }
      }

      // reorder and copy
      double[] buf = _buffer[_nextBuffer];
      _nextBuffer = 1 - _nextBuffer;
      sbyte si = _sign[i];
      for (int j = 0; j < len; j++)
      {
        buf[j] = (double)si * _sign[j] * data[_index[j]];
      }
      return buf;
    }

    public override double[] GetQD()
    {
      return _QD;
    }
  }
}
