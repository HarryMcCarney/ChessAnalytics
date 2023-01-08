
module ActiveUsers 
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

let getActiveUsers date users :seq<string> = 
    Thread.Sleep 10000
    let ts = convertToTimestamp date

    try 
        let rs = response users
        printfn "returned users from lichess api"   
        match rs.Body with
                | Text x -> let users = UserProfile.Parse(x)
                            printfn "found %i active users" users.Length
                            users
                | _ -> failwith "request failed" 
        |> Seq.filter (fun x -> match x.SeenAt with | Some y -> y >= ts 
                                                    | None -> 1=2
                                                    )
        |> Seq.filter(fun x -> x.Perfs.Rapid.Games > 100)
        |> Seq.map (fun x -> x.Username)
    with 
    | :? System.Net.WebException as ex -> printfn "Waiting 60 seconds : %s" (ex.ToString()); Thread.Sleep 90000; [||]
    


