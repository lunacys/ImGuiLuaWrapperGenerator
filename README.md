# ImGuiLuaWrapperGenerator

A simple ImGUI generator which generates a separate static wrapper class for MoonSharp.

This generator also adds a `MoonSharpUserData` attribute to the class in order to MoonSharp to automatically mark all members to add to `UserData`.

You can find an example of generated output in [this file](Example.cs).

## Build

In order to build this project you need **.NET Core 3.1** or higher. Simply run this command in the root of the repo:

```powershell
dotnet build
```

## Usage

This application requires exactly 2 arguments: namespace name and output file location (with extension), so you can run it like this:

```powershell
./ImGuiWrapperGenerator MyNameSpace "C:/Program Files/MyProgram/ImGuiWrapper.cs"
```
