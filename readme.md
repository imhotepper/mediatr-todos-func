
# Readme

## Autorun on code change [gist link](https://gist.github.com/imhotepper/ef6f932f7dcda819c5c17d2644900b4f)


Ensure the following is present in the [project-name].csproj file: 

```
  <Target Name="RunFunctions">
    <Exec Command="func start" />
  </Target>
  
```

Run it with:

```cmd
dotnet watch msbuild /t:RunFunctions
```
