[![](https://img.shields.io/nuget/v/Soenneker.Extensions.MethodInfo.svg?style=for-the-badge)](https://www.nuget.org/packages/Soenneker.Extensions.MethodInfo/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.extensions.methodinfo/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.extensions.methodinfo/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/Soenneker.Extensions.MethodInfo.svg?style=for-the-badge)](https://www.nuget.org/packages/Soenneker.Extensions.MethodInfo/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.extensions.methodinfo/codeql.yml?label=CodeQL&style=for-the-badge)](https://github.com/soenneker/soenneker.extensions.methodinfo/actions/workflows/codeql.yml)

# ![](https://user-images.githubusercontent.com/4441470/224455560-91ed3ee7-f510-4041-a8d2-3fc093025112.png) Soenneker.Extensions.MethodInfo
A collection of useful MethodInfo methods.

## Installation

```bash
dotnet add package Soenneker.Extensions.MethodInfo
```

## Quick start

```csharp
using Soenneker.Extensions.MethodInfo;

// Given an existing System.Reflection.MethodInfo? named methodInfo:
var result = methodInfo.GetSignature();
```

## Common operations

- `GetSignature()` - Returns a string representation of the method signature for the specified method information.
- `ToOriginalMemberName()` - Returns the original member name associated with the specified method, removing special prefixes used for property and event accessors.
