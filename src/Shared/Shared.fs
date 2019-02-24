namespace Shared

type Score =
    | Good
    | SoSo
    | Poor

type Vote =
    { Comment : string
      Name : string
      Score : Score }

type VotingResults =
    { Comments : (string * string) []
      Scores : Map<Score, int> }

type Counter = { Value : int }

module Route =
    /// Defines how routes are generated on server and mapped from client
    let builder typeName methodName =
        sprintf "/api/%s/%s" typeName methodName

/// A type that specifies the communication protocol between client and server
/// to learn more, read the docs at https://zaid-ajaj.github.io/Fable.Remoting/src/basics.html
type IVotingApi =
    { vote : Vote -> Async<VotingResults> }
