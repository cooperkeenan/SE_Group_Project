# EnviroMonitorApp Documentation

Welcome to the EnviroMonitorApp API documentation. This site provides detailed information about the classes and methods in the EnviroMonitorApp codebase.

## Project Overview

EnviroMonitorApp is an application for monitoring and analyzing environmental data including:

- Air quality measurements
- Weather conditions
- Water quality metrics

## Documentation Sections

- [API Documentation](api/index.html) - Technical documentation of classes and methods

## Getting Started

To use this library in your project, add a reference to EnviroMonitorApp.Core in your project file.

```csharp
// Example: Retrieving environmental data
var dataService = new EnvironmentalDataApiService(
    airApi, 
    weatherApi, 
    waterApi, 
    apiKeys);

var weatherData = await dataService.GetWeatherAsync(
    DateTime.Now.AddDays(-7), 
    DateTime.Now, 
    "London");
```