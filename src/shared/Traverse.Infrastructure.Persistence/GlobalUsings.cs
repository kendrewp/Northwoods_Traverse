// Centralised global usings for Persistence — keeps individual files focused on
// the type definitions rather than the import noise. Every type here is used
// across multiple files in this project.
global using System;
global using System.Collections.Generic;
global using System.Threading;
global using System.Threading.Tasks;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.EntityFrameworkCore.Metadata.Builders;
global using Traverse.Domain.Primitives.Abstractions;
