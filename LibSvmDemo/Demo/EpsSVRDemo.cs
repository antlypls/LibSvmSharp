using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibSvm;

namespace LibSvmDemo.Demo
{
  internal static class EpsSVRDemo
  {
    public static void Run()
    {
      Console.WriteLine("EpsSVRDemo");
      var rnd = new Random();

      var trainData = DemoHelper.Range(-10.0, 10.01, 0.1).Select(val => new { X = val, Y = DemoHelper.Sinc(val) + (rnd.NextDouble() - 0.5) / 4 });

      var parameters = new SvmParameter
      {
        SvmType = SvmType.EPSILON_SVR,
        KernelType = KernelType.Rbf,
        Gamma = 0.5,
        CacheSize = 128,
        C = 1,
        Eps = 1e-3,
        P = 0.1,
        Shrinking = true,
        Probability = false
      };

      var problem = new SvmProblem
      {
        Y = trainData.Select(p => p.Y).ToArray(),
        X = trainData.Select(p => p.X.ToSvmNodes()).ToArray()
      };

      parameters.Check(problem);

      var model = Svm.svm_train(problem, parameters);

      foreach (var item in DemoHelper.Range(-1.0, 1.01, 0.1))
      {
        var x = item.ToSvmNodes();
        var yPred = model.Predict(x);
        var yReal = DemoHelper.Sinc(item);
        Console.WriteLine("x: {0}", item);
        Console.WriteLine("y_real: {0}", yReal);
        Console.WriteLine("y_pred: {0}", yPred);
        Console.WriteLine();
      }
    }

  }
}
