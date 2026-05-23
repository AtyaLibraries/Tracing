# Atya.Diagnostics.Tracing Sample

This console sample demonstrates the package as a consumer would use it:

- registering tracing services with dependency injection
- creating internal, client, producer, consumer, and server activities
- applying service/default tags
- writing propagation headers
- recreating an `ActivityContext` from a captured snapshot
- marking success and error outcomes

## Run

```bash
dotnet run --project samples/Tracing.Samples.Console/Tracing.Samples.Console.csproj --configuration Release
```
