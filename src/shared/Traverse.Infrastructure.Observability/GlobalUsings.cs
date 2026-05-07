// Centralised global usings for Observability — reduces import noise in
// individual files. Every namespace listed here is used across multiple files.
global using System;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Hosting;
global using Microsoft.Extensions.Diagnostics.HealthChecks;
global using OpenTelemetry;
global using OpenTelemetry.Trace;
global using OpenTelemetry.Metrics;
global using Serilog;
global using Serilog.Events;
global using Serilog.Formatting.Compact;
