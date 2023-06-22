

module PlayerRating 
#r "nuget: FSharp.Data"
#r "nuget: FSharp.Json"
open FSharp.Data
open System.Threading
open System
open FSharp.Json

type VariantRating = {
    name : string
    points : int array array
}

type RatingDate = {
    UserName : string
    Variant : string
    Date : DateTime
    Rating: int 
}

let parseDate (v : int array) = 
    try
        new DateTime(v[0], (v[1]+1), v[2]); 
    with 
    | :? System.ArgumentOutOfRangeException as ex -> printfn "Cant parse %A"  v; reraise() 
    

let getRatingsForVariant (user:string) (variant: string) (ratings: int array array) : RatingDate array   = 
    ratings
    |> Array.map (fun x -> {UserName = user; Variant = variant; Date = (parseDate x); Rating = x[3]})


let  response (user:string) =
    let userRating = sprintf "https://lichess.org/api/user/%s/rating-history" user
    Http.Request(
        userRating,
        httpMethod = "GET",
        headers = [ "Accept", "application/text"
                    "Content-Type", "application/text" ]
            )

let getUserRatings user  = 
    Thread.Sleep 500
    try 
        let rs = response user
        printfn "returned ratings from lichess api"   
        match rs.Body with
                | Text x ->  (Json.deserialize<VariantRating[]> x) 
                                |> Array.map (fun x -> getRatingsForVariant user x.name x.points)
                                |> Array.collect (fun x -> x)
                | _ -> failwith "request failed" 
    with 
    | :? System.Net.WebException as ex -> printfn "Waiting 60 seconds : %s" (ex.Status.ToString()); Thread.Sleep 90000; [||]




