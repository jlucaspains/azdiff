# azdiff
An Azure resource group deep comparison via ARM templates.

## Installation
```powershell
dotnet tool install --global lpains.azdiff --prerelease
```

## General usage
Once installed, you may access the tool by running `azdiff` in your terminal. See next sections for more information on each command or refer to the CLI help via `azdiff -h`.

In general, the commands provided will require at least a source and a target and will output one diff file per resource identified and compared.

## azdiff arm
Compare two ARM template files. 

For each Azure Resource identified in the template file, a `.diff` file is created with the differences between the source and target. The file name is the resource name and the content is the diff output.

```powershell
  azdiff arm --sourceFile
             --targetFile
             [--outputFolder]
             [--ignoreType]
             [--replaceStringsFile]
             [-?, -h, --help]
```

### Examples
Basic usage:
```powershell
azdiff arm --sourceFile .\source.json `
           --targetFile .\target.json
```

Advanced usage:
```powershell
azdiff arm --sourceFile .\source.json `
           --targetFile .\target.json `
           --ignoreType "Microsoft.Web/staticSites/customDomains" `
           --ignoreType "Microsoft.Web/staticSites/databaseConnections" `
           --replaceStringsFile .\replaceStrings.json
```

Output file:
```diff
  {
    "type": "Microsoft.Web/staticSites",
    "apiVersion": "2023-01-01",
    "name": "stapp-blog-centralus-001",
    "location": "Central US",
    "sku": {
      "name": "Free",
      "tier": "Free"
    },
    "properties": {
      "repositoryUrl": "https://github.com/jlucaspains/blog-v2",
-     "branch": "v1",
+     "branch": "release/v1",
      "stagingEnvironmentPolicy": "Enabled",
      "allowConfigFileUpdates": true,
      "provider": "GitHub",
      "enterpriseGradeCdnStatus": "Disabled"
    }
  }

```

### Parameters
#### `--sourceFile`

The comparison source json file. It should be an exported ARM template.

#### `--targetFile`

The comparison target json file. It should be an exported ARM template.

#### `--outputFolder`

The folder path for output. Defaults to `diffs`.

#### `--ignoreType`

A list of types to ignore in the ARM comparison. You may use this option multiple times.

#### `--replaceStringsFile`

Replacement strings json file. The json must be a collection of replacements like the example below:

```json
[
    {
        "target": "Name",
        "input": "DEV",
        "replacement": "env"
    }
]
```

You may use Name to replace parts of the name property of a resource or Body to replace parts in the overal json of the resource. This is particularly useful when dealing with environment comparison (e.g.: DEV vs TEST) where the resource names or parts of its json will differ in a predictable way.