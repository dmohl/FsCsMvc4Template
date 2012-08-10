﻿namespace FsMvc4TemplateWizard

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
    [<DefaultValue>] val mutable isWebApi : bool

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
                this.isWebApi <- dialog.IsWebApi
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
                let mutable selectedWebAppProjectName = "WebApp"
                let webName = match this.isWebApi with
                              | true -> selectedWebProjectName <- "WebApi"
                                        this.safeProjectName + "WebApi"
                              | _ -> this.safeProjectName + "Web"
                let webAppName = match this.isWebApi with
                                 | true -> selectedWebAppProjectName <- "WebAppApi"
                                           this.safeProjectName + "WebAppApi"
                                 | _ -> this.safeProjectName + "WebApp"
                let webAppTestsName = this.safeProjectName + "WebAppTests"

                let templatePath = this.vsixInstallPath 
                try
                    let AddProject status projectVsTemplateName projectName =
                        this.dte2.StatusBar.Text <- status
                        let path = templatePath.Replace("FsMvc4.vstemplate", projectVsTemplateName)
                        this.dte2.Solution.AddFromTemplate(path, Path.Combine(this.destinationPath, projectName), 
                            projectName, false) |> ignore
                    AddProject "Installing the C# Web project..." 
                        (Path.Combine(selectedWebProjectName, selectedWebProjectName + ".vstemplate")) webName
                    AddProject "Adding the F# Web App project..." 
                        (Path.Combine(selectedWebAppProjectName, selectedWebAppProjectName + ".vstemplate")) webAppName
                    if this.includeTestProject then
                        AddProject "Adding the F# Web App Tests project..." 
                            (Path.Combine("WebAppTests", "WebAppTests.vstemplate")) webAppTestsName

                    let projects = BuildProjectMap (this.dte.Solution.Projects)

                    this.dte2.StatusBar.Text <- "Adding NuGet packages..."
                    try

// This is the List from the C# ASP.NET MVC Empty template found at C:\Program Files (x86)\Microsoft Visual Studio 10.0\Common7\IDE\ProjectTemplatesCache\CSharp\Web\1033\EmptyMvcWebApplicationProjectTemplatev4.0.cshtml.zip
//  <package id="EntityFramework" version="5.0.0-rc" />
//  <package id="jQuery" version="1.6.2" />
//  <package id="jQuery.UI.Combined" version="1.8.11" />
//  <package id="jQuery.Validation" version="1.8.1" />
//  <package id="knockoutjs" version="2.0.0" />
//  <package id="Microsoft.AspNet.Mvc" version="4.0.20505.0" />
//  <package id="Microsoft.AspNet.Providers" version="1.1" />
//  <package id="Microsoft.AspNet.Providers.Core" version="1.0" />
//  <package id="Microsoft.AspNet.Razor" version="2.0.20505.0" />
//  <package id="Microsoft.AspNet.Web.Optimization" version="1.0.0-beta2" />
//  <package id="Microsoft.AspNet.WebApi" version="4.0.20505.0" />
//  <package id="Microsoft.AspNet.WebApi.Client" version="4.0.20505.0" />
//  <package id="Microsoft.AspNet.WebApi.Core" version="4.0.20505.0" />
//  <package id="Microsoft.AspNet.WebApi.WebHost" version="4.0.20505.0" />
//  <package id="Microsoft.AspNet.WebPages" version="2.0.20505.0" />
//  <package id="Microsoft.jQuery.Unobtrusive.Ajax" version="2.0.20505.0" />
//  <package id="Microsoft.jQuery.Unobtrusive.Validation" version="2.0.20505.0" />
//  <package id="Microsoft.Net.Http" version="2.0.20505.0" />
//  <package id="Microsoft.Web.Infrastructure" version="1.0.0.0" />
//  <package id="Modernizr" version="2.0.6" />
//  <package id="Newtonsoft.Json" version="4.5.1" />
//  <package id="WebGrease" version="1.0.0" />

                        (projects.TryFind webName).Value |> InstallPackages this.serviceProvider (templatePath.Replace("FsMvc4.vstemplate", ""))
                        <| [("EntityFramework", "5.0.0"); ("jQuery.UI.Combined", "1.8.11"); ("jQuery", "1.6.4"); ("jQuery.Validation", "1.9.0.1")
                            ("knockoutjs", "2.1.0"); ("Microsoft.AspNet.Mvc", "4.0.20710.0"); ("Microsoft.AspNet.Providers", "1.2"); ("Microsoft.AspNet.WebApi.Core", "4.0.20710.0"); 
                            ("Microsoft.AspNet.Providers.Core", "1.1"); ("Microsoft.AspNet.Razor", "2.0.20710.0"); ("Microsoft.AspNet.Web.Optimization", "1.0.0") 
                            ("Microsoft.AspNet.WebApi", "4.0.20710.0"); ("Microsoft.AspNet.WebApi.Client", "4.0.20710.0"); ("Microsoft.AspNet.WebApi.WebHost", "4.0.20710.0")
                            ("Microsoft.AspNet.WebPages", "2.0.20710.0"); ("Microsoft.jQuery.Unobtrusive.Ajax", "2.0.20710.0"); ("Microsoft.jQuery.Unobtrusive.Validation", "2.0.20710.0")
                            ("Microsoft.Net.Http", "2.0.20710.0"); ("Microsoft.Web.Infrastructure", "1.0.0.0"); ("Modernizr", "2.5.3"); ("Microsoft.AspNet.Providers", "1.2");
                            ("Newtonsoft.Json", "4.5.6"); ("WebGrease", "1.1.0"); ("Microsoft.AspNet.Providers.LocalDb", "1.1"); ("Microsoft.AspNet.Web.Optimization", "1.0.0");]

                        // Need separate NuGet package installs for MVC and WebApi? This might improve perf. 
                    with
                    | ex -> failwith (sprintf "%s\n\r%s\n\r%s\n\r%s\n\r%s" 
                                "The NuGet installation process failed."
                                "Ensure that you have installed at least the release candidate of ASP.NET MVC 4." 
                                "See http://asp.net/mvc/mvc4 for more information."
                                "The actual exception message is: "
                                ex.Message)

                    this.dte2.StatusBar.Text <- "Updating project references..."
                    [(webName, webAppName); (webAppTestsName, webAppName)]
                    |> BuildProjectReferences projects 
                with
                | ex -> failwith (sprintf "%s\n\r%s\n\r%s\n\r%s\n\r%s" 
                            "The project creation has failed."
                            "Ensure that you have installed at least the release candidate of ASP.NET MVC 4." 
                            "See http://asp.net/mvc/mvc4 for more information."
                            "The actual exception message is: "
                            ex.Message)
            finally
                Cursor.Current <- currentCursor
            
