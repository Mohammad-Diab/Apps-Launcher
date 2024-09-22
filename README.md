# Apps Launcher

A small launcher that sits at the edge of the screen and starts your programs from a searchable list. Written in C# with WPF on .NET Framework 4.0.

## Features

- Type in the search box to filter the list by program name or folder; **Enter** starts the first match. Typing the exact name of a program in the Windows or `system32` folder (`notepad`, `cmd`, `regedit`) puts it at the top.
- Right-click a program to start it, run it as administrator, run it with arguments, edit or delete it, open its folder, or copy its path.
- Add programs one by one, drag `.exe` files onto the list, or add every program in a folder at once. Each program has a name, a category and a priority.
- **Edit apps list** filters, sorts, checks and deletes entries in bulk.
- **Check apps list** removes entries whose file no longer exists.
- **Backup apps list** and **Restore apps list** save the list to a file and load it back.
- The window slides to the edge of the screen when the mouse leaves it and comes back when the mouse returns; **Options** picks the left or right edge. It can hide its title bar and has an icon in the notification area.

The list and the icon cache are kept in `%AppData%\Apps Launcher`.

## Requirements

Windows with .NET Framework 4.0 or later. Windows 8, 10 and 11 already include it.

## Usage

Download the latest `Apps-Launcher-v<version>.7z` from [Releases](https://github.com/Mohammad-Diab/Apps-Launcher/releases), extract it, and run `Apps Launcher.exe`. There is no installer.

The exe is not signed, so Windows SmartScreen may warn the first time you run it: click **More info**, then **Run anyway**.

## Building

```
git clone https://github.com/Mohammad-Diab/Apps-Launcher.git
```

Open `Apps Launcher.sln` in Visual Studio and build, or run:

```
MSBuild "Apps Launcher.sln" /p:Configuration=Release
```

The exe is written to `Apps Launcher\bin\Release`.

The project targets .NET Framework 4.0. Recent Visual Studio versions don't ship its reference assemblies; if the build stops with `MSB3644`, extract the [Microsoft.NETFramework.ReferenceAssemblies.net40](https://www.nuget.org/packages/Microsoft.NETFramework.ReferenceAssemblies.net40) NuGet package and point MSBuild at its `build` folder:

```
MSBuild "Apps Launcher.sln" /p:Configuration=Release /p:TargetFrameworkRootPath=<package>\build
```

## Author

Mohammad Diab

## License

[Apache 2.0](LICENSE)
