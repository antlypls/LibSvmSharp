using System;
using System.Collections.Generic;
using System.Linq;
using LibSvm;
using LibSvmExtras.Models;

namespace LibSvmExtras.Trainers
{
  internal class SvrTrainer<TPattern> : TrainerBase<TPattern>, ITrainer<Tuple<TPattern, double>, double, TPattern>
  {
    internal SvrTrainer(SvmParameter<TPattern> parameters)
      : base(parameters)
    {

    }
    public IModel<TPattern, double> Train(IEnumerable<Tuple<TPattern, double>> data)
    {
      var problem = new SvmProblem<TPattern>
      {
        Y = data.Select(p => p.Item2).ToArray(),
        X = data.Select(p => p.Item1).ToArray()
      };

      var model = TrainSvmModel(problem);

      return new RegressionModel<TPattern>(model);
    }
  }
}
