#load "packages/FsLab/Themes/DefaultWhite.fsx"
#load "packages/FsLab/FsLab.fsx"

open System.IO
open Deedle
open FSharp.Data
open XPlot.GoogleCharts
open XPlot.GoogleCharts.Deedle

// The JSON provider is not generative so it needs the sample data at runtime
// We can easily embed it in a script as a literal.
[<Literal>]
let ForecastSample = """
{
  "data": {
    "type": "finansraadetForecast",
    "attributes": {
      "end_year": 2046,
      "start_year": 2017,
      "forecasts": [
        {
          "opening_balance": 100000,
          "gross_return": 3400,
          "year": 2017,
          "net_return": 2195,
          "fees": 1205,
          "closing_balance": 102195
        }
      ]
    },
    "id": "90dcc2a9-8c77-43a9-8053-ce0b4f5446dc"
  }
}

"""

type ForecastProvider = JsonProvider<ForecastSample, RootName="Forecast">

let dataFile = Path.Combine(__SOURCE_DIRECTORY__, "./data/forecast.json")
let forecast = ForecastProvider.Load(dataFile)


let dataFor fn =
     [for x in forecast.Data.Attributes.Forecasts -> ((string x.Year), (fn x))]
     
let fees = dataFor (fun x -> x.Fees)
let netReturns = dataFor (fun x -> x.NetReturn)
let opens = dataFor (fun x -> x.OpeningBalance)
let closes = dataFor (fun x -> x.ClosingBalance)
let cumSumFeesNoYear = fees |> Seq.map snd |> Seq.scan (+) 0 
let cumSumFees =
    Seq.zip (Seq.map fst fees) (fees |> Seq.map snd |> Seq.scan (+) 0)
    |> List.ofSeq


let valueAndFeesBarChart =
    [closes; cumSumFees]
    |> Chart.Bar
    |> Chart.WithTitle "Forecast account value"
    |> Chart.WithXTitle "Year"
    |> Chart.WithOptions (Options(legend=Legend(position="bottom")))
    |> Chart.WithLabels ["Account Value (year end)";"Fees to Date"]

let returnsAndFeesLineChart =
    [netReturns; fees]
    |> Chart.Line
    |> Chart.WithTitle "Return forecast"
    |> Chart.WithXTitle "Year"
    |> Chart.WithOptions (Options(legend=Legend(position="bottom")))
    |> Chart.WithLabels ["Net Returns (for year)"; "Fees (for year)"]


let areaData =
    [
        [for x in forecast.Data.Attributes.Forecasts -> (x.Year, x.NetReturn)]
        [for x in forecast.Data.Attributes.Forecasts -> (x.Year, x.Fees)]
    ]

let returnsAndFeesAreaChart =
    areaData    
    |> Chart.Area
    |> Chart.WithTitle "Returns and Fees Forecast"
    |> Chart.WithOptions (Options(legend=Legend(position="bottom")))
    |> Chart.WithLabels ["Net Returns (for year)"; "Fees (for year)"]


