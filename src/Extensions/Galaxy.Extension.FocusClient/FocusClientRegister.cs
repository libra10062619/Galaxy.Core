using Galaxy.Core.Extensions;
using Galaxy.Core.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Galaxy.Extension.FocusClient
{
    internal class FocusClientRegister
    {
        readonly FocusClientOptions _options;
        readonly IActionDescriptorCollectionProvider _actionDescriptorCollectionProvider;
        readonly IHttpClientFactory _httpClientFactory;
        readonly ILogger _logger;
        readonly string _focusUrl;
        readonly JsonSerializerOptions _serializerOptions;
        readonly CancellationTokenSource _cts;

        public FocusClientRegister(FocusClientOptions options,
            IActionDescriptorCollectionProvider actionDescriptorCollectionProvider,
            IHttpClientFactory httpClientFactory,
            ILogger<FocusClientRegister> logger)
        {
            _options = options;
            _logger = logger;
            _actionDescriptorCollectionProvider = actionDescriptorCollectionProvider;
            _httpClientFactory = httpClientFactory;
            Ensure.NotNull(options);

            _focusUrl = new Uri(_options.FocusHost).Append("/meta-data/register").AbsoluteUri;
            _serializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            _cts = new CancellationTokenSource();
        }

        public void Register()
        {
            var descipters = _actionDescriptorCollectionProvider
                .ActionDescriptors.Items.Cast<ControllerActionDescriptor>()
                .Where(p => p.MethodInfo.CustomAttributes.Any(x => x.AttributeType == typeof(FocusClientAttribute)))
                ;
            Task.Factory.StartNew(async () =>
            {
                var focusDtos = descipters.SelectMany(p => GetFocusMetadatas(p)).Distinct(new FocusMetadataCompare()).ToList();
               
                var tasks = focusDtos.Select(p => RegisterClient(p)).ToList();
                
                while (tasks.Any())
                {
                    var finished = await Task.WhenAny(tasks);
                    tasks.Remove(finished);
                }

            }, _cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default).ConfigureAwait(false);
        }

        private IEnumerable<FocusMetadata> GetFocusMetadatas(ControllerActionDescriptor controllerActionDescriptor)
        {
            var actionName = controllerActionDescriptor.ActionName;
            var controller = controllerActionDescriptor.ControllerName;            
            var paramterTypes = controllerActionDescriptor.Parameters?
                .Select(p => p.ParameterType.FullName).Aggregate((x1, x2) => $"{x1},{x2}");
            var path = $"{_options.ContextPath}/{Regex.Replace(controllerActionDescriptor.AttributeRouteInfo.Template, "\\{.[^\\}]*\\}", "**")}"
                .Replace("//", "/");
            //return new FocusMetadata[0];
            return controllerActionDescriptor.MethodInfo.GetCustomAttributes<FocusClientAttribute>()
                .Select(p => new FocusMetadata
                {
                    AppName = _options.AppName,
                    ServiceName = controller,
                    MethodName = actionName,
                    Path = path,
                    PathDesc = p.Description,
                    ParameterTypes = paramterTypes,
                    RpcExt = "",
                    RpcType = "http"
                });
        }

        private IEnumerable<FocusMetadata> GetFocusMetadatas(MethodInfo method)
        {
            var actionName = method.Name;
            var controller = method.DeclaringType.Name;
            var paramterTypes = method.GetParameters()?.Select(p => p.ParameterType.FullName).Aggregate((x1, x2) => $"{x1},{x2}");

            return method.GetCustomAttributes<FocusClientAttribute>()
                .Select(p => new FocusMetadata
                {
                    AppName = _options.AppName,
                    ServiceName = controller,
                    MethodName = actionName,
                    Path = $"{_options.ContextPath}/{p.GetGetFocusPath()}".Replace("//", "/"),
                    PathDesc = p.Description,
                    ParameterTypes = paramterTypes,
                    RpcExt = "",
                    RpcType = "http"
                });
            /*
            foreach (var attr in method.GetCustomAttributes<FocusClientAttribute>())
            {
                var meta = new FocusMetadata
                {
                    AppName = _options.AppName,
                    ServiceName = controller,
                    MethodName = actionName,
                    Path = $"{_options.ContextPath}/{attr.Path}".Replace("//", "/"),
                    PathDesc = attr.Description,
                    ParameterTypes = paramterTypes,
                    RpcExt = "",
                    RpcType = "http"
                };
                yield return meta;
            */
        }
        
        public async Task RegisterClient(ControllerActionDescriptor p, CancellationToken token = default)
        {
            var meta = new FocusMetadata
            {
                AppName = _options.AppName,
                ServiceName = p.ControllerName,
                MethodName = p.ActionName,
                Path = $"{_options.ContextPath}/{p.AttributeRouteInfo.Template}".Replace("//", "/"),
                PathDesc = p.MethodInfo.CustomAttributes.Single(o => o.AttributeType == typeof(FocusClientAttribute))
                            .ConstructorArguments[0].Value.ToString(),
                ParameterTypes = p.Parameters?.Select(o => o.ParameterType.FullName).Aggregate((x1, x2) => $"{x1},{x2}"),
                RpcExt = "",
                RpcType = "http"
            };

            await RegisterClient(meta, token);
        }

        public async Task RegisterClient(FocusMetadata metadata, CancellationToken token = default)
        {
            try
            {
                var jsonData = JsonSerializer.Serialize(metadata, _serializerOptions);// metadata.Serialize();
                var client = _httpClientFactory.CreateClient();
                var httpContent = new StringContent(jsonData);
                httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json")
                {
                    CharSet = "utf-8"
                };
                //client.DefaultRequestHeaders.Add("Content-Type", "application/json; charset=utf-8");
                var response = await client.PostAsync(_focusUrl, httpContent, token);

                var content = await response.Content.ReadAsStringAsync();

                if (content == "success")
                {
                    _logger.LogInformation($"Api '{metadata.Path}' has been registered as {metadata.ServiceName}_{metadata.MethodName}.");
                }
                else
                {
                    _logger.LogError($"Api '{metadata.Path}' registration failed. ");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
            }
        }
    }

    internal class FocusMetadataCompare : IEqualityComparer<FocusMetadata>
    {
        public bool Equals([AllowNull] FocusMetadata x, [AllowNull] FocusMetadata y)
        {
            return (x.Path == y.Path);
        }

        public int GetHashCode([DisallowNull] FocusMetadata obj)
        {
            return obj.ToString().ToLower().GetHashCode();
        }
    }
}
