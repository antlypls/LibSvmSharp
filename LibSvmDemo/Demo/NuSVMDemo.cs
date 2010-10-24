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
        SvmType = SvmType.NU_SVC,
        KernelType = KernelType.Rbf,
        Gamma = 0.5,
        Nu = 0.1,
        CacheSize = 128,
        Eps = 1e-3,
        Shrinking = true,
        Probability = false
      };

      var problem = new SvmProblem
      {
        Y = trainData.Select(p => (double)p.Label).ToArray(),
        X = trainData.Select(p => p.ToSvmNodes()).ToArray()
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
