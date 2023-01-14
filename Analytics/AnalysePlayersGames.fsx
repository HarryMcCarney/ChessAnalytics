
#r "nuget: fsharp.data"
#r "nuget: Plotly.NET, 3.0.1"
#r "nuget: Plotly.NET.Interactive, 3.0.0"
#r "nuget: FSharp.Stats"

open System.IO
open System
open Plotly.NET
open FSharp.Stats
open FSharp.Data
open FSharp.Stats.Fitting
open LinearRegression.OrdinaryLeastSquares
open GoodnessOfFit.OrdinaryLeastSquares.Linear.Univariable
open FSharp.Stats.Distributions
open FSharp.Stats
open FSharp.Stats.Fitting.LinearRegression


let files = Directory.GetFiles "C:/LichessData/data/games" 
           
let countGames (f: string) = 
    
    let player = (Path.GetFileName f).Replace(".csv", "")
    try 
        let csv = CsvFile.Load(f, hasHeaders = false, separators="|")
        Some (player, (csv.Rows |> Seq.toArray |> Array.length))

    with 
    | :? System.Exception as ex -> printfn "Cant read file for %s so skipping" player; None


let gameCount = files 
                |> Seq.map (fun f -> countGames f)
                |> Seq.filter(fun x -> x.IsSome) 
                |> Seq.map( fun x -> x.Value)
                |> Map.ofSeq
                

let ratingsFile = "C:/LichessData/data/" + "2023-1-07--15-48-46-playerratings.csv"

let ratingsCsv = CsvFile.Load(ratingsFile, hasHeaders = true, separators="|")

type MonthlyRating = {
    UserName : string
    Month : DateTime
    Rating : float
}

let lastDayOfMonth (dt : DateTime) = 
    DateTime(dt.Year, dt.Month, DateTime.DaysInMonth(dt.Year, dt.Month))

let getRatingGain (ratings: seq<MonthlyRating>) = 
    let sortedseq = ratings 
                    |> Seq.sortBy (fun x -> x.Month)
    (sortedseq |> Seq.last).Rating
    - 
    (sortedseq |> Seq.head).Rating

let results = 
            ratingsCsv.Rows
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

let testRandom = Random(12345)

let randomFromArrayWithPercentage (percentage: float) (arr: (float * float) array) =
    arr
    |> Array.sortBy(fun _ -> testRandom.NextDouble())
    |> Array.take (Math.Max(int (float arr.Length * percentage), 1))

let values = results
            |> Seq.filter (fun (p, r) -> gameCount.ContainsKey p)
            |> Seq.map (fun (p,r) ->  ((gameCount.TryFind p).Value) |> float, r)
            |> Seq.toArray
            |> fun x -> randomFromArrayWithPercentage 0.5 x


let x = values |> Seq.map fst
let y = values |> Seq.map snd

let yVector = values |> Seq.map snd |> vector
let xVector = values |> Seq.map fst |> vector

//Least squares simple linear regression
let coefficientsLinearLS = 
    OrdinaryLeastSquares.Linear.Univariable.coefficient xVector yVector
let fittingFunctionLinearLS x = 
    OrdinaryLeastSquares.Linear.Univariable.fit coefficientsLinearLS x

let fittingLS = 
    let fit = 
        x
        |> Seq.toArray
        |> Array.map (fun x -> x, fittingFunctionLinearLS x)
    Chart.Line(fit)

let fittedValues = 
    x
    |> Seq.toArray
    |> Array.map (fun x -> fittingFunctionLinearLS x)


[   
Chart.Point(values);
fittingLS
]
|> Chart.combine
|> Chart.show

