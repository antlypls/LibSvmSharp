module OneClass

open Helpers

open LibSvmFs
open LibSvmFs.SVMFS

let run = 
  let printVal ((x, y), label) = 
    printfn "(%f; %f): %b" x y label

  printfn "--- One Class Demo ---"

  let class1 = generateClass 0 (0.5, 0.5) 100
  
  let rbfKernel = Rbf 0.5
                                                          
  let common = {CacheSize = 128.0; Eps = 0.001; Shrinking = true; Probability = false}
  
  let oneClassTrain = SVMFS.CreateOneClass common 0.5 rbfKernel
  
  let trainData = class1 |> List.map (fun ((x,y), l) -> [x; y])
  
  let oneClassDecisionDunction = oneClassTrain trainData
  
  let point1 = (0.9, 0.9)
  let point2 = (0.5, 0.5)
  let point3 = (0.45, 0.45)

  let classify = tupleToList >> oneClassDecisionDunction
  
  let label1  = classify point1 
  let label2  = classify point2 
  let label3  = classify point3 
  
  printfn "Classification Results"
  
  printVal (point1, label1)
  printVal (point2, label2)
  printVal (point3, label3)
