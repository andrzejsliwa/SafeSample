open System.IO
open System.Threading.Tasks

open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open FSharp.Control.Tasks.V2
open Giraffe
open Saturn
open Shared

open Fable.Remoting.Server
open Fable.Remoting.Giraffe

let votes = System.Collections.Concurrent.ConcurrentBag<Vote>()

let countVotes() : VotingResults =
    let vs =
        votes
        |> Seq.toArray
        |> Array.filter (fun v -> v.Name <> "" && v.Comment <> "")
    let comments =
        vs
        |> Array.map (fun v -> (v.Name, v.Comment))
    let scores =
        vs
        |> Array.countBy (fun v -> v.Score)
        |> Map.ofArray
    { Comments = comments
      Scores = scores }

let vote (v : Vote) : Async<VotingResults> =
    async {
        do votes.Add v
        do! Async.Sleep 1000
        return countVotes()
    }

let tryGetEnv =
    System.Environment.GetEnvironmentVariable >>
    function
  | null
  | "" -> None
  | x -> Some x

let publicPath = Path.GetFullPath "../Client/public"

let port = "SERVER_PORT" |> tryGetEnv |> Option.map uint16 |> Option.defaultValue 8085us

let getInitCounter() : Task<Counter> = task { return { Value = 42 } }

let votingApi = {
    vote = vote
 }

let webApp =
    Remoting.createApi()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.fromValue votingApi
    |> Remoting.buildHttpHandler

let app = application {
    url ("http://0.0.0.0:" + port.ToString() + "/")
    use_router webApp
    memory_cache
    use_static publicPath
    use_gzip
 }

run app
