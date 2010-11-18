using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibSvmExtras;
using LibSvmExtras.Svm;
using LibSvmExtras.Kernel;

namespace LibSvmExtrasDemo.Demo
{
  internal static class CSVMDemo
  {
    public static void Run()
    {
      Console.WriteLine("CSVMDemo");
      var class1 = DemoHelper.GenerateClass(0, 0.1, 0.1, 50);
      var class2 = DemoHelper.GenerateClass(1, 0.8, 0.8, 50);

      var trainData = class1.Concat(class2);

      var trainer = SVM.Create(new CSupportVectorClassification<double[]>(1.0), new RbfKernel(0.5));
      var model = trainer.Train(trainData.Select(p => Tuple.Create(p.ToArray(), p.Label)));

      var x = new Point(0.9, 0.9).ToArray();
      var resx = model.Predict(x);
      Console.WriteLine(resx);

      var y = new Point(0.1, 0.1).ToArray();
      var resy = model.Predict(y);
      Console.WriteLine(resy);
    }
  }
}
