using System;
using System.Linq;
using LibSvmExtras;
using LibSvmExtras.Kernel;
using LibSvmExtras.Svm;

namespace LibSvmExtrasDemo.Demo
{
  internal static class EpsSVRDemo
  {
    public static void Run()
    {
      Console.WriteLine("EpsSVRDemo");
      var rnd = new Random();

      var trainData = DemoHelper.Range(-10.0, 10.01, 0.1).Select(val => new { X = val, Y = DemoHelper.Sinc(val) + (rnd.NextDouble() - 0.5) / 4 });

      var trainer = SVM.Create(new EpsilonSupportVectorRegression<double[]>(1.0, 0.1), new RbfKernel(0.5));
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
