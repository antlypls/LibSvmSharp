module NuSvr

open Helpers

open LibSvmFs
open LibSvmFs.SVMFS

let run =
  printfn "--- Nu-SVR Demo ---"

  let rbfKernel = Rbf 0.5

  let common = {CacheSize = 128.0; Eps = 0.001; Shrinking = true; Probability = false}

  let nusvrTrain = SVMFS.CreateNuSvr common 1.0 0.1 rbfKernel

  let trainData =
    [for i in 0..200 -> -10.0 + double(i)*0.1] |> List.map (fun x -> ([x], sinc(x) + randomNumber()))

  let nusvrDecisionFunction = nusvrTrain trainData

  printfn "Regression Results"

  [for i in 0..20 -> -1.0 + double(i)*0.1] |> List.iter (fun x ->
    printfn "x:   %f" x
    printfn "y_r: %f" (sinc x)
    printfn "y_p: %f" (nusvrDecisionFunction [x])
    )
