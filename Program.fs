open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling.Operator.Validation
open System

type Product =
    { name: string
      price: decimal
      validUntil: DateOnly }

type Order = { id: Guid; products: Product list }

type Error =
    { errors: string list
      field: string
      childErrors: Error list }

let createProduct name price validUntil =
    { name = name
      price = price
      validUntil = validUntil }

let createOrder id products = { id = id; products = products }


let const' x _ = x

let tryCreateGuid (id: string) : Result<Guid, string> =
    match Guid.TryParse(id) with
    | (true, guid) -> Ok guid
    | _ -> Error "Invalid Guid"

let createOrderId (id: string) =
    tryCreateGuid id
    |> Validation.ofResult
    |> Validation.mapError (fun x ->
        { errors = [ x ]
          field = "id"
          childErrors = [] })

let createName name =
    name
    |> String.IsNullOrWhiteSpace
    |> Result.requireFalse "Name cannot be empty"
    |> Result.map (const' name)
    |> Validation.ofResult
    |> Validation.mapError (fun x ->
        { errors = [ x ]
          field = "name"
          childErrors = [] })


let createPrice price =
    price >= 0m
    |> Result.requireTrue "Price cannot be negative"
    |> Result.map (const' price)
    |> Validation.ofResult
    |> Validation.mapError (fun x ->
        { errors = [ x ]
          field = "price"
          childErrors = [] })

let createValidUntil validUntil =
    validUntil >= DateOnly.FromDateTime(DateTime.UtcNow)
    |> Result.requireTrue "Valid until cannot be in the past"
    |> Result.map (const' validUntil)
    |> Validation.ofResult
    |> Validation.mapError (fun x ->
        { errors = [ x ]
          field = "validUntil"
          childErrors = [] })


//validating with functions
let productsResult =

    createName "" |> Validation.map createProduct |> Validation.apply
    <| createPrice -20m
    |> Validation.apply
    <| createValidUntil (DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1.0)))
    |> Validation.map List.singleton


let orderResult =
    createOrderId "123e4567-e89b-12d3-a456-"
    |> Validation.map createOrder
    |> Validation.apply
    <| (productsResult
        |> Validation.mapErrors (fun x ->
            [ { errors = [ "Invalid product" ]
                field = "products"
                childErrors = x } ]))


//validating with operators
let productsResult2 =
    createProduct 
        <!> createName ""
        <*> createPrice -20m
        <*> createValidUntil (DateTime.UtcNow |> _.AddDays(-1) |> DateOnly.FromDateTime)
    |> (<!>) List.singleton

let orderResult2 =
    createOrder <!> createOrderId "123e4567-e89b-12d3-a456-"
    <*> (productsResult2
         |> Validation.mapErrors (fun x ->
             [ { errors = [ "Invalid product" ]
                 field = "products"
                 childErrors = x } ]))


//validating with computation expressions
let productsResult3 =
    validation {
        let! name = createName ""
        and! price = createPrice -20m
        and! validUntil = createValidUntil (DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1.0)))
        return createProduct name price validUntil
    }


let orderResult3 =
    validation {
        let! id = createOrderId "123e4567-e89b-12d3-a456-"

        and! products =
            productsResult3
            |> Validation.mapErrors (fun x ->
                [ { errors = [ "Invalid product" ]
                    field = "products"
                    childErrors = x } ])

        return products |> List.singleton |> createOrder id
    }
