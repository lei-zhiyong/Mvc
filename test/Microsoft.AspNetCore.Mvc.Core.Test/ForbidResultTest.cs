// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc
{
    public class ForbidResultTest
    {
        [Fact]
        public async Task ExecuteResultAsync_InvokesForbidAsyncOnAuthenticationService()
        {
            // Arrange
            var httpContext = new Mock<HttpContext>();
            var auth = new Mock<IAuthenticationService>();
            auth
                .Setup(c => c.ForbidAsync(httpContext.Object, "", null))
                .Returns(TaskCache.CompletedTask)
                .Verifiable();
            httpContext.Setup(c => c.RequestServices).Returns(CreateServices(auth.Object));
            var result = new ForbidResult("", null);
            var routeData = new RouteData();

            var actionContext = new ActionContext(
                httpContext.Object,
                routeData,
                new ActionDescriptor());

            // Act
            await result.ExecuteResultAsync(actionContext);

            // Assert
            auth.Verify();
        }

        [Fact]
        public async Task ExecuteResultAsync_InvokesForbidAsyncOnAllConfiguredSchemes()
        {
            // Arrange
            var httpContext = new Mock<HttpContext>();
            var authProperties = new AuthenticationProperties();
            var auth = new Mock<IAuthenticationService>();
            auth
                .Setup(c => c.ForbidAsync(httpContext.Object, "Scheme1", authProperties))
                .Returns(TaskCache.CompletedTask)
                .Verifiable();
            auth
                .Setup(c => c.ForbidAsync(httpContext.Object, "Scheme2", authProperties))
                .Returns(TaskCache.CompletedTask)
                .Verifiable();
            httpContext.Setup(c => c.RequestServices).Returns(CreateServices(auth.Object));
            var result = new ForbidResult(new[] { "Scheme1", "Scheme2" }, authProperties);
            var routeData = new RouteData();

            var actionContext = new ActionContext(
                httpContext.Object,
                routeData,
                new ActionDescriptor());

            // Act
            await result.ExecuteResultAsync(actionContext);

            // Assert
            auth.Verify();
        }

        public static TheoryData ExecuteResultAsync_InvokesForbidAsyncWithAuthPropertiesData =>
            new TheoryData<AuthenticationProperties>
            {
                null,
                new AuthenticationProperties()
            };

        [Theory]
        [MemberData(nameof(ExecuteResultAsync_InvokesForbidAsyncWithAuthPropertiesData))]
        public async Task ExecuteResultAsync_InvokesForbidAsyncWithAuthProperties(AuthenticationProperties expected)
        {
            // Arrange
            var httpContext = new Mock<HttpContext>();
            var auth = new Mock<IAuthenticationService>();
            auth
                .Setup(c => c.ForbidAsync(httpContext.Object, null, expected))
                .Returns(TaskCache.CompletedTask)
                .Verifiable();
            httpContext.Setup(c => c.RequestServices).Returns(CreateServices(auth.Object));
            var result = new ForbidResult(expected);
            var routeData = new RouteData();

            var actionContext = new ActionContext(
                httpContext.Object,
                routeData,
                new ActionDescriptor());

            // Act
            await result.ExecuteResultAsync(actionContext);

            // Assert
            auth.Verify();
        }

        [Theory]
        [MemberData(nameof(ExecuteResultAsync_InvokesForbidAsyncWithAuthPropertiesData))]
        public async Task ExecuteResultAsync_InvokesForbidAsyncWithAuthProperties_WhenAuthenticationSchemesIsEmpty(
            AuthenticationProperties expected)
        {
            // Arrange
            var httpContext = new Mock<HttpContext>();
            var auth = new Mock<IAuthenticationService>();
            auth
                .Setup(c => c.ForbidAsync(httpContext.Object, null, expected))
                .Returns(TaskCache.CompletedTask)
                .Verifiable();
            httpContext.Setup(c => c.RequestServices).Returns(CreateServices(auth.Object));
            var result = new ForbidResult(expected)
            {
                AuthenticationSchemes = new string[0]
            };
            var routeData = new RouteData();

            var actionContext = new ActionContext(
                httpContext.Object,
                routeData,
                new ActionDescriptor());

            // Act
            await result.ExecuteResultAsync(actionContext);

            // Assert
            auth.Verify();
        }

        private static IServiceProvider CreateServices(IAuthenticationService auth)
        {
            return new ServiceCollection()
                .AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance)
                .AddSingleton(auth)
                .BuildServiceProvider();
        }
    }
}