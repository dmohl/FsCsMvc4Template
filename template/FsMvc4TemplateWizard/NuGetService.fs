[<AutoOpen>]
module NuGetService
    open System
    open System.IO
    open EnvDTE
    open Microsoft.Win32
    open Microsoft.VisualStudio.ComponentModelHost
    open Microsoft.VisualStudio.Shell.Interop
    open NuGet.VisualStudio

//    let GetNuGetPackageLocalPath (serviceProvider:IServiceProvider) =
//        let vsService = serviceProvider.GetService(typeof<IVsShell>) :?> IVsShell
//        match vsService = null with
//        | false ->
//            let _, path = vsService.GetProperty -9007
//            Path.Combine(string path, @"Extensions\Microsoft Corporation\ASP.NET MVC 4 template packages\1.0\Packages")
//        | _ -> null 

    let InstallPackages (serviceProvider:IServiceProvider) nuGetPackagePath (project:Project) packages =
        let componentModel = 
            serviceProvider.GetService(typeof<SComponentModel>) :?> IComponentModel
        let installer = componentModel.GetService<IVsPackageInstaller>()
        //let nugetPackageLocalPath = GetNuGetPackageLocalPath serviceProvider
        packages 
        |> Seq.iter (fun (packageId, version:string) -> 
                         installer.InstallPackage(Path.Combine(nuGetPackagePath, "NuGetPackages"), 
                             project, packageId, NuGet.SemanticVersion(version), false))  

