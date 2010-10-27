using System;
using LibSvm;

namespace LibSvmExtras.Trainers
{
  internal class TrainerBase
  {
    private readonly SvmParameter _parameters;

    protected TrainerBase(SvmParameter parameters)
    {
      _parameters = parameters;
    }

    protected SvmModel TrainSvmModel(SvmProblem problem)
    {
      _parameters.Check(problem);
      var model = LibSvm.Svm.svm_train(problem, _parameters);
      return model;
    }
  }
}
