﻿module Funogram.Generator.Program

open System
open System.IO
open Argu
open Funogram.Generator.Types

type CliArguments =
  | [<AltCommandLine("-t")>] Types
  | [<AltCommandLine("-m")>] Methods
  | Cached

  interface IArgParserTemplate with
    member x.Usage =
      match x with
      | Types -> "Generate types"
      | Methods -> "Generate methods"
      | Cached -> "Take HTML page from local cache if possible"

let processAsync (args: CliArguments list) =
  async {
    let cached = true // args |> List.contains CliArguments.Cached
    let! html =
      Loader.mkLoader ()
      |> Loader.withCache cached
      |> Loader.loadAsync
    
    let types =
      TypesParser.mkParser html
      |> TypesParser.withResultPath (Path.Combine(Constants.OutputDir, "types.json"))
      |> TypesParser.loadRemapData "./RemapTypes.json"
      |> TypesParser.parse

    ()
  }

[<EntryPoint>]
let main argv =
  let parser = ArgumentParser.Create<CliArguments>()
  try
    let result = parser.ParseCommandLine(inputs = argv, raiseOnUsage = true)
    let result = result.GetAllResults()

    try
      processAsync result |> Async.RunSynchronously
    with
    | e ->
      printfn "%A" e
  with e ->
    printfn "%s" (parser.PrintUsage())
  0
