using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibSvm;

namespace LibSvmDemo.Demo
{
  internal static class OneClassDemo
  {
    public static void Run()
    {
      Console.WriteLine("OneClassDemo");
      var trainData = DemoHelper.GenerateClass(0, 0.5, 0.5, 100);

      var parameters = new SvmParameter
      {
        SvmType = SvmType.ONE_CLASS,
        KernelType = KernelType.Rbf,
        Gamma = 0.5,
        Nu = 0.5,
        CacheSize = 128,
        Eps = 1e-3,
        Shrinking = true,
        Probability = false
      };

      var problem = new SvmProblem
      {
        Y = trainData.Select(p => 1.0).ToArray(),
        X = trainData.Select(p => p.ToSvmNodes()).ToArray()
      };

      parameters.Check(problem);

      var model = Svm.svm_train(problem, parameters);

      var x = new Point(0.9, 0.9).ToSvmNodes();
      var resx = model.Predict(x);
      Console.WriteLine(resx);

      var y = new Point(0.5, 0.5).ToSvmNodes();
      var resy = model.Predict(y);
      Console.WriteLine(resy);

      var z = new Point(0.45, 0.45).ToSvmNodes();
      var resz = model.Predict(z);
      Console.WriteLine(resz);
    }
  }
}
