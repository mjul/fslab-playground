//
// Note: to use the R Type Provider you need to set up a config file
// in ~/.rprovider.conf with the paths to mono and R.
//
// For more information refer to:
// http://bluemountaincapital.github.io/FSharpRProvider/mac-and-linux.html
//
// HOWEVER, it does not quite seem to work for me.
//
// Instead, we use a workaround to set the environment variables in the script:

System.Environment.SetEnvironmentVariable("R_HOME",
                                          "/usr/local/Cellar/r/3.3.1_2/R.framework/Resources")
System.Environment.SetEnvironmentVariable("MONO",
                                          "/usr/local/bin/mono")


#load "packages/FsLab/Themes/DefaultWhite.fsx"
#load "packages/FsLab/FsLab.fsx"
#load "packages/RProvider/RProvider.fsx"

open System
open Deedle
open FSharp.Data

open RProvider
open RProvider.graphics
open RProvider.grDevices
open RProvider.datasets


let xs = [1..10];;
let avg = R.mean(xs)

printfn "xs = %A, average=%f" xs (avg.GetValue<float>())
