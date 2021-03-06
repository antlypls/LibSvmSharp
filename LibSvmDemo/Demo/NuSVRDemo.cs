﻿using System;
using System.Linq;
using LibSvm;

namespace LibSvmDemo.Demo
{
  internal static class NuSVRDemo
  {
    public static void Run()
    {
      Console.WriteLine("NuSVRDemo");
      var rnd = new Random();

      var trainData = DemoHelper.Range(-10.0, 10.01, 0.1).Select(val => new { X = val, Y = DemoHelper.Sinc(val) + (rnd.NextDouble() - 0.5) / 4 });

      var parameters = new SvmParameter<double[]>
      {
        SvmType = SvmType.NU_SVR,
        KernelFunc = Kernels.Rbf(0.5),
        Nu = 0.1,
        CacheSize = 128,
        C = 1,
        Eps = 0.1,
        Shrinking = true,
        Probability = false
      };

      var problem = new SvmProblem<double[]>
      {
        Y = trainData.Select(p => p.Y).ToArray(),
        X = trainData.Select(p => p.X.ToArray()).ToArray()
      };

      parameters.Check(problem);

      var model = Svm.Train(problem, parameters);

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
