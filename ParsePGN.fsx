
#r "nuget: Deedle, 2.5.0"
open System.IO
open System.Threading
open System.Collections.Generic
open System.Text.RegularExpressions
open Deedle 
open System.Diagnostics
open System

let mutable debug = 0

let fileNow() =
    DateTime.Now.ToString("yyyy-M-dd--HH-mm-ss");

let log m = 
    if debug = 1 then 
        printfn "%s" m

let path = "C:/LichessData/data/"  

type Game() = 
        member val Event = "" with get, set
        member val Site = "" with get, set
        member val White = "" with get, set
        member val Black = "" with get, set   
        member val Result = "" with get, set
        member val UTCDate = "" with get, set
        member val UTCTime = "" with get, set
        member val WhiteElo = "" with get, set
        member val BlackElo = "" with get, set
        member val WhiteRatingDiff = "" with get, set
        member val BlackRatingDiff = "" with get, set
        member val ECO = "" with get, set
        member val Opening = "" with get, set
        member val TimeControl = "" with get, set
        member val Termination  = "" with get, set


let mutable games = new List<Game>()

let rxKey = new Regex("[A-Z].*?(?= )")
let rxValue = new Regex("( .*?(?=]))")
let rxEvent = new Regex("\[Event")

let captureMatch (line: string) (rx : Regex) = 
    if (rx.IsMatch(line)) then
        let v = (rx.Matches(line)[0]).Value
        log (sprintf"val: %s" v)
        v.Replace("\"", "").Trim();
    else 
        ""

let parseLine (line: string) (game: Game) =
    match (captureMatch line rxKey) with 
        | "Event" -> game.Event <- captureMatch line rxValue; log "Event matched"
        | "Site" -> game.Site <- captureMatch line rxValue; log "Site matched"
        | "White" -> game.White <- captureMatch line rxValue; log "White matched"
        | "Black" ->  game.Black <- captureMatch line rxValue; log "Black matched"
        | "Result" -> game.Result <- captureMatch line rxValue; log "Result matched"
        | "UTCDate" -> game.UTCDate <- captureMatch line rxValue; log "UTCDate matched"
        | "UTCTime" -> game.UTCTime <- captureMatch line rxValue; log "UTCTime matched"
        | "WhiteElo" -> game.WhiteElo <- captureMatch line rxValue; log "WhiteElo matched"
        | "BlackElo" -> game.BlackElo <- captureMatch line rxValue; log "BlackElo matched"
        | "WhiteRatingDiff" -> game.WhiteRatingDiff <- captureMatch line rxValue; log "WhiteRatingDiff matched"
        | "BlackRatingDiff" -> game.BlackRatingDiff <- captureMatch line rxValue; log "BlackRatingDiff matched"
        | "ECO" -> game.ECO <- captureMatch line rxValue; log "ECO matched"
        | "Opening" -> game.Opening <- captureMatch line rxValue; log "Opening matched"
        | "TimeControl" -> game.TimeControl <- captureMatch line rxValue; log "TimeControl matched"
        | "Termination" -> game.Termination <- captureMatch line rxValue; log "Termination matched"
        | _ -> ()

let readFile  file : Unit = 
    let timer = new Stopwatch()
    timer.Start()
    let filename = sprintf "%s%s" path file
    
    using (File.OpenText(filename)) ( fun file1 ->

        let mutable newLine = true;
        let mutable game = new Game()   
        
        while (newLine) do
            let line = file1.ReadLine()
            if (line = null) then
                games.Add game
                printfn "games count %i"  games.Count
                newLine <- false // reached end of file
            else
                
                match (rxEvent.IsMatch line, game.Event = "") with
                    | true, true ->    parseLine line game; log "found first game"  // found first game 
                    | true, false ->   games.Add game; game <- new Game(); parseLine line game; 
                                        log "found next game"; if games.Count % 100000 = 0 then printfn "games count %i"  games.Count // next game
                    | false, false -> parseLine line game; log "found next property" //next property 
                    | false, true ->  (); log "skip line"//skip line until first Event is found

                log (sprintf "games count %i"  games.Count)
                //Thread.Sleep 500
        )
    printfn "Parsed %i games in %f seconds" games.Count timer.Elapsed.TotalSeconds

    let fimeWithTime = sprintf "%s-%s"  (fileNow()) (file.Replace(".pgn", ".csv"))
    let csvfilename = sprintf "%s%s" path (fimeWithTime.Replace(".pgn", ".csv"))

    let df = games |> Frame.ofRecords
    df.SaveCsv(csvfilename, separator='|')
    printfn "Saved %i games in %f" games.Count timer.Elapsed.TotalSeconds
    timer.Stop()


//readFile "lichess_db_standard_rated_2015-02.pgn";; 117/139 - using deedle

readFile "Nakamura.pgn";;


