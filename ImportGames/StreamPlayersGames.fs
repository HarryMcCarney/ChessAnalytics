module StreamPlayersGames

open FSharp.Data
open System.Threading
open System.IO

let  response (user:string) =
    let userGamesUrl = sprintf "https://lichess.org/api/games/user/%s?perfType=rapid&Moves=false&rated=true" user
    printfn "%s" userGamesUrl
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
        printfn "streaming games for %s" user 

        let reader = new StreamReader(rs.ResponseStream) 
        ParsePGN.parseStream reader (sprintf "C:/LichessData/data/games/%s.csv" user)

    with 
    | :? System.Net.WebException as ex -> printfn "Waiting 60 seconds : %s" (ex.Message.ToString()); Thread.Sleep 90000; ()

