# comments-build-analyzer

*Analyze C# comments at build time and raise warnings or errors.*

This analyzer parses comments in C# code, looking for `FIXME` and `FIXME!` strings, and issues corresponding 
warnings (`ZB1001` and `ZB1002` respectively). This is a good way to ensure, at build time, that you do not
have FIXMEs lying around.

For instance, the following code will trigger two warnings:

```csharp
public class MyClass
{
	public void MyMehtod1() // FIXME: fix spelling
	{ }

	// FIXME! should not be public
	public void MyMethod2()
	{ }
}
```

This can be coupled with configuration-dependent editor configuration to fail the *Release* build in case
some FIXMEs are left in the code. Assuming the `.csproj` file contains:

```xml
  <ItemGroup Condition="'$(Configuration)'=='Debug'">
      <EditorConfigFiles Include="debug.editorconfig" Link=".editorconfig"/>
  </ItemGroup>

  <ItemGroup Condition="'$(Configuration)'=='Release'">
      <EditorConfigFiles Include="release.editorconfig" Link=".editorconfig"/>
  </ItemGroup>
```

And `debug.editorconfig` contains:

```txt
dotnet_diagnostic.ZB1002.severity = warning # FIXME!
```

And `release.editorconfig` contains:

```txt
dotnet_diagnostic.ZB1002.severity = error # FIXME!
```

Then, the *Debug* build will only raise a warning for each `FIXME!` comments left in the code, but the
*Release* build will raise errors and thus will fail. This means that you can add a FIXME comment in
your code that will not fail your in-progress development build (contrary to a `#error` statement),
yet would be detected by a *Release* build.