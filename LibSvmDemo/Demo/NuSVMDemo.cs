using System;
using System.Linq;
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

      var parameters = new SvmParameter<double[]>
      {
        SvmType = SvmType.NU_SVC,
        KernelFunc = Kernels.Rbf(0.5),
        Nu = 0.1,
        CacheSize = 128,
        Eps = 1e-3,
        Shrinking = true,
        Probability = false
      };

      var problem = new SvmProblem<double[]>
      {
        Y = trainData.Select(p => (double)p.Label).ToArray(),
        X = trainData.Select(p => p.ToArray()).ToArray()
      };

      parameters.Check(problem);

      var model = Svm.Train(problem, parameters);

      var x = new Point(0.9, 0.9).ToArray();
      var resx = model.Predict(x);
      Console.WriteLine(resx);

      var y = new Point(0.0, 0.0).ToArray();
      var resy = model.Predict(y);
      Console.WriteLine(resy);
    }
  }
}
