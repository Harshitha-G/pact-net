﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Nancy;
using Nancy.Routing;
using PactNet.Mappers;
using PactNet.Validators;

namespace PactNet.Consumer.Mocks.MockService
{
    public class PactNancyRequestDispatcher : IRequestDispatcher
    {
        private static PactProviderServiceRequest _request;
        private static PactProviderServiceResponse _response;
        private readonly IPactProviderServiceRequestValidator _requestValidator;

        public PactNancyRequestDispatcher(IPactProviderServiceRequestValidator requestValidator)
        {
            _requestValidator = requestValidator;
        }

        public Task<Response> Dispatch(NancyContext context, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var tcs = new TaskCompletionSource<Response>();

            try
            {
                var response = HandleRequest(context.Request);
                context.Response = response;
                tcs.SetResult(context.Response);
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }

            return tcs.Task;
        }

        public static void Set(PactProviderServiceRequest request)
        {
            _request = request;
        }

        public static void Set(PactProviderServiceResponse response)
        {
            _response = response;
        }

        private Response HandleRequest(Request request)
        {
            _requestValidator.Validate(_request, new PactProviderServiceRequestMapper().Convert(request));
            return GenerateResponse();
        }

        private Response GenerateResponse()
        {
            var mapper = new NancyResponseMapper();
            return mapper.Convert(_response);
        }

        public static void Reset()
        {
            _request = null;
            _response = null;
        }
    }
}