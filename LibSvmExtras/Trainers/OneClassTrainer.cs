using System.Collections.Generic;
using System.Linq;
using LibSvm;
using LibSvmExtras.Models;

namespace LibSvmExtras.Trainers
{
  internal class OneClassTrainer<TPattern> : TrainerBase<TPattern>, ITrainer<TPattern, bool, TPattern>
  {
    internal OneClassTrainer(SvmParameter<TPattern> parameters)
      : base(parameters)
    {

    }

    public IModel<TPattern, bool> Train(IEnumerable<TPattern> data)
    {
      var problem = new SvmProblem<TPattern>
      {
        Y = data.Select(p => 1.0).ToArray(),
        X = data.Select(p => p).ToArray()
      };

      var model = TrainSvmModel(problem);

      return new OneClassModel<TPattern>(model);
    }
  }
}
