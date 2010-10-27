using System;
using System.Collections.Generic;
using System.Linq;
using LibSvm;
using LibSvmExtras.Models;

namespace LibSvmExtras.Trainers
{
  internal class OneClassTrainer : TrainerBase, ITrainer<double[], bool>
  {
    internal OneClassTrainer(SvmParameter parameters)
      : base(parameters)
    {

    }

    public IModel<bool> Train(IEnumerable<double[]> data)
    {
      var problem = new SvmProblem
      {
        Y = data.Select(p => 1.0).ToArray(),
        X = data.Select(p => p.ToSvmNodes()).ToArray()
      };

      var model = TrainSvmModel(problem);

      return new OneClassModel(model);
    }
  }
}
