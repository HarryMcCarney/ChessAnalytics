(**
---
title: Some other stuff
category: Reports
categoryindex: 1
index: 1    
---
*)

(** 
# Introduction
*)

(***hide***)
#r "nuget: fsharp.data"
#r "nuget: Plotly.NET, 3.0.1"
#r "nuget: Plotly.NET.Interactive, 3.0.0"
#r "nuget: FSharp.Stats"

open FSharp.Data
open System
open Plotly.NET
open FSharp.Stats
open FSharp.Stats.Distributions

let file = "C:/LichessData/data/" + "2023-1-07--15-48-46-playerratings.csv"

let csv = CsvFile.Load(file, hasHeaders = true, separators = "|")

type MonthlyRating =
    { UserName: string
      Month: DateTime
      Rating: float }

let lastDayOfMonth (dt: DateTime) =
    DateTime(dt.Year, dt.Month, DateTime.DaysInMonth(dt.Year, dt.Month))

let getRatingGain (ratings: seq<MonthlyRating>) =
    let sortedseq = ratings |> Seq.sortBy (fun x -> x.Month)
    (sortedseq |> Seq.last).Rating - (sortedseq |> Seq.head).Rating

let results =
    csv.Rows
    |> Seq.map (fun r ->
        { UserName = (r.GetColumn "UserName")
          Month = lastDayOfMonth ((r.GetColumn "Date").AsDateTime())
          Rating = (r.GetColumn "Rating").AsFloat() })
    |> Seq.groupBy (fun x -> x.UserName, x.Month)
    |> Seq.map (fun (g, r) ->
        { UserName = (fst g)
          Month = (snd g)
          Rating = (r |> Seq.averageBy (fun x -> x.Rating)) })
    |> Seq.groupBy (fun x -> x.UserName)
    |> Seq.map (fun x -> (fst x), (snd x) |> getRatingGain)


let trackingMean =
    csv.Rows
    |> Seq.map (fun r ->
        { UserName = (r.GetColumn "UserName")
          Month = lastDayOfMonth ((r.GetColumn "Date").AsDateTime())
          Rating = (r.GetColumn "Rating").AsFloat() })
    |> Seq.groupBy (fun x -> x.Month)
    |> Seq.map (fun (m, r) -> (m, (r |> Seq.averageBy (fun x -> x.Rating))))
    |> Seq.sortBy fst

let trackingMedian =
    csv.Rows
    |> Seq.map (fun r ->
        { UserName = (r.GetColumn "UserName")
          Month = lastDayOfMonth ((r.GetColumn "Date").AsDateTime())
          Rating = (r.GetColumn "Rating").AsFloat() })
    |> Seq.groupBy (fun x -> x.Month)
    |> Seq.map (fun (m, r) -> (m, (r |> Seq.map (fun x -> x.Rating) |> Seq.median)))
    |> Seq.sortBy fst

let dates = (trackingMean |> Seq.map fst)
let means = (trackingMean |> Seq.map snd)
let medians = (trackingMedian |> Seq.map snd)

let meanLine = Chart.Line(dates, means, Name = "Means")
let medianLine = Chart.Line(dates, medians, Name = "Medians")

let density (xs: seq<float>) =
    Chart.Histogram(xs, HistNorm = StyleParam.HistNorm.Probability)

let vals = results |> Seq.map snd |> Seq.filter (fun x -> x <> 0) |> Seq.sort

[ meanLine; medianLine ]
|> Chart.combine
|> Chart.withDescription (
    ChartDescription.create
        "Average Rapid rating for 5k players with more than 100 rapid games"
        "Shows inflation and deflation. Likely due to influx of beginners during lockdowns"
)
|> GenericChart.toChartHTML
(***include-it-raw***)


(***hide***)
density vals
|> Chart.withXAxisStyle ("Rating change")
|> Chart.withYAxisStyle ("Probability")
|> Chart.withDescription (ChartDescription.create "Rating change of 5k Lichess players from 2015 to 2022" "")
|> GenericChart.toChartHTML
(***include-it-raw***)
