namespace LibSvm
{
  abstract class QMatrix
  {
    public abstract double[] GetQ(int column, int len);
    public abstract double[] GetQD();
    public abstract void SwapIndex(int i, int j);
  }
}
