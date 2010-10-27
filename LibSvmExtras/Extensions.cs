using System;
using LibSvm;

namespace LibSvmExtras
{
  internal static class Extensions
  {
    public static SvmNode[] ToSvmNodes(this double[] vector)
    {
      var result = new SvmNode[vector.Length];

      for (int i = 0; i < vector.Length; i++)
      {
        result[i] = new SvmNode(i, vector[i]);
      }

      return result;
    }
  }
}
