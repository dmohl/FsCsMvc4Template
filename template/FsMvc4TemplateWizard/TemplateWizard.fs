namespace FsMvc4TemplateWizard

open System
open System.IO
open System.Windows.Forms
open System.Collections.Generic
open EnvDTE
open EnvDTE80
open Microsoft.VisualStudio.Shell
open Microsoft.VisualStudio.TemplateWizard
open VSLangProj
open NuGet.VisualStudio
open FsCsMvc4Dialog

type TemplateWizard() =
    [<DefaultValue>] val mutable solution : Solution2
    [<DefaultValue>] val mutable dte : DTE
    [<DefaultValue>] val mutable dte2 : DTE2
    [<DefaultValue>] val mutable serviceProvider : IServiceProvider
    [<DefaultValue>] val mutable destinationPath : string
    [<DefaultValue>] val mutable safeProjectName : string
    [<DefaultValue>] val mutable includeTestProject : bool
    [<DefaultValue>] val mutable vsixInstallPath : string
    let mutable selectedWebProjectName = "Razor"
    interface IWizard with
        member this.RunStarted (automationObject:Object, 
                                replacementsDictionary:Dictionary<string,string>, 
                                runKind:WizardRunKind, customParams:Object[]) =
            this.vsixInstallPath <- customParams |> Seq.cast |> Seq.find(fun x -> x.Contains ".vstemplate")
            this.dte <- automationObject :?> DTE
            this.dte2 <- automationObject :?> DTE2
            this.solution <- this.dte2.Solution :?> EnvDTE80.Solution2
            this.serviceProvider <- new ServiceProvider(automationObject :?> 
                                     Microsoft.VisualStudio.OLE.Interop.IServiceProvider)
            this.destinationPath <- replacementsDictionary.["$destinationdirectory$"]
            this.safeProjectName <- replacementsDictionary.["$safeprojectname$"]

            let dialog = new TemplateWizardDialog()
            match dialog.ShowDialog().Value with
            | true -> 
                this.includeTestProject <- dialog.IncludeTestsProject
                selectedWebProjectName <- dialog.SelectedViewEngine
            | _ ->
                raise (new WizardCancelledException())
        member this.ProjectFinishedGenerating project = "Not Implemented" |> ignore
        member this.ProjectItemFinishedGenerating projectItem = "Not Implemented" |> ignore
        member this.ShouldAddProjectItem filePath = true
        member this.BeforeOpeningFile projectItem = "Not Implemented" |> ignore
        member this.RunFinished() = 
            let currentCursor = Cursor.Current
            Cursor.Current <- Cursors.WaitCursor
            try
                let webName = this.safeProjectName + "Web"
                let webAppName = this.safeProjectName + "WebApp"
                let webAppTestsName = this.safeProjectName + "WebAppTests"

                let templatePath = this.vsixInstallPath 
                try
                    let AddProject status projectVsTemplateName projectName =
                        this.dte2.StatusBar.Text <- status
                        let path = templatePath.Replace("FsMvc4.vstemplate", projectVsTemplateName)
                        this.dte2.Solution.AddFromTemplate(path, Path.Combine(this.destinationPath, projectName), 
                            projectName, false) |> ignore
                    AddProject "Installing the C# Web project..." 
                        (Path.Combine(selectedWebProjectName, selectedWebProjectName+ ".vstemplate")) webName
                    AddProject "Adding the F# Web App project..." 
                        (Path.Combine("WebApp", "WebApp.vstemplate")) webAppName
                    if this.includeTestProject then
                        AddProject "Adding the F# Web App Tests project..." 
                            (Path.Combine("WebAppTests", "WebAppTests.vstemplate")) webAppTestsName

                    let projects = BuildProjectMap (this.dte.Solution.Projects)

                    this.dte2.StatusBar.Text <- "Adding NuGet packages..."
                    try

// This is the List from the C# ASP.NET MVC Empty template found at C:\Program Files (x86)\Microsoft Visual Studio 10.0\Common7\IDE\ProjectTemplatesCache\CSharp\Web\1033\EmptyMvcWebApplicationProjectTemplatev4.0.cshtml.zip
//            <package id="jQuery" version="1.6.2" />
//            <package id="jQuery.Validation" version="1.8.1" />
//            <package id="jQuery.UI.Combined" version="1.8.11" />
//            <package id="jQuery.Validation.Unobtrusive" version="2.0.20126.16343" />
//            <package id="jQuery.Ajax.Unobtrusive" version="2.0.20126.16343" />
//            <package id="EntityFramework" version="4.1.10331.0" />
//            <package id="Modernizr" version="2.0.6" />
//            <package id="Microsoft.Web.Infrastructure" version="1.0.0.0" />
//            <package id="AspNetRazor.Core" version="2.0.20126.16343" />
//            <package id="AspNetWebPages.Core" version="2.0.20126.16343" />
//            <package id="System.Web.Http.Common" version="4.0.20126.16343" />
//            <package id="AspNetMvc" version="4.0.20126.16343" />
//            <package id="System.Json" version="4.0.20126.16343" />
//            <package id="System.Net.Http" version="2.0.20126.16343" />
//            <package id="System.Net.Http.Formatting" version="4.0.20126.16343" />
//            <package id="AspNetWebApi.Core" version="4.0.20126.16343" />
//            <package id="AspNetWebApi" version="4.0.20126.16343" />
//            <package id="System.Web.Providers.Core" version="1.0" />
//            <package id="System.Web.Providers" version="1.1" />
//            <package id="Microsoft.Web.Optimization" version="1.0.0-beta" />
//            <package id="knockoutjs" version="2.0.0.0" />

                        (projects.TryFind webName).Value |> InstallPackages this.serviceProvider (templatePath.Replace("FsMvc4.vstemplate", ""))
                        <| [("AspNetMvc", "4.0.20126.16343"); ("AspNetWebPages.Core", "2.0.20126.16343"); ("jQuery", "1.6.2"); ("jQuery.Mobile", "1.0")
                            ("jQuery.Ajax.Unobtrusive", "2.0.20126.16343"); ("jQuery.Validation", "1.8.1"); ("jQuery.Validation.Unobtrusive", "2.0.20126.16343") 
                            ("knockoutjs", "2.0.0.0"); ("jQuery.UI.Combined", "1.8.11"); ("Modernizr", "2.0.6"); ("EntityFramework", "4.1.10331.0")
                            ("Microsoft.Web.Optimization", "1.0.0-beta"); ("Microsoft.Web.Infrastructure", "1.0.0.0"); ("System.Web.Providers.Core", "1.0")
                            ("AspNetRazor.Core", "2.0.20126.16343"); ("System.Web.Http.Common", "4.0.20126.16343"); ("System.Json", "4.0.20126.16343")
                            ("System.Net.Http", "2.0.20126.16343"); ("System.Net.Http.Formatting", "4.0.20126.16343");  ("System.Web.Providers", "1.1")
                            ("AspNetWebApi.Core", "4.0.20126.16343"); ("AspNetWebApi", "4.0.20126.16343");]

                        // Need separate NuGet package installs for MVC and WebApi? This might improve perf. 
                    with
                    | ex -> failwith (sprintf "%s\n\r%s\n\r%s\n\r%s\n\r%s" 
                                "The NuGet installation process failed."
                                "Ensure that you have installed at least the beta version of ASP.NET MVC 4." 
                                "See http://asp.net/mvc/mvc4 for more information."
                                //("See http://asp.net/mvc/mvc4 for more information." + " debug: NuGet Path = " + NuGetService.GetNuGetPackageLocalPath(this.serviceProvider))
                                "The actual exception message is: "
                                ex.Message)

                    this.dte2.StatusBar.Text <- "Updating project references..."
                    [(webName, webAppName); (webAppTestsName, webAppName)]
                    |> BuildProjectReferences projects 
                with
                | ex -> failwith (sprintf "%s\n\r%s\n\r%s\n\r%s\n\r%s" 
                            "The project creation has failed."
                            "Ensure that you have installed at least the beta version of ASP.NET MVC 4." 
                            "See http://asp.net/mvc/mvc4 for more information."
                            "The actual exception message is: "
                            ex.Message)
            finally
                Cursor.Current <- currentCursor
            
