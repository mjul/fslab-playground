#load "packages/FsLab/Themes/DefaultWhite.fsx"
#load "packages/FsLab/FsLab.fsx"

#r "MathNet.Numerics.dll"

open System
open System.IO
open Deedle
open FSharp.Data
open XPlot.GoogleCharts
open XPlot.GoogleCharts.Deedle
open MathNet.Numerics


(* Data Sources *)
let DATADIR = Path.Combine(__SOURCE_DIRECTORY__, "data")
let STOCKSFILE = Path.Combine(DATADIR, "single-stock.csv")


[<Literal>]
let TEMPLATE = __SOURCE_DIRECTORY__ + "/" + "stocks-template.csv"
type StocksCsv = CsvProvider<TEMPLATE, ";">

let stockData = StocksCsv.Load(STOCKSFILE)

let valueSeries =
    [for row in stockData.Rows -> row.Date, (float row.Value)]
    |> series


(* Let's have a look at the stock data *)
let lineChart title ts =
    ts
    |> Series.observations
    |> Chart.Line
    |> Chart.WithTitle title
    
let chart = valueSeries |> lineChart "Stock Price"


let absoluteReturns (ts : Series<DateTime,float>)  =
    // The return since the previous observation
    // E.g. comparing today's Close to yesterday's Close 
    ts |> Series.diff 1


let relativeReturns ts =
    let previousDayValues = ts |> Series.shift 1
    Series.zipInner previousDayValues (absoluteReturns ts)
    |> Series.mapValues (fun (v,r) -> r/v)



let absReturnsChart =
    absoluteReturns valueSeries |> lineChart "Absolute Daily Returns" 

let relReturnsChart =
    relativeReturns valueSeries |> lineChart  "Relative Daily Returns"


let durationInYears (ts:Series<DateTime,'v>) =
    let a = ts |> Series.firstKey
    let b = ts |> Series.lastKey
    let duration = b.Subtract(a) + TimeSpan.FromDays(1.0)
    // use standardised year length
    duration.TotalDays/365.0


let annualizeDailyVariance (daysPerYear:float) dailySigma =
    let sigmaFactor = sqrt daysPerYear
    sigmaFactor * dailySigma
    

let annualizedVariance (ts:Series<DateTime, float>) =
    let tradingDays = ts |> Series.countValues
    let tradingDaysPerYear = float(tradingDays) / (durationInYears ts)
    ts
    |> Stats.variance
    |> annualizeDailyVariance tradingDaysPerYear
    

let annualisedStandardDeviation ts =
    ts
    |> annualizedVariance
    |> sqrt



let lastNYears n (ts : Series<DateTime,float>) =
    let lastDay = ts |> Series.lastKey
    let firstDay = lastDay.AddYears(-n).AddDays(1.0)
    ts |> Series.filter (fun date value -> (date >= firstDay))
    
let lastThreeYears ts =
    lastNYears 3 ts


let normalizeSeries ts =
    let baseLine = ts |> Series.firstValue
    ts |> Series.mapValues (fun (x:float) -> x/baseLine)


let threeYearValues = valueSeries |> lastThreeYears |> normalizeSeries
let threeYearReturns = threeYearValues |> absoluteReturns
let threeYearRelativeReturns = threeYearValues |> relativeReturns
    
(* Chart some data *)

let c3 = threeYearValues |> lineChart "Normalized Values"
let c3dr = threeYearReturns |> lineChart "Normalized Values, Daily Returns"
let c3relret = threeYearRelativeReturns |> lineChart "Normalized Values, Relative Daily Returns"

let c3dist =
    threeYearReturns |> Series.values |> Seq.map (fun x -> ("Group-A", x))
    |> Chart.Histogram
    |> Chart.WithTitle "Daily Returns distribution"

let c3reldist =
    threeYearRelativeReturns |> Series.values |> Seq.map (fun x -> ("Group-A", x))
    |> Chart.Histogram
    |> Chart.WithTitle "Daily Relative Returns distribution"

(* Get the statistics *)

let absStd = threeYearReturns |> annualisedStandardDeviation 
let relStd = threeYearRelativeReturns |> annualisedStandardDeviation
printfn "Standard deviation:  (absolute): %.3f p.a.    (relative): %.3f%% p.a." absStd (100.0*relStd)



let firstDayOfMonth (dt:DateTime) =
    new DateTime(dt.Year, dt.Month, 1)

let lastDayOfMonth (dt:DateTime) =
    (new DateTime(dt.Year, dt.Month, 1)).AddMonths(1).AddDays(-1.0)

let resampleDates dateBucketFn ts =
    ts
    |> Series.keys
    |> Seq.map dateBucketFn
    |> Set.ofSeq
    |> Seq.sort
    
// Months are represented by first day of month
let firstDayOfMonthValues ts =
    let firstDayOfMonths = resampleDates firstDayOfMonth ts
    ts
    |> Series.resample firstDayOfMonths Direction.Forward
    |> Series.mapValues Series.firstValue 

// Months are represented by the first day of the month
let lastDayOfMonthValues ts =
    let firstDayOfMonths = resampleDates firstDayOfMonth ts
    ts
    |> Series.resample firstDayOfMonths Direction.Forward
    |> Series.mapValues Series.lastValue 


let geometricMean (xs:float seq) =
    let sumOfLogs = xs |> Seq.map (fun x -> abs (log x)) |> Seq.sum
    let n = Seq.length xs
    exp (sumOfLogs/(float n))


// We need 37 months of prices to get 36 months of returns
let last37Months = valueSeries |> lastDayOfMonthValues |> Series.takeLast (36+1)
let lastThreeYearRelativeReturns = last37Months |> relativeReturns
let lastThreeYearRelativeReturn =
    let first = last37Months |> Series.firstValue
    let last = last37Months |> Series.lastValue
    last/first

let lastThreeYearGeometricMeanMonthlyReturn =
    Math.Pow(lastThreeYearRelativeReturn, 1.0/36.0) - 1.0

let sigmaMonthly = lastThreeYearRelativeReturns |> Stats.stdDev
let sigmaAnnual = (sqrt 12.0) * sigmaMonthly

let stdDevGeomMonthly =
    lastThreeYearRelativeReturns
    |> Series.values
    |> Seq.sumBy (fun x -> pown (x-lastThreeYearGeometricMeanMonthlyReturn) 2)
    |> sqrt
    |> sqrt
    
let stdDevGeomAnnual =  stdDevGeomMonthly / (sqrt 12.0)
