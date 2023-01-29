// For more information see https://aka.ms/fsharp-console-apps

open FSharp.Data
open StreamPlayersGames
open FSharp.Collections
open System.Threading

open System.IO

type Player = {
    UserName : string
}


let run (file:string) = 
    let csv = CsvFile.Load(file, hasHeaders = true, separators="|")

    let files = Directory.GetFiles "C:/LichessData/data/games" 
                |> Seq.map (fun x -> (Path.GetFileName x).Replace(".csv", ""))
    
    printfn "done %i file of %i players" (files |> Seq.toArray |> Array.length)  (csv.Rows |> Seq.toArray |>  Array.length)
                
    csv.Rows
    |> Seq.map (fun r -> { UserName = (r.GetColumn "UserName")})
    |> Seq.filter (fun f -> not (files |> Seq.contains f.UserName))
    |> Seq.distinct
    |> Seq.toArray
    //|> Array.Parallel.map (fun x -> getUserGames (x.UserName))
    |> Array.map (fun x -> getUserGames (x.UserName))
    |> ignore
    // Return 0. This indicates success.


[<EntryPoint>]
let main playerFile =
    let file = "C:/LichessData/data/" + (playerFile[0])
    try
        run file
    with 
    | :? System.Exception as ex ->  printfn "%s" ex.Message; Thread.Sleep 10000; run file
    0   