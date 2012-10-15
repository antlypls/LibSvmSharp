namespace LibSvmExtrasDemo.Demo
{
  internal struct Point
  {
    public Point(double x, double y)
      : this()
    {
      X = x;
      Y = y;
    }

    public double X { get; set; }
    public double Y { get; set; }
    public int Label { get; set; }

    public double[] ToArray()
    {
      return new[] { X, Y };
    }
  }
}
