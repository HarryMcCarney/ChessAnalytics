#r "nuget: fsharp.data"
#r "nuget: Plotly.NET, 3.0.1"
#r "nuget: Plotly.NET.Interactive, 3.0.0"
#r "nuget: FSharp.Stats"

open FSharp.Data
open System.IO
open System
open Plotly.NET

//Rated Rapid game|https://lichess.org/dxp68ScS|5meoDIPT|phuc2013|0-1|2023.01.14|10:01:10|1620|1584|-18|+6|C24||600+0|Normal

type Game = {
    Variant: string
    Url: string 
    White: string
    Black: string
    Result: string 
    Date: string  
    Time: string
    WhiteRating: string
    BlackRating: string
    WhiteRatingChange: string
    BlackRatingChange: string
    Opening: string
    Misc : string
    Termination: string
} 

type Player = {
    WhiteOpeningRatio: float
    BlackOpeningRatio : float 
    RatingChange: float 
}

let files = Directory.GetFiles "C:/LichessData/data/games" 
         
        

let buildPlayer (f: string) = 
    let name: string = (Path.GetFileName f).Replace(".csv", "")

    let csv = CsvFile.Load(f, hasHeaders = true, separators="|")
    csv.Rows
    |> Seq.map(fun r -> {Variant = r[0]; Url = r[1]; White = r[2]; Black = r[3]; Result = r[4]; Date = r[5]; 
                            Time = r[6]; WhiteRating = r[7]; BlackRating = r[8]; WhiteRatingChange = r[9]; BlackRatingChange = r[10];
                            Opening =r[11]; Misc = r[12]; Termination = r[13]  } )
    |> Seq.filter(fun g -> g.White = name )
    |> Seq.map(fun g -> name, g.Date, g.Opening)
    |> Seq.toArray
    |> Array.sortBy(fun (n, d, o) -> d)
    |> Array.map (fun (n,d,o) -> n,o )
    |> Array.chunkBySize 100
    |> Array.map(fun a ->  a |> Array.distinct |> Array.countBy fst |> Array.map(fun (n, o) -> n, float o / 100.))
    |> Array.concat
    |> Array.groupBy(fun (n,o) -> n)
    |> Array.map(fun (k,t) ->k,  t |> Array.averageBy(fun (n, r) -> r ))


let openings = 
    files 
    |> Seq.truncate 50
    |> Seq.map(fun f -> buildPlayer f)
    |> Seq.concat


let ratingsFile = "C:/LichessData/data/" + "2023-1-07--15-48-46-playerratings.csv"

let ratingCsv = CsvFile.Load(ratingsFile, hasHeaders = true, separators="|")

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

let ratings = 
            ratingCsv.Rows
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


let data  =
        openings
        |> Seq.map (fun (name, o) ->  o, ratings |> Seq.tryFind (fun (n,r)  -> n = name) |> snd)
        |> Seq.filter (fun (o,r) -> r.IsSome )
        |> Seq.map(fun (o,r)-> o, r.Value)


Chart.Point data
|> Chart.show