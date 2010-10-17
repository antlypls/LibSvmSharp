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
      var rnd = new Random();
      var trainData = DemoHelper.GenerateClass(0, 0.5, 0.5, 100);

      var param = new SvmParameter();

      param.svm_type = SvmType.ONE_CLASS;
      param.kernel_type = KernelType.RBF;
      //param.degree = 3;
      param.gamma = 10;
      //param.coef0 = 0;
      param.nu = 0.9;
      param.cache_size = 128;
      //param.C = 1;
      param.eps = 1e-3;
      //param.p = 0.1;
      param.shrinking = 1;
      param.probability = 0;
      //param.nr_weight = 0;
      //param.weight_label = new int[0];
      //param.weight = new double[0];

      var prob = new SvmProblem();

      prob.l = trainData.Count();

      prob.y = trainData.Select(p => 1.0).ToArray();

      prob.x = trainData.Select(p => p.ToSvmNodes()).ToArray();

      var model = Svm.svm_train(prob, param);

      var x = new Point(0.9, 0.9).ToSvmNodes();
      var resx = model.Predict(x);
      Console.WriteLine(resx);

      var y = new Point(0.1, 0.1).ToSvmNodes();
      var resy = model.Predict(y);
      Console.WriteLine(resy);

      var z = new Point(10, 10).ToSvmNodes();
      var resz = model.Predict(z);
      Console.WriteLine(resz);
    }
  }
}
