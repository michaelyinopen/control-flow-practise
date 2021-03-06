﻿using System;
using System.Collections.Generic;
using System.Text;

namespace ControlFlowPractise.Common.ControlFlow
{
    public class Result<T, TFailure>
        where T : class
        where TFailure : class
    {
        public bool IsSuccess { get; }
        public T? Success { get; }
        public TFailure? Failure { get; }

        private Result()
        {
            IsSuccess = true;
        }

        public Result(T success)
        {
            IsSuccess = true;
            Success = success;
        }

        public Result(TFailure failure)
        {
            IsSuccess = false;
            Failure = failure;
        }

        public static Result<T, TFailure> SuccessNull()
        {
            return new Result<T, TFailure>();
        }
    }
}
