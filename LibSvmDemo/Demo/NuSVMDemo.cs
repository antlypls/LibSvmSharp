using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibSvm;

namespace LibSvmDemo.Demo
{
  internal static class NuSVMDemo
  {
    public static void Run()
    {
      Console.WriteLine("NuSVMDemo");
      var class1 = DemoHelper.GenerateClass(0, 0.1, 0.1, 50);
      var class2 = DemoHelper.GenerateClass(1, 0.8, 0.8, 50);

      var trainData = class1.Concat(class2);

      var parameters = new SvmParameter
      {
        svm_type = SvmType.NU_SVC,
        kernel_type = KernelType.RBF,
        gamma = 0.5,
        nu = 0.1,
        cache_size = 128,
        eps = 1e-3,
        shrinking = true,
        probability = false
      };

      var problem = new SvmProblem
      {
        l = trainData.Count(),
        y = trainData.Select(p => (double)p.Label).ToArray(),
        x = trainData.Select(p => p.ToSvmNodes()).ToArray()
      };

      parameters.Check(problem);

      var model = Svm.svm_train(problem, parameters);

      var x = new Point(0.9, 0.9).ToSvmNodes();
      var resx = model.Predict(x);
      Console.WriteLine(resx);

      var y = new Point(0.0, 0.0).ToSvmNodes();
      var resy = model.Predict(y);
      Console.WriteLine(resy);
    }
  }
}
