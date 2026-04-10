# Contributing

Contributions are welcome. Please open an issue first for larger changes so we can align on the approach.

## Development

```shell
dotnet restore CifraId.slnx
dotnet build CifraId.slnx -c Release
dotnet test CifraId.slnx -c Release
```

## Pull requests

- Keep changes focused and consistent with existing style.
- Add or update tests when behavior changes.
- Ensure `dotnet build` and `dotnet test` succeed with warnings as errors enabled on the library projects.
