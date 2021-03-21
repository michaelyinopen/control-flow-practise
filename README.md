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

[More info on Story](https://github.com/michaelyinopen/control-flow-practise/wiki/Story).

// how to run or test

// database migration/ ef core

// to see design and implementation detail, goto the wiki page

// move all to wiki/////////////////////////

---

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
conformance indicator = conformance messages, level: error, warning, information

Warranties the entire order, not individual products

WarrantyProof returned in response of commit

## Error handling
How to handle error at each step

- Service Error (Proxy throws exception)
- Response header errors
- Conformance NO, with ConformanceMessage of Level Error

Failure after called external party will have requestId

**ConformanceIndicator and CaseStatus always refers to the WarrantyCase, never refers to the latest request.**

Whether of not a request is successful, depends on the updated CaseStatus of response.

E.g. 1 updating order to some wrong information, will change complianceIndicator to `NO`, CaseStatus will revert to `Claimed` (Success)

E.g. 2 attempt to Commit with wrong date, will respond with complianceIndicator `YES` and CaseStatus remains at `Certified`. However, CaseStatus does not progress to Committed.

Each action will have its expected response, if unsatisfied, means the http request overall is a failure.

Success Condition
| Operation  | Expected Indicator | Expected CaseStatus | Expected |
|------------|--------------------|---------------------|----------|
| Create | - | - | WarrantyCaseId generated (Validation) |
| Verify | - | - | -
| Verify before Commit | Yes (Validation) | Certified | WarrantyAmount not null (Validation)
| Commit | Yes (Validation) | Committed (or Completed) | WarrantyAmount not null, have WarrantyProof (Validation)
| Cancel | No | Cancelled

Expected CaseStatus is nnot checked by validation, it is checked by successful condition.

Conformance messages can have either meanings
1. Could have subject of the latest request; or
2. Or subject of the case status

Always showing all messages, ~~could show the messages specifically for the latest request? No need.~~

## From http request's perspective
What is a successful response mean\
What is the response in case of error

## How else is data used?
Endpoints to read data

## Special Scenarios
1. Use old response if failed
2. Call twice, verify before commit
3. Different request types
4. Compliance/Case Status
5. Operation
6. commit sends more info and saves additional info separately
7. Daily error report


# Design


## Syntax
use nullable reference type and have constructors to initialize non-nullable properties, because cannot use C# 9 features like record type

## Functional Programming
Just use `Result` and `Unit`. Yes, the maps, binds are missing, and code is imperative. But it has not become a second nature for me to write those, so i go with the easy way, while leveraging the clarity that `Result` provides.

Violating some core principles of Functional Programming, because this way is easy(familiar).

Result.Failure can carry more information, e.g. SuccessfulConditionFailure has WarrantyCaseResponse. 

## Success Condition
There could be cases where successful response, but expected `Indicator` and `CaseStatus` does not meet the condition for success, therefore overall is a failure. Need a way to represent this. (Failure with VerifyWarrantyCaseResponse as a property?)

Http response structure
```
{
  Success: bool
  // can have value even if there is failure
  // If there is saved WarrantyCaseVerification without failure,
  // there will be a response
  // e.g. there will be response if
  // - PreCommitVerify saved but failed successful condition
  // - Commit saved but failed successful condition
  Respose?: VerifyWarrantyCaseResponse
  Failure?: something,
}
```

Non-conformance is a success, except when violating success condition

## Data storage
Models converted to Data Model inside the Data project.

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

WarrantyProof also log style.\
Steps to get warranty proof

1. Get WarrantyCaseVerification of last commit
2. Verify that there are no cancels after that last commit
3. Get wararanty proof from RequestId

### Test uses localDb
need to be able to create localDb for the tests

### Tests in xunit projects
The most important is to test external effects: response and External Party.

Test with localDb. Pre-fill database with test case data. Then assert the state of database after the test action completed.
<details>
  <summary>Why not test with only external effects</summary>

It is difficult to test each function in isolation, only by inspecting external effects.
- e.g. to test Verify only, need to make subsequent Gets, to make assertions.
- e.g. need to make several Verify calls to setup the condition, before the actual test action.
</details>

### How to Add Database Migrate
1. In Visual Studio PAckage MAnager Console, set Api as startup project
2. Set the Data project as Default project
3. `Add-Migration <name> -Context ComprehensiveDataDbContext`
4. how to apply migration??

### Remove migration
`Remove-Migration -Context ComprehensiveDataDbContext`

## Test with Postman
Import `control-flow-practise\Postman\control-flow-practise.postman_collection.json` to postman, launch Api and make requests.

## Pre-commit validation
For one VerifyWarrantyCaseRequest request with operation commit, this service will make a verify call to external party first, to ensure the response case status is Certified.

This pre-commit validation and commit will have the same content (except requestType).

If the pre-commit validation has failure, or the response case status is not Certified, this service will not make the commit call, save WarrantyCaseVerification with VerifyBeforeCommitFailure and return isSuccess:false.

The pre-commit validation is saved as WarrantyCaseVerification in Comprehensive database, and has request and response saved in Budget database.

# Useful
> With direct equalities on each of the columns in the key of the index, we can sort by the last column in the index.
>
> Unfortunately, this doesn’t work if we…
>
> Have an incomplete WHERE clause
> Our WHERE clause has inequalities (ranges) in it
> By incomplete WHERE clause, I mean one that doesn’t utilize all of our index key columns, and by inequalities I mean >, >=, <, <=, and <>.

https://www.brentozar.com/archive/2018/04/index-key-column-order-and-supporting-sorts/

# todo

- Review readme and cross check with Wiki (Story is different)