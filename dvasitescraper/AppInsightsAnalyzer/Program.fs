// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.

open FSharp.Data

type AppInsights = CsvProvider<"C:/Users/nick/Desktop/chatlogs20170601.csv">
type CustomDimensions = JsonProvider<"""{"from":"DVAESABot@jLW4QPxAl6g","to":"Gmm4l4WEFQb"}""">
[<EntryPoint>]
let main argv = 
   let csvPath = argv.GetValue(0)
   let logs = AppInsights.Load("C:/Users/nick/Desktop/chatlogs20170601.csv")
     
   let royTan = "Gmm4l4WEFQb"
   let tinaLemon = "4GsLO47nY5l"
   let ianPink = "7SyGzSQrdp0"
      
   let getRowsForHuman humanName = 
    logs.Rows 
    |> Seq.filter (fun r -> CustomDimensions.Parse(r.CustomDimensions).To.Equals(humanName) || CustomDimensions.Parse(r.CustomDimensions).From.Equals(humanName))
    |> Seq.sortBy (fun r -> r.Timestamp)
    |> Seq.map (fun r -> if (CustomDimensions.Parse(r.CustomDimensions).From.Equals(humanName)) then "Human: " + r.Message else  "Bot: " + r.Message)

   
   let printLogs humanId humanName = 
    printfn "PERSONA: %s\n=============================" humanName

    let rows = getRowsForHuman humanId
    for row in rows do 
        printfn "%s" row
       

   printLogs royTan "Roy Tan"
   printLogs tinaLemon "Tina Lemon"
   printLogs ianPink "Ian Pink"

   0
   