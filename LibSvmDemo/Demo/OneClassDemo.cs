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

      var param = new SvmParameter();

      param.svm_type = SvmType.ONE_CLASS;
      param.kernel_type = KernelType.RBF;
      param.gamma = 0.5;
      param.nu = 0.5;
      param.cache_size = 128;
      param.eps = 1e-3;
      param.shrinking = true;
      param.probability = false;

      var prob = new SvmProblem();

      prob.l = trainData.Count();

      prob.y = trainData.Select(p => 1.0).ToArray();

      prob.x = trainData.Select(p => p.ToSvmNodes()).ToArray();

      param.Check(prob);

      var model = Svm.svm_train(prob, param);

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
