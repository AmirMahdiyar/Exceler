# 🚀 Exceler

[![NuGet Version](https://img.shields.io/nuget/v/Exceler.svg?style=flat-square&color=blue)](https://www.nuget.org/packages/Exceler)
[![Downloads](https://img.shields.io/nuget/dt/Exceler.svg?style=flat-square&color=green)](https://www.nuget.org/packages/Exceler)
[![Framework](https://img.shields.io/badge/.NET-6.0%20%7C%207.0%20%7C%208.0%20%7C%209.0-purple.svg?style=flat-square)](#)
[![License](https://img.shields.io/badge/License-MIT-yellow.svg?style=flat-square)](LICENSE)
[![codecov](https://codecov.io/gh/AmirMahdiyar/Exceler/graph/badge.svg?token=YOUR_TOKEN)](https://codecov.io/gh/AmirMahdiyar/Exceler)

An ultra-fast, modern, and memory-efficient spreadsheet processing framework for **.NET 6, 7, 8, and 9**. It encapsulates low-level workbook operations to deliver high-performance importing and exporting through a clean, declarative API.

---

## ⚡ Why Exceler?

Modern .NET applications frequently struggle with Excel processing due to slow runtime reflection and unnecessary memory allocations from repetitive object transformations. **Exceler** solves these challenges with a clean, cloud-native architecture:

*   **Zero-Allocation Mapping Pipeline:** Eliminates boxing and runtime reflection by compiling mapping configurations into high-performance **Expression Trees** at application startup. Row mapping executes at the raw speed of handwritten, hard-coded C# code.
*   **Memory-Conscious Chunking & Streaming:** Employs lazy deferred execution (`IEnumerable` streaming) and asynchronous chunking (`ReadInChunksAsync` via `IAsyncEnumerable`) to deliver processed rows on-demand, preventing large in-memory collections of DTOs and minimizing Garbage Collector (GC) pressure.
*   **Streaming EF Core Integration:** Directly stream database queries via `IAsyncEnumerable<T>` and the `.ToExcelAsync()` fluent extension without buffering giant lists in RAM with `.ToListAsync()`.
*   **Fluent & Clean Design:** Eliminates dirty cell-coordinate code from your business services. All mappings, header designs, column widths, and cell formatting are defined declaratively in clean, reusable profile classes.
*   **Production-Ready Pipelines:** Built-in support for strongly-typed value conversion (`IExcelValueConverter<T>`), asynchronous business rule validation (`IAsyncExcelValidator<T>`), asynchronous domain enrichment (`IAsyncExcelProcessor<TIn, TOut>`), and Right-to-Left (RTL) worksheets.
*   **Bulletproof Parsing:** Built-in safeguards against corrupt files, formula-based cells, type mismatches, unexpected nulls, and duplicate or missing headers.
*   **Highly Tested:** Backed by an extensive automated test suite covering complex edge cases with 100% parallel test execution.

---

## 🧠 Architecture & Memory Model: Exceler & EPPlus

To deliver rich spreadsheet capabilities—including high-fidelity styling, formulas, custom palettes, and full OpenXML specification compliance—Exceler builds upon the powerful, industry-standard **EPPlus** engine. Understanding how both layers interact helps you achieve optimal performance:

*   **EPPlus (Document Object Model):** EPPlus is a mature, feature-complete library that operates on an in-memory DOM (`ExcelPackage`). To provide complete workbook manipulation, rich cell formatting, and formula evaluation, EPPlus naturally maintains the workbook structure in memory during read and write operations. This comprehensive DOM model is what enables such deep OpenXML compatibility across Windows, Linux, and Docker environments.
*   **Exceler (Zero-Allocation Application Layer):** Where traditional approaches incur significant additional memory overhead by allocating intermediate lists, boxing primitive types, and using heavy runtime reflection, Exceler introduces a **Zero-Allocation pipeline** on top of EPPlus. By utilizing Expression Tree compilation, span-based parsing (`ColorHelper`), and streaming row dispatch (`ReadInChunksAsync`), Exceler ensures that your application layer adds virtually zero extra allocations, keeping memory usage predictable and GC pauses minimal even when handling massive datasets.

---

## 📦 Installation

```bash
dotnet add package Exceler
```

---

## 🚀 Quick Start

Implement high-performance spreadsheet import and export in just 4 clean steps:

### Step 1: Register Core Services
```csharp
// Program.cs
using Exceler.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Automatically registers IExcelReader, IExcelWriter, and scans assemblies for profiles
builder.Services.AddExcelCore(options =>
{
    options.UseNonCommercialLicense();
    options.RegisterFromAssemblyContaining<EmployeeExcelProfile>();
});
```

### Step 2: Define Model and Fluent Mapping Profile
```csharp
using Exceler.Configuration;

// The raw model representing the sheet columns
public class EmployeeExcelInput
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string HireDate { get; set; } = string.Empty;
}

// Fluent mapping profile
public class EmployeeExcelProfile : ExcelProfile<EmployeeExcelInput>
{
    public EmployeeExcelProfile()
    {
        Map(x => x.Id).ToColumn(1).WithHeader("Id").IsBold(true);
        Map(x => x.FullName).ToColumn(2).WithHeader("Name");
        Map(x => x.HireDate).ToColumn(3).WithHeader("Date");
    }
}
```

### Step 3: Fast Excel Reading (Lazy Importing)
```csharp
using Exceler.Abstractions;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/employees")]
public class EmployeesController : ControllerBase
{
    private readonly IExcelReader _excelReader;

    public EmployeesController(IExcelReader excelReader)
    {
        _excelReader = excelReader;
    }

    [HttpPost("import")]
    public IActionResult Import(IFormFile file)
    {
        using var stream = file.OpenReadStream();
        
        // Lazy-loaded row stream. Avoids storing redundant DTO collections in memory.
        var results = _excelReader.Read<EmployeeExcelInput, EmployeeDto>(stream);

        foreach (var row in results)
        {
            if (row.IsValid)
            {
                var dto = row.Data;
                // Process and save in chunks...
            }
            else
            {
                var errors = row.Errors;
                // Track validation errors...
            }
        }

        return Ok();
    }
}
```

### Step 4: Styled Excel Writing (Exporting)
```csharp
using Exceler.Abstractions;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/employees")]
public class EmployeesController : ControllerBase
{
    private readonly IExcelWriter _excelWriter;

    public EmployeesController(IExcelWriter excelWriter)
    {
        _excelWriter = excelWriter;
    }

    [HttpGet("export")]
    public async Task<IActionResult> Export()
    {
        var list = new List<EmployeeExcelInput>
        {
            new() { Id = 1, FullName = "John Doe", HireDate = "2026-01-15" },
            new() { Id = 2, FullName = "Jane Doe", HireDate = "2026-02-20" }
        };

        // Generates structured, styled Excel file directly to a stream
        var memoryStream = new MemoryStream();
        await _excelWriter.WriteAsync(list, memoryStream, sheetName: "Active Employees");
        memoryStream.Position = 0;

        return File(
            memoryStream, 
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
            "employees.xlsx"
        );
    }
}
```

### Step 5: Streaming Directly from EF Core (`AsAsyncEnumerable`)
Export millions of database records directly to Excel without buffering them into memory with `.ToListAsync()`:

```csharp
using Exceler.Abstractions;
using Exceler.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[HttpGet("export-stream")]
public async Task<IActionResult> ExportStream(
    [FromServices] AppDbContext dbContext,
    [FromServices] IExcelWriter excelWriter)
{
    var memoryStream = new MemoryStream();

    // Stream records row-by-row directly from the database into the Excel workbook!
    await dbContext.Employees
        .AsNoTracking()
        .AsAsyncEnumerable()
        .ToExcelAsync(excelWriter, memoryStream, sheetName: "Database Stream");

    memoryStream.Position = 0;
    return File(
        memoryStream,
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        "employees_stream.xlsx"
    );
}
```

---

## 🛡️ Reliability & Enterprise Testing

Exceler is built with enterprise stability in mind. The framework is heavily tested against severe edge cases, including:

*   **Missing, empty, or duplicate headers**
*   **Mathematical formulas hiding inside standard cells (calculates automatically)**
*   **Type-casting failures (safely captures format errors without crashing the pipeline)**
*   **Complete separation of row failures (one bad row does not abort file processing)**
*   **Comprehensive test suite running fully in parallel**

---

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

> ⚠️ **EPPlus Licensing Note:** Exceler utilizes the industry-proven **EPPlus** library under the hood for lower-level spreadsheet manipulations. Please note that EPPlus is licensed under the **PolyForm Noncommercial License 1.0.0**. If you are using Exceler in a commercial/corporate environment, please ensure you comply with the commercial licensing terms of EPPlus.

---

## 🤝 Contributing

Contributions, issues, and feature requests are welcome! Feel free to check the issues page.
