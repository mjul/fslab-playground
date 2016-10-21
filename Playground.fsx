#load "packages/FsLab/Themes/DefaultWhite.fsx"
#load "packages/FsLab/FsLab.fsx"

open System
open Deedle
open FSharp.Data
open XPlot.GoogleCharts
open XPlot.GoogleCharts.Deedle
open RProvider

let octoberFirst = new DateTime(2016,1,1)
let octoberDays = [for i in 1..31 -> octoberFirst.AddDays(float (i-1)]
                   
let isWorkingDay (date:DateTime) =
  let weekday = date.DayOfWeek
  weekday <> DayOfWeek.Saturday && weekday <> DayOfWeek.Sunday

let workingDays = octoberDays |> List.filter isWorkingDay

// Increasing value every date
let ts = List.zip workingDays [for v in 1..(Seq.length workingDays) -> float v] |> series
let noWednesdays = ts |> Series.filter (fun d v -> d.DayOfWeek <> DayOfWeek.Wednesday)
    
// Reintroduce the missing Wednesdays keys, but the series has a 'missing' value for these
let missingWednesdays = noWednesdays |> Series.realign octoberDays


// Fill a missing value with the midpoint between the values before and after
let midpointFill (key:'k) (before:('k*float) option) (after:('k*float) option) =
    match before, after with
    | Some (k1,v1), Some (k2,v2) -> (v1 + v2)/2.0

let interpolated = missingWednesdays |> Stats.interpolate octoberDays midpointFill 




