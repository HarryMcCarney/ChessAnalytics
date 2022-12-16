#r "nuget: fsharp.data"

open FSharp.Data


let  response (user:string) =
    let userRating = sprintf "https://lichess.org/api/games/user/%s" user
    Http.Request(
        userRating,
        httpMethod = "GET",
        headers = [ "Accept", "application/text"
                    "Content-Type", "application/text" ]
            )

let getUserGames user  = 
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
