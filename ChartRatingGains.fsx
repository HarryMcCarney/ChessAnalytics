
#r "nuget: fsharp.data"
#r "nuget: Plotly.NET, 3.0.1"
#r "nuget: Plotly.NET.Interactive, 3.0.0"
#r "nuget: FSharp.Stats"

open FSharp.Data
open System
open Plotly.NET
open FSharp.Stats

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

let getPercentiles d = 
    [|0.01 .. 0.01 .. 1.|] |> Array.map(fun y -> Quantile.mode y d)

let percentiles = results
                |> Seq.map (fun (x, y)-> y )
                |> getPercentiles

results 
|> Seq.filter (fun (u, r) -> r > (percentiles[75]))
|> Seq.map ()


Chart.Line(percentiles, [|0.01 .. 0.01 .. 1.|] )
|> Chart.show 

results
|> Seq.map (fun (x, y)-> y )
|> Chart.Histogram // need to remove inactive players to avoid spike at zero change
|> Chart.show

