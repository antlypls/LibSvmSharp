using System;
using System.Collections.Generic;
using System.Linq;
using LibSvm;
using LibSvmExtras.Models;

namespace LibSvmExtras.Trainers
{
  internal class SvcTrainer<TPattern> : TrainerBase<TPattern>, ITrainer<Tuple<TPattern, int>, int, TPattern>
  {
    internal SvcTrainer(SvmParameter<TPattern> parameters)
      : base(parameters)
    {

    }

    public IModel<TPattern, int> Train(IEnumerable<Tuple<TPattern, int>> data)
    {
      var problem = new SvmProblem<TPattern>
      {
        Y = data.Select(p => (double)p.Item2).ToArray(),
        X = data.Select(p => p.Item1).ToArray()
      };

      var model = TrainSvmModel(problem);

      return new ClassificationModel<TPattern>(model);
    }
  }
}
