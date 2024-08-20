# Windows Installer for .NET TWAIN Document Scanner
The project demonstrates how to use [WiX Toolset](https://wixtoolset.org/) to create a Windows Installer for a .NET TWAIN document scanner application and Dynamsoft Service. 

## How to Build the Installer
1. Build the `WinFormsDocScan` project to generate the document scanner application. You need to request a [30-day FREE trial license](https://www.dynamsoft.com/customer/license/trialLicense?product=dwt) and update the license key in the `Form1.cs` file.

    ![windows-dotnet-desktop-document-scanner](https://github.com/yushulx/dotnet-twain-scanner-installer/assets/2202306/884ea3dd-d620-49ad-91d1-1acd5e214ba0)

        
2. Build the `PackageDocScan` project to generate a MSI package for the document scanner application. You need to modify the file paths in the `ExampleComponents.wxs` file.
    
    ```xml
    <File Source="..\WinFormsDocScan\bin\Release\net7.0-windows\WinFormsDocScan.exe" KeyPath="yes" />
    <File Source="..\WinFormsDocScan\bin\Release\net7.0-windows\Twain.Wia.Sane.Scanner.dll" />
    <File Source="..\WinFormsDocScan\bin\Release\net7.0-windows\WinFormsDocScan.deps.json" />
    <File Source="..\WinFormsDocScan\bin\Release\net7.0-windows\WinFormsDocScan.dll" />
    <File Source="..\WinFormsDocScan\bin\Release\net7.0-windows\WinFormsDocScan.pdb" />
    <File Source="..\WinFormsDocScan\bin\Release\net7.0-windows\WinFormsDocScan.runtimeconfig.json" />
    ```

    ![docscan-msi-installer](https://github.com/yushulx/dotnet-twain-scanner-installer/assets/2202306/111d7d06-3142-4caf-a3c5-ae97fd995ad8)

3. Build the `BundleDocScan` project to generate a bundle installer for the document scanner application and Dynamsoft Service. You need to modify the file paths in the `Bundle.wxs` file.

    ```xml
    <MsiPackage SourceFile="..\PackageDocScan\bin\x64\Release\en-US\PackageDocScan.msi" />
    <MsiPackage SourceFile="msi\DynamsoftServiceSetup.msi" />
    ```

    ![wix-bundle](https://github.com/yushulx/dotnet-twain-scanner-installer/assets/2202306/89851867-d603-4ef4-afaa-9ee1965ae3fd)
