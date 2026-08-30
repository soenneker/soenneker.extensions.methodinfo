[![](https://img.shields.io/nuget/v/Soenneker.Extensions.MethodInfo.svg?style=for-the-badge)](https://www.nuget.org/packages/Soenneker.Extensions.MethodInfo/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.extensions.methodinfo/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.extensions.methodinfo/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/Soenneker.Extensions.MethodInfo.svg?style=for-the-badge)](https://www.nuget.org/packages/Soenneker.Extensions.MethodInfo/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.extensions.methodinfo/codeql.yml?label=CodeQL&style=for-the-badge)](https://github.com/soenneker/soenneker.extensions.methodinfo/actions/workflows/codeql.yml)

# ![](https://user-images.githubusercontent.com/4441470/224455560-91ed3ee7-f510-4041-a8d2-3fc093025112.png) Soenneker.Extensions.MethodInfo
Builds compact diagnostic method signatures and maps compiler-generated accessor names back to their member names.

## Installation

```bash
dotnet add package Soenneker.Extensions.MethodInfo
```

## Build a diagnostic signature

```csharp
using Soenneker.Extensions.MethodInfo;

MethodInfo method = typeof(CustomerService).GetMethod(nameof(CustomerService.Find))!;
string signature = method.GetSignature();

// Example: "public virtual Customer Find(String id)"
```

`GetSignature()` includes:

- The reflection access level.
- One of `abstract`, `static`, or unsealed `virtual` when applicable.
- The return type's simple `Name`.
- The method name.
- Each parameter type's simple `Name` and parameter name.

The result is cached for the lifetime of the `MethodInfo` object. A null method returns an empty string.

This output is intended for logs, diagnostics, and display. It is not guaranteed to be valid C# or a unique method identifier: namespaces, declaring type, generic arguments, custom modifiers, attributes, default values, and complete `ref`/`out` syntax are not rendered. Use metadata tokens or a purpose-built canonical representation when identity matters.

## Recover a property or event name

```csharp
MethodInfo getter = typeof(Customer).GetProperty(nameof(Customer.Name))!.GetMethod!;
string memberName = getter.ToOriginalMemberName(); // "Name"
```

For special-name methods, `ToOriginalMemberName()` removes `get_`, `set_`, `add_`, or `remove_`. For another special name containing an underscore, it returns the text after the first underscore—for example, `op_Addition` becomes `Addition`. A non-special method keeps its name, and a null method returns an empty string.

The method performs name transformation only; it does not resolve and return the associated `PropertyInfo` or `EventInfo`.
