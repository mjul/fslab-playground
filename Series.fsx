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

let DAYS = 30
let dates = [for x in 0..DAYS -> System.DateTime(2017,1,1).AddDays(float x)]
let randomWalk =
    Random.Random.doubleSeq()
    |> Seq.scan (+) 100.0 
    |> Seq.take DAYS

let daily = Seq.zip dates randomWalk |> series
let firstFriday = System.DateTime(2017,1,6)
let nextDay dt =
    firstFriday.AddDays(1.0)
let firstSaturday = nextDay firstFriday

(*
   Sets the cut-off to start a new interval every Saturday.
   Since we use Lookup.Smaller this means that the weekly observation is up to and including the Fridays.
   The last value of the previous week is registered on the cut-off day (Saturday)
*)

let weeklyUpToAndIncludingFriday =
    daily |> Series.lookupTimeAt firstSaturday (TimeSpan.FromDays(7.0)) Direction.Backward Lookup.Smaller


// Show the data side-by-side
frame ["Daily" => daily; "Weekly" => weeklyUpToAndIncludingFriday]

