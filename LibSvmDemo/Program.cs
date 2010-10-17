using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibSvm;

namespace LibSvmDemo
{
  struct Point
  {
    public Point(double x, double y)
      : this()
    {
      X = x;
      Y = y;
    }

    public double X { get; set; }
    public double Y { get; set; }
    public int Label { get; set; }

    public SvmNode[] ToSvmNodes()
    {
      return new SvmNode[] { new SvmNode(1, X), new SvmNode(2, Y) };
    }
  }

  class Program
  {
    static List<Point> GenerateClass(int label, double x, double y, int count)
    {
      var rndx = new Random();
      var rndy = new Random();
      var data = Enumerable.Range(0, count).Select(_ => new Point()
      {
        X = x + (rndx.NextDouble() - 0.5) / 4,
        Y = y + (rndx.NextDouble() - 0.5) / 4,
        Label = label
      });

      return data.ToList();
    }

    static void Main(string[] args)
    {
      var rnd = new Random();
      var class1 = GenerateClass(0, 0.1, 0.1, 50);
      var class2 = GenerateClass(1, 0.8, 0.8, 50);

      var trainData = class1.Concat(class2);


      var param = new SvmParameter();

      param.svm_type = SvmType.C_SVC;
      param.kernel_type = KernelType.RBF;
      param.degree = 3;
      param.gamma = 0.5;
      param.coef0 = 0;
      param.nu = 0.5;
      param.cache_size = 40;
      param.C = 1;
      param.eps = 1e-3;
      param.p = 0.1;
      param.shrinking = 1;
      param.probability = 0;
      param.nr_weight = 0;
      param.weight_label = new int[0];
      param.weight = new double[0];

      var prob = new SvmProblem();

      prob.l = trainData.Count();

      prob.y = trainData.Select(p => (double)p.Label).ToArray();

      prob.x = trainData.Select(p => p.ToSvmNodes()).ToArray();

      var model = Svm.svm_train(prob, param);

      var x = new Point(0.9, 0.9).ToSvmNodes();

      var res = Svm.svm_predict(model, x);

      Console.WriteLine(res);

      Console.ReadKey();
    }
  }
}
