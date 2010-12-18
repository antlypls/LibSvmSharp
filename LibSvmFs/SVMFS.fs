// FSharp binding for libsvmsharp

namespace LibSvmFs

open LibSvm

module SVMFS = 
  type C = float
  type Nu = float
  type Eps = float
  
  type Gamma = float
  type Degree = int
  type R = float
  
  type KernelType =
    | Linear
    | Poly of Gamma*Degree*R
    | Rbf of Gamma
    | Sigmoid of Gamma*R
  
  type CommonParams =
    { CacheSize: double
      Eps: double
      Shrinking: bool
      Probability: bool }

  let private listToSvmNodes arr = 
    List.mapi (fun idx vl -> new SvmNode(idx, vl)) arr

  let private applyKernel (svmp: SvmParameter) kernel = 
    match kernel with
    | Linear ->
      svmp.KernelType <- LibSvm.KernelType.Linear
  
    | Poly(gamma, degree, r) ->
      svmp.KernelType <- LibSvm.KernelType.Poly
      svmp.Gamma <- gamma
      svmp.Degree <- degree
      svmp.Coef0 <- r
  
    | Rbf(gamma) ->
      svmp.KernelType <- LibSvm.KernelType.Rbf
      svmp.Gamma <- gamma
  
    | Sigmoid(gamma, r) ->
      svmp.KernelType <- LibSvm.KernelType.Sigmoid
      svmp.Gamma <- gamma
      svmp.Coef0 <- r
  
  let private applyCommonParams (svmp: SvmParameter) prmt = 
    svmp.CacheSize <- prmt.CacheSize
    svmp.Eps <- prmt.Eps
    svmp.Shrinking <- prmt.Shrinking
    svmp.Probability <- prmt.Probability

  let private createSV<'a, 'b> (fromDouble: double -> 'a) (fillProblem: 'b list -> SvmProblem -> unit) (applySvmParameters: SvmParameter -> unit) (svmParams: CommonParams) (kernel: KernelType) = 
    let svmp = new SvmParameter()
    applySvmParameters svmp
    applyKernel svmp kernel
    applyCommonParams svmp svmParams

    let train (data: 'b list) = 
      let problem = new SvmProblem()
      fillProblem data problem

      svmp.Check(problem)

      let model = LibSvm.Svm.Train(problem, svmp)

      let predict (point: double list) = 
        let nodes = point |> listToSvmNodes |> List.toArray
        let res = fromDouble(model.Predict(nodes))
        res 

      predict
    train

  let private createSvm<'a> (fromDouble: double -> 'a) (toDouble: 'a list -> double list) (applySvmParameters: SvmParameter -> unit) (svmParams: CommonParams) =
    let fillProblem (data: (double list * 'a) list) (problem: SvmProblem) =
      let x, y = List.unzip data
      problem.Y <- y |> toDouble |> List.toArray
      problem.X <- x |> List.map (listToSvmNodes >> List.toArray) |> List.toArray

    createSV<'a, (double list * 'a)> fromDouble fillProblem applySvmParameters svmParams

  let private createSvc =
    createSvm<int> (fun x -> int(x)) (List.map (fun i -> (double) i)) 

  let private createSvr =
    let id = fun x -> x
    createSvm<double> id id

  let CreateCSvc (svmParams: CommonParams) (c: C) = 
    let applySvmParameters (svmp: SvmParameter) =
      svmp.C <- c
      svmp.SvmType <- SvmType.C_SVC

    createSvc applySvmParameters svmParams

  let CreateNuSvc (svmParams: CommonParams) (nu: Nu) = 
    let applySvmParameters (svmp: SvmParameter) =
      svmp.Nu <- nu
      svmp.SvmType <- SvmType.NU_SVC

    createSvc applySvmParameters svmParams

  let CreateOneClass (svmParams: CommonParams) (nu: Nu) = 
    let applySvmParameters (svmp: SvmParameter) =
      svmp.Nu <- nu
      svmp.SvmType <- SvmType.ONE_CLASS

    let fillProblem (data: (double list) list) (problem: SvmProblem) =
      let x = data
      problem.Y <- x |> List.map (fun i -> 1.0) |> List.toArray
      problem.X <- x |> List.map (listToSvmNodes >> List.toArray) |> List.toArray

    let fromDouble (x: double) =
      x > 0.0

    createSV<bool, double list> fromDouble fillProblem applySvmParameters svmParams

  let CreateEpsilonSvr (svmParams: CommonParams) (c: C) (eps: Eps) =
    let applySvmParameters (svmp: SvmParameter) =
      svmp.C <- c
      svmp.P <- eps
      svmp.SvmType <- SvmType.EPSILON_SVR

    createSvr applySvmParameters svmParams

  let CreateNuSvr (svmParams: CommonParams) (c: C) (nu: Nu) = 
    let applySvmParameters (svmp: SvmParameter) =
      svmp.C <- c
      svmp.Nu <- nu
      svmp.SvmType <- SvmType.NU_SVR

    createSvr applySvmParameters svmParams
