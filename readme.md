# LocoMat

LocoMat is a tool to help localize Blazor Razor components. This tool can be used to automate the process of localizing Blazor Razor components by processing Razor files and updating the localized resources file.

This program is focused on providing localization support for applications built with Radzen Blazor Studio.

Supported Radzen Blazor components:
RadzenTemplateForm, RadzenDropDownDataGridColumn, RadzenDataGridColumn, RadzenLabel, RadzenRequiredValidator, RadzenButton, and RadzenPanelMenuItem

## Features

* Generate resource files from .razor files in your Blazor application
* Translate resource files to different languages using Translator API
* Exclude specific files from localization
* Create default localization settings
* Easy-to-use command-line interface with shortcuts for faster input

## Installation

Install the program as dotnet tool:

```sh
dotnet tool install --global LocoMat 
```

Uninstall the program:

```sh
dotnet tool uninstall --global LocoMat
```
Update the program as dotnet tool:

```sh
dotnet tool update --global LocoMat 
```

## Usage

Use the following command to run the tool:

`LocoMat <command> [options]`

Commands:

- `localize`, `l`: Localizes the source files.
    - `-p`: Path to the project file. Defaults to the first .csproj file in the current directory.
    - `-r`: Path to the resource file. Defaults to 'Resources/SharedResources.resx'.
    - `-x`: Comma-separated list of file names to exclude from localization. Defaults to 'App.razor,_Imports.razor,RedirectToLogin.razor,CulturePicker.razor'.
    - `-i`: File name pattern to include in localization. Defaults to '*.razor'.
    - `-test`: Runs in test mode without actually changing any files.


- `translate`, `t`: Translates the resource files.
    - `-t`: Comma-separated list of target languages for translation. Defaults to empty (i.e. no translation).
    - `-e`: Email address. Required for translation service.


- `scaffold`, `s`: (experimental) Generate support classes for  localizing localizable texts from Radzen.Blazor components.
    - `-p`: Path to the project file. Defaults to the first .csproj file in the current directory.
    - `-t`: Comma-separated list of target languages for translation. Defaults to empty (i.e. no translation).
    - `-e`: Email address. Required for translation service.


- `restore`: Restores the original source files from the backup.
    - `-f`: Forces overwrite existing files when restoring from backup.


- `settings`, `s`: Displays or changes the application settings.


- `help`, `h`: Displays the help message.

Switches:

- `-v`: Enables verbose output.
- `-d`: Enables debug output.
- `-q`: Enables quiet output.
- `-b`: Backup files during localization.
- `-s`: Saves the settings to the configuration file.

### Examples

Localize a Blazor project and generate resource files:

```
LocoMat l -p ./MyBlazorProject.csproj -r ./Resources/SharedResources.resx -x App.razor,_Imports.razor 
```

Translate an existing resource file to Czech:

```
LocoMat t -r ./MyBlazorProject/Resources/SharedResources.resx -t cs-CZ -e my@email.com
```

Create a default localization settings file:

```
LocoMat s
```

Display help information:

```
LocoMat h
```

## Backups
It is highly recommended that you back up your data before using LocoMat for mass changes to your codebase. The tool has the potential to make sweeping changes to your source code and resource files, and any mistakes made can be difficult or impossible to reverse. Backing up your data ensures that you have a safe point to return to in case something goes wrong during the localization process.

As part of the localization process, LocoMat creates a backup of the modified files in a zip file located in the ./LocalizerBackup folder. The file name is generated using the current date and time, formatted as backup{DateTime.Now:yyyy-MM-ddTHH-mm-ss}.zip. Using the restore command will revert the changes made to the files by replacing them with the unchanged files from the most recent backup archive.

The restore command of LocoMat will only replace files that have not been modified since the last backup created during the localization process. Any files that have been changed after the localization process will not be affected by the restore command.  if you use the --force switch with the restore command, LocoMat will replace all files in their original state, even if they were modified after the localization process.

## Good practices
To ensure a smooth and safe localization process when using LocoMat for mass changes to your codebase, it is recommended that you follow these steps:

1. Commit your current progress by using the following command:

   ```
   git add .
   git commit -m "Preparing for localization"
   ```

2. Create a new branch on which to run the localization process:

   ```
   git checkout -b localization-branch
   ```

3. Run LocoMat on this new branch in project directory:

   ```
   locomat localize [options]
   ```

4. After LocoMat has completed the localization process, compare the changes made on the `localization-branch` with the original branch to ensure everything works as intended.

5. If you're satisfied with the localization results, merge the changes back into the original branch:

   ```
   git checkout original-branch
   git merge localization-branch
   ```

6. If something goes wrong during the localization process, you can revert the changes made on the `localization-branch`:

   ```
   git checkout localization-branch
   git revert HEAD --no-edit
   ```

By following this recommended practice, you can ensure that your codebase remains organized and that any changes made by LocoMat are safe and easily reversible.


## License

LocoMat is released under the [MIT License](LICENSE).
