using Sanatana.Patterns.Pipelines;
using System;
using System.Collections.Generic;

namespace Sanatana.Contents.Pipelines
{
    public interface IPipelineExceptionHandler
    {
        void Handle<TInput, TOutput>(PipelineContext<TInput, TOutput> context);
    }
}