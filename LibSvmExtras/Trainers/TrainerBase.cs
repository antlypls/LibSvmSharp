using System;
using LibSvm;

namespace LibSvmExtras.Trainers
{
  internal class TrainerBase<TPattern>
  {
    private readonly SvmParameter<TPattern> _parameters;

    protected TrainerBase(SvmParameter<TPattern> parameters)
    {
      _parameters = parameters;
    }

    protected SvmModel<TPattern> TrainSvmModel(SvmProblem<TPattern> problem)
    {
      _parameters.Check(problem);
      var model = LibSvm.Svm.Train(problem, _parameters);
      return model;
    }
  }
}
