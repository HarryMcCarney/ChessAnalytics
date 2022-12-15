


#load "LichessApi.fsx"
#load "FindPlayers.fsx"
#r "nuget: Deedle, 2.5.0"

open Deedle 
open PlayerActivity
open LichessArchive 
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

#load "RatingAnalysis.fsx"

open PlayerRating

let file = "C:/LichessData/data/" + "2022-12-06--19-46-12-playercohort.csv"

let users = Frame.ReadCsv(file,separators="|", hasHeaders=true,inferTypes=true)

let ratings = users
                |> Frame.getCol  "UserName" 
                |> Series.values
                //|> Seq.take 1000 //1k players
                |> Seq.map (fun x ->  PlayerRating.getUserRatings x)
                |> Seq.toArray
                |> Array.collect (fun x -> x)
                |> Array.filter (fun x -> x.Variant = "Rapid")
                |> Frame.ofRecords 


let path = "C:/LichessData/data/" 
let fileNow() =
    DateTime.Now.ToString("yyyy-M-dd--HH-mm-ss");

let fileWithTime = sprintf "%s%s-playerratings.csv" path (fileNow())

ratings.SaveCsv(fileWithTime, separator='|')


#r "nuget: fsharp.data"
#r "nuget: Plotly.NET, 3.0.1"
#r "nuget: Plotly.NET.Interactive, 3.0.0"

open FSharp.Data
open System
open Plotly.NET



let file = "C:/LichessData/data/" + "2022-12-09--22-27-20-playerratings.csv"

let csv = CsvFile.Load(file, hasHeaders = true, separators="|")

type MonthlyRating = {
    UserName : string
    Month : DateTime
    Rating : float
}

open CsvExtensions

let lastDayOfMonth (dt : DateTime) = 
    DateTime(dt.Year, dt.Month, DateTime.DaysInMonth(dt.Year, dt.Month))

let getRatingGain (ratings: seq<MonthlyRating>) = 
    let sortedseq = ratings 
                    |> Seq.sortBy (fun x -> x.Month)
    (sortedseq |> Seq.last).Rating
    - 
    (sortedseq |> Seq.head).Rating

let results = 
            csv.Rows
            |> Seq.map (fun r -> 
                            { UserName = (r.GetColumn "UserName"); 
                                Month = lastDayOfMonth ((r.GetColumn "Date").AsDateTime()); 
                                Rating = (r.GetColumn "Rating").AsFloat();
                            };
                    )
            |> Seq.groupBy (fun x -> x.UserName, x.Month )
            |> Seq.map(fun (g, r) -> {UserName = (fst g);  Month =  (snd g); Rating = (r |> Seq.averageBy (fun x -> x.Rating))}
                    )
            |> Seq.groupBy (fun x -> x.UserName)
            |> Seq.map (fun x -> (fst x), (snd x) |> getRatingGain  )

results
|> Seq.map (fun (x, y)-> y )
|> Chart.Histogram
|> Chart.show

