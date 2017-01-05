#load "packages/FsLab/Themes/DefaultWhite.fsx"
#load "packages/FsLab/FsLab.fsx"

#r "MathNet.Numerics.dll"

open System
open Deedle
open FSharp.Data
open XPlot.GoogleCharts
open XPlot.GoogleCharts.Deedle
open MathNet.Numerics

let tradingDaysInAYear = 252


let randomWalk dailyReturn dailyStandardDeviation (days:int) =
    let steps = MathNet.Numerics.Generate.Normal(days, dailyReturn, dailyStandardDeviation)
    let prices = Array.scan (+) 1.0 steps
    prices


let annualReturn = 0.05
let dailyReturn = Math.Pow((1.0+annualReturn), (1.0/(float tradingDaysInAYear)))-1.0
let dailyStandardDeviation = 0.10/(sqrt (float tradingDaysInAYear))

let plotWalks N =
    let walks = [for i in 1..N ->
                 randomWalk dailyReturn dailyStandardDeviation (1*tradingDaysInAYear)]

    let average = [for day in 1..tradingDaysInAYear do
                       let values = [for i in 1..N -> walks.[i-1].[day-1]]
                       yield (List.average values)]
        
    let averageChart =
        [average |> Array.ofList |> Array.indexed]
        |> Chart.Line
        |> Chart.WithLabel "Average"
        |> Chart.WithOptions (Options(lineWidth=5))

    let combinedSeries =
        seq { yield! walks; yield (average |> Array.ofList) }
        |> Seq.map Array.indexed
        |> List.ofSeq
    
    let isAverageIndicator = [for i in 1..N+1 -> (i > N)]
    let widths = [for isAvg in isAverageIndicator -> if isAvg then 5 else 1]
    let options = Options(series=[|for w in widths -> Series(lineWidth=w)|])

    combinedSeries
    |> Chart.Combo
    |> Chart.WithOptions options
    |> Chart.WithTitle "Random Walks"


plotWalks 10;;



