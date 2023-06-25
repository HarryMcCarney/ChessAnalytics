#load "LichessArchive.fsx"
#load "ActiveUsers.fsx"
#r "nuget: Deedle, 2.5.0"

open Deedle 
open LichessArchive
open ActiveUsers 
open System

let players : seq<string> = LichessArchive.getPlayers()

let path = "C:/LichessData/data/" 
let fileNow() =
    DateTime.Now.ToString("yyyy-M-dd--HH-mm-ss");

let fileWithTime = sprintf "%s%s-playercohort.csv" path (fileNow())

let activeSince = (DateTime.Now.AddDays(-30))

let playerCohort =  players 
                    |> Seq.toArray
                    |> Array.chunkBySize 300
                    |> Array.map (fun x ->(String.Join(",", x)))
                    |> Array.map (fun x -> getActiveUsers activeSince x)

    
let df = playerCohort
        |> Seq.concat  
        |> Series.ofValues
        |> fun s -> ["UserName", s]
        |> Frame.ofColumns


df.SaveCsv(fileWithTime, separator='|')

printfn "saved %s" fileWithTime