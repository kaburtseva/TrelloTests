﻿using System;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;
using NLog;
using RestSharp;
using TrelloAssignment.Helpers;

namespace TrelloAssignment.API.Base
{
    public abstract class BaseObject
    {
        protected readonly IRestClient RestClient;
        private static ILogger Logger;

        public string TokenAccess = RunGlobalSettingHelper.RunEnvironment.GetSection("token").Value;
        public string KeyAccess = RunGlobalSettingHelper.RunEnvironment.GetSection("key").Value;
        public string ListForCard = RunGlobalSettingHelper.RunEnvironment.GetSection("idList").Value;

        protected BaseObject(IRestClient restClient, ILogger logger)
        {
            RestClient = restClient;
            Logger = logger;

        }

        protected virtual IRestResponse Execute(IRestRequest request)
        {
            IRestResponse response = null;
            var timer = Stopwatch.StartNew();

            try
            {
                timer.Start();
                response = RestClient.Execute(request);
                timer.Stop();
                return response;
            }
            finally
            {
                LogRequest(request, response, timer.ElapsedMilliseconds);
            }
        }

        protected virtual IRestResponse<T> Execute<T>(IRestRequest request) where T : new()
        {
            IRestResponse<T> response = null;
            var timer = new Stopwatch();

            try
            {
                timer.Start();
                response = RestClient.Execute<T>(request);
                timer.Stop();

                return response;
            }
            catch (JsonSerializationException e)
            {

                Logger.Error($"Failed to deserialize object {typeof(T).FullName}: {response?.Content}, exception ${e}");
            }
            finally
            {
                LogRequest(request, response, timer.ElapsedMilliseconds);
            }

            return default(IRestResponse<T>);
        }

        private void LogRequest(IRestRequest request, IRestResponse response, long durationInMilliseconds)
        {
            Logger.Info($"Request completed in {durationInMilliseconds} ms, " +
                               $"Request: {ConvertRestRequestToString(request)}, " +
                               $"Response: {ConvertRestResponseToString(response)}");
        }

        private string ConvertRestRequestToString(IRestRequest request)
        {
            var convertedRequest = new
            {
                uri = RestClient.BuildUri(request),
                resource = request.Resource,
                method = request.Method.ToString(),
                parameters = request.Parameters.Select(parameter => new
                {
                    name = parameter.Name,
                    value = parameter.Value,
                    type = parameter.Type.ToString()
                })
            };

            return JsonConvert.SerializeObject(convertedRequest);
        }

        private string ConvertRestResponseToString(IRestResponse response)
        {
            var convertedResponse = new
            {
                uri = response.ResponseUri,
                statusCode = response.StatusCode,
                headers = response.Headers,
                content = response.Content,
                errorMessage = response.ErrorMessage
            };

            return JsonConvert.SerializeObject(convertedResponse);
        }
    }
}
