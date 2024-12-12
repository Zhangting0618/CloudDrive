using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Ptcent.Cloud.Drive.Application.PipeLineBehavior
{
    public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;
        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var context = new ValidationContext<TRequest>(request);
            var failures = _validators
                .Select(v => v.Validate(context))
                .SelectMany(result => result.Errors)
                .Where(f => f != null)
                .ToList();

            if (failures.Count != 0)
            {

                string result = string.Join(",", failures.Select(c => c.ErrorMessage));
                var data = (TResponse)Activator.CreateInstance(typeof(TResponse));
                Type examType = typeof(TResponse);
                 PropertyInfo code = examType.GetProperty("Code");
                code.SetValue(data, 422);
                PropertyInfo message = examType.GetProperty("Message");
                message.SetValue(data, failures.Select(c => c.ErrorMessage).FirstOrDefault());
                return await Task.FromResult(data);
            }
            return await next();
        }
    }
}
