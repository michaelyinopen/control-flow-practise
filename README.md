# control-flow-practise

## Goal

To practise control flow and error handling in an ASP.NET Core project.

The main operation:

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

There could be errors in each step, that terminate the operation, or change the execution of subsequent steps.

The challenge is to implement this somewhat complex control flow and to handle the errors gracefully.

## Story

This project features an imaginary story of machine's warranty. In a machine transaction, a warranty is purchased together with the machine. The buyer buys the machine and the warranty from the seller, but the warranty is issued by the manufacturer directly.

This project is how the seller communicates with the manufacturer about warranty information.

[More info on the Story](https://github.com/michaelyinopen/control-flow-practise/wiki/Story).

## How to run locally
### Prerequisite
- Visual Studio 2019(+) installed with IIS Express and localDb

Steps

1. Clone repository
2. Open ControlFlowPractise.sln
3. Set ControlFlowPractise.Api as startup project and hit F5 (Debug using IIS express)

You can make requests to the url `http://localhost:54208`.

### Example requests with Postman
Import `Postman\control-flow-practise.postman_collection.json` in postman, and send the sample requests.\
All requests will respond with `status: 200` but have content `"isSuccess": false` because the external party service is not implemented.

## How to run tests

In visual studio, view Test Explorer. Build the solution, then run all tests.

## Databases
Connection strings are in appsettings.json `ConnectionStrings` section.

The Api project will create or apply migrations at runtime. If there is an alternative database migration strategy, remove related code in Startup.cs Configure method.

### Database Commands
1. In Visual Studio set Api as startup project
2. In Package Manager Console, Set the target Data project as default project

```
Add-Migration <name> -Context ComprehensiveDataDbContext
```

```
Remove-Migration -Context ComprehensiveDataDbContext
```

```
Update-Database -Context ComprehensiveDataDbContext
```

## Wiki

[Check out the wiki for details and design decisions](https://github.com/michaelyinopen/control-flow-practise/wiki).

## Usage Specification
Directly read [[Usage Specification]] for how to consume the service.
