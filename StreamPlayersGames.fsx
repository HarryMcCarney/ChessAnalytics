#r "nuget: fsharp.data"
#load "ParsePGN.fsx"

open FSharp.Data
open System.Threading

let  response (user:string) =
    let userGamesUrl = sprintf "https://lichess.org/api/games/user/%s?perf=Rapid&Moves=false" user

    Http.RequestStream(
        userGamesUrl,
        httpMethod = "GET",
        headers = [ "Accept", "application/text"
                    "Content-Type", "application/text" ]
            )

let getUserGames user  = 
    Thread.Sleep 500
    try 
        let rs = response user
        printfn "returned ratings from lichess api"  

        let reader = new StreamReader(rs.ResponseStream) 
        ParsePGN.parseStream reader (sprintf "data/%s.csv" user)

    with 
    | :? System.Net.WebException as ex -> printfn "Waiting 60 seconds : %s" (ex.Status.ToString()); Thread.Sleep 90000; ()



getUserGames "JulesBonnot";;