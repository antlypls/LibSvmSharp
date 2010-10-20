using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibSvm;

namespace LibSvmDemo.Demo
{
  internal static class EpsSVRDemo
  {
    private static IEnumerable<double> Range(double begin, double end, double step) 
    {
      for (double val = begin; val <= end; val+= step)
      {
        yield return val;
      }
    }

    public static void Run()
    {
      Console.WriteLine("EpsSVRDemo");
      var rnd = new Random();

      var trainData = Range(-10.0, 10.01, 0.1).Select(val => new { X = val, Y = DemoHelper.Sinc(val) + (rnd.NextDouble() - 0.5) / 4 });

      var param = new SvmParameter();

      param.svm_type = SvmType.EPSILON_SVR;
      param.kernel_type = KernelType.RBF;
      param.gamma = 0.5;
      param.cache_size = 128;
      param.C = 1;
      param.eps = 1e-3;
      param.p = 0.1;
      param.shrinking = true;
      param.probability = false;

      var prob = new SvmProblem();

      prob.l = trainData.Count();

      prob.y = trainData.Select(p => p.Y).ToArray();

      prob.x = trainData.Select(p => p.X.ToSvmNodes()).ToArray();

      param.Check(prob);

      var model = Svm.svm_train(prob, param);


      foreach (var item in Range(-1.0, 1.01, 0.1))
      {
        var x = item.ToSvmNodes();
        var y_pred = model.Predict(x);
        var y_real = DemoHelper.Sinc(item);
        Console.WriteLine("x: {0}", item);
        Console.WriteLine("y_real: {0}", y_real);
        Console.WriteLine("y_pred: {0}", y_pred);
      }
    }

  }
}
