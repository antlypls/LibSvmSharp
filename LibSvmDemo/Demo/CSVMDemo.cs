using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibSvm;

namespace LibSvmDemo.Demo
{
  internal static class CSVMDemo
  {
    public static void Run()
    {
      Console.WriteLine("CSVMDemo");
      var class1 = DemoHelper.GenerateClass(0, 0.1, 0.1, 50);
      var class2 = DemoHelper.GenerateClass(1, 0.8, 0.8, 50);

      var trainData = class1.Concat(class2);

      var param = new SvmParameter();

      param.svm_type = SvmType.C_SVC;
      param.kernel_type = KernelType.RBF;
      param.gamma = 0.5;
      param.cache_size = 128;
      param.C = 1;
      param.eps = 1e-3;
      param.shrinking = true;
      param.probability = false;

      var prob = new SvmProblem();

      prob.l = trainData.Count();

      prob.y = trainData.Select(p => (double)p.Label).ToArray();

      prob.x = trainData.Select(p => p.ToSvmNodes()).ToArray();

      param.Check(prob);

      var model = Svm.svm_train(prob, param);

      var x = new Point(0.9, 0.9).ToSvmNodes();
      var res = model.Predict(x);
      Console.WriteLine(res);

      var y = new Point(0.1, 0.1).ToSvmNodes();
      var resy = model.Predict(y);
      Console.WriteLine(resy);
    }
  }
}
