[<AutoOpen>]
module NuGetService
    open System
    open System.IO
    open EnvDTE
    open Microsoft.Win32
    open Microsoft.VisualStudio.ComponentModelHost
    open NuGet.VisualStudio

    let InstallPackages (serviceProvider:IServiceProvider) nuGetPackagePath (project:Project) packages =
        let componentModel = 
            serviceProvider.GetService(typeof<SComponentModel>) :?> IComponentModel
        let installer = componentModel.GetService<IVsPackageInstaller>()
        packages 
        |> Seq.iter (fun (packageId, version:string) -> 
                         installer.InstallPackage(Path.Combine(nuGetPackagePath, "NG"), 
                             project, packageId, new Version(version), false))  

//    let InstallPackages (serviceProvider:IServiceProvider) nuGetPackagePath (project:Project) packages =
//        let componentModel = 
//            serviceProvider.GetService(typeof<SComponentModel>) :?> IComponentModel
//        let installer = componentModel.GetService<IVsPackageInstaller>() :?> VsPackageInstaller
//
//        //let nugetPackageLocalPath = GetNuGetPackageLocalPath serviceProvider
//        packages 
//        |> Seq.iter (fun (packageId, version:string) -> 
//                         installer.InstallPackage(Path.Combine(nuGetPackagePath, "NuGetPackages"), 
//                             project, packageId, NuGet.SemanticVersion(version), false))  
//
