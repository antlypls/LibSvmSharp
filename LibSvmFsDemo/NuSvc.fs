module NuSvc

open Helpers

open LibSvmFs
open LibSvmFs.SVMFS

let run = 
  let printVal ((x, y), label) = 
    printfn "(%f; %f): %i" x y label

  printfn "--- Nu-SVC Demo ---"

  let class1 = generateClass 0 (0.1, 0.1) 50
  let class2 = generateClass 1 (0.9, 0.9) 50 
  
  let rbfKernel = Rbf 0.5
  
  let common = {CacheSize = 128.0; Eps = 0.001; Shrinking = true; Probability = false}
  
  let nusvcTrain = SVMFS.CreateNuSvc common 0.5 rbfKernel
  
  let trainData = class1 @ class2 |> List.map (fun ((x,y), l) -> ([x; y], l))
  
  let nusvcDecisionFunction =  nusvcTrain trainData
  
  let point1 = (0.1, 0.1)
  let point2 = (0.9, 0.9)
  
  let classify = tupleToList >> nusvcDecisionFunction
  
  let label1  = classify point1 
  let label2  = classify point2 
  
  printfn "Classification Results"
  
  printVal (point1, label1)
  printVal (point2, label2)
