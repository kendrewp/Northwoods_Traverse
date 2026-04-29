// Centralised global usings for Http infrastructure — reduces import noise in
// individual files. Every namespace listed here is used across multiple files
// in this project.
global using System;
global using System.Threading;
global using System.Threading.Tasks;
global using Microsoft.AspNetCore.Http;
global using Microsoft.AspNetCore.Diagnostics;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;
global using FluentValidation;
global using Traverse.Domain.Primitives.Exceptions;
global using Serilog.Context;
