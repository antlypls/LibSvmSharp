// FSharp binding for libsvmsharp

namespace LibSvmFs

open LibSvm

module SVMFS = 
  module Kernels =
    let Linear () = fun (x, y) -> Kernels.Linear().Invoke(x |> List.toArray, y |> List.toArray)
    let Rbf gamma = fun (x, y) -> Kernels.Rbf(gamma).Invoke(x |> List.toArray, y |> List.toArray)
    let Sigmoid gamma r = fun (x, y) -> Kernels.Sigmoid(gamma, r).Invoke(x |> List.toArray, y |> List.toArray)
    let Polynomial gamma degree r = fun (x, y) -> Kernels.Polynomial(gamma, degree, r).Invoke(x |> List.toArray, y |> List.toArray)

  type C = float
  type Nu = float
  type Eps = float
  
  type CommonParams =
    { CacheSize: double
      Eps: double
      Shrinking: bool
      Probability: bool }

  let private applyKernel (svmp: SvmParameter<'pattern>) (kernel: 'pattern*'pattern -> float) =
    svmp.KernelFunc <- new System.Func<'pattern, 'pattern, float>(fun x y -> kernel (x, y))
  
  let private applyCommonParams (svmp: SvmParameter<'pattern>) prmt =
    svmp.CacheSize <- prmt.CacheSize
    svmp.Eps <- prmt.Eps
    svmp.Shrinking <- prmt.Shrinking
    svmp.Probability <- prmt.Probability

  let private createSV<'a, 'b, 'pattern> (fromDouble: double -> 'a) (fillProblem: 'b list -> SvmProblem<'pattern> -> unit) (applySvmParameters: SvmParameter<'pattern> -> unit) (svmParams: CommonParams) (kernel: 'pattern * 'pattern -> float) =
    let svmp = new SvmParameter<'pattern>()
    applySvmParameters svmp
    applyKernel svmp kernel
    applyCommonParams svmp svmParams

    let train (data: 'b list) = 
      let problem = new SvmProblem<'pattern>()
      fillProblem data problem

      svmp.Check(problem)

      let model = LibSvm.Svm.Train(problem, svmp)

      let predict (point: 'pattern) =
        let res = fromDouble(model.Predict(point))
        res 

      predict
    train

  let private createSvm<'a, 'pattern> (fromDouble: double -> 'a) (toDouble: 'a list -> double list) (applySvmParameters: SvmParameter<'pattern> -> unit) (svmParams: CommonParams) =
    let fillProblem (data: ('pattern * 'a) list) (problem: SvmProblem<'pattern>) =
      let x, y = List.unzip data
      problem.Y <- y |> toDouble |> List.toArray
      problem.X <- x |> List.toArray

    createSV<'a, ('pattern * 'a),'pattern> fromDouble fillProblem applySvmParameters svmParams

  let private createSvc<'pattern> =
    createSvm<int, 'pattern> (fun x -> int(x)) (List.map (fun i -> (double) i))

  let private createSvr<'pattern> =
    let id = fun x -> x
    createSvm<double, 'pattern> id id

  let CreateCSvc (svmParams: CommonParams) (c: C) = 
    let applySvmParameters (svmp: SvmParameter<'pattern>) =
      svmp.C <- c
      svmp.SvmType <- SvmType.C_SVC

    createSvc applySvmParameters svmParams

  let CreateNuSvc (svmParams: CommonParams) (nu: Nu) = 
    let applySvmParameters (svmp: SvmParameter<'pattern>) =
      svmp.Nu <- nu
      svmp.SvmType <- SvmType.NU_SVC

    createSvc applySvmParameters svmParams

  let CreateOneClass (svmParams: CommonParams) (nu: Nu) = 
    let applySvmParameters (svmp: SvmParameter<'pattern>) =
      svmp.Nu <- nu
      svmp.SvmType <- SvmType.ONE_CLASS

    let fillProblem (data: 'pattern list) (problem: SvmProblem<'pattern>) =
      let x = data
      problem.Y <- x |> List.map (fun i -> 1.0) |> List.toArray
      problem.X <- x |> List.toArray

    let fromDouble (x: double) =
      x > 0.0

    createSV<bool, 'pattern, 'pattern> fromDouble fillProblem applySvmParameters svmParams

  let CreateEpsilonSvr (svmParams: CommonParams) (c: C) (eps: Eps) =
    let applySvmParameters (svmp: SvmParameter<'pattern>) =
      svmp.C <- c
      svmp.P <- eps
      svmp.SvmType <- SvmType.EPSILON_SVR

    createSvr applySvmParameters svmParams

  let CreateNuSvr (svmParams: CommonParams) (c: C) (nu: Nu) = 
    let applySvmParameters (svmp: SvmParameter<'pattern>) =
      svmp.C <- c
      svmp.Nu <- nu
      svmp.SvmType <- SvmType.NU_SVR

    createSvr applySvmParameters svmParams
