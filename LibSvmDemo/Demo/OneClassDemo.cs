using System;
using System.Linq;
using LibSvm;

namespace LibSvmDemo.Demo
{
  internal static class OneClassDemo
  {
    public static void Run()
    {
      Console.WriteLine("OneClassDemo");
      var trainData = DemoHelper.GenerateClass(0, 0.5, 0.5, 100);

      var parameters = new SvmParameter<double[]>
      {
        SvmType = SvmType.ONE_CLASS,
        KernelFunc = Kernels.Rbf(0.5),
        Nu = 0.5,
        CacheSize = 128,
        Eps = 1e-3,
        Shrinking = true,
        Probability = false
      };

      var problem = new SvmProblem<double[]>
      {
        Y = trainData.Select(p => 1.0).ToArray(),
        X = trainData.Select(p => p.ToArray()).ToArray()
      };

      parameters.Check(problem);

      var model = Svm.Train(problem, parameters);

      var x = new Point(0.9, 0.9).ToArray();
      var resx = model.Predict(x);
      Console.WriteLine(resx);

      var y = new Point(0.5, 0.5).ToArray();
      var resy = model.Predict(y);
      Console.WriteLine(resy);

      var z = new Point(0.45, 0.45).ToArray();
      var resz = model.Predict(z);
      Console.WriteLine(resz);
    }
  }
}
