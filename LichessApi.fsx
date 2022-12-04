

#r "nuget: FSharp.Data"

open FSharp.Data
open System.Threading
open System

[<Literal>]
let ActivitySample =  "C:/LichessData/LichessSamples/Activity.json"

let usersById ="https://lichess.org/api/users"


let getGamesUrl user = 
    sprintf "https://lichess.org/api/games/user/%s?since=1667313709000&moves=false&pertype=rapid,blitz" user


let createDate y m d = 
    let date = sprintf "%i-%i-%i" y (m+1) d
    DateTime.Parse date

type UserProfile = JsonProvider<ActivitySample, InferTypesFromValues=true>

let httpBody : HttpRequestBody = TextRequest "julesbonnot,jimsprout"

let response =
            Http.Request(
                usersById,
                httpMethod = "POST",
                headers =
                    [ "Accept", "application/text"
                      "Content-Type", "application/text" ],
                body = httpBody
                
            )

let body = 
   match response.Body with
            | Text x -> x
            | _ -> failwith "request failed"


let users = UserProfile.Parse(body)

users
|> Seq.map (fun x -> x.SeenAt)