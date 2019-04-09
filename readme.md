# Welcome to QShine
QShine is a lightweight pluggable framework library for .NET application. It helps developer easily use third-party components without code dependency.
The pluggable components added into application through environment configuration setting and file copy. No application code change required.

```c#

    //Build application environment
    ApplicationEnvironment
        .Build()
        .Startup<IBoot>();
    
    //get environment variable
    var environmentVar1 = ApplicationEnvironment.Current["appVar1"];

    //get pluggable component instance
    var repository = ApplicationEnvironment.Current.CreateComponent<IInvoiceRepository>();

    //get pluggable provider
    var emailProvider = ApplicationEnvironment.Current.CreateProvider<IEmailProvider>();
    emailProvider.Send(subject, body, to);

    //get mapped provider by bus name
    var auditBus = ApplicationEnvironment.Current.CreateMappedProvider<IEventBus>("securityAuditBus");
    auditBus.Publish(auditLog);

```



## Common library services
- [Application Environment Configuration](qshine/docs/applicationEnvironment.md)
- [Application Startup](qshine/docs/applicationStartup.md)
- [Logging](qshine/docs/logging.md)
- [DI/IoC](qshine/docs/di.md)
- Database
- Context Store
- Cache
- Audit
- Messaging Bus
- Globalization
- Utility


### Build project
To build the solution you need enable the Nuget package manager => Allow NuGet download missing package.
1. VS =>Tools=>Options=>Nuget Package Manager => General => Allow NuGet to download missing packages CHECKED.
2. re-build solution. If solution failed try to rebuild solution several times. Some projects depends on other projects and bin files from Nuget package.
3. Using default setting or change config setting for your own test.