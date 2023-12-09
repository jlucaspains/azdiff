# azdiff
`azdiff` is a command-line tool designed to perform deep comparisons between Azure resources.

## Installation
```powershell
dotnet tool install --global lpains.azdiff --prerelease
```

## General usage
Upon installation, access the tool by executing `azdiff` in your terminal. For specific command details, refer to the sections below or utilize the CLI help via `azdiff -h`.

The commands provided typically require a source and a target, generating a distinct `.diff` file for each identified and compared resource.

## azdiff arm
Facilitates the comparison between two ARM template files, analyzing Azure resources present in each file and generating one `.diff` file per resource illustrating the differences between the source and target resources.

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

Output file (diff_stapp-blog-centralus-001.diff):
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

JSON file containing replacement strings. Example:

```json
[
    {
        "target": "Name",
        "input": "DEV",
        "replacement": "env"
    }
]
```

The `target` property indicates whether the name (target: Name) property or the whole file (target: Body) will apply replacements. This is particularly useful when dealing with environment comparison (e.g.: DEV vs TEST) where the resource names or parts of its json will differ in a predictable way.

## azdiff rg
Facilitates the comparison between two Azure Resource Groups, analyzing Azure resources present in each resource group and generating one `.diff` file per resource illustrating the differences between the source and target resources.

```powershell
  azdiff rg --sourceResourceGroupId
             --targetResourceGroupId
             [--outputFolder]
             [--ignoreType]
             [--replaceStringsFile]
             [-?, -h, --help]
```

### Examples
Basic usage:
```powershell
azdiff rg --sourceResourceGroupId /subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/rg-dev-001 `
          --targetResourceGroupId /subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/rg-test-001
```

Advanced usage:
```powershell
azdiff rg --sourceResourceGroupId /subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/rg-dev-001 `
          --targetResourceGroupId /subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/rg-test-001 `
          --ignoreType "Microsoft.Web/staticSites/customDomains" `
          --ignoreType "Microsoft.Web/staticSites/databaseConnections" `
          --replaceStringsFile .\replaceStrings.json
```

Output file (diff_stapp-blog-centralus-001.diff):
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
#### `--sourceResourceGroupId`

The comparison source resource group id.

#### `--targetResourceGroupId`

The comparison target resource group id.

#### `--outputFolder`

The folder path for output. Defaults to `diffs`.

#### `--ignoreType`

A list of types to ignore in the ARM comparison. You may use this option multiple times.

#### `--replaceStringsFile`

JSON file containing replacement strings. Example:

```json
[
    {
        "target": "Name",
        "input": "DEV",
        "replacement": "env"
    }
]
```

The `target` property indicates whether the name (target: Name) property or the whole file (target: Body) will apply replacements. This is particularly useful when dealing with environment comparison (e.g.: DEV vs TEST) where the resource names or parts of its json will differ in a predictable way.