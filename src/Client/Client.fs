module Client

open Elmish
open Elmish.React
open Fable.Helpers.React
open Fable.Helpers.React.Props
open Fable.PowerPack.Fetch
open Fable.FontAwesome
open Fable.Core.JsInterop
open Thoth.Json
open Shared
open Fulma

// The model holds data that you want to keep track of while the application is running
// in this case, we are keeping track of a counter
// we mark it as optional, because initially it will not be available from the client
// the initial value will be requested from server
type Model =
    { Comment : string
      Name : string
      Score : Score option
      isLoading : bool
      Results : VotingResults option }

// The Msg type defines what events/actions can occur while the application is running
// the state of the application changes *only* in reaction to these events
type Msg =
    | SetComment of string
    | SetName of string
    | SetScore of Score
    | Submit
    | GotResults of Result<VotingResults, exn>

module Server =
    open Shared
    open Fable.Remoting.Client

    /// A proxy you can use to talk to server directly
    let api : IVotingApi =
        Remoting.createApi()
        |> Remoting.withRouteBuilder Route.builder
        |> Remoting.buildProxy<IVotingApi>

// defines the initial state and initial command (= side-effect) of the application
let init() : Model * Cmd<Msg> =
    let initialModel =
        { Comment = ""
          Name = ""
          Score = None
          isLoading = false
          Results = None }

    let loadCountCmd = Cmd.none
    (initialModel, loadCountCmd)

let makeVote (model : Model) : Vote =
    { Comment = model.Comment
      Name = model.Name
      Score = defaultArg model.Score Good }

// The update function computes the next state of the application based on the current state and the incoming events/messages
// It can also run side-effects (encoded as commands) like calling the server via Http.
// these commands in turn, can dispatch messages to which the update function will react.
let update msg (currentModel : Model) : Model * Cmd<Msg> =
    match msg with
    | SetComment comment ->
        let nextModel = { currentModel with Comment = comment }
        (nextModel, Cmd.none)
    | SetName name ->
        let nextModel = { currentModel with Name = name }
        (nextModel, Cmd.none)
    | SetScore score ->
        let nextModel = { currentModel with Score = Some score }
        (nextModel, Cmd.none)
    | Submit ->
        let nextModel = { currentModel with isLoading = true }
        let cmd =
            Cmd.ofAsync Server.api.vote (makeVote nextModel) (Ok >> GotResults)
                (Error >> GotResults)
        (nextModel, cmd)
    | GotResults(Ok results) ->
        let nextModel =
            { currentModel with isLoading = false
                                Results = Some results }
        (nextModel, Cmd.none)
    | GotResults _ ->
        let nextModel = { currentModel with isLoading = false }
        (nextModel, Cmd.none)

let button txt onClick =
    Button.button [ Button.IsFullWidth
                    Button.Color IsPrimary
                    Button.OnClick onClick ] [ str txt ]

let imgSrc =
    "https://crossweb.pl/upload/gallery/cycles/11255/300x300/lambda_days.png"
let field input = Field.div [] [ Field.body [] [ input ] ]

let iconAndColor score =
    match score with
    | Good -> (Fa.Solid.Smile, IsSuccess)
    | SoSo -> (Fa.Solid.Meh, IsWarning)
    | Poor -> (Fa.Solid.Frown, IsDanger)

let handleSelectedScore (model : Model) score =
    match (model.Score, iconAndColor score) with
    | (None, (icon, _)) -> (icon, IsWhite)
    | (Some s, (icon, _)) when s <> score -> (icon, IsWhite)
    | (_, other) -> other

let scores (model : Model) dispatch =
    let item score =
        let (scoreIcon, scoreColor) = handleSelectedScore model score
        Level.item []
            [ Button.a [ Button.Color scoreColor
                         Button.OnClick(fun _ -> dispatch (SetScore score)) ]
                  [ Fa.i [ scoreIcon
                           Fa.Size Fa.Fa2x ] [] ] ]
    Level.level [ Level.Level.IsMobile ] [ item Good
                                           item SoSo
                                           item Poor ]

let comment (model : Model) dispatch =
    Textarea.textarea
        [ Textarea.Placeholder "Comment"
          Textarea.DefaultValue model.Comment
          Textarea.OnChange(fun ev -> dispatch (SetComment ev.Value)) ] []

let name (model : Model) dispatch =
    Input.text [ Input.Placeholder "Name"
                 Input.DefaultValue model.Name
                 Input.OnChange(fun ev -> dispatch (SetName ev.Value)) ]

let submit (model : Model) dispatch =
    Button.a [ Button.IsFullWidth
               Button.Color IsPrimary
               Button.OnClick(fun _ -> dispatch Submit)
               Button.IsLoading model.isLoading ] [ str "Save" ]

let formBox model dispatch =
    Box.box' [] [ field (scores model dispatch)
                  field (comment model dispatch)
                  field (name model dispatch)
                  field (submit model dispatch) ]

let resultsBox (results : VotingResults) =
    let item score =
        let count = defaultArg (Map.tryFind score results.Scores) 0
        let (icon, _) = iconAndColor (score)
        Level.item []
            [ div []
                  [ Fa.i [ icon
                           Fa.Size Fa.Fa2x ] [ h2 [] [ str (string count) ] ] ] ]
    Box.box' []
        [ Level.level [ Level.Level.IsMobile ] [ item Good
                                                 item SoSo
                                                 item Poor ]

          Content.content [ Content.Size IsSmall ]
              [ ul [] [ for (name, comment) in results.Comments ->
                            li [] [ i [] [ str (sprintf "'%s'" comment)
                                           str (sprintf " - %s" name) ] ] ] ] ]

let containerBox model dispatch =
    match model.Results with
    | Some results -> resultsBox results
    | None -> formBox model dispatch

let view (model : Model) (dispatch : Msg -> unit) =
    div []
        [ Navbar.navbar [ Navbar.Color IsPrimary ]
              [ Navbar.Item.div [] [ Heading.h2 [] [ str "SAFE Template" ] ] ]

          Container.container []
              [ Columns.columns []
                    [ Column.column [ Column.Width(Screen.All, Column.Is6)
                                      Column.Offset(Screen.All, Column.Is3) ]
                          [ Level.level []
                                [ Level.item []
                                      [ Image.image [ Image.Is64x64 ]
                                            [ img [ Src imgSrc ] ] ] ]
                            containerBox model dispatch ] ] ] ]
#if DEBUG
open Elmish.Debug
open Elmish.HMR
#endif


Program.mkProgram init update view
#if DEBUG
|> Program.withConsoleTrace
|> Program.withHMR
#endif


|> Program.withReact "elmish-app"
#if DEBUG
|> Program.withDebugger
#endif



|> Program.run
