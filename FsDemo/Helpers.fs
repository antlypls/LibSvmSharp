module Helpers

open System

let randomNumber = 
  let rnd = new Random()
  (fun() -> (rnd.NextDouble() - 0.5) / 4.0)

let generateClass label (x, y) count = 
  let generatePoint() = 
    let withMean z = 
      z +  randomNumber()
    let x_ = withMean x
    let y_ = withMean y
    ((x_, y_), label)
  [for x in 1..count -> generatePoint()]

let sinc x =
  match x with
  | _ when x = 0.0 -> 1.0
  | _ -> sin(x)/x

let tupleToList (x,y) = [x; y]
