# control-flow-practise

## Objective

1. Accept http request
2. Validate request
3. Convert request
4. Save request
5. Call some external party service
6. Save raw response
7. Interpret response
    1. Validate response
    2. Convert response
8. Save converted response
9. Return http response
10. (Reconcille service)
11. daily report?

## Error handling
How to handle error at each step

- Service Error (Proxy throws exception)
- Response header errors
- Conformance NO, with ConformanceMessage of Level Error

## From http request's perspective
What is a successful response mean\
What is the response in case of error

## How else is data used?
Endpoints to read data

## Special Scenarios
1. Use old response if failed
2. Call twice, validate before submit
3. Different request types
4. Compliance/Case Status
5. Original Request Type
6. commit sends more infor and saves additional info separately

## Story
Buing a machine, ask for the price of the warranty\
Warranty service of the manufacturer needs a record of some details

- Order
- Product(s) in the order
- Buyer
- Vendor
- Transaction date time
- Warranty amount ($)

All unique products, now a warranty case only has one order,\
and each order only has one product

### Warranty service
- Create warranty case
- Verify/ Ask for estimate/ Check for update/ Query/ Call/ Make updates
  - response status
    - waiting for claim
    - claimed (can have estimation)
    - certified (ready to commit) (must have final quote)
    - committed (committed to pay)
    - completed (everything after payment is also done)
    - cancelled
- Commit (money is reserved, committed to pay)
- Cancel

Header, body
conformance indicator = conformance messages, level error warning information

Warranties the entire order, not individual products

# Design

## Project structure
Separate project with considerations of

1. Build
2. Deployment
3. Logical
4. Test

E.g. Data will be separated into a different project, because it handles a concern that other parts of solution do not care. Also separately testable.

ControlFlowPractise
* Api
* Common
* Core
* Data
* ExternalParty (has proxy to act as the substitute)

Not have a Client project, to avoid coupling bwtween this service and consumer,\
and cannot avoid to manually track what models are exposed and what was updated

Models converted to Data Model inside Data project

## Syntax
use nullable reference type and have constructors to initialize non-nullable properties, because cannot use C# 9 features like record type

## Functional Programming
Just use `Result` and some version of `Unit`. Yes the maps, binds are missing, and code is imperitive. But it has not become second nature for me to write those, so i go with the easy way, while leveraging the clarity that `Result` provides.

## Data storage
Each accepted request has an entry, regardless of successful or fail.\
Each raw request is an entry.\
Each raw response is an entry.\
WarrantyProof is separated.

Two databases: budget and comprehensive.

Have dateTime column.\
Log style, keep all accepted requests.\
Need a head table? to record the latest correct result? No.\
Saving raw request and raw response are write only operations, and will not be concerrency blocked.\
Raw request and raw resposne can be correlated by requestId.\
Concurrency will be last write wins.

### Test uses localDb
need to be able to create localDb for the tests

### How to Add Database Migrate
1. In Visual Studio PAckage MAnager Console, set Api as startup project
2. Set the Data project as Default project
3. Add-Migration \<name>
4. how to apply migration??

# todo

add index to databases
naming warranty request/ esternal party request

