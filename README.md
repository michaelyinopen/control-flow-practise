# control-flow-practise

## Objective

1. Handle http request
2. Validate request
3. Convert request
4. Call some external party service
5. Interpret response
    1. Validate response
    2. Convert response
6. Save raw and converted response
7. Return http response

## Error handling
How to handle error at each step

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

# Design

## Project structure
Separate project with considerations of

1. Build
2. Logical
3. Test

E.g. Data will be separated into a different project, because it handles a concern that other parts of solution do not care. Also separately testable.

ControlFlowPractise
* Api
* Data
* Service/Core
* Common
* ExternalParty (has proxy to act as the substitute)

Not have a Client project, to avoid coupling bwtween this service and consumer,\
and cannot avoid to manually track what models are exposed and what was updated

Models converted to Data Model inside Data project
