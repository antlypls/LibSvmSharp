﻿using System;
using System.Linq;
using LibSvmExtras;
using LibSvmExtras.Kernel;
using LibSvmExtras.Svm;

namespace LibSvmExtrasDemo.Demo
{
  internal static class NuSVMDemo
  {
    public static void Run()
    {
      Console.WriteLine("NuSVMDemo");
      var class1 = DemoHelper.GenerateClass(0, 0.1, 0.1, 50);
      var class2 = DemoHelper.GenerateClass(1, 0.8, 0.8, 50);

      var trainData = class1.Concat(class2);

      var trainer = SVM.Create(new NuSupportVectorClassification<double[]>(0.1), new RbfKernel(0.5));
      var model = trainer.Train(trainData.Select(p => Tuple.Create(p.ToArray(), p.Label)));

      var x = new Point(0.9, 0.9).ToArray();
      var resx = model.Predict(x);
      Console.WriteLine(resx);

      var y = new Point(0.0, 0.0).ToArray();
      var resy = model.Predict(y);
      Console.WriteLine(resy);
    }
  }
}
