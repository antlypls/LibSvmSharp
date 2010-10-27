using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibSvmExtras;
using LibSvmExtras.Svm;
using LibSvmExtras.Kernel;

namespace LibSvmExtrasDemo.Demo
{
  internal static class NuSVRDemo
  {
    public static void Run()
    {
      Console.WriteLine("NuSVRDemo");
      var rnd = new Random();

      var trainData = DemoHelper.Range(-10.0, 10.01, 0.1).Select(val => new { X = val, Y = DemoHelper.Sinc(val) + (rnd.NextDouble() - 0.5) / 4 });

      var trainer = SVM.Create(new NuSupportVectorRegression(1, 0.1), new RbfKernel(0.5));
      var model = trainer.Train(trainData.Select(p => Tuple.Create(p.X.ToArray(), p.Y)));

      foreach (var item in DemoHelper.Range(-1.0, 1.01, 0.1))
      {
        var x = item.ToArray();
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
