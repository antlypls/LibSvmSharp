using System;
using System.Collections.Generic;
using System.Linq;
using LibSvm;
using LibSvmExtras.Models;

namespace LibSvmExtras.Trainers
{
  internal class SvrTrainer : TrainerBase, ITrainer<Tuple<double[], double>, double>
  {
    internal SvrTrainer(SvmParameter parameters)
      : base(parameters)
    { 
    
    }
    public IModel<double> Train(IEnumerable<Tuple<double[], double>> data)
    {
      var problem = new SvmProblem
      {
        Y = data.Select(p => p.Item2).ToArray(),
        X = data.Select(p => p.Item1.ToSvmNodes()).ToArray()
      };

      var model = TrainSvmModel(problem);

      return new RegressionModel(model);
    }
  }
}
