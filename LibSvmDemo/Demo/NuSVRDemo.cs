using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibSvm;

namespace LibSvmDemo.Demo
{
  class NuSVRDemo
  {

    public static void Run()
    {
      Console.WriteLine("NuSVRDemo");
      var rnd = new Random();

      var trainData = DemoHelper.Range(-10.0, 10.01, 0.1).Select(val => new { X = val, Y = DemoHelper.Sinc(val) + (rnd.NextDouble() - 0.5) / 4 });

      var parameters = new SvmParameter
      {
        svm_type = SvmType.NU_SVR,
        kernel_type = KernelType.RBF,
        gamma = 0.5,
        nu = 0.1,
        cache_size = 128,
        C = 1,
        eps = 0.1,
        shrinking = true,
        probability = false
      };

      var problem = new SvmProblem
      {
        l = trainData.Count(),
        y = trainData.Select(p => p.Y).ToArray(),
        x = trainData.Select(p => p.X.ToSvmNodes()).ToArray()
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
      }
    }
  }
}
