module Database

open FSharp.Data
open FSharp.Data.Npgsql

[<Literal>]
let Example = "Host=localhost;Username=postgres;Database=ocean_development;Port=5432"

type ExampleConn = NpgsqlConnection<Example>

let getFromUsers() =
    use cmd = ExampleConn.CreateCommand<"SELECT email, phone, uuid, id FROM users"> (Example)
    for x in cmd.Execute() do
        printfn "email : %s %s" x.email x.phone.Value
