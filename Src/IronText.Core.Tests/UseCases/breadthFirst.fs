#light

module MyTree

open System
open System.Collections.Generic

type Tree = Branch of int * Tree * Tree
          | Leaf of int

let rec nextValues acc = function
    | [] ->
        acc
    | item :: rest ->
        match item with
        | Leaf value  -> 
            nextValues (acc @ [value]) rest
        | Branch(value, left, right) -> 
            nextValues (acc @ [value]) (rest @ [ left; right ])

let rec breadthFirst tree =
    nextValues [] [tree] 

let rec nextValuesRL acc = function
    | [] ->
        acc
    | item :: rest ->
        match item with
        | Leaf value  -> 
            nextValuesRL (acc @ [value]) rest
        | Branch(value, left, right) -> 
            nextValuesRL (acc @ [value]) (left :: right :: rest)

let rec depthFirst tree =
    nextValuesRL [] [tree] 

let print x = printfn "%A" x

let t = Branch(
            1,
            Branch(
                2, 
                Leaf 3,
                Leaf 4),
            Branch(
                5,
                Leaf 6,
                Leaf 7))

print "Breadth-first traversal:"
t |> breadthFirst |> print

print "Expected result"
print [1; 2; 5; 3; 4; 6; 7]

print "depth-first traversal:"
t |> depthFirst |> print
