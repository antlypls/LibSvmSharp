module EpsSvr

open Helpers

open LibSvmFs
open LibSvmFs.SVMFS

let run =
  printfn "--- Eps-SVR Demo ---"

  let rbfKernel = Kernels.Rbf 0.5
  
  let common = {CacheSize = 128.0; Eps = 0.001; Shrinking = true; Probability = false}

  let esvrTrain = SVMFS.CreateEpsilonSvr common 1.0 0.1 rbfKernel

  let trainData =
    [for i in 0..200 -> -10.0 + double(i)*0.1] |> List.map (fun x -> ([x], sinc(x) + randomNumber()))

  let esvrDecisionFunction = esvrTrain trainData

  printfn "Regression Results"

  [for i in 0..20 -> -1.0 + double(i)*0.1] |> List.iter (fun x ->
    printfn "x:   %f" x
    printfn "y_r: %f" (sinc x)
    printfn "y_p: %f" (esvrDecisionFunction [x])
    )
