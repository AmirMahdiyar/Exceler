# 🚀 Exceler

[![NuGet Version](https://img.shields.io/nuget/v/Exceler.svg?style=flat-square&color=blue)](https://www.nuget.org/packages/Exceler)
[![Downloads](https://img.shields.io/nuget/dt/Exceler.svg?style=flat-square&color=green)](https://www.nuget.org/packages/Exceler)
[![Framework](https://img.shields.io/badge/.NET-6.0%20%7C%208.0%20%7C%209.0-purple.svg?style=flat-square)](#)
[![License](https://img.shields.io/badge/License-MIT-yellow.svg?style=flat-square)](LICENSE)

An ultra-fast, modern, and memory-efficient spreadsheet processing framework for **.NET 6, 7 , 8, and 9**. It encapsulates low-level workbook operations to deliver high-performance importing and exporting through a clean, declarative API.

---

## ⚡ Why Exceler?

Unlike traditional .NET Excel libraries that load entire files into memory and rely on slow runtime Reflection, **Exceler** is designed for modern, cloud-native enterprise requirements:

*   **Zero-Allocation Row Chunking:** Uses lazy deferred execution (`IEnumerable` streaming) to read massive Excel files row-by-row, keeping the RAM footprint near-zero and preventing Garbage Collector spikes.
*   **Startup expression compilation:** Scans and compiles mapping configurations into optimized **Expression Trees** at application startup [1, 2]. Object mapping executes at the speed of handwritten, hard-coded code.
*   **Fluent & Clean Design:** Eliminates dirty Excel parsing code from your business services. All mappings, header designs, and cell formatting are defined declaratively in clean profile classes [1].
*   **Production-Ready Pipelines:** Built-in support for strongly-typed value conversion, fail-fast business rule validation, and multi-sheet exports [3-5].

---

## 📦 Installation

```bash
dotnet add package Exceler
🚀 Quick Start
Implement high-performance spreadsheet import and export in just 4 clean steps.
Step 1: Register Core Services
// Program.cs
using Exceler.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Automatically registers IExcelReader, IExcelWriter, and scans assemblies for profiles
builder.Services.AddExcelCore(options =>
{
    options.UseNonCommercialLicense();
    options.RegisterFromAssemblyContaining<EmployeeExcelProfile>();
});
Step 2: Define Model and Fluent Mapping Profile
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
Step 3: Fast Excel Reading (Lazy Importing)
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
        
        // Lazy-loaded row stream. Avoids storing the whole spreadsheet in memory.
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
Step 4: Styled Excel Writing (Exporting)
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

        // Generates structured, styled Excel file bytes instantly
        byte[] fileBytes = await _excelWriter.Write(list, sheetName: "Active Employees");

        return File(
            fileBytes, 
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
            "employees.xlsx"
        );
    }
}
```

---

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

> ⚠️ **EPPlus Licensing Note:** Exceler utilizes the industry-proven **EPPlus** library under the hood for lower-level spreadsheet manipulations. Please note that EPPlus is licensed under the **PolyForm Noncommercial License 1.0.0**. If you are using Exceler in a commercial/corporate environment, please ensure you comply with the commercial licensing terms of EPPlus.
