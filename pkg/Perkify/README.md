# Perkify

Perkify is a NuGet package designed for managing balances, budgets, authorizations, delegations, expirations, and related business logic. It helps developers quickly integrate core functionalities into their applications.

## Features

- **Balance Management**: Provides a robust mechanism to manage balances.
- **Budget Control**: Includes built-in budget management with support for complex budget combinations.
- **Authorization & Delegation**: Flexible management of authorizations and delegations to cater to various business scenarios.
- **Expiration Handling**: Automated detection and renewal support for expirations.
- **Extensible Design**: Follows the principle of composition over inheritance, making it easy to extend and customize.

## Installation

Install via the NuGet Package Manager:

```powershell
Install-Package Perkify
```

Or using the .NET CLI:

```bash
dotnet add package Perkify
```

## Usage Example

The following is a simple example demonstrating how to initialize and use the core functionalities provided by Perkify:

```csharp
using Perkify;
using Perkify.Core;

// Initialize balance management
Balance myBalance = new Balance(1000);

try
{
    // Perform a deduction operation
    myBalance.Deduct(150);
    Console.WriteLine("Deduction successful, new balance: " + myBalance.CurrentBalance);
}
catch (Exception ex)
{
    Console.WriteLine("Operation failed: " + ex.Message);
}
```

## Documentation & Support

- Refer to the [project documentation](../doc) for detailed usage instructions, architecture design, and developer guides.
- For any questions or suggestions, please open an issue on GitHub.

## Contributing

Contributions are welcome! Please see the [contributing guide](CONTRIBUTING.md) for more information.

## License

This project is licensed under the [MIT License](LICENSE.md).
