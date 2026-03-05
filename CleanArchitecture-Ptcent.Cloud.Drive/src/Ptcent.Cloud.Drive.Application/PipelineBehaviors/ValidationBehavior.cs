using FluentValidation;
using MediatR;
using Ptcent.Cloud.Drive.Application.Contracts.Responses;

namespace Ptcent.Cloud.Drive.Application.PipelineBehaviors
{
    /// <summary>
    /// 验证行为管道
    /// </summary>
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
            if (_validators.Any())
            {
                var context = new ValidationContext<TRequest>(request);

                var validationResults = await Task.WhenAll(
                    _validators.Select(v => v.ValidateAsync(context, cancellationToken))
                );

                var failures = validationResults
                    .SelectMany(r => r.Errors)
                    .Where(f => f != null)
                    .ToList();

                if (failures.Any())
                {
                    // 如果响应类型是 ResponseMessageDto，直接返回错误信息
                    if (typeof(TResponse).IsGenericType &&
                        typeof(TResponse).GetGenericTypeDefinition() == typeof(ResponseMessageDto<>))
                    {
                        var response = Activator.CreateInstance<TResponse>();
                        var responseType = typeof(TResponse);

                        responseType.GetProperty(nameof(ResponseMessageDto<bool>.Code))?.SetValue(response, WebApiResultCode.ValidationError);
                        responseType.GetProperty(nameof(ResponseMessageDto<bool>.IsSuccess))?.SetValue(response, false);
                        responseType.GetProperty(nameof(ResponseMessageDto<bool>.Message))?.SetValue(response, failures.First().ErrorMessage);

                        return response;
                    }

                    throw new ValidationException(failures);
                }
            }

            return await next();
        }
    }
}
