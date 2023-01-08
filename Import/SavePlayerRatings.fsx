
#load "PlayerRating.fsx"

open PlayerRating

let file = "C:/LichessData/data/" + "2023-1-07--13-41-41-playercohort.csv"

let users = Frame.ReadCsv(file,separators="|", hasHeaders=true,inferTypes=true)

let ratings = users
                |> Frame.getCol  "UserName" 
                |> Series.values
                //|> Seq.take 1000 //1k players
                |> Seq.map (fun x ->  PlayerRating.getUserRatings x)
                |> Seq.toArray
                |> Array.collect (fun x -> x)
                |> Array.filter (fun x -> x.Variant = "Rapid")
                |> Frame.ofRecords 


let path = "C:/LichessData/data/" 
let fileNow() =
    DateTime.Now.ToString("yyyy-M-dd--HH-mm-ss");

let fileWithTime = sprintf "%s%s-playerratings.csv" path (fileNow())

ratings.SaveCsv(fileWithTime, separator='|')