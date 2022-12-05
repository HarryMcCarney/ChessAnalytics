

module PlayerActivity
#r "nuget: FSharp.Data"

open FSharp.Data
open System.Threading
open System

[<Literal>]
let private ActivitySample =  "C:/LichessData/LichessSamples/Activity.json"

let private usersById ="https://lichess.org/api/users"


type private UserProfile = JsonProvider<ActivitySample, InferTypesFromValues=true>

let private response (users:string) =
    let httpBody = TextRequest users
    Http.Request(
        usersById,
        httpMethod = "POST",
        headers = [ "Accept", "application/text"
                    "Content-Type", "application/text" ],
        body = httpBody
    )

let private convertToTimestamp dt =
    let epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    let elapsedTime = dt - epoch;
    (int64)elapsedTime.TotalMilliseconds 

let getActiveUsers date users = 
    let ts = convertToTimestamp date
    let rs = response users 
    match rs.Body with
            | Text x -> UserProfile.Parse(x)
            | _ -> failwith "request failed" 
    |> Seq.filter (fun x -> x.SeenAt >= 1667313709000L)
    |> Seq.map (fun x -> x.Username)




