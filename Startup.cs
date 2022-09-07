using System;
using System.Reflection;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using todos;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;


[assembly: FunctionsStartup(typeof(Startup))]

namespace todos;

public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        builder.Services.AddLogging();
    }
}