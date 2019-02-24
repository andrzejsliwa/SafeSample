#I "/Users/andrzejsliwa/.nuget/packages/fsharp.data.npgsql/0.1.44-beta/typeproviders/fsharp41/netcoreapp2.0"
#r "Microsoft.Extensions.FileProviders.Physical.dll"
#r "FSharp.Data.Npgsql.DesignTime.dll"
#r "Npgsql.dll"

#I "/Users/andrzejsliwa/.nuget/packages/fsharp.data.npgsql/0.1.44-beta/lib/netstandard2.0/"
#r "FSharp.Data.Npgsql.dll"

#load "Database.fs"
open Database
getFromUsers()
