using System;
using System.Collections.Generic;
using System.Linq;
using LibSvm;
using LibSvmExtras.Models;

namespace LibSvmExtras.Trainers
{
  internal class SvcTrainer : TrainerBase, ITrainer<Tuple<double[], int>, int>
  {
    internal SvcTrainer(SvmParameter parameters)
      : base(parameters)
    { 
    
    }

    public IModel<int> Train(IEnumerable<Tuple<double[], int>> data)
    {
      var problem = new SvmProblem
      {
        Y = data.Select(p => (double)p.Item2).ToArray(),
        X = data.Select(p => p.Item1.ToSvmNodes()).ToArray()
      };

      var model = TrainSvmModel(problem);

      return new ClassificationModel(model);
    }
  }
}
