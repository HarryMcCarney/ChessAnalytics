#r "nuget: Deedle, 2.5.0"
open Deedle 

let path = "C:/LichessData/data/" 
let file = (path + "2022-12-02--22-03-51-lichess_db_standard_rated_2015-02.csv")

let headers ="Event (string),\ 
            Site (string),\ 
            White (string),\ 
            Black (string),\ 
            Result (string),\ 
            UTCDate (string),\
            UTCTime (string),\
            WhiteElo (string),\ 
            BlackElo (string),\ 
            WhiteRatingDiff (string),\ 
            BlackRatingDiff (string),\
            ECO (string),\ 
            Opening (string),\ 
            TimeControl (string),\ 
            Termination (string)
            "
let games = Frame.ReadCsv(file,separators="|", hasHeaders=false,schema=headers,inferTypes=false)

let whitePlayers = games
                    |> Frame.getCol "Column3" 
                    |> Series.map (fun x y -> (string y))
                    |> Series.values

let blackPlayers = games
                    |> Frame.getCol "Column4" 
                    |> Series.map (fun x y -> (string y))
                    |> Series.values


let players = whitePlayers 
                |> Seq.append blackPlayers
                |> Seq.distinct

players
|> Seq.length