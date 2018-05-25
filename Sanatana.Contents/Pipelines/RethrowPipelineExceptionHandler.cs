using Sanatana.Patterns.Pipelines;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.Contents.Pipelines
{
    public class RethrowPipelineExceptionHandler : IPipelineExceptionHandler
    {
        public void Handle<TInput, TOutput>(PipelineContext<TInput, TOutput> context)
        {
            if(context.Exceptions != null
                && context.Exceptions.Count > 0)
            {
                throw new PipelineException<TInput>("Pipeline execution was interrupted by an exception."
                    , context.Exceptions, context.Input);
            }
        }
    }
}
