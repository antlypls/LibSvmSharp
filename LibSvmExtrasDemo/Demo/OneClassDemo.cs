using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibSvmExtras;
using LibSvmExtras.Svm;
using LibSvmExtras.Kernel;

namespace LibSvmExtrasDemo.Demo
{
  internal static class OneClassDemo
  {
    public static void Run()
    {
      Console.WriteLine("OneClassDemo");
      var trainData = DemoHelper.GenerateClass(0, 0.5, 0.5, 100);

      var trainer = SVM.Create(new OneClass(0.5), new RbfKernel(0.5));
      var model = trainer.Train(trainData.Select(p => p.ToArray()));

      var x = new Point(0.9, 0.9).ToArray();
      var resx = model.Predict(x);
      Console.WriteLine(resx);

      var y = new Point(0.5, 0.5).ToArray();
      var resy = model.Predict(y);
      Console.WriteLine(resy);

      var z = new Point(0.45, 0.45).ToArray();
      var resz = model.Predict(z);
      Console.WriteLine(resz);
    }
  }
}
