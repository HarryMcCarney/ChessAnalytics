// For more information see https://aka.ms/fsharp-console-apps

open FSharp.Data
open StreamPlayersGames
open FSharp.Collections

open System.IO

type Player = {
    UserName : string
}

[<EntryPoint>]
let main playerFile =
    let file = "C:/LichessData/data/" + (playerFile[0])
    let csv = CsvFile.Load(file, hasHeaders = true, separators="|")

    let files = Directory.GetFiles "C:/LichessData/data/games" 
                |> Seq.map (fun x -> (Path.GetFileName x).Replace(".csv", ""))
    
    printfn "%A" files
                
    csv.Rows
    |> Seq.map (fun r -> { UserName = (r.GetColumn "UserName")})
    |> Seq.filter (fun f -> not (files |> Seq.contains f.UserName))
    |> Seq.distinct
    |> Seq.toArray
    //|> Array.Parallel.map (fun x -> getUserGames (x.UserName))
    |> Array.map (fun x -> getUserGames (x.UserName))
    |> ignore
    // Return 0. This indicates success.
    0   